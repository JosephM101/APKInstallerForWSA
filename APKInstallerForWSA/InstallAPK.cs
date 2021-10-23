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
using System.Windows.Shell;
using System.Windows.Threading;

namespace APKInstallerForWSA
{
    public partial class InstallAPK : Form
    {
        public InstallAPK(string apkPath)
        {
            InitializeComponent();
            installApk(apkPath);
        }

        String installCommand;

        Dispatcher currentDispatcher = Dispatcher.CurrentDispatcher;

        void installApk(string apk)
        {
            installCommand = "install \"" + apk + "\"";
            //MessageBox.Show(installCommand);
            Task task = new Task(() =>
            {
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "adb.exe",
                        Arguments = installCommand,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        StandardOutputEncoding = System.Text.Encoding.UTF8,
                        CreateNoWindow = true
                    }
                };
                proc.Start();

                string DataOutput = "";

                while (!proc.StandardOutput.EndOfStream)
                {
                    var line = proc.StandardOutput.ReadLine();
                    MessageBox.Show(line);
                    DataOutput += line.ToString();
                }
                proc.WaitForExit();
                //string DataOutput = proc.StandardOutput.ReadToEnd();

                //if (DataOutput.Contains("Success"))
                //{
                //    //System.Windows.Application.Current.Dispatcher.Invoke(
                //    currentDispatcher.Invoke(
                //    () =>
                //    {
                //        label1.Text = "Done!";
                //        progressBar1.Style = ProgressBarStyle.Continuous;
                //        progressBar1.Value = 0;
                //        this.Refresh();
                //        progressBar1.Value = 100;
                //        button1.Enabled = true;
                //    });
                //}
                //else
                //{
                //    //System.Windows.Application.Current.Dispatcher.Invoke(
                //    currentDispatcher.Invoke(
                //    () =>
                //    {
                //        label1.Text = "Error installing";
                //        progressBar1.Style = ProgressBarStyle.Continuous;
                //        progressBar1.Value = 0;
                //        this.Refresh();
                //        button1.Enabled = true;
                //        MessageBox.Show("Error installing APK. \n\nMessage: " + DataOutput, "Error installing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //    });
                //}
                currentDispatcher.Invoke(
                    () =>
                    {
                        label1.Text = "Done!";
                        progressBar1.Style = ProgressBarStyle.Continuous;
                        progressBar1.Value = 0;
                        this.Refresh();
                        progressBar1.Value = 100;
                        button1.Enabled = true;
                        
                    });
            });
            task.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
