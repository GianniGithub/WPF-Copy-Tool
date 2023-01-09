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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Main;
        TextBoxOutputter outputter;
        public static bool Kopiert = false;
        public event EventHandler<EndArgs> OnDone;

        public MainWindow()
        {
            Main = this;
            InitializeComponent();


            outputter = new TextBoxOutputter(TestBox);
            Console.SetOut(outputter);
            Console.WriteLine("Started");
            Init.InitialConsol();
            //TreeSelcetKopzurueck win2 = new TreeSelcetKopzurueck();
            //win2.Show();
        }
        public MainWindow(bool auto)
        {
            Main = this;
            InitializeComponent();
            outputter = new TextBoxOutputter(TestBox);
            Console.SetOut(outputter);
            Console.WriteLine("Started");
            Init.InitialConsol();
            if(auto)
            {
                GetSize.Canceling();
                Kopiere.StarteKopieren();
                ButKopiere.Content = "Abbrechen";
                Kopiert = true;
            }
        }

        private void ButKopiere_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Main.radioButtonzuruck.Visibility = Visibility.Visible;
            var butt = (Button)sender;
            if (!Kopiert)
            {
                if (Kopiere.prozessIsRunning)
                {
                    Console.WriteLine("Fast Copy Fenster ist noch Offen");
                    butt.Content = "Wartet...";
                    return;
                }
                GetSize.Canceling();
                Kopiere.StarteKopieren();
                butt.Content = "Abbrechen";
                Kopiert = true; 
           
            }
            else
            {
                GetSize.Canceling();
                //Kopiere.runningFastCopyProzess.CloseMainWindow();
                if (Kopiere.runningFastCopyProzess != null && !Kopiere.runningFastCopyProzess.HasExited)
                    Kopiere.runningFastCopyProzess.CloseMainWindow();
                butt.Content = "Kopiere";
                Kopie.Kopien.Clear();
                OnDone(this, new EndArgs(false));
                Kopiert = false;
                
            }
            
        }
        public void ResetCopyProzess(bool KopierenFehlgeschlagen)
        {
            try
            {
                GetSize gz;
                ButKopiere.Content = "Kopiere";
                OnDone(this, new EndArgs(KopierenFehlgeschlagen));
                if (GetSize.BereitsBerechnet(ComboBoxSettings.GetPfadfromSelection(), out gz))
                    labelSpeicherPlatzUsr.Content = ByteSize.FromBytes(gz.result.GetValueOrDefault()).ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public void SetKopiertToFalse()
        {
            Dispatcher.Invoke(() => Kopiert = false);
        }
        
        internal void ColorRichBoxRed()
        {
            //ConsolTextBox.Background = Brushes.Red;
        }

        private void ConsolTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        
        

    }
}
