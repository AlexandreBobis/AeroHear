using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AeroHear.Audio;
using AeroHear.Utils;
using NAudio.CoreAudioApi;

namespace AeroHear.Forms
{
    public partial class MainForm : Form
    {
        private readonly MultiAudioPlayer _player = new();
        private readonly List<MMDevice> _devices = AudioDeviceManager.GetOutputDevices();
        private readonly List<CheckBox> _deviceCheckboxes = new();
        private DelayCalibrationControl _delayCalibration;

        private string _audioFilePath = "";

        public MainForm()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            Text = "AeroHear";
            Width = 700;
            Height = 520; // Increased to accommodate larger calibration control

            var btnLoad = new Button { Text = "Charger audio", Top = 10, Left = 20, Width = 120 };
            btnLoad.Click += BtnLoad_Click;
            Controls.Add(btnLoad);

            var btnPlay = new Button { Text = "Lire", Top = 10, Left = 150, Width = 100 };
            btnPlay.Click += BtnPlay_Click;
            Controls.Add(btnPlay);

            var btnStop = new Button { Text = "Stop", Top = 10, Left = 260, Width = 100 };
            btnStop.Click += (s, e) => _player.Stop();
            Controls.Add(btnStop);

            var btnTest = new Button { Text = "Tester latence", Top = 10, Left = 370, Width = 120 };
            btnTest.Click += async (s, e) =>
            {
                int ms = await LatencyTester.EstimateLatencyAsync(() => System.Media.SystemSounds.Beep.Play());
                MessageBox.Show($"Latence estimée : {ms} ms");
            };
            Controls.Add(btnTest);

            var grp = new GroupBox { Text = "Périphériques Bluetooth", Top = 50, Left = 20, Width = 650, Height = 200 };
            Controls.Add(grp);

            int y = 20;
            foreach (var device in _devices)
            {
                var check = new CheckBox
                {
                    Text = device.FriendlyName,
                    Left = 10,
                    Top = y,
                    Width = 600
                };
                grp.Controls.Add(check);
                _deviceCheckboxes.Add(check);
                y += 25;
            }

            _delayCalibration = new DelayCalibrationControl(_devices)
            {
                Left = 20,
                Top = 270,
                Width = 640,
                Height = 200  // Increased height to accommodate volume controls and auto calibration button
            };
            Controls.Add(_delayCalibration);
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "Fichiers audio (*.mp3;*.wav;*.flac)|*.mp3;*.wav;*.flac|Tous les fichiers (*.*)|*.*"
            };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _audioFilePath = dlg.FileName;
                MessageBox.Show("Fichier chargé : " + Path.GetFileName(_audioFilePath));
            }
        }

        private void BtnPlay_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_audioFilePath))
            {
                MessageBox.Show("Veuillez charger un fichier audio.");
                return;
            }

            var selected = _deviceCheckboxes
                .Where(c => c.Checked)
                .Select(c => c.Text)
                .ToList();

            if (selected.Count == 0)
            {
                MessageBox.Show("Veuillez sélectionner au moins un périphérique Bluetooth.");
                return;
            }

            var delays = _delayCalibration.GetDelays();
            var volumes = _delayCalibration.GetVolumes();
            _player.PlayToDevices(selected, _audioFilePath, delays, volumes);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}
