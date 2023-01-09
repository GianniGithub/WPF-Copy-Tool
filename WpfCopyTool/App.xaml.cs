using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WpfCopyTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected void Application_Startup(object sender, StartupEventArgs e)
        {
            for (int i = 0; i != e.Args.Length; ++i)
            {
                if (e.Args[i] == "/auto")
                {
                    new MainWindow(true).Show();
                    return;
                }
                if (e.Args[i] == "/?" || e.Args[i] == "--help" || e.Args[i] != "/auto")
                {
                    new CommandHelper().Show();
                    return;
                }
            }
            new MainWindow().Show();
        }
       
    }
}
