using ASCOM.iOptronZEQ25.Server.Properties;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Windows.Forms;

namespace ASCOM.iOptronZEQ25.Server
{
    public partial class CommunicationSettingsControl : UserControl
    {
        public CommunicationSettingsControl()
        {
            InitializeComponent();
            var currentSelection = Settings.Default.CommPortName;
            var ports = new SortedSet<string>(SerialPort.GetPortNames());
            if (!ports.Contains(currentSelection))
                {
                ports.Add(currentSelection);
                }
            CommPortName.Items.Clear();
            CommPortName.Items.AddRange(ports.ToArray());
            var currentIndex = CommPortName.Items.IndexOf(currentSelection);
            CommPortName.SelectedIndex = currentIndex;
            }

        internal void Save()
        {
            Settings.Default.ConnectionString = $"{Settings.Default.CommPortName}:9600";
            Settings.Default.Save();
        }
    }
}
