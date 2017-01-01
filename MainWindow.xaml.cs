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

			string xml = null;
			try
			{
				xml = File.ReadAllText(name + "\\BackupSpecs.xml", Encoding.Unicode);
			}
			catch
			{
			}

			string[] files = Directory.GetFiles(name, "*.vhd");
			List<Record> records = new List<Record>();

			foreach (var file in files)
			{
				string path = FindPath(xml, file);
				if (path.Length == 3)
				{
					Record record = new Record(file, path, Properties.Settings.Default.MountedFiles.Contains(file));
					records.Add(record);
				}
			}

			records.Sort((x, y) => x.Path.CompareTo(y.Path));
			for (int i = 1; i < records.Count; i++)
				records[i].ShortName = records[i].Date = "";
			Records.AddRange(records);
		}

		private string FindPath(string xml, string fileName)
		{
			if (xml == null)
				return "";

			string name = Path.GetFileNameWithoutExtension(fileName);
			int i = xml.IndexOf(name);
			if (i < 0)
				return "";
			i += name.Length;

			string str = "FilePath=\"";
			i = xml.IndexOf(str, i);
			if (i < 0)
				return "";
			i += str.Length;

			int j = xml.IndexOf("\"", i);
			if (j < 0)
				return "";

			string path = xml.Substring(i, j - i);
			return path;
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

		DriveInfo GetDelta(DriveInfo[] infos1, DriveInfo[] infos2)
		{
			foreach (var info2 in infos2)
			{
				DriveInfo info1 = infos1.FirstOrDefault(x => x.Name == info2.Name);
				if (info1 == null)
					return info2;
			}
			return null;
		}
	}
}
