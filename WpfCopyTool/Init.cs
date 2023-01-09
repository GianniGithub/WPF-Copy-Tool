using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ByteSizeLib;

namespace WpfCopyTool
{
    public enum hinOderZurueck { hin, zuruck }
    class Init
    {

        public static hinOderZurueck KopierRichtung { get; set; }
        public static string exePfad; //            ""D:\\!!FastCopy!!!""
        public static string UserProfilPfad; //     "C:\\Users\\ed5830"
        public static string DLaufwerk; //          "D:\\"
        public static string dlDirZiel; //          "D:\\ed5830\\D_laufwerk"
        public static string usrDirZiel; //         "D:\\ed5830\\user"
        public static string MainIDdir; //          "D:\\ed5830"

        public static void InitialConsol()
        {
            
            //ini Pfade
            ComboBoxSettings.SetCurrentUser(Environment.UserName.ToLower());
            exePfad = getExeDiretoryPath();
            UserProfilPfad = getUserDiretoryPath();
            DLaufwerk = "D:\\";
            Kopiere.FastCopyExePfad = getFastCopyExePfad();
            MainIDdir = getSicherungUserDir();
            Init.usrDirZiel = Init.MainIDdir + "\\user";
            Init.dlDirZiel = Init.MainIDdir + "\\D_laufwerk";

            //Set Label User
            var Label = String.Format("UserID: {0}  {1}  PC: {2}", Environment.UserName, Environment.UserDomainName, Environment.MachineName);
            MainWindow.Main.LabelID.Content = Label;
            Console.WriteLine(Label);

            //Prüft auf EPCADM USer
            if (Environment.UserName == "EPCADM" || Environment.UserName =="dus-root")
                Console.WriteLine("Kein Anwender angemeldet, sondern "+ Environment.UserName);
            
            ComboBoxSettings.Initialisiere();

            // Radio Butten Events
            MainWindow.Main.radioButtonhin.Checked += RadioButtonhin_Checked;
            MainWindow.Main.radioButtonzuruck.Checked += RadioButtonzuruck_Checked;
            RadioCheck();

        }

        private static void RadioButtonhin_Checked(object sender, RoutedEventArgs e)
        {
            resetMenu();
            KopierRichtung = hinOderZurueck.hin;
            Console.WriteLine("Kopiere von PC auf Ext. HDD - Wurde Ausgewählt!");
            MainWindow.Main.CopyBacktreeView.Visibility = Visibility.Hidden;
            MainWindow.Main.SizeInfoLabel.Visibility = Visibility.Visible;
            prueftOrdnerAufUserPC();
            ComboBoxSettings.Update();
            // DriveInfo über D und Ext Laufwerk
            var KopiDrive = new DriveInfo(exePfad);
            string info = "Freier Speicherplatz auf Ext Platte " + exePfad + " : " + ByteSize.FromBytes(KopiDrive.TotalFreeSpace).ToString();
            Console.WriteLine(info);
            MainWindow.Main.SizeInfoLabel.Text = info;
        }

        private static void RadioButtonzuruck_Checked(object sender, RoutedEventArgs e)
        {
            GetSize.Canceling();
            resetMenu();
            KopierRichtung = hinOderZurueck.zuruck;
            Console.WriteLine("Kopiere von Ext. HDD auf PC - Wurde Ausgewählt!");
            MainWindow.Main.SizeInfoLabel.Visibility = Visibility.Hidden;
            MainWindow.Main.CopyBacktreeView.Visibility = Visibility.Visible;
            prueftOrdnerAufUserPC();
            ComboBoxSettings.Update();
            // if (MainWindow.Main.CopyBacktreeView.ItemsSource == null || MainWindow.Main.CopyBacktreeView.ItemsSource. == n)
            Kopiere.ErstelleDirTree();
        }

