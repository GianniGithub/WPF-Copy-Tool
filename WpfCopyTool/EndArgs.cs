using System;

namespace WpfCopyTool
{
    public class EndArgs : EventArgs
    {
        public readonly string PathLogFile;
        public readonly bool KopieFail;
        public readonly string User;

        public EndArgs()
        {
        }

        public EndArgs(bool KopieFail)
        {
            this.KopieFail = KopieFail;
            PathLogFile = Parameter.LogfilePfad;
            User = ComboBoxSettings.aktuellerUser;
        }
    }
}