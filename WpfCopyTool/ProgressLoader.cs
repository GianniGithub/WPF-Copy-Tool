using ByteSizeLib;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfCopyTool.TreeView_control;

namespace WpfCopyTool
{
    internal class ProgressLoader
    {
        public float KopieCount;
        ProgressBar probar;

        public static void reset()
        {
            MainWindow.Main.ProgressBarZip.Value = 0;
        }


        public ProgressLoader(int count)
        {
            this.KopieCount = count;
            probar = MainWindow.Main.ProgressBarZip;
            Kopie.NextJob += Kopie_NextJob;
        }

        private void Kopie_NextJob(object sender, KopieArgs e)
        {
            float result = (e.Kopien / KopieCount) * 100f;
            MainWindow.Main.Dispatcher.Invoke(() => probar.Value = result);
        }


    }
}