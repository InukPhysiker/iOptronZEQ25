﻿using ASCOM.iOptronZEQ25.Server.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public void Save()
            {
            Settings.Default.ConnectionString = $"{Settings.Default.CommPortName}:9600";
            Settings.Default.Save();
        }
    }
}