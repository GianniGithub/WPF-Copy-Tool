using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

namespace WpfCopyTool
{
    static class LogOutput
    {
        public static void initialisiere()
        {
            MainWindow.Main.OnDone += Main_OnDone;
        }

        static void Main_OnDone(object sender, EndArgs e)
        {
            try
            {
                var Main = (MainWindow)sender;
                Main.SizeInfoLabel.Text = string.Join("\n", GetLastLogOfFile(e.PathLogFile).Reverse());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler beim Ausslesen der LOG:\n"+ex.Message);
            }
        }
        static IEnumerable<string> GetLastLogOfFile(string file)
        {
            int SkipCounter = Kopie.KopienCount;
            foreach (var line in File.ReadLines(file).Reverse())
            {
                if (line.Length == 0) continue;
                if (line.StartsWith("Verify")) continue;
                if (line.StartsWith("File")) continue;
                if (line.StartsWith("Trans")) continue;
                if (line.StartsWith("Total")) continue;
                if (line.StartsWith("Skip")) continue;
                if (line.StartsWith("FastCopy")) continue;
                if (line.StartsWith(@"<Command>")) continue;
                if (line.StartsWith(@"<DestDir>")) continue;
                if (line.StartsWith(@"--")) continue;

                if (line.StartsWith("=="))
                {
                    if (SkipCounter <= 0) yield break;
                    SkipCounter--;
                    yield return line.Remove(0, 15);
                    continue;
                }

                yield return line;
            }
        }

    }
}
