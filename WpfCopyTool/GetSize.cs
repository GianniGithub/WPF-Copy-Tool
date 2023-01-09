using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows;

namespace WpfCopyTool
{
    class GetSize : GetFiles
    {

        readonly System.Windows.Controls.Label label;
        public static Dictionary<string, GetSize> UsersDict = new Dictionary<string, GetSize>();
        public long? result = null;

        public static bool BereitsBerechnet(string FilePfad, out GetSize Result)
        {
            return (UsersDict.TryGetValue(FilePfad, out Result));
        }

        public GetSize(string FilePfad, System.Windows.Controls.Label label) : base(FilePfad)
        {

            this.label = label;
            this.label.Content = "Berechne...";
            UsersDict.Add(FilePfad, this);
        }
        

        public long? GetSizeAsync()
        {

            long FolderSize = 0;
         
            foreach (string filename in GetAllFilesFromFolder(FilePfad, true, true, false))
            {
                if (tok.IsCancellationRequested)
                {
                    return null;
                }
                try
                {
                    FileInfo info = new FileInfo(filename);
                    FolderSize += info.Length;
                }
                catch 
                {
                }
            }

            sorceToken.Dispose();
            result = FolderSize;
            return FolderSize;

        }

        // Kopiert aus https://stackoverflow.com/questions/1281620/checking-for-directory-and-file-write-permissions-in-net
        public static bool HasWritePermissionOnDir(string path)
        {
            var writeAllow = false;
            var writeDeny = false;

            try
            {
                var accessControlList = Directory.GetAccessControl(path);
                if (accessControlList == null)
                    return false;
                var accessRules = accessControlList.GetAccessRules(true, true,
                                            typeof(System.Security.Principal.SecurityIdentifier));
                if (accessRules == null)
                    return false;

                foreach (FileSystemAccessRule rule in accessRules)
                {
                    if ((FileSystemRights.Write & rule.FileSystemRights) != FileSystemRights.Write)
                        continue;

                    if (rule.AccessControlType == AccessControlType.Allow)
                        writeAllow = true;
                    else if (rule.AccessControlType == AccessControlType.Deny)
                        writeDeny = true;
                }
            }
            catch
            {
                return false;
            }

            return writeAllow && !writeDeny;
        }

        public new static void Canceling()
        {
            if (tokens.Count == 0) return;
            for (int i = 0; i < tokens.Count; i++)
            {
                try
                {
                    tokens[i].Cancel();
                    tokens[i].Dispose();
                }
                catch { }
                finally
                {
                    tokens.RemoveAt(i);
                }
            }
            MainWindow.Main.Dispatcher.Invoke(() => {
                var con = MainWindow.Main.labelSpeicherPlatzUsr.Content;
                if (con.ToString() == "Berechne..." || con.ToString() == "Berechnet noch...")
                    con = "";
            });
        }

    }

    class GetFiles
    {
        public readonly string FilePfad;

        public CancellationTokenSource sorceToken;
        public readonly CancellationToken tok;
        public static List<CancellationTokenSource> tokens = new List<CancellationTokenSource>();

        public GetFiles(string FilePfad)
        {
            this.FilePfad = FilePfad;

            sorceToken = new CancellationTokenSource();
            this.tok = sorceToken.Token;
            tokens.Add(sorceToken);
        }

        public IEnumerable<string> GetAllFilesFromFolder()
        {
            return GetAllFilesFromFolder(FilePfad, true, true, true);
        }
        public IEnumerable<string> GetAllFilesFromFolder(string root, bool searchSubfolders, bool ReparsePoint, bool accessCheck)
        {
            Queue<string> folders = new Queue<string>();
            folders.Enqueue(root);
            while (folders.Count != 0)
            {
                string currentFolder = folders.Dequeue();
                string[] filesInCurrent = new string[1];
                try
                {
                    if (tok.IsCancellationRequested)
                        break;
                    filesInCurrent = System.IO.Directory.GetFiles(currentFolder, "*.*", System.IO.SearchOption.TopDirectoryOnly);
                }
                catch { }

                foreach (var file in filesInCurrent)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        if (accessCheck)
                        {
                            if (fileInfo.Attributes.HasFlag(FileAttributes.System))
                                continue;
                        }

                        if (ReparsePoint && fileInfo.Attributes.HasFlag(FileAttributes.ReparsePoint))
                            continue;
                    }
                    catch
                    {
                        continue;
                    }
                    yield return file;
                }

                try
                {
                    if (searchSubfolders)
                    {
                        if (tok.IsCancellationRequested)
                            break;
                        string[] foldersInCurrent = System.IO.Directory.GetDirectories(currentFolder, "*.*", System.IO.SearchOption.TopDirectoryOnly);
                        foreach (string _current in foldersInCurrent)
                        {
                            DirectoryInfo FolderInfo = new DirectoryInfo(_current);
                            if (ReparsePoint && FolderInfo.Attributes.HasFlag(FileAttributes.ReparsePoint))
                                continue;
                            folders.Enqueue(_current);
                        }
                    }
                }
                catch { }
            }

        }

        public virtual void Canceling()
        {
            if (tokens.Count == 0) return;
            for (int i = 0; i < tokens.Count; i++)
            {
                try
                {
                    tokens[i].Cancel();
                    tokens[i].Dispose();
                }
                catch { }
                finally
                {
                    tokens.RemoveAt(i);
                }
            }
        }
    }
}