        static void RadioCheck()
        {
            // Auto Check 
            if (!DoesPfadExist(Kopiere.FastCopyExePfad))
            {
                Console.WriteLine("No Fastcopy.exe in " + Kopiere.FastCopyExePfad);
            }
            //Prüft Ob Hin oder zurück Kopiert wird
            if (Directory.Exists(getSicherungUserDir()))
            {
                KopierRichtung = hinOderZurueck.zuruck;
                MainWindow.Main.radioButtonzuruck.IsChecked = true;
            }
            else
            {
                KopierRichtung = hinOderZurueck.hin;
                MainWindow.Main.radioButtonhin.IsChecked = true;
                if(ComboBoxSettings.keineUserAufHDD)
                    MainWindow.Main.radioButtonzuruck.Visibility = Visibility.Hidden;
            }

            //OnAbbruch.Initialisiere();
            Drucker.Initialisiere();
            NetworkDrives.Initialisiere();
            LogOutput.initialisiere();



            switch (KopierRichtung)
            {
                case hinOderZurueck.hin:
                    prueftOrdnerAufUserPC();
                    break;
                case hinOderZurueck.zuruck:
                    prueftOrdnerAufExtFestplatte();
                    break;
                default:
                    break;
            }
        }
        #region Prüfe Checkboxen auf Ordner und Berechne Speicherplatz
        /// <summary>
        /// Obsolet
        /// </summary>
        private static void prueftOrdnerAufExtFestplatte()
        {
            //Prüft CheckBoxen auf Existens auf ext. Festplatte
            //gibt es ein D Laufwerk?
            MainIDdir = getSicherungUserDir();
            if (CeckDir(MainIDdir, "D_laufwerk"))
            {
                MainWindow.Main.checkBoxD.IsChecked = true;
            }
            else
            {
                MainWindow.Main.checkBoxD.IsChecked = false;
                //MainWindow.Main.checkBoxD.IsEnabled = false;
            }
            //gibt es ein User Ordner?
            if (CeckDir(MainIDdir, "user"))
                MainWindow.Main.checkBoxUser.IsChecked = true;
            else
            {
                MainWindow.Main.checkBoxUser.IsChecked = false;
                //MainWindow.Main.checkBoxUser.IsEnabled = false;
            }

            //gibt es ein TDK Ordner?
            if (CeckDir(MainIDdir, "TDK"))
                MainWindow.Main.checkBoxTDK.IsChecked = true;
            else
            {
                MainWindow.Main.checkBoxTDK.IsChecked = false;
                //MainWindow.Main.checkBoxTDK.IsEnabled = false;
            }
        }

        static void CheckDLaufwerkKopierZiel()
        {
            if (!GibtEsEinDLaufwerk())
            {
                Init.DLaufwerk = UserProfilPfad + "\\D_laufwerk";
            }
        }
        private static void prueftOrdnerAufUserPC()
        {
            //Prüft CheckBoxen auf Existens auf User PC und Speicherplatz

            //gibt es ein D Laufwerk?
            if (hinOderZurueck.hin == KopierRichtung)
                GibtEsEinDLaufwerk();
            else
                CheckDLaufwerkKopierZiel();

            //gibt es ein User Ordner?
            GibtEsEinUserOrdner();

            //gibt es ein TDK Ordner?
            GibtEsEinTDKOrdner();
        }

        private static void GibtEsEinTDKOrdner()
        {
            if (Directory.Exists("C:\\TDK"))
                MainWindow.Main.checkBoxTDK.IsChecked = true;
            else MainWindow.Main.checkBoxTDK.IsEnabled = false;
        }

        private static void GibtEsEinUserOrdner()
        {
            if (Directory.Exists(UserProfilPfad))
            {
                MainWindow.Main.checkBoxUser.IsChecked = true;
                getSpeicherPlatz(MainWindow.Main.labelSpeicherPlatzUsr, UserProfilPfad);
            }
            else MainWindow.Main.checkBoxUser.IsChecked = false;
        }

        private static bool GibtEsEinDLaufwerk()
        {
            DLaufwerk = "D:\\";
            if (Directory.Exists(DLaufwerk))
            {
                //Prüft ob Festplatte HDD das D: Laufwerk ist
                if (exePfad != DLaufwerk)
                {
                    MainWindow.Main.checkBoxD.IsChecked = true;
                    var DDrive = new DriveInfo(DLaufwerk);
                    var size = ByteSize.FromBytes(DDrive.TotalSize - DDrive.AvailableFreeSpace).ToString();
                    MainWindow.Main.labelSpeicherPlatzDLaufwerk.Content = size;
                    return true;
                }
                else
                {
                    MainWindow.Main.checkBoxD.IsEnabled = false;
                    Console.WriteLine("D Laufwerk ist Ext. Festplatte!");
                    return false;
                    //getSpeicherPlatz(MainWindow.Main.labelSpeicherPlatzDLaufwerk, DLaufwerk);        //nurTemporär - bitte dann Löschen
                }

            }
            else
            {
                MainWindow.Main.checkBoxD.IsChecked = false;
                return false;
            }

        }

