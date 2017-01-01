//***************************************************************************************
// Copyright © 2017 Wolfgang Foerster (wolfoerster@gmx.de). All Rights Reserved.
//
//***************************************************************************************
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using System.Windows.Input;
using System.Collections.Generic;

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
			SaveSettings();
		}

		void SaveSettings()
		{
			string files = "";
			foreach (var record in Records)
			{
				if (record.Mounted)
					files += record.FileName + "|";
			}
			Properties.Settings.Default.MountedFiles = files;
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

			DriveInfo[] infos = DriveInfo.GetDrives();
			foreach (var info in infos)
			{
				if (string.IsNullOrWhiteSpace(info.Name)
					|| info.Name.StartsWith("A:")
					|| info.Name.StartsWith("B:")
					|| !info.IsReady)
					continue;

				string name = info.Name + "WindowsImageBackup";
				if (!Directory.Exists(name))
					continue;

				string[] subdirs = Directory.GetDirectories(name);
				foreach (var subdir in subdirs)
					CheckDirectory(subdir);
			}
		}

		void CheckDirectory(string name)
		{
			string[] subdirs = Directory.GetDirectories(name);
			name = subdirs.FirstOrDefault(x => x.IndexOf("\\Backup ") > 0);
			if (name == null)
				return;

			string[] files = Directory.GetFiles(name, "*.vhd");
			foreach (var file in files)
			{
				Record record = new Record(file, Properties.Settings.Default.MountedFiles.Contains(file));
				Records.Add(record);
			}
		}

		bool MountVHD(string fileName, bool mode)
		{
			if (!File.Exists(fileName))
				return false;

			SaveSettings();
			string script = Path.GetTempPath() + "diskpart.txt";
			File.WriteAllText(script, string.Format("select vdisk file=\"{0}\"\r\n{1} vdisk", fileName, mode ? "attach" : "detach"));

			Process process = new Process();
			process.StartInfo.FileName = "diskpart.exe";
			process.StartInfo.Arguments = string.Format("/s \"{0}\"", script);
			process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			process.Start();

			bool result = false;
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
	}
}
