# VirtualDisks
Tool to easily mount/unmount VHD files from Windows system image backups

<img src="https://s28.postimg.org/o4eqzq9b1/Virtual_Disks.jpg" style="width:880px;">

Windows 7 comes with a system image backup utility that can be launched by the Control Panel (Backup and Restore/Create a system image) or via command line (sdclt.exe /BLBBACKUPWIZARD). This tool, which is also available in Windows 8 and 10, allows you to create backups of your complete hard drives. These backups can later be used to restore a crashed system.

Each drive is stored in a file with extension ".vhd" (Virtual Hard Disk), and the good news is, that you can mount these files with the Disk Management tool and access all files individually with the Windows Explorer. The bad news is, that mounting and unmounting vhd files with the Windows tool is cumbersome. So I wrote this small WPF application to help me with this.

Upon startup, the application looks for a folder named 'WindowsImageBackup' at the root of each available hard drive. This folder contains subfolders with the name of each computer for which a system image has been created. Within the system specific folder there is a folder named 'Backup yyyy.MM.dd hhmmss' which contains the vhd files as well as other files.

The application shows all available vhd files with their original drive letter and you can easily mount/unmount them by just clicking the checkbox next to them.