        public static async void getSpeicherPlatz(Label label, string FilePfad)
        {
            GetSize sizeClass;
            long? result;
            if (!GetSize.BereitsBerechnet(FilePfad, out sizeClass))
            {
                sizeClass = new GetSize(FilePfad, label);
                result = await Task.Run(() => sizeClass.GetSizeAsync());
            }
            else
                result = sizeClass.result;

            if (sizeClass.tok.IsCancellationRequested)
            {
                label.Content = "";
                GetSize.UsersDict.Remove(FilePfad);
                return;
            }

            if (sizeClass.FilePfad == DLaufwerk)
            {
                label.Content = ByteSize.FromBytes((long)result).ToString();
                return;
            }

            if (result!=0 && result != null && sizeClass.FilePfad == UserProfilPfad)
                label.Content = ByteSize.FromBytes((long)result).ToString();
            else if(result == null) label.Content = "Berechnet noch...";
            else label.Content = "< 1 MB"; //wenn nicht null und 0
        }
        static void resetMenu()
        {
            GetSize.Canceling();
            MainWindow.Main.checkBoxD.IsChecked = false;
            MainWindow.Main.checkBoxUser.IsChecked = false;
            MainWindow.Main.checkBoxTDK.IsChecked = false;

            MainWindow.Main.checkBoxD.IsEnabled = true;
            MainWindow.Main.checkBoxUser.IsEnabled = true;
            MainWindow.Main.checkBoxTDK.IsEnabled = true;

        }

        #endregion

        void ButtonStartKopieren()
        {

            switch (KopierRichtung)
            {
                case hinOderZurueck.hin:
                    Kopiere.KopiereHin();
                    break;
                case hinOderZurueck.zuruck:
                    Kopiere.KopiereZurueck();
                    break;
                default:
                    break;
            }

            Console.WriteLine(" --ENDE-- ");

        }
        
        public static string getSicherungUserDir()
        {
            var laufwerk = exePfad.Split('\\');
            var userDir = String.Format("{0}\\{1}", Init.exePfad, ComboBoxSettings.aktuellerUser);
            return userDir;
        }
        public static string getLaufwerkBuchstaben(string Pfad)
        {
            var laufwerk = Pfad.Split('\\');
            return laufwerk[0];
        }
        public static string MakeSicherungUserDIr()
        {
            var Pfad = getSicherungUserDir();
            Directory.CreateDirectory(Pfad);
            return Pfad;


        }
        public static string MakeDir(string startPfad, string DirName)
        {
            var DDir = String.Format("{0}\\{1}", startPfad, DirName);
            Directory.CreateDirectory(DDir);
            return DDir;
        }
        public static bool CeckDir(string startPFad, string Dirname)
        {
            var DDir = String.Format("{0}\\{1}", startPFad, Dirname);
            if (Directory.Exists(DDir))
                return true;
            else return false;
             
        }
        public static string getExeDiretoryPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
            //"c:\\users\\ed5830\\documents\\visual studio 2015\\Projects\\CopyUserProfile\\CopyUserProfile\\bin\\Debug\\"
        }
        public static string getUserDiretoryPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            //"C:\\Users\\ed5830"
        }
        public static string getFastCopyExePfad()
        {
            StringBuilder sb = new StringBuilder(exePfad);
            sb.Length -= sb.Length - 3;
            sb.Append("!!FastCopy!!!\\FastCopy.exe");
            return sb.ToString();
            //"d:\\!!FastCopy!!!\\FastCopy.exe"
        }
        public static bool DoesPfadExist(string Pfad)
        {
            try
            {
                return File.Exists(Pfad);
            }
            catch
            {
                return false;
            }
        }
    }
}
