using System.Collections.Generic;

namespace WpfCopyTool.TreeView_control
{
    public enum KopierBackOptionen
    { Win7zu10,
      SelbesWindows }

    public  class BackupVorlagen : BackupFilesHierarchi
    {
        KopierBackOptionen KopieOption { get; set; }

        public BackupVorlagen(KopierBackOptionen KopieOption) : base ()
        {
            var TreeViewName = KopierBackOptionen.GetName(typeof(KopierBackOptionen), KopieOption);
            base.Filename = GetFolderName(TreeViewName);
            this.KopieOption = KopieOption;
        }
        protected override void LoadChildren()
        {
            //tue nichts
        }
        public BackupVorlagen(string Main, params BackupFilesHierarchi[] Childs ) : base(Main, Childs)
        {
            IsExpanded = true;
        }
        public BackupVorlagen()
        {
            
        }
    }
}