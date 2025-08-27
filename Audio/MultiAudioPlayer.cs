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

        public void PlayToDevices(List<string> deviceIds, string filePath, Dictionary<string, int> deviceDelays = null)
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
                    
                    if (delay > 0)
                    {
                        // Play with delay
                        Task.Delay(delay).ContinueWith(_ =>
                        {
                            PlayToSingleDevice(deviceNumber, filePath);
                        });
                    }
                    else
                    {
                        // Play immediately
                        PlayToSingleDevice(deviceNumber, filePath);
                    }
                }
                catch
                {
                    // If device fails to initialize, skip it
                    continue;
                }
            }
        }

        private void PlayToSingleDevice(int deviceNumber, string filePath)
        {
            try
            {
                var reader = new AudioFileReader(filePath);
                var outputDevice = new WaveOutEvent { DeviceNumber = deviceNumber };
                outputDevice.Init(reader);
                outputDevice.Play();

                _players.Add(outputDevice);
                _audioStreams.Add(reader);
            }
            catch
            {
                // If device fails to initialize, skip it
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
