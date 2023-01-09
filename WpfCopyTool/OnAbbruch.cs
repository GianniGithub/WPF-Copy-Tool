using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace WpfCopyTool
{
    static class OnAbbruch
    {
        static MainWindow main;
        public static void Initialisiere()
        {
            MainWindow.Main.OnDone += Main_OnDone;
        }

        private static void Main_OnDone(object sender, EndArgs e)
        {
            main = sender as MainWindow;
            if (e.KopieFail)
            {
                SetFarbeAufRotAsync();
            }
        }

        private static async void SetFarbeAufRotAsync()
        {
            main.TestBox.Background = new SolidColorBrush(Colors.Red);
            await Task.Run(() =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(10));
            });
            main.MouseEnter += Main_MouseEnter;


        }

        private static void Main_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            main.MouseEnter -= Main_MouseEnter;
            main.TestBox.Background = new SolidColorBrush(Colors.White);
        }

    }
}
