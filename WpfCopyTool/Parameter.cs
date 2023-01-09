using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfCopyTool
{
    internal class Parameter
    {
        public static string LogfilePfad;

        public string UserHin;
        public string Userzurueck;

        public string D_hin;
        public string D_zuruck;

        public string TDKhin;
        public string TDKzuruck;

        public Parameter()
        {
            MakeInstatnzLogFile89();
            //hin
            UserHin = String.Format("/cmd=diff /verify /logfile=\"{0}\" /force_close /open_window /acl=false /stream /bufsize=256 \"{1}\"\\ /to=\"{2}\"", LogfilePfad, Init.UserProfilPfad, Init.usrDirZiel);
            D_hin = String.Format("/cmd=diff /verify /logfile=\"{0}\" /force_close /open_window /acl=false /stream /bufsize=256 \"{1}\" /to=\"{2}\"", LogfilePfad, Init.DLaufwerk, Init.dlDirZiel);
            TDKhin = String.Format("/cmd=diff /verify /logfile=\"{0}\" /force_close /open_window /acl=false /stream /bufsize=256 c:\\TDK /to={1}\\TDK", LogfilePfad, Init.MainIDdir);
            //zurück
            Userzurueck = String.Format("/cmd=diff /verify /logfile=\"{0}\" /force_close /open_window /acl=false /stream /bufsize=256 \"{1}\"\\ /to=\"{2}\"", LogfilePfad, Init.usrDirZiel, Init.UserProfilPfad);
            D_zuruck = String.Format("/cmd=diff /verify /logfile=\"{0}\" /force_close /open_window /acl=false /stream /bufsize=256 \"{1}\" /to=\"{2}\"", LogfilePfad, Init.dlDirZiel, Init.DLaufwerk);
            TDKzuruck = String.Format("/cmd=diff /verify /logfile=\"{0}\" /force_close /open_window /acl=false /stream /bufsize=256 \"{1}\"\\TDK /to=c:\\TDK", LogfilePfad, Init.UserProfilPfad);

        }


        /// <summary>
        /// Erstellt Parameter, Beispiel: "D:\\ed5830\\user\\"+FromPath = AppData   ;  "C:\\Users\\ed5830\\"+ToPath = AppData\\local
        /// </summary>
        public static string ParameterBuilderZurueck(string FromPath, string ToPath) //FroPath = TDK        ToPath = TDK
        {
            MakeLogFile89();
            return String.Format("/cmd=diff /verify /logfile=\"{0}\" /force_close /open_window /acl=false /stream /bufsize=256 \"{1}\" /to=\"{2}\"", LogfilePfad, Init.usrDirZiel + "\\" + FromPath, Init.UserProfilPfad + "\\" + ToPath);
        }
        /// <summary>
        /// Erstellt Parameter, Beispiel: "D:\\ed5830\\user\\"+Path = AppData\\local ;  "C:\\Users\\ed5830\\"+Path = AppData\\local
        /// </summary>
        public static string ParameterBuilderZurueck(string Path)
        {
            string Sorce = CheckPath(Path);
            MakeLogFile89();
            return String.Format("/cmd=diff /verify /logfile=\"{0}\" /force_close /open_window /acl=false /stream /bufsize=256 \"{1}\" /to=\"{2}\"", LogfilePfad, Init.usrDirZiel + Sorce, Init.UserProfilPfad + Sorce);
        }
        public static string ParameterBuilderZurueckMitExcludes(Kopie kop, string Excludes, string Includes)
        {
            //string Sorce = CheckPath(Path);
            MakeLogFile89();
            return String.Format("/cmd=diff {3} {4} /verify /logfile=\"{0}\" /force_close /open_window /acl=false /stream /bufsize=256 \"{1}\" /to=\"{2}\"", LogfilePfad, kop.KopieSorce + kop.partPath, kop.KopierZiel + kop.partPath, Excludes, Includes);
        }

        private static string CheckPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            else return path;
        }

        public static void MakeLogFile89()
        {
            if(LogfilePfad == null)
            {
                LogfilePfad = String.Format("{0}\\{1}_log.log", Init.MainIDdir, ComboBoxSettings.aktuellerUser);
            }
        }
        public static void MakeInstatnzLogFile89()
        {
             LogfilePfad = String.Format("{0}\\{1}_log.log", Init.MainIDdir, ComboBoxSettings.aktuellerUser);
        }



    }
}
