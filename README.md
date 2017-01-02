# VirtualDisks
Tool to easily mount/unmount VHD files from Windows system image backups

<img src="https://s27.postimg.org/nfd3cen1v/Virtual_Disks.jpg" style="width:880px;">

## Introduction

Windows 7 comes with a system image backup utility that can be launched by the Control Panel (Backup and Restore/Create a system image) or via command line (sdclt.exe /BLBBACKUPWIZARD). This tool, which is also available in Windows 8 and 10, allows you to create backups of your complete hard disks. These backups can later be used to restore a crashed system.

Each drive is stored in a file with extension ".vhd" (Virtual Hard Disk), and the good news is that you can mount these files with the Disk Management tool and access all files individually with the Windows Explorer. The bad news is that mounting and unmounting VHD files with the Windows tool is very cumbersome.

## Background

When creating a system image backup on an external hard disk, the VHD files are stored in a folder like this: 'WindowsImageBackup\ComputerName\Backup yyyy-MM-dd hhmmss'. This folder also contains a file called 'BackupSpecs.xml', which contains information about the saved drives, e.g. the original drive letter and the name of the VHD file for that drive.

For years I have been using the Windows Disk Management tool (Control Panel\Administrative Tools\Computer Management\Disk Management) to attach VHD files to the system (Disk Management\Action\Attach VHD) but:

* it takes a lot of mouse clicks until you can select a VHD file
* once you are there, you have no clue which original drive is stored in the file

The other day I learned that 'DiskPart.exe' (comes with Windows) can do the job as well. There is a nice article about how to attach/detach VHD files directly from the Windows Explorer [here](http://www.howtogeek.com/51174/mount-and-unmount-a-vhd-file-in-windows-explorer-via-a-right-click/). Although this helps to save a lot of mouse clicks, you still have to guess which drive is stored in a particular file. And once you have attached a file, you need to remember its path when it comes to detach.

So I started writing this small utility to help me with this. Upon startup, it looks for the 'WindowsImageBackup' folder at the root of each available hard drive. Every VHD file which is mapped to something like 'C:\' or 'D:\' is added to a list which is presented on the UI. You can see the name of the computer, the original drive letter and the date and size of the backup as well.

To mount a drive, just click on the checkbox next to it. If mounting was successful, a new drive letter will appear to the right. To unmount the drive, just click the checkbox again.