using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.IO;

namespace WpfCopyTool
{
    internal class ComboBoxSettings
    {
        static private ComboBox comBox;
        static List<string> UserDirs = new List<string>();
        static List<string> UserNamesHin = new List<string>();
        static List<string> UserNamesZuruck = new List<string>();
        public static string aktuellerUser { get; private set; }

        public static bool keineUserAufHDD { get; private set; }

        //UserProfilPfad
        //    dlDirZiel
        //    usrDirZiel
        //    MainIDdir

        public ComboBoxSettings(ComboBox comboBoxKopierVorlagen)
        {

        }

        private static void setZurueckSettings()
        {
            UserNamesZuruck.Clear();
            UserDirs.Clear();
            var DDirs  = Directory.GetDirectories(Init.exePfad, "*.*", System.IO.SearchOption.TopDirectoryOnly);
            foreach (var sub in DDirs)
            {
                try
                {
                    if (Directory.Exists(sub + "\\user") || Directory.Exists(sub + "\\D_laufwerk") || Directory.GetFiles(sub, aktuellerUser + "_*").Count() != 0 )
                    {
                        var user = sub.Remove(0, sub.LastIndexOf('\\') + 1);
                        UserNamesZuruck.Add(user.ToLower());
                        UserDirs.Add(sub);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            keineUserAufHDD = UserNamesZuruck.Count == 0 ? true : false;
            replaceCurrentUserToFirstIndex(UserNamesZuruck);
            SelectComboxItem(comBox, UserNamesZuruck, aktuellerUser);
        }
        static void replaceCurrentUserToFirstIndex(List<string> userList)
        {
            int index = userList.IndexOf(Environment.UserName.ToLower());
            if (index > 0)
            {
                var temp = userList[0];
                userList[0] = userList[index];
                userList[index] = temp;
            }

        }
        private static void SelectComboxItem(ComboBox comBox, List<string> UserNames, string aktuellerUser)
        {
            comBox.ItemsSource = UserNamesZuruck;
            for (int i = 0; i < UserNames.Count; i++)
            {
                if (string.Equals(UserNames[i], aktuellerUser, StringComparison.OrdinalIgnoreCase))
                    comBox.SelectedItem = UserNames[i];
            }
        }

        internal static void SetCurrentUser(string userName)
        {
            aktuellerUser = userName;
        }

        private static void ComBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string Item = (string)comBox.SelectedItem; 
            if (Item == null)
            {
                Item = Init.KopierRichtung == hinOderZurueck.hin ? UserNamesHin[0] : UserNamesZuruck[0];
                comBox.SelectedItem = Item;
            }
            changeUserTarget(aktuellerUser, Item);
            aktuellerUser = Item;
            if (Init.KopierRichtung == hinOderZurueck.hin)
                Init.getSpeicherPlatz(MainWindow.Main.labelSpeicherPlatzUsr, Init.UserProfilPfad);
            else
                Kopiere.ErstelleDirTree();
        }

        private static void changeUserTarget(string aktuellerUser, string neuerUser)
        {
            if (aktuellerUser == neuerUser)
                return;
            Init.UserProfilPfad = Init.UserProfilPfad.Replace(aktuellerUser, neuerUser);
            Init.dlDirZiel = Init.dlDirZiel.Replace(aktuellerUser, neuerUser);
            Init.usrDirZiel = Init.usrDirZiel.Replace(aktuellerUser, neuerUser);
            Init.MainIDdir = Init.MainIDdir.Replace(aktuellerUser, neuerUser);
        }

        private static void setHinsettings()
        {
            UserNamesHin.Clear();
            var user = Init.getUserDiretoryPath();
            var UserDir = user.Remove(user.LastIndexOf('\\'));
            UserDirs = Directory.GetDirectories(UserDir, "*.*", System.IO.SearchOption.TopDirectoryOnly).ToList();
            for (int i = UserDirs.Count-1; i >= 0; i--)
            {
                try
                {
                    DirectoryInfo FolderInfo = new DirectoryInfo(UserDirs[i]);
                    if (FolderInfo.Attributes.HasFlag(FileAttributes.ReparsePoint) || FolderInfo.Attributes.HasFlag(FileAttributes.Hidden))
                    {
                        UserDirs.RemoveAt(i);
                        continue;
                    }
                    var User = UserDirs[i].Remove(0, user.LastIndexOf('\\') + 1);
                    UserNamesHin.Add(User.ToLower());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            replaceCurrentUserToFirstIndex(UserNamesHin);
            SelectComboxItem(comBox, UserNamesHin, aktuellerUser);
        }



        public ComboBoxSettings()
        {

        }
        internal static void Update()
        {
            //if (UserNamesHin.Count == 0 || UserNamesZuruck.Count == 0)
            //{
            //    comBox = MainWindow.Main.comboBoxKopierVorlagen;
            //    if (Init.KopierRichtung == hinOderZurueck.hin)
            //        setHinsettings(); // nur zum TEsten
            //    else
            //        setZurueckSettings();
            //    return;
            //}
            comBox.ItemsSource = null;
            if (Init.KopierRichtung == hinOderZurueck.hin)
            {
                comBox.ItemsSource = UserNamesHin;
                Init.getSpeicherPlatz(MainWindow.Main.labelSpeicherPlatzUsr, Init.UserProfilPfad);
            }
            else
            {
                comBox.ItemsSource = UserNamesZuruck;

            }
        }
        internal static void Initialisiere()
        {
            comBox = MainWindow.Main.comboBoxKopierVorlagen;
            setHinsettings();
            setZurueckSettings();
            comBox.SelectionChanged -= ComBox_SelectionChanged;
            comBox.SelectionChanged += ComBox_SelectionChanged;
        }
        internal static string GetPfadfromSelection()
        {
            foreach (var dir in UserDirs)
            {
                if (dir.Contains(aktuellerUser))
                    return dir;
            }
            return null;
        }
    }
}