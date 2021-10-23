using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace APKInstallerForWSA
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            //Debug.WriteLine("Hello, world!");
        }

        void PrintStatus(string status)
        {
            statusLabel.Text = status;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            PrintStatus("Killing existing ADB processes...");
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "adb.exe",
                    Arguments = "kill-server",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            proc.WaitForExit();

            PrintStatus("Starting new ADB process...");
            this.Refresh();
            var proc2 = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "adb.exe",
                    Arguments = "start-server",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            proc2.Start();
            proc2.WaitForExit();
            string output = proc2.StandardOutput.ReadToEnd();
            //MessageBox.Show(output);
            if (output.Contains("daemon started successfully"))
            {
                //ADB is running
                button3.Enabled = true;
                label2.Visible = true;
            }
            PrintStatus("Ready");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            PrintStatus("Connecting to device...");
            string lineToLookFor = "connected to " + textBox1.Text;
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "adb.exe",
                    Arguments = "connect " + textBox1.Text,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            proc.Start();

            string output = proc.StandardOutput.ReadToEnd();
            //MessageBox.Show(output);
            if (output.Contains(lineToLookFor))
            {
                label3.Visible = true;
                button1.Enabled = true;
            }
            else if (output.Contains("unable to connect"))
            {
                MessageBox.Show("Error connecting to device. It may not exist, or is inaccessible. Make sure the IP address you entered is correct. \n\nError message: " + output);
            }
            else
            {
                MessageBox.Show("Error connecting to device. It may not exist, or is inaccessible. Make sure the IP address you entered is correct. \n\nError message: ADB did not respond.");
            }
            PrintStatus("Ready");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(openFileDialog1.ShowDialog()==DialogResult.OK)
            {
                new InstallAPK(openFileDialog1.FileName).ShowDialog();
            }
        }
    }
}