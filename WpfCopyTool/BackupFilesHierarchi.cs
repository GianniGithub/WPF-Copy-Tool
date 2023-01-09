using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfCopyTool.TreeView_control
{
    public class RootTreeKopie : BackupFilesHierarchi
    {
        ObservableCollection<BackupFilesHierarchi> rootCollection;

        public RootTreeKopie(string Filename)
            : base( Filename,  null, true)
        {
            rootCollection = new ObservableCollection<BackupFilesHierarchi>();
            rootCollection.Add(this);
        }
        public void GetKopieNotes()
        {
            List<BackupFilesHierarchi> TempUnCheckedNodes = new List<BackupFilesHierarchi>();
            foreach (var node in Collect(rootCollection).Where(x=>x.IsChecked == true))
            {
                if (node.FullPathName == null)
                    continue;
                new Kopie(node.FullPathName);
                node.IsChecked = false;
                TempUnCheckedNodes.Add(node);
            }
            TempUnCheckedNodes.ForEach(x => x.IsChecked = true);
        }
    }
    /// <summary>
    /// Class Types für logic for Treeview
    /// </summary>
    /// <remark>http://openbook.rheinwerk-verlag.de/visual_csharp_2012/1997_24_001.html</remark>
    public class BackupFilesHierarchi : INotifyPropertyChanged
    {
        /// <summary>
        /// Binding Object: Um den Eigenschaften Werte zuzuweisen, müssen diese als Property geprägt sein, d. h., sie müssen einen set- und get-Accessor haben.
        /// </summary>
        public string Filename { get; set; }
        public string FullPathName { get; set; }

        Dictionary<string, BackupFilesHierarchi> DictSubDirs = new Dictionary<string, BackupFilesHierarchi>();
        static readonly BackupFilesHierarchi DummyChild = new BackupFilesHierarchi();

        readonly ObservableCollection<BackupFilesHierarchi> _children;
        readonly BackupFilesHierarchi _parent;

        bool _isExpanded;
        bool _isSelected;
        bool? _isChecked = false;


        public bool? IsChecked { get { return _isChecked; }
            set
            {
                if (value == _isChecked) return;
                _isChecked = value;

                this.OnPropertyChanged("IsChecked");

                SetChildsLikeParent(value);

                UpdateAlleAeste(Parent);
            }
        }
        void SetChildsLikeParent(bool? value)
        {
            if (value != null && _children != null)
            {
                foreach (var item in _children)
                {
                    item.IsChecked = value;
                }
            }
        }

        static void UpdateAlleAeste(BackupFilesHierarchi Parent)
        {
            var aktuellParent = Parent;
            while (aktuellParent != null)
            {
                bool? ChildsinAst;
                if (aktuellParent._children.Count != 0)
                    ChildsinAst = CheckAllChildsCheckboxes(aktuellParent);
                else return;
                if (ChildsinAst == true || ChildsinAst == false)
                {
                    aktuellParent._isChecked = ChildsinAst;
                }
                else
                {
                    aktuellParent._isChecked = null;
                } 

                aktuellParent.OnPropertyChanged("IsChecked");
                aktuellParent = aktuellParent.Parent;
            }
        }
        public static RootTreeKopie GetRoot(BackupFilesHierarchi Ast)
        {
            var aktuellParent = Ast;
            while (aktuellParent != null)
            {
                aktuellParent = aktuellParent.Parent;
            }
            return (RootTreeKopie)aktuellParent;
        }

        protected IEnumerable<BackupFilesHierarchi> Collect(ObservableCollection<BackupFilesHierarchi> Children)
        {
            if (Children != null)
                foreach (BackupFilesHierarchi node in Children)
                {
                    yield return node;
                    if (node != null)
                        foreach (var child in Collect(node.Children))
                        {
                            if(child != null)
                                yield return child;
                         }
                }
        }
        /// <summary>
        /// Sind Alle Childd Äste auf True, oder auf False, dann wird Parent Ast angepasst, sonst null
        /// </summary>
        static bool? CheckAllChildsCheckboxes(BackupFilesHierarchi Parent) // SetParentsCheckBox()value
        {
            if (Parent._children.Count != 0 && Parent._children.All(x => x._isChecked == true))
                return true;
            else if (Parent._children.Count != 0 && Parent._children.All(x => x._isChecked == false))
                return false;
            else return null;
        }


        public BackupFilesHierarchi(string Filename, BackupFilesHierarchi parent, bool lazyLoadChildren)
        {
            this.FullPathName = Filename;
            this.Filename = GetFolderName(Filename);

            _parent = parent;

            _children = new ObservableCollection<BackupFilesHierarchi>();

            if (lazyLoadChildren)
                _children.Add(DummyChild);
        }
        public BackupFilesHierarchi(string Name, params BackupFilesHierarchi[] Childs )
        {
            Filename = Name;
            _children = new ObservableCollection<BackupFilesHierarchi>();
            foreach (var item in Childs)
            {
                _children.Add(item);
            }
        }
        /// <summary>
        /// Parameterloser Konstrukot
        /// </summary>
        /// <remarks>Damit die Klasse im XAML-Code instanziiert werden kann, muss sie einen parameterlosen Konstruktor haben.</remarks>
        public BackupFilesHierarchi()
        {
        }

        public ObservableCollection<BackupFilesHierarchi> Children
        {
            get { return _children; }
        }

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    this.OnPropertyChanged("IsExpanded");
                }

                // Expand all the way up to the root.
                if (_isExpanded && _parent != null)
                    _parent.IsExpanded = true;

                // Lazy load the child items, if necessary.
                if (this.HasDummyChild)
                {
                    this._children.Remove(DummyChild);
                    this.LoadChildren();
                }
            }

        }


        public BackupFilesHierarchi getFolder(List<string> Folders)
        {
            string folder=Folders[0];
            try
            {
                Folders.RemoveAt(0);
                if (_children.Count == 1 && this._children[0] == DummyChild)
                    IsExpanded = true;
                if (Folders.Count == 0) return DictSubDirs[folder];
                else return DictSubDirs[folder].getFolder(Folders);
            }
            catch (Exception)
            {
                if (folder != @"My Documents")
                    Console.WriteLine("Nicht Gefunden: "+ folder); 
                return null;
            }

        }
        /// <summary>
        /// Gibt TreeViwe object zurück aus Full Pfad Namen und Expandet den Pfad
        /// </summary>
        /// <param name="FullPath">z.b. @"D:\ed5830\user\AppData\Local\Apps\2.0\Data"</param>
        /// <returns></returns>
        /// <example>
        /// root.FindTreeViewVonFullPath(@"AppData\Local\Apps\2.0\Data").IsChecked = true;
        /// root.FindTreeViewVonFullPath(@"D:\ed5830\user\AppData\Local\Apps\2.0\Data").IsChecked = true;
        /// Gleiches Ergebniss!
        /// </example>
        public static BackupFilesHierarchi ExpandToPath(BackupFilesHierarchi root, string FullPath)
        {
            var splitted = FullPath.Split('\\').ToList();
            var range = splitted.FindIndex(x => x == "user");
            splitted.RemoveRange(0, range+1);
            return root.getFolder(splitted);
        }
        /// <summary>
        /// Invoked when the child items need to be loaded on demand.
        /// Subclasses can override this to populate the Children collection.
        /// </summary>
        protected virtual void LoadChildren()
        {
            string[] foldersInCurrent = System.IO.Directory.GetDirectories(FullPathName, "*.*", System.IO.SearchOption.TopDirectoryOnly);
            foreach (var item in foldersInCurrent)
            {
                try
                {
                    bool child = true;
                    var DictInfo = new DirectoryInfo(item);
                    if (DictInfo.Attributes.HasFlag(FileAttributes.ReparsePoint) || DictInfo.Attributes.HasFlag(FileAttributes.System))
                        continue;
                    if (DictInfo.GetDirectories().Length == 0)
                        child = false;
                    var TreeView = new BackupFilesHierarchi(item, this, child);
                    if (_isChecked == true) TreeView.IsChecked = true;
                    Children.Add(TreeView);
                    DictSubDirs.Add(GetFolderName(item), TreeView);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public bool HasDummyChild
        {
            get { return this._children.Count == 1 && this._children[0] == DummyChild; }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    this.OnPropertyChanged("IsSelected");
                }
            }
        }

        public BackupFilesHierarchi Parent
        {
            get { return _parent; }
        }
        protected static string GetFolderName(string Pfad)
        {
            var laufwerk = Pfad.Split('\\');
            return laufwerk.Last();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
