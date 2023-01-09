using ByteSizeLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfCopyTool.TreeView_control;

namespace WpfCopyTool
{
    class Kopiere
    {
        public static string WasAktuellKopiertwird { get; private set; }
        public static string FastCopyExePfad;
        public static Parameter parms;
        public static Process runningFastCopyProzess;
        public static bool prozessIsRunning = false;
        public static List<RootTreeKopie> Roots;
        static string aktuellDargestellterTreeFromUser;


        public static void StarteKopieren()
        {

            switch (Init.KopierRichtung)
            {
                case hinOderZurueck.hin:
                    KopiereHin();
                    break;
                case hinOderZurueck.zuruck:
                    KopiereZurueck();
                    break;
                default:
                    break;
            }
        }
        public static void KopiereZurueck()
        {
            MainWindow.Main.CopyBacktreeView.IsEnabled = false;
            parms = new Parameter();
            Init.usrDirZiel = Init.MainIDdir+ "\\user";
            Init.dlDirZiel = Init.MainIDdir + "\\D_laufwerk";
            // Installiert Drucker und Netzlaufwerke
            var printer = new Drucker();
            var drives = new NetworkDrives();
            // Ladet alle Kopier Check aus Root Tree
            foreach (var rootdir in Roots)
            {
                rootdir.GetKopieNotes();
            }
            if(!LastStartCheck())
                MainWindow.Main.CopyBacktreeView.IsEnabled = true;
        } 
        public static void KopiereHin()
        {
            //Erstelle Ordner
            Init.MainIDdir = Init.MakeSicherungUserDIr();
            if(MainWindow.Main.checkBoxUser.IsChecked==true)
                Init.usrDirZiel = Init.MakeDir(Init.MainIDdir, "user");
            if (MainWindow.Main.checkBoxD.IsChecked == true)
                Init.dlDirZiel = Init.MakeDir(Init.MainIDdir, "D_laufwerk");
            if (MainWindow.Main.checkBoxTDK.IsChecked == true)
                Init.MakeDir(Init.MainIDdir, "TDK");
            parms = new Parameter();
            Kopie.Kopien.Clear();
            //Copiere c:

            if (Directory.Exists(Init.UserProfilPfad) && MainWindow.Main.checkBoxUser.IsChecked == true)
            {
                new Kopie(parms.UserHin, ComboBoxSettings.aktuellerUser + " User Dir").FullPath = Init.UserProfilPfad;
            }

            //Koperie D_Laufwerk
            if (Directory.Exists(Init.DLaufwerk) && MainWindow.Main.checkBoxD.IsChecked == true)
            {
                new Kopie(parms.D_hin, "D Laufwerk").FullPath = Init.DLaufwerk;
            }

            //Koperie TDK
            if (Directory.Exists("C:\\TDK") && MainWindow.Main.checkBoxTDK.IsChecked==true)
            {
                new Kopie(parms.TDKhin, "TDK Laufwerk").FullPath = "C:\\TDK";
            }
            // Get Alle Drucker und Netzlaufwerke und Speichert Sie zu XML
            var printer = new Drucker();
            var drives = new NetworkDrives();

            //Starte Ersten Kopiervorgang
            LastStartCheck();

        }
        static bool LastStartCheck()
        {
            // Check ob Fast Copy läuft
            if (!Init.DoesPfadExist(Kopiere.FastCopyExePfad))
            {
                SetKopierbouttenback("No Fastcopy.exe in " + Kopiere.FastCopyExePfad);
                return false;
            }
            //Starte Ersten Kopiervorgang
            if (Kopie.Kopien == null || Kopie.Kopien.Count == 0)
            {
                SetKopierbouttenback("Es wurden keine Daten zum Kopieren ausgewählt!");
                return false;
            }
            else
            {
                ProgressLoader pro = new ProgressLoader(Kopie.Kopien.Count);
                Kopie.Kopien.Dequeue().starte();
            }
            return true;
        }
        static void SetKopierbouttenback(string Message)
        {
            var t = Task.Run(() =>
            {
                MainWindow.Main.Dispatcher.Invoke(() => {
                    MainWindow.Main.ResetCopyProzess(true);
                    MainWindow.Main.SetKopiertToFalse();
                });
            });
            TimeSpan ts = TimeSpan.FromMilliseconds(15);
            if (!t.Wait(ts))
                Console.WriteLine(Message);
        }
        public static void StartFastCopy(Kopie settings)
        {
            var tcs = new TaskCompletionSource<bool>();
            WasAktuellKopiertwird = settings.WasKopiertwird;
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = FastCopyExePfad;
            startInfo.Arguments = settings.parameter;
            runningFastCopyProzess = Process.Start(startInfo);
            prozessIsRunning = true;
            runningFastCopyProzess.EnableRaisingEvents = true;
            runningFastCopyProzess.Exited += nextJob;
        }

