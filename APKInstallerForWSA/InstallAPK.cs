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
using AndroidDebugBridge;

namespace APKInstallerForWSA
{
    public partial class InstallAPK : Form
    {
        public InstallAPK(string apkPath)
        {
            InitializeComponent();
            installApk(apkPath);
        }

        Dispatcher currentDispatcher = Dispatcher.CurrentDispatcher;

        void installApk(string apk)
        {
            Task task = new Task(() =>
            {
                ADB adb = new ADB();
                adb.ApkInstallFailed += Adb_ApkInstallFailed;
                bool success = adb.InstallApk(apk);

                if (success)
                {
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
                }
                else
                {
                    currentDispatcher.Invoke(
                        () =>
                        {
                            label1.Text = "Error installing.";
                            progressBar1.Style = ProgressBarStyle.Continuous;
                            progressBar1.Value = 0;
                            this.Refresh();
                            button1.Enabled = true;
                        });
                }
            });
            task.Start();
        }

        private void Adb_ApkInstallFailed(object sender, ApkInstallFailedEventArgs e)
        {
            MessageBox.Show(String.Format("Installation failed. \n\n{0}", e.ErrorMessage), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
