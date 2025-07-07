using System.Collections.Generic;
using NAudio.CoreAudioApi;

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
    }
}
