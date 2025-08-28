using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.CoreAudioApi;

namespace AeroHear.Utils
{
    public class DelayCalibrationControl : UserControl
    {
        private readonly Dictionary<string, int> _deviceDelays = new();
        private readonly Dictionary<string, float> _deviceVolumes = new();
        private readonly List<MMDevice> _devices;
        private readonly List<Label> _deviceLabels = new();
        private readonly List<NumericUpDown> _delayControls = new();
        private readonly List<NumericUpDown> _volumeControls = new();

        public DelayCalibrationControl(List<MMDevice> devices)
        {
            _devices = devices;
            InitializeControl();
        }

        private void InitializeControl()
        {
            BackColor = Color.FromArgb(240, 240, 240);
            BorderStyle = BorderStyle.FixedSingle;
            
            var titleLabel = new Label
            {
                Text = "Calibrage des délais et volumes",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Left = 10,
                Top = 10,
                Width = 250,
                Height = 25
            };
            Controls.Add(titleLabel);

            // Headers
            var delayHeader = new Label
            {
                Text = "Délai (ms)",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Left = 420,
                Top = 35,
                Width = 80,
                Height = 15,
                TextAlign = ContentAlignment.MiddleCenter
            };
            Controls.Add(delayHeader);

            var volumeHeader = new Label
            {
                Text = "Volume (%)",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Left = 520,
                Top = 35,
                Width = 80,
                Height = 15,
                TextAlign = ContentAlignment.MiddleCenter
            };
            Controls.Add(volumeHeader);

            int y = 55;
            foreach (var device in _devices)
            {
                // Device name label
                var deviceLabel = new Label
                {
                    Text = device.FriendlyName,
                    Left = 10,
                    Top = y,
                    Width = 400,
                    Height = 25,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                Controls.Add(deviceLabel);
                _deviceLabels.Add(deviceLabel);

                // Delay input control
                var delayControl = new NumericUpDown
                {
                    Left = 420,
                    Top = y,
                    Width = 80,
                    Height = 25,
                    Minimum = 0,
                    Maximum = 2000,
                    Value = 0,
                    Increment = 10
                };
                delayControl.ValueChanged += (s, e) =>
                {
                    _deviceDelays[device.FriendlyName] = (int)delayControl.Value;
                };
                Controls.Add(delayControl);
                _delayControls.Add(delayControl);

                // Volume input control
                var volumeControl = new NumericUpDown
                {
                    Left = 520,
                    Top = y,
                    Width = 80,
                    Height = 25,
                    Minimum = 0,
                    Maximum = 100,
                    Value = 100,
                    Increment = 5
                };
                volumeControl.ValueChanged += (s, e) =>
                {
                    _deviceVolumes[device.FriendlyName] = (float)volumeControl.Value / 100f;
                };
                Controls.Add(volumeControl);
                _volumeControls.Add(volumeControl);

                // Unit labels
                var delayUnitLabel = new Label
                {
                    Text = "ms",
                    Left = 505,
                    Top = y,
                    Width = 15,
                    Height = 25,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                Controls.Add(delayUnitLabel);

                var volumeUnitLabel = new Label
                {
                    Text = "%",
                    Left = 605,
                    Top = y,
                    Width = 15,
                    Height = 25,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                Controls.Add(volumeUnitLabel);

                // Initialize values
                _deviceDelays[device.FriendlyName] = 0;
                _deviceVolumes[device.FriendlyName] = 1.0f;

                y += 30;
            }

            // Reset button
            var resetButton = new Button
            {
                Text = "Réinitialiser",
                Left = 10,
                Top = y + 10,
                Width = 100,
                Height = 30
            };
            resetButton.Click += (s, e) =>
            {
                foreach (var control in _delayControls)
                {
                    control.Value = 0;
                }
                foreach (var control in _volumeControls)
                {
                    control.Value = 100;
                }
            };
            Controls.Add(resetButton);

            // Auto calibration button
            var autoCalibButton = new Button
            {
                Text = "Calibrage auto",
                Left = 120,
                Top = y + 10,
                Width = 120,
                Height = 30,
                BackColor = Color.LightBlue
            };
            autoCalibButton.Click += async (s, e) =>
            {
                await PerformAutoCalibration();
            };
            Controls.Add(autoCalibButton);

            // Set the control height to accommodate all content
            Height = y + 60; // Enough space for buttons + padding
        }

        public Dictionary<string, int> GetDelays()
        {
            return new Dictionary<string, int>(_deviceDelays);
        }

        public Dictionary<string, float> GetVolumes()
        {
            return new Dictionary<string, float>(_deviceVolumes);
        }

        public void SetDelays(Dictionary<string, int> delays)
        {
            for (int i = 0; i < _devices.Count; i++)
            {
                var deviceName = _devices[i].FriendlyName;
                if (delays.ContainsKey(deviceName))
                {
                    _delayControls[i].Value = delays[deviceName];
                }
            }
        }

        public void SetVolumes(Dictionary<string, float> volumes)
        {
            for (int i = 0; i < _devices.Count; i++)
            {
                var deviceName = _devices[i].FriendlyName;
                if (volumes.ContainsKey(deviceName))
                {
                    _volumeControls[i].Value = (decimal)(volumes[deviceName] * 100);
                }
            }
        }

        private async Task PerformAutoCalibration()
        {
            var progressForm = new Form
            {
                Text = "Calibrage automatique en cours...",
                Width = 400,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                StartPosition = FormStartPosition.CenterParent
            };

            var progressLabel = new Label
            {
                Text = "Mesure de la latence des périphériques...",
                Left = 20,
                Top = 20,
                Width = 350,
                Height = 20
            };
            progressForm.Controls.Add(progressLabel);

            var progressBar = new ProgressBar
            {
                Left = 20,
                Top = 50,
                Width = 350,
                Height = 20,
                Maximum = _devices.Count
            };
            progressForm.Controls.Add(progressBar);

            progressForm.Show();

            try
            {
                var baselineLatency = int.MaxValue;
                var deviceLatencies = new Dictionary<string, int>();

                // Measure latency for each device
                for (int i = 0; i < _devices.Count; i++)
                {
                    var device = _devices[i];
                    progressLabel.Text = $"Test de {device.FriendlyName}...";
                    progressBar.Value = i;
                    Application.DoEvents();

                    // Simulate latency measurement (in a real implementation, this would play a test sound)
                    var latency = await LatencyTester.EstimateLatencyAsync(() => 
                    {
                        System.Media.SystemSounds.Beep.Play();
                    });
                    
                    deviceLatencies[device.FriendlyName] = latency;
                    baselineLatency = Math.Min(baselineLatency, latency);

                    await Task.Delay(500); // Brief pause between tests
                }

                progressBar.Value = _devices.Count;
                progressLabel.Text = "Application des corrections...";
                Application.DoEvents();

                // Apply delay corrections
                for (int i = 0; i < _devices.Count; i++)
                {
                    var deviceName = _devices[i].FriendlyName;
                    var correction = Math.Max(0, deviceLatencies[deviceName] - baselineLatency);
                    _delayControls[i].Value = correction;
                }

                await Task.Delay(500);
                MessageBox.Show($"Calibrage automatique terminé!\nLatence de base: {baselineLatency}ms", 
                    "Calibrage automatique", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du calibrage automatique: {ex.Message}", 
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                progressForm.Close();
            }
        }
    }
}