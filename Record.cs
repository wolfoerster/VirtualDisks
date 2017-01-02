//***************************************************************************************
// Copyright © 2017 wolfoerster@gmx.de. All Rights Reserved.
//
//***************************************************************************************
using System;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows;

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

        public Record()
        {
            Children = new List<Record>();
        }

        public Record(string name)
            : this()
        {
            Name = name;
        }

        public Record(string name, string fileName, bool isMounted)
            : this(name)
        {
            FileName = fileName;
            mounted = isMounted;
            FileInfo info = new FileInfo(fileName);
            double size = info.Length / 1024.0 / 1024.0 / 1024.0;
            Size = size.ToString("F3") + " GB";
        }

        public string Name { get; set; }
        public string FileName { get; set; }
        public string Size { get; set; }
        public string Drive { get; set; }
        public List<Record> Children { get; set; }
        public static Func<string, bool, bool> OnMountedChanged;

        public Visibility Visibility
        {
            get { return FileName == null ? Visibility.Collapsed : Visibility.Visible; }
        }

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
                            Drive = GetDrive(oldInfos);
                            FirePropertyChanged("Drive");
                        }
                    }
                }
            }
        }
        private bool mounted;

        string GetDrive(DriveInfo[] oldInfos)
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
