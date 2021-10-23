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
using System.IO;

namespace AndroidDebugBridge
{
    public class AndroidDevice
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public AndroidDevice(string name, string id)
        {
            Name = name;
            Id = id;
        }
    }

    [Serializable]
    public class AdbNotFoundException : Exception
    {
        private readonly string adb_path;

        public AdbNotFoundException(string adbPath) : base(String.Format("ADB does not exist, or executable is invalid: {0}", adbPath))
        {
            adb_path = adbPath;
        }

        public string AdbPath
        {
            get { return AdbPath; }
        }
    }

    [Serializable]
    public class NonexistentDeviceException : Exception
    {
        private readonly AndroidDevice device;
        public NonexistentDeviceException(AndroidDevice device) : base("Could not connect to device.")
        {
            this.device = Device;
        }

        private AndroidDevice Device
        {
            get
            {
                if(device != null)
                {
                    return device;
                }
                else
                {
                    throw new Exception("Device is null.");
                }
            }
        }
    }

    [Serializable]
    public class ADBConnectionErrorException : Exception
    {
        private readonly string deviceIp;
        public ADBConnectionErrorException(string ip) : base(string.Format("Could not connect to wireless ADB instance at {0}.", ip))
        {
            deviceIp = ip;
        }

        private string IpAddress
        {
            get
            {
                if(deviceIp != null)
                {
                    return deviceIp;
                }
                else
                {
                    throw new Exception("Device IP is null.");
                }
            }
        }
    }
    public delegate void ShellCommandStartedEventHandler(object sender, ShellCommandStartedEventArgs e);
    public delegate void ShellCommandCompletedEventHandler(object sender, ShellCommandCompletedEventArgs e);

    public class ShellCommandStartedEventArgs : EventArgs
    {
        public string Command { get; internal set; }
        public ShellCommandStartedEventArgs(string command)
        {
            Command = command;
        }
    }

    public class ShellCommandCompletedEventArgs : EventArgs
    {
        //public string Command { get; internal set; }
        //public string Output { get; internal set; }
        //public string Error { get; set; }
        //public int ExitCode { get; set; }

        public AdbCommandResult AdbCommandResult { get; internal set; }
        public ShellCommandCompletedEventArgs(AdbCommandResult adbCommandResult)
        {
            AdbCommandResult = adbCommandResult;
        }

        // public ShellCommandCompletedEventArgs(string command, string output)
        // {
        //     Command = command;
        //     Output = output;
        // }
    }

    public class ADB
    {
        //ADB path
        string adbPath;
        string adbPathCMD;
        private string adbVersion;

        //Event handlers
        public event EventHandler<ShellCommandStartedEventArgs> ShellCommandStarted;
        public event EventHandler<ShellCommandCompletedEventArgs> ShellCommandCompleted;
        protected virtual void OnShellCommandStarted(ShellCommandStartedEventArgs e)
        {
            EventHandler<ShellCommandStartedEventArgs> handler = ShellCommandStarted;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        protected virtual void OnShellCommandCompleted(ShellCommandCompletedEventArgs e)
        {
            EventHandler<ShellCommandCompletedEventArgs> handler = ShellCommandCompleted;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public string AdbVersion
        {
            get
            {
                return adbVersion;
            }
        }
        /// <summary>
        /// Wrapper for interfacing with Android Debug Bridge command-line tool
        /// </summary>
        public ADB()
        {
            if (File.Exists(@"C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe"))
            {
                adbPath = @"C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe";
                adbPathCMD = "\"" + adbPath + "\"";
            }
            else if (File.Exists(@"C:\Program Files\Android\android-sdk\platform-tools\adb.exe"))
            {
                adbPath = @"C:\Program Files\Android\android-sdk\platform-tools\adb.exe";
                adbPathCMD = "\"" + adbPath + "\"";
            }
            else
            {
                adbPath = "adb";
                adbPathCMD = adbPath; // Quotes are not needed for environment variables.
            }

            TestAdb();
        }

        /// <summary>
        /// Wrapper for interfacing with Android Debug Bridge command-line tool
        /// </summary>
        public ADB(string AdbPath)
        {
            if (!File.Exists(AdbPath))
            {
                throw new FileNotFoundException(String.Format("Error initializing ADB: File {0} does not exist.", AdbPath));
            }
            else
            {
                adbPath = AdbPath;
                adbPathCMD = "\"" + adbPath + "\"";
                TestAdb();
            }
        }

        public void StartServer()
        {
            string output = runAdbCommand("start-server", true);
            if(output.Contains("daemon started successfully"))
            {
                return;
            }
            else
            {
                throw new Exception("Could not start ADB server.");
            }
        }

        private void TestAdb()
        {
            string output = runAdbCommand("version", true);
            if(output.Contains("Android Debug Bridge"))
            {
                adbVersion = output.Substring(output.IndexOf("Android Debug Bridge") + 21, output.IndexOf("(") - output.IndexOf("Android Debug Bridge") - 21);
            }
            else
            {
                throw new AdbNotFoundException(adbPath);
            }
        }

        public string RunShellCommand(string command)
        {
            ShellCommandStartedEventArgs shellCommandStartedEventArgs = new ShellCommandStartedEventArgs(command);
            OnShellCommandStarted(shellCommandStartedEventArgs);

            string output = runAdbCommand("shell " + command, false);

            ShellCommandCompletedEventArgs shellCommandCompletedEventArgs = new ShellCommandCompletedEventArgs(new AdbCommandResult(output, false, ""));
            OnShellCommandCompleted(shellCommandCompletedEventArgs);

            return output;
        }

        public void KillServer()
        {
            runAdbCommand("kill-server", true);
        }

        public List<AndroidDevice> GetDevices()
        {
            List<AndroidDevice> devices = new List<AndroidDevice>();
            string output = runAdbCommand("devices", true);
            string[] lines = output.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                if (line.Contains("List of devices attached"))
                {
                    continue;
                }
                if (line.Contains("device"))
                {
                    string[] parts = line.Split('\t');
                    devices.Add(new AndroidDevice(parts[0], parts[1]));
                }
            }
            return devices;
        }

        string GetAndroidVersion(string deviceId)
        {
            // ProcessStartInfo startInfo = new ProcessStartInfo();
            // startInfo.FileName = "cmd.exe";
            // startInfo.Arguments = "/C adb shell getprop ro.build.version.release";
            // startInfo.UseShellExecute = false;
            // startInfo.CreateNoWindow = true;
            // startInfo.RedirectStandardOutput = true;
            // startInfo.RedirectStandardError = true;
            // Process.Start(startInfo);
            return runAdbCommand("shell getprop ro.build.version.release", false);
        }

        private string runAdbCommand(string command, bool WaitForExit)
        {
            // ProcessStartInfo startInfo = new ProcessStartInfo();
            // startInfo.FileName = adbPath;
            // startInfo.Arguments = command;
            // startInfo.UseShellExecute = false;
            // startInfo.RedirectStandardOutput = true;
            // startInfo.CreateNoWindow = true;
            // startInfo.RedirectStandardError = true;
            // Process process = Process.Start(startInfo);
            // string output = process.StandardOutput.ReadToEnd();
            // process.WaitForExit();
            // return output;

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = String.Format("/C {0} {1}", adbPathCMD, command);
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            Process process = Process.Start(startInfo);
            string output = process.StandardOutput.ReadToEnd();
            if(WaitForExit)
            {
                process.WaitForExit();
            }
            return output;
        }
    }

    public class AdbCommandResult
    {
        private string output;
        private bool commandFailed;
        private string errorOutput;

        public AdbCommandResult(string output, bool commandFailed, string errorOutput)
        {
            this.output = output;
            this.commandFailed = commandFailed;
            this.errorOutput = errorOutput;
        }

        public string Output { get { return output; } }
        public bool CommandFailed { get { return commandFailed; } }
        public string ErrorOutput { get { return errorOutput; } }
    }
}