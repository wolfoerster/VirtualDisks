//***************************************************************************************
// Copyright © 2017 Wolfgang Foerster (wolfoerster@gmx.de). All Rights Reserved.
//
//***************************************************************************************
using System;
using System.IO;

namespace VirtualDisks
{
	public class Record
	{
		public Record(string fileName, string path, bool isMounted)
		{
			FileName = fileName;
			Path = path;
			mounted = isMounted;

			string dir = "WindowsImageBackup\\";
			int i = fileName.IndexOf(dir);
			string name = fileName.Substring(i + dir.Length);

			dir = "\\Backup ";
			i = name.IndexOf(dir);
			ShortName = name.Substring(0, i);

			name = name.Substring(i + dir.Length);
			i = name.IndexOf(' ');
			Date = name.Substring(0, i);

			FileInfo info = new FileInfo(fileName);
			double size = info.Length / 1024.0 / 1024.0 / 1024.0;
			Size = size.ToString("F3");
		}

		public string FileName { get; set; }
		public string ShortName { get; set; }
		public string Path { get; set; }
		public string Date { get; set; }
		public string Size { get; set; }
		public static Func<string, bool, bool> OnMountedChanged;

		public bool Mounted
		{
			get { return mounted; }
			set
			{
				if (mounted != value)
				{
					mounted = value;
					if (OnMountedChanged != null)
						OnMountedChanged(FileName, mounted);
				}
			}
		}
		private bool mounted;
	}
}
