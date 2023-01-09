using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfCopyTool
{
    public class Kopie
    {
        public static int KopienCount { get; private set; }
        public static bool MitZip;
        public static Queue<Kopie> Kopien = new Queue<Kopie>();
        public string partPath { get; private set; }
        public string FullPath { get; set; }
        public string parameter { get; private set; }
        public string WasKopiertwird { get; private set; }
        public string KopierZiel { get; private set; }
        public string KopieSorce { get; private set; }
        readonly int UserLange;


        List<string> _exludes = new List<string>();
        List<string> _include = new List<string>();
        public static event EventHandler<KopieArgs> NextJob;

        public Kopie(string parameter, string WasKopiertWird)
        {
            setCounter();
            this.WasKopiertwird = WasKopiertWird;
            this.parameter = parameter;
            Kopien.Enqueue(this);
        }
        public Kopie(string FullPath)
        {
            UserLange = ComboBoxSettings.aktuellerUser.Length;
            setCounter();
            this.FullPath = FullPath;
            CheckKopierZiel(FullPath);
            partPath = fullToPartParth(FullPath);
            CheckIfDLaufwerkKeinenPartPathHat();
            Kopien.Enqueue(this);

        }

        private void CheckIfDLaufwerkKeinenPartPathHat()
        {
            if (partPath == "" && KopierZiel == "D:\\")
                KopieSorce = KopieSorce + "\\*";
        }

        void setCounter()
        {
            if (Kopien.Count == 0)
                KopienCount = 0;
            else
                KopienCount++;
        }
        void OnZip()
        {
            Zip.ErstelleZIP(FullPath);
        }
        public void starte()
        {
            if (Init.KopierRichtung == hinOderZurueck.zuruck) UpdateParameter();
            Kopiere.StartFastCopy(this);
            Console.WriteLine("Starte Kopieren Von: "+ WasKopiertwird);
            NextJob(this, new KopieArgs() { Kopien = Kopien.Count });
        }
        string fullToPartParth(string FullPath)
        {
            int returnIndex = -1;
            string returnFolder = "";  
            string ergebniss;

            string[] BackupFolders = new string[3] { "user", "D_laufwerk","TDK" };
            for (int i = 0; i < BackupFolders.Length; i++)
            { // Aber es gibt ein Problem wenn er den Ganzen User kopiert, oder ganz D laufwerk, er kopiert beides auch nur nach c user 
                returnIndex = FullPath.IndexOf(BackupFolders[i]); // findet zuerst was kopiert wird, also user oder Dlaufwerk
                if (returnIndex == -1)
                    continue;
                else
                {
                    returnFolder = BackupFolders[i]; // dann übergibt er in an den neuen Path
                    break;
                }
            }
            int UserLange = ComboBoxSettings.aktuellerUser.Length;
            if (returnIndex == -1 || returnIndex > (4 + UserLange)) Console.WriteLine("no such Folder: "+ FullPath); 
            ergebniss = FullPath.Remove(0, returnIndex + returnFolder.Length);
            if (ergebniss.Length == 0)
                WasKopiertwird = returnFolder;
            else
                WasKopiertwird = ergebniss;
            return ergebniss;
            //return ergebniss.Length == 0 ? returnFolder : ergebniss;
        }

        private void CheckKopierZiel(string fullPath)
        {
            var ret = fullPath.IndexOf("user");
            if (checkLaengeUndObEsVorkommt(ret))
            {
                KopieSorce = Init.usrDirZiel;
                KopierZiel = Init.UserProfilPfad;
                return;
            }
            ret = fullPath.IndexOf("D_laufwerk");
            if (checkLaengeUndObEsVorkommt(ret))
            {
                KopieSorce = Init.dlDirZiel;
                KopierZiel = Init.DLaufwerk;
                return;
            }
            ret = fullPath.IndexOf("TDK");
            if (checkLaengeUndObEsVorkommt(ret))
            {
                KopieSorce = Init.MainIDdir + "\\TDK";
                KopierZiel = "c:\\TDK";
                return;
            }
            Console.WriteLine("Keinen oder Falschen 'user',' D_Laufwerk' oder 'TDK' im User Namen Ordner");
        }

        private bool checkLaengeUndObEsVorkommt(int ret)
        {
            return ret != -1 && ret <= (4 + UserLange);
            
        }

        void UpdateParameter()
        {
            parameter = Parameter.ParameterBuilderZurueckMitExcludes(this, GetExcludeParameter(), GetIncludeParameter());
        }

        #region Ex- InCludeParameter
        public void AddExludeParameter(string toExclude)
        {
            _exludes.Add(toExclude + "\\;");
        }
        public void AddIncludeParameter(string toInclude)
        {
            _include.Add(toInclude + "\\;");
        }
        string GetExcludeParameter()
        {
            if (_exludes == null || _exludes.Count == 0)
                return "";
            StringBuilder sb = new StringBuilder();
            sb.Append("/exclude=\"");
            foreach (var s in _exludes)
            {
                sb.Append(s);
            }
            sb.Append("\"");
            return sb.ToString();
        }
        string GetIncludeParameter()
        {
            if (_include == null || _include.Count == 0)
                return "";
            StringBuilder sb = new StringBuilder();
            sb.Append("/include=\"");
            foreach (string s in _include)
            {
                var index = s.IndexOf("user");
                sb.Append(s.Remove(0,index+4));
            }
            sb.Append("\"");
            return sb.ToString();
        }
        #endregion
    }
    public class KopieArgs : EventArgs
    {
        public int Kopien;
    }
}
