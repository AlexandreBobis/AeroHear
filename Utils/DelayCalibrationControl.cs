using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NAudio.CoreAudioApi;

namespace AeroHear.Utils
{
    public class DelayCalibrationControl : Control
    {
        private readonly Dictionary<string, int> _deviceDelays = new();
        private readonly List<MMDevice> _devices;
        private readonly List<Label> _deviceLabels = new();
        private readonly List<NumericUpDown> _delayControls = new();

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
                Text = "Calibrage des délais (ms)",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Left = 10,
                Top = 10,
                Width = 200,
                Height = 25
            };
            Controls.Add(titleLabel);

            int y = 40;
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

                // Unit label
                var unitLabel = new Label
                {
                    Text = "ms",
                    Left = 510,
                    Top = y,
                    Width = 30,
                    Height = 25,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                Controls.Add(unitLabel);

                // Initialize delay to 0
                _deviceDelays[device.FriendlyName] = 0;

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
            };
            Controls.Add(resetButton);
        }

        public Dictionary<string, int> GetDelays()
        {
            return new Dictionary<string, int>(_deviceDelays);
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
    }
}