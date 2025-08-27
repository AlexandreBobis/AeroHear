using System.Collections.Generic;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace AeroHear.Audio
{
    public static class AudioDeviceManager
    {
        public static List<MMDevice> GetOutputDevices()
        {
            var enumerator = new MMDeviceEnumerator();
            var devices = new List<MMDevice>();
            foreach (var device in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                devices.Add(device);
            }
            return devices;
        }

        public static int GetWaveOutDeviceNumber(string mmDeviceFriendlyName)
        {
            // Try exact match first
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                var caps = WaveOut.GetCapabilities(i);
                if (caps.ProductName == mmDeviceFriendlyName)
                    return i;
            }

            // Try partial match (sometimes names differ slightly)
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                var caps = WaveOut.GetCapabilities(i);
                if (caps.ProductName.Contains(mmDeviceFriendlyName) || 
                    mmDeviceFriendlyName.Contains(caps.ProductName))
                    return i;
            }

            return -1; // Device not found
        }
    }
}
