using ASCOM.Utilities;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ASCOM.iOptronZEQ25
{
    [ComVisible(false)]					// Form not registered for COM!
    public partial class SetupDialogForm : Form
    {
        private readonly TraceLogger tl; // Holder for a reference to the driver's trace logger

        public SetupDialogForm(TraceLogger tlDriver)
        {
            InitializeComponent();

            // Save the provided trace logger for use within the setup dialogue
            tl = tlDriver;

            // Initialise current values of user settings from the ASCOM Profile
            InitUI();
        }

        private void CmdOK_Click(object sender, EventArgs e) // OK button event handler
        {
            // Place any validation constraint checks here
            // Update the state variables with results from the dialogue
            //Telescope.comPort = (string)comboBoxComPort.SelectedItem;
            var currentConnectionString = iOptronZEQ25.Properties.Settings.Default.ConnectionString;

            var newCOMPort = (string)comboBoxComPort.SelectedItem;
            var newConnectionString = $"{newCOMPort}:9600";

            iOptronZEQ25.Properties.Settings.Default.COMPort = newCOMPort;
            iOptronZEQ25.Properties.Settings.Default.ConnectionString = newConnectionString;

            SharedResources.CommPortName = newCOMPort;
            SharedResources.ConnectionString = newConnectionString;

            // tl.Enabled = chkTrace.Checked;
            //Properties.Settings.Default.Trace = chkTrace.Checked;
            iOptronZEQ25.Properties.Settings.Default.Save();

            // Properties.Settings.Default.CommPort = (string)comboBoxComPort.SelectedItem;
            //Log.Info($"SetupDialog successful, saving settings");
            if (newConnectionString != currentConnectionString)
            {
                //Log.Warn(
                //    $"Connection string has changed from {oldConnectionString} to {newConnectionString} - replacing the TansactionProcessorFactory");
                SharedResources.UpdateTransactionProcessFactory();
            }
            Close();
        }

        private void CmdCancel_Click(object sender, EventArgs e) // Cancel button event handler
        {
            iOptronZEQ25.Properties.Settings.Default.Reload();
            Close();
        }

        private void BrowseToAscom(object sender, EventArgs e) // Click on ASCOM logo event handler
        {
            try
            {
                System.Diagnostics.Process.Start("https://ascom-standards.org/");
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    MessageBox.Show(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }

        private void InitUI()
        {
            var onlineClients = SharedResources.ConnectionManager.OnlineClientCount;
            if (onlineClients == 0)
            {
                comboBoxComPort.Enabled = true;
                ConnectionErrorProvider.SetError(comboBoxComPort, string.Empty);
            }
            else
            {
                comboBoxComPort.Enabled = false;
                ConnectionErrorProvider.SetError(comboBoxComPort,
                    "Connection settings cannot be changed while there are connected clients");
            }
            //chkTrace.Checked = tl.Enabled;
            // set the list of com ports to those that are currently available
            comboBoxComPort.Items.Clear();
            comboBoxComPort.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());      // use System.IO because it's static
            // select the current port if possible
            //if (comboBoxComPort.Items.Contains(Telescope.comPort))
            //{
            //    comboBoxComPort.SelectedItem = Telescope.comPort;
            //}
            if (comboBoxComPort.Items.Contains(iOptronZEQ25.Properties.Settings.Default.COMPort))
            {
                comboBoxComPort.SelectedItem = iOptronZEQ25.Properties.Settings.Default.COMPort;
            }
        }
    }
}