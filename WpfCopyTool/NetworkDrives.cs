using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Management;
using System.Windows;
using Utilities.Network;
using System.Windows.Controls;

namespace WpfCopyTool
{
    [Serializable]
    public class Drive
    {
        public string driveName; //     X:\
        public string driveTarget; //   \\svmp0300\software
        public Drive(string driveName, string driveTarget)
        {
            this.driveName = driveName;
            this.driveTarget = driveTarget;
        }
        public Drive() { }

    }
    [Serializable]
    class NetworkDrives : AsXML<Drive>
    {
        public static void Initialisiere()
        {
            var CheckBoxDrive = MainWindow.Main.checkBoxDrives;

            MainWindow.Main.radioButtonhin.Checked += (o, arg) =>
            {
                CheckBoxDrive.IsEnabled = true;
                CheckBoxDrive.IsChecked = true;
            };
            MainWindow.Main.radioButtonzuruck.Checked += (o, arg) =>
            { InitZurueck(CheckBoxDrive); };

            if (Init.KopierRichtung == hinOderZurueck.hin)
                CheckBoxDrive.IsChecked = true;
            else
                InitZurueck(CheckBoxDrive);

        }
        static void InitZurueck(CheckBox CheckBoxDrive)
        {
            var XMLFileNameType = typeof(Drive).Name + 's';
            string FileName = String.Format("{0}\\{1}_{2}.xml", Init.MainIDdir, Environment.UserName, XMLFileNameType);
            if (File.Exists(FileName))
                CheckBoxDrive.IsChecked = true;
            else
            {
                CheckBoxDrive.IsChecked = false;
                CheckBoxDrive.IsEnabled = false;
            }
        }
        public NetworkDrives()
        {
            if (MainWindow.Main.checkBoxDrives.IsChecked == true)
            {
                if (Init.KopierRichtung == hinOderZurueck.hin)
                {
                    FindUNCPaths();
                    CreateBatch();
                    ToXML();
                    Console.WriteLine("Netzwerk Drives wurden ausgelesen");
                }
                else
                {
                    LoadXML();
                    MapDrives();
                } 
            }
        }

        public async void MapDrives()
        {
            NetworkDrive nd = new NetworkDrive();
            //nd.MapNetworkDrive(@"\\server\path", "Z:", "myuser", "mypwd");
            await Task.Run(() => 
            {
                foreach (var drive in KopieItems)
                {
                    var result = nd.MapNetworkDrive(drive.driveTarget, drive.driveName, null, null, 0x00000001);
                    switch (result)
                    {
                        case 0:
                            Console.WriteLine("Successfully mapped: " + drive.driveTarget);
                            break;
                        case 85:
                            Console.WriteLine(String.Format("Drive {0} allready exist", drive.driveTarget));
                            break;
                        default:
                            Console.WriteLine("Mapped Net Drive: " + drive.driveTarget + " ErrorCode: " + result);
                            break;
                    }
                }
            });
        }
        private void CreateBatch()
        {
            string FileName = String.Format("{0}\\{1}_mapped_Drives.cmd", Init.MainIDdir, Environment.UserName);
            using (var tw = new StreamWriter(FileName, false))
            {
                tw.WriteLine("@echo off");
                foreach (var item in KopieItems)
                {
                    var MapCommand = String.Format("net use {0} {1} /persistent:yes", item.driveName, item.driveTarget);
                    tw.WriteLine(MapCommand);
                }
            }
        }

        private void FindUNCPaths()
        {
            DriveInfo[] dis = DriveInfo.GetDrives();
            foreach (DriveInfo di in dis)
            {
                if (di.DriveType == DriveType.Network)
                {
                    DirectoryInfo dir = di.RootDirectory;
                    // "x:"
                    KopieItems.Add(new Drive(dir.FullName.Remove(2,1), GetUNCPath(dir.FullName.Substring(0, 2))));
                }
            }
        }

        private string GetUNCPath(string path)
        {
            if (path.StartsWith(@"\\"))
            {
                return path;
            }

            ManagementObject mo = new ManagementObject();
            mo.Path = new ManagementPath(String.Format("Win32_LogicalDisk='{0}'", path));

            // DriveType 4 = Network Drive
            if (Convert.ToUInt32(mo["DriveType"]) == 4)
            {
                return Convert.ToString(mo["ProviderName"]);
            }
            else
            {
                return path;
            }
        }
    }
}
