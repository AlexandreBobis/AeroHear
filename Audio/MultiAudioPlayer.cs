using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;

namespace AeroHear.Audio
{
    public class MultiAudioPlayer
    {
        private readonly List<IWavePlayer> _players = new List<IWavePlayer>();
        private readonly List<WaveStream> _audioStreams = new List<WaveStream>();
        private readonly object _lock = new object();

        public void PlayToDevices(List<string> deviceIds, string filePath, Dictionary<string, int> deviceDelays = null, Dictionary<string, float> deviceVolumes = null)
        {
            Stop();

            foreach (var id in deviceIds)
            {
                var deviceNumber = GetDeviceNumber(id);
                if (deviceNumber == -1)
                {
                    // Skip devices that cannot be found
                    continue;
                }

                try
                {
                    var delay = deviceDelays?.ContainsKey(id) == true ? deviceDelays[id] : 0;
                    var volume = deviceVolumes?.ContainsKey(id) == true ? deviceVolumes[id] : 1.0f;
                    
                    if (delay > 0)
                    {
                        // Play with delay using Task.Run to avoid blocking
                        Task.Run(async () =>
                        {
                            await Task.Delay(delay);
                            PlayToSingleDevice(deviceNumber, filePath, volume);
                        });
                    }
                    else
                    {
                        // Play immediately
                        PlayToSingleDevice(deviceNumber, filePath, volume);
                    }
                }
                catch
                {
                    // If device fails to initialize, skip it
                    continue;
                }
            }
        }

        private void PlayToSingleDevice(int deviceNumber, string filePath, float volume = 1.0f)
        {
            try
            {
                var reader = new AudioFileReader(filePath);
                reader.Volume = Math.Max(0.0f, Math.Min(1.0f, volume)); // Clamp volume between 0 and 1
                var outputDevice = new WaveOutEvent { DeviceNumber = deviceNumber };
                outputDevice.Init(reader);
                outputDevice.Play();

                lock (_lock)
                {
                    _players.Add(outputDevice);
                    _audioStreams.Add(reader);
                }
            }
            catch
            {
                // If device fails to initialize, skip it
            }
        }

        public void Stop()
        {
            lock (_lock)
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
        }

        private int GetDeviceNumber(string deviceName)
        {
            return AudioDeviceManager.GetWaveOutDeviceNumber(deviceName);
        }
    }
}