        private static void nextJob(object sender, EventArgs e)
        {
            prozessIsRunning = false;
            if (runningFastCopyProzess.ExitCode != 0)
            {
                if(ComboBoxSettings.aktuellerUser != Environment.UserName)
                {
                    Console.WriteLine("Kein Zugriff auf "+ ComboBoxSettings.aktuellerUser+" ! Run as Admin ");
                    Console.WriteLine("Abgeschlossen mit Fehlern: " + WasAktuellKopiertwird);
                }
                else
                    Console.WriteLine("Abgeschlossen mit Fehlern: " + WasAktuellKopiertwird);
            }
            else
                Console.WriteLine("Abgeschlossen: " + WasAktuellKopiertwird);
            if (Kopie.Kopien.Count != 0)
                Kopie.Kopien.Dequeue().starte();
            else
            {
                MainWindow.Main.Dispatcher.Invoke(() => {
                    ComboBoxSettings.Initialisiere();
                    MainWindow.Main.CopyBacktreeView.IsEnabled = true;
                    MainWindow.Main.ResetCopyProzess(false);
                });
                Console.WriteLine("Kopieren Beendet, All Done!");
                TextBoxOutputter.SaveTextBoxOutputToLogFile();
                MainWindow.Main.SetKopiertToFalse();

            }

        }
        static void AbbruchVomKopieren()
        {
            MainWindow.Kopiert = false;
            Kopie.Kopien.Clear();
            GetSize.Canceling();
            if (runningFastCopyProzess != null && !runningFastCopyProzess.HasExited)runningFastCopyProzess.CloseMainWindow();
            MainWindow.Main.Dispatcher.Invoke(() => {
                MainWindow.Main.CopyBacktreeView.IsEnabled = true;
                ProgressLoader.reset();
                GetSize gz;
                MainWindow.Main.ResetCopyProzess(true);
                if (GetSize.BereitsBerechnet(ComboBoxSettings.GetPfadfromSelection(), out gz))
                  MainWindow.Main.labelSpeicherPlatzUsr.Content = ByteSize.FromBytes(gz.result.GetValueOrDefault()).ToString();
            });
        }
        public static void ErstelleDirTree()
        {
            if (bereitserstellt())
                return;
            else
                aktuellDargestellterTreeFromUser = ComboBoxSettings.aktuellerUser;

            List<BackupFilesHierarchi> firsList = new List<BackupFilesHierarchi>();
            Roots = new List<RootTreeKopie>();
            string[] StammKopien = new string[3] { Init.usrDirZiel,Init.dlDirZiel, Init.getSicherungUserDir() + "\\TDK" };
            string[] StammNamen = new string[3] {"User", "D_Laufwerk", "TDK" };
            RootTreeKopie[] RootTrees = new RootTreeKopie[3];
            for (int i = 0; i < StammKopien.Length; i++)
            {
                if (Directory.Exists(StammKopien[i]))
                {
                    RootTrees[i] = new RootTreeKopie(StammKopien[i]);
                    firsList.Add(RootTrees[i]);
                    RootTrees[i].Filename = StammNamen[i];
                    RootTrees[i].IsChecked = true;
                    Roots.Add(RootTrees[i]);
                }
            }
            MainWindow.Main.CopyBacktreeView.ItemsSource = firsList;
            if (RootTrees[0] == null)
                return;
            RootTrees[0].IsChecked = false;
            selectTreeFolders(@"AppData\Local\IBM\Notes", RootTrees[0]);
            
            selectTreeFolders(@"Desktop", RootTrees[0]);
            selectTreeFolders(@"Downloads", RootTrees[0]);

            selectTreeFolders(@"Eigene Dokumente", RootTrees[0]);
            selectTreeFolders(@"Documents", RootTrees[0]);
            selectTreeFolders(@"My Documents", RootTrees[0]);

            //selectTreeFolders(@"Eigene Bilder", RootTrees[0]);
            selectTreeFolders(@"Pictures", RootTrees[0]);


            //selectTreeFolders(@"Eigene Musik", RootTrees[0]);
            selectTreeFolders(@"Music", RootTrees[0]);

            //selectTreeFolders(@"Eigene Videos", RootTrees[0]);
            selectTreeFolders(@"Videos", RootTrees[0]);

            selectTreeFolders(@"Favorites", RootTrees[0]);
            //selectTreeFolders(@"Favoriten", RootTrees[0]);

            selectTreeFolders(@"Links", RootTrees[0]);

            selectTreeFolders(@"Drives", RootTrees[0]);

            selectTreeFolders(@"Contacts", RootTrees[0]);
            selectTreeFolders(@"Searches", RootTrees[0]);



            MainWindow.Main.CopyBacktreeView.SelectedItemChanged += CopyBacktreeView_SelectedItemChanged;
        }

        private static bool bereitserstellt()
        {
            return aktuellDargestellterTreeFromUser != null && 
                aktuellDargestellterTreeFromUser == ComboBoxSettings.aktuellerUser;
        }

        private static BackupFilesHierarchi selectTreeFolders(string UserProfilPath, RootTreeKopie Tree)
        {
            try
            {
                string FullPath = Init.usrDirZiel + '\\' + UserProfilPath;
                if (Directory.Exists(FullPath))
                {
                    var UpLevelPath = BackupFilesHierarchi.ExpandToPath(Tree, FullPath);
                    UpLevelPath.IsChecked = true;
                    return UpLevelPath;
                    //var UpLevelPath = BackupFilesHierarchi.ExpandToPath(Tree, GetLowLevelPath(FullPath));
                    //UpLevelPath.IsExpanded = true; //D:\ed5830\user\AppData\Local\IBM\Notes
                }
                return null;
                //else
                //{
                //     if (Tree != null) Tree.IsExpanded = true;
             
                //}
            }
            catch (Exception)
            {
                return null;

            }

        }

        private static string GetLowLevelPath(string userProfilPath)
        {
            var range = userProfilPath.LastIndexOf('\\');
            if (range != -1)
                return userProfilPath.Remove(0, range+1);
            else
                return userProfilPath;
        }

        private static void CopyBacktreeView_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            // Hier soll am besten Speicher geladen werden
            var dsds = e.NewValue;
            var obj = sender;
            var sd = e.Source;
        }
    }
}
