using ASCOM.iOptronZEQ25;
using ASCOM.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ASCOM.iOptronZEQ25
{
    [ComVisible(false)]					// Form not registered for COM!
    public partial class SetupDialogForm : Form
    {
        readonly TraceLogger tl; // Holder for a reference to the driver's trace logger

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
            Properties.Settings.Default.COMPort = (string)comboBoxComPort.SelectedItem;
            tl.Enabled = chkTrace.Checked;
            Properties.Settings.Default.Save();

            // Properties.Settings.Default.CommPort = (string)comboBoxComPort.SelectedItem;
            // Close();
        }

        private void CmdCancel_Click(object sender, EventArgs e) // Cancel button event handler
        {
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
            chkTrace.Checked = tl.Enabled;
            // set the list of com ports to those that are currently available
            comboBoxComPort.Items.Clear();
            comboBoxComPort.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());      // use System.IO because it's static
            // select the current port if possible
            //if (comboBoxComPort.Items.Contains(Telescope.comPort))
            //{
            //    comboBoxComPort.SelectedItem = Telescope.comPort;
            //}
            if (comboBoxComPort.Items.Contains(Properties.Settings.Default.COMPort))
            {
                comboBoxComPort.SelectedItem = Properties.Settings.Default.COMPort;
            }
        }
    }
}