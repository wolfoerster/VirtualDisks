//***************************************************************************************
// Copyright © 2017 wolfoerster@gmx.de. All Rights Reserved.
//
//***************************************************************************************
using System.IO;
using System.Windows;
using System.Diagnostics;
using System.Windows.Input;
using System.Collections.Generic;
using System.Text;

namespace VirtualDisks
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeRecords();
            DataContext = this;

            Top = Properties.Settings.Default.Top;
            Left = Properties.Settings.Default.Left;
            if (Top <= 0 || Left <= 0)
                WindowStartupLocation = WindowStartupLocation.CenterScreen;

            Closing += MeClosing;
        }

        void MeClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Left = Left;
            Properties.Settings.Default.Top = Top;
            Properties.Settings.Default.Save();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape)
                Close();
        }

        public List<Record> Records { get; set; }

        void InitializeRecords()
        {
            Records = new List<Record>();
            Record.OnMountedChanged = MountVHD;

            var infos = DriveInfo.GetDrives();
            foreach (var info in infos)
            {
                if (string.IsNullOrWhiteSpace(info.Name)
                    || info.Name.StartsWith("A:")
                    || info.Name.StartsWith("B:")
                    || !info.IsReady)
                    continue;

                var name = info.Name + "WindowsImageBackup";
                if (!Directory.Exists(name))
                    continue;

                var subdirs = Directory.GetDirectories(name);
                foreach (var subdir in subdirs)
                    CheckDirectory(subdir);
            }
        }

        void CheckDirectory(string dir)
        {
            var computer = new Record(dir.Substring(dir.LastIndexOf("\\") + 1));
            Records.Add(computer);

            var subdirs = Directory.GetDirectories(dir);
            foreach (var subdir in subdirs)
            {
                var str = "\\Backup ";
                var idx = subdir.IndexOf(str);
                if (idx < 0)
                    continue;

                var specs = File.ReadAllText(subdir + "\\BackupSpecs.xml", Encoding.Unicode);
                var backup = new Record(Beautify(subdir.Substring(idx + str.Length)));
                computer.Children.Add(backup);

                var vhds = new List<Record>();
                var files = Directory.GetFiles(subdir, "*.vhd");
                foreach (var file in files)
                {
                    var path = FindPath(specs, file);
                    if (path.Length == 3)
                    {
                        bool isMounted = Properties.Settings.Default.MountedFiles.Contains(file);
                        vhds.Add(new Record(path, file, isMounted));
                    }
                }

                vhds.Sort((vhd1, vhd2) => vhd1.Name.CompareTo(vhd2.Name));
                vhds.ForEach(vhd => backup.Children.Add(vhd));
            }
        }

        string Beautify(string name)
        {
            name = name.Insert(name.Length - 4, ":");
            name = name.Insert(name.Length - 2, ":");
            return name;
        }

        string FindPath(string specs, string fileName)
        {
            var name = Path.GetFileNameWithoutExtension(fileName);
            var i = specs.IndexOf(name);
            if (i < 0)
                return "";
            i += name.Length;

            var str = "FilePath=\"";
            i = specs.IndexOf(str, i);
            if (i < 0)
                return "";
            i += str.Length;

            var j = specs.IndexOf("\"", i);
            if (j < 0)
                return "";

            var path = specs.Substring(i, j - i);
            return path;
        }

        bool MountVHD(string fileName, bool mode)
        {
            if (!File.Exists(fileName))
                return false;

            Remember(fileName);
            var script = Path.GetTempPath() + "diskpart.txt";
            File.WriteAllText(script, string.Format("select vdisk file=\"{0}\"\r\n{1} vdisk", fileName, mode ? "attach" : "detach"));

            var process = new Process();
            process.StartInfo.FileName = "diskpart.exe";
            process.StartInfo.Arguments = string.Format("/s \"{0}\"", script);
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();

            var result = false;
            while (true)
            {
                process.WaitForExit(7000);
                if (process.HasExited)
                {
                    if (process.ExitCode == 0)
                        result = true;
                    else
                        MessageBox.Show(string.Format("Exit code = {0}", (uint)process.ExitCode), "MountVHD Error");
                    break;
                }

                var answer = MessageBox.Show("Mounting the VHD takes long! Wait another 7 seconds?", "MountVHD", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (answer == MessageBoxResult.No)
                    break;
            }

            File.Delete(script);
            return result;
        }

        void Remember(string fileName)
        {
            var files = Properties.Settings.Default.MountedFiles;
            Properties.Settings.Default.MountedFiles = files.Contains(fileName) ? files.Replace(fileName, "") : files + fileName;
            Properties.Settings.Default.Save();
        }
    }
}
