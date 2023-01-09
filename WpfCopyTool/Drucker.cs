using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace WpfCopyTool
{

    public class Drucker : AsXML<DruckerInfo>
    {
        public static void Initialisiere()
        {
            var CheckBoxPrinter = MainWindow.Main.checkBoxPrinter;

            MainWindow.Main.radioButtonhin.Checked += (o,arg) =>
            {
                CheckBoxPrinter.IsEnabled = true;
                CheckBoxPrinter.IsChecked = true;
            };
            MainWindow.Main.radioButtonzuruck.Checked += (o, arg) =>
            {   InitZurueck(CheckBoxPrinter);   };

            if (Init.KopierRichtung == hinOderZurueck.hin)
                CheckBoxPrinter.IsChecked = true;
            else
                InitZurueck(CheckBoxPrinter);

        }
        static void InitZurueck(CheckBox CheckBoxPrinter)
        {
            var XMLFileNameType = typeof(DruckerInfo).Name + 's';
            string FileName = String.Format("{0}\\{1}_{2}.xml", Init.MainIDdir, Environment.UserName, XMLFileNameType);
            if (File.Exists(FileName))
                CheckBoxPrinter.IsChecked = true;
            else
            {
                CheckBoxPrinter.IsChecked = false;
                CheckBoxPrinter.IsEnabled = false;
            }
        }
        public Drucker()
        {
            if (MainWindow.Main.checkBoxPrinter.IsChecked == true)
            {
                if (Init.KopierRichtung == hinOderZurueck.hin)
                {
                    KopieItems = new List<DruckerInfo>();
                    GetDruckerToXMLParallel();
                }
                else
                {
                    LoadXML();
                    InstallDruckerParallel();
                } 
            }

        }
        async Task GetDruckerToXMLParallel()
        {
            await Task.Run(() => GetDruckerNamen());
            ToXML();
            Console.WriteLine("Drucker wurden ausgelesen");
        }
        async Task InstallDruckerParallel()
        {
            foreach (var item in KopieItems.Where(x => x.isNetworkPrinter))
                await Task.Run(() => InstallDrucker(item));
        }

        public void GetDruckerNamen()
        {

            var printerQuery = new ManagementObjectSearcher("SELECT * from Win32_Printer");
            foreach (var printer in printerQuery.Get())
            {
                string name = (string)printer.GetPropertyValue("Name");
                string status = (string)printer.GetPropertyValue("Status");
                bool isDefault = (bool)printer.GetPropertyValue("Default");
                bool isNetworkPrinter = (bool)printer.GetPropertyValue("Network");
                DruckerInfo info = new DruckerInfo(name, status, isDefault, isNetworkPrinter);
                KopieItems.Add(info);
            }
        }

        void InstallDrucker(DruckerInfo info)
        {
            using (ManagementClass win32Printer = new ManagementClass("Win32_Printer"))
            {
                using (ManagementBaseObject inputParam =
                   win32Printer.GetMethodParameters("AddPrinterConnection"))
                {
                    inputParam.SetPropertyValue("Name", @info.name);

                    using (ManagementBaseObject result = (ManagementBaseObject)win32Printer.InvokeMethod("AddPrinterConnection", inputParam, null))
                    {
                        uint errorCode = (uint)result.Properties["returnValue"].Value;

                        switch (errorCode)
                        {
                            case 0:
                                Console.WriteLine("Successfully connected printer: "+info.name);
                                break;
                            case 5:
                                Console.WriteLine("Access Denied. " + info.name);
                                break;
                            case 123:
                                Console.WriteLine("The filename, directory name, or volume label syntax is incorrect. " + info.name);
                                break;
                            case 1801:
                                Console.WriteLine("Invalid Printer Name. " + info.name);
                                break;
                            case 1930:
                                Console.WriteLine("Incompatible Printer Driver. " + info.name);
                                break;
                            case 3019:
                                Console.WriteLine("The specified printer driver was not found on the system and needs to be downloaded. " + info.name);
                                break;
                        }
                    }
                }
            }
        }
    }
}
