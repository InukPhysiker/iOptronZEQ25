﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ASCOM.iOptronZEQ25
{
    [ComVisible(false)] // Form not registered for COM!
    public partial class ServerSetupDialogForm : Form
    {
        public ServerSetupDialogForm()
        {
            InitializeComponent();
        }

        private void AboutBox_Click(object sender, EventArgs e)
        {
            //using (var aboutBox = new AboutBox())
            //{
            //	aboutBox.ShowDialog();
            //}
        }

        private void BrowseToAscom(object sender, EventArgs e)
        {
            try
            {
                Process.Start("http://ascom-standards.org/");
            }
            catch (Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    MessageBox.Show(noBrowser.Message);
            }
            catch (Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            communicationSettingsControl1.Save();
        }

        private void ServerSetupDialogForm_Load(object sender, EventArgs e)
        {
            var onlineClients = SharedResources.ConnectionManager.OnlineClientCount;
            if (onlineClients == 0)
            {
                communicationSettingsControl1.Enabled = true;
                ConnectionErrorProvider.SetError(communicationSettingsControl1, string.Empty);
            }
            else
            {
                communicationSettingsControl1.Enabled = false;
                ConnectionErrorProvider.SetError(communicationSettingsControl1,
                    "Connection settings cannot be changed while there are connected clients");
            }
        }
    }
}