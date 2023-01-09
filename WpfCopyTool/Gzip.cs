using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Ionic.Zip;
using System.Diagnostics;

namespace WpfCopyTool
{
    public static class Zip
    {
        static Task task;
        static int entriesTotal;

        static Zip()
        {

        }

        public static async void ErstelleZIP(string Ordner)
        {
            task = Task.Run(() => ErstelleZipParallel(Ordner));
            await task;
        }
        public static void ErstelleZipParallel(string Ordner)
        {

            var files = new GetFiles(Ordner);
            using (ZipFile zip = new ZipFile())
            {
                zip.AddProgress += Zip_AddProgress;
                foreach (var file in files.GetAllFilesFromFolder())
                {
                    try
                    {
                        using (Stream stream = new FileStream(file, FileMode.Open))
                        {
                            // File/Stream manipulating code here
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        continue;
                    }
                    zip.Password = "Hallo123!";
                    zip.AddFile(file, GetFolderName(file));
                }

                // testet ==== zip.AddFile(@"C:\Users\ed5830\NTUSER.DAT");
                //Hier jetzt noch mit Ornder inkl Files aus Stream
                // add this map file into the "images" directory in the zip archive
                // add the report into a different directory in the archive
                zip.SaveProgress += Zip_SaveProgress;
                zip.UseZip64WhenSaving = Zip64Option.Always;
                entriesTotal = zip.Entries.Count;
                zip.ZipErrorAction = ZipErrorAction.Skip;
                zip.ZipError += Zip_ZipError;

                zip.Save(Init.MainIDdir + "\\test1.zip");
            }

        }

        private static void Zip_ZipError(object sender, ZipErrorEventArgs e)
        {
            MainWindow.Main.Dispatcher.Invoke(() => MainWindow.Main.SizeInfoLabel.Inlines.Add(e.Exception.Message));
        }

        private static void Zip_SaveProgress(object sender, SaveProgressEventArgs e)
        {
            if (e.EntriesTotal == 0) return;
            MainWindow.Main.Dispatcher.Invoke(() =>
            {
                var progress = (e.EntriesSaved / e.EntriesTotal) * 100;
                MainWindow.Main.ProgressBarZip.Value = progress;
            });

        }

        private static void Zip_AddProgress(object sender, AddProgressEventArgs e)
        {
            MainWindow.Main.Dispatcher.Invoke(() => MainWindow.Main.SizeInfoLabel.Text = e.CurrentEntry.FileName);
        }

        static string GetFolderName(string FullFileName)
        {
            var range = FullFileName.LastIndexOf('\\');
            return FullFileName.Remove(range + 1);
        }
        static string getFileNameofExaptionMassage(string ExaptionMassage)
        {
            var startindex = ExaptionMassage.IndexOf('\"')+1;
            var result = ExaptionMassage.Remove(0, startindex);
            startindex = result.IndexOf('\"');
            return result.Remove(startindex);
        }


    }
}
