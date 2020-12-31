using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASCOM.iOptronZEQ25.Properties
{
    [DeviceId("ASCOM.iOptronZEQ25.Telescope", DeviceName = "iOptron ZEQ25 Telescope")]
    [SettingsProvider(typeof(ASCOM.SettingsProvider))]
    internal partial class Settings
    {
    }
}
