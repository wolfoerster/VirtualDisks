//***************************************************************************************
// Copyright © 2017 Wolfgang Foerster (wolfoerster@gmx.de). All Rights Reserved.
//
//***************************************************************************************
using System;
using System.IO;
using System.Linq;
using System.ComponentModel;

namespace VirtualDisks
{
    public class Record : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void FirePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null && propertyName != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

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
            Size = size.ToString("F3") + " GB";
        }

        public string FileName { get; set; }
        public string ShortName { get; set; }
        public string Path { get; set; }
        public string Date { get; set; }
        public string Size { get; set; }
        public string NewPath { get; set; }
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
                    {
                        DriveInfo[] oldInfos = DriveInfo.GetDrives();
                        if (OnMountedChanged(FileName, mounted))
                        {
                            NewPath = GetNewPath(oldInfos);
                            FirePropertyChanged("NewPath");
                        }
                    }
                }
            }
        }
        private bool mounted;

        string GetNewPath(DriveInfo[] oldInfos)
        {
            DriveInfo[] newInfos = DriveInfo.GetDrives();
            if (newInfos.Length <= oldInfos.Length)
                return "";

            foreach (var newInfo in newInfos)
            {
                DriveInfo oldInfo = oldInfos.FirstOrDefault(x => x.Name == newInfo.Name);
                if (oldInfo == null)
                    return newInfo.Name;
            }
            return "";
        }
    }
}
