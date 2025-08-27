using System.Collections.Generic;
using System.IO;
using NAudio.Wave;

namespace AeroHear.Audio
{
    public class MultiAudioPlayer
    {
        private readonly List<IWavePlayer> _players = new List<IWavePlayer>();
        private readonly List<WaveStream> _audioStreams = new List<WaveStream>();

        public void PlayToDevices(List<string> deviceIds, string filePath)
        {
            Stop();

            foreach (var id in deviceIds)
            {
                var reader = new AudioFileReader(filePath);
                var outputDevice = new WaveOutEvent { DeviceNumber = GetDeviceNumber(id) };
                outputDevice.Init(reader);
                outputDevice.Play();

                _players.Add(outputDevice);
                _audioStreams.Add(reader);
            }
        }

        public void Stop()
        {
            foreach (var player in _players)
            {
                player.Stop();
                player.Dispose();
            }

            foreach (var stream in _audioStreams)
            {
                stream.Dispose();
            }

            _players.Clear();
            _audioStreams.Clear();
        }

        private int GetDeviceNumber(string deviceName)
        {
            return AudioDeviceManager.GetWaveOutDeviceNumber(deviceName);
        }
    }
}
