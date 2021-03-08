using System.Configuration;

namespace ASCOM.iOptronZEQ25.Properties
{
    [DeviceId("ASCOM.iOptronZEQ25.Telescope", DeviceName = "iOptron ZEQ25 Telescope")]
    [SettingsProvider(typeof(ASCOM.SettingsProvider))]
    internal partial class Settings
    {
    }
}