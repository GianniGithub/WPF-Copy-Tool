using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Controls;

namespace WpfCopyTool
{
    public class TextBoxOutputter : TextWriter
    {
        static TextBox textBox = null;
        static List<string> AllConsolOuts;

        public TextBoxOutputter(TextBox output)
        {
            textBox = output;
            AllConsolOuts = new List<string>();
            AllConsolOuts.Add("-------------=======oo( O )oo=======-------------");
        }

        public override void Write(char value)
        {
            base.Write(value);
            textBox.Dispatcher.BeginInvoke(new Action(() =>
            {
                textBox.AppendText(value.ToString());
                textBox.ScrollToEnd();
            }));
            //textBox.Focus();
            //textBox.CaretIndex = textBox.Text.Length;
            
        }
        //
        // Summary:
        //     Writes the specified string value, followed by the current line terminator, to
        //     the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        public override void WriteLine(string value)
        {
            base.WriteLine(value);
            AllConsolOuts.Add(DateTime.Now.ToString() + "   " + value);
        }

        public override Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }

        public static void SaveTextBoxOutputToLogFile()
        {
            textBox.Dispatcher.BeginInvoke(new Action(() =>
            {
                File.AppendAllLines(Parameter.LogfilePfad, AllConsolOuts);
            }));
        }

        static string[] GetTextFromTextBox(TextBox textBox)
        {
            int length = textBox.LineCount;
            var erg = new string[length];
            for (int i = 0; i < length; i++)
            {
                erg[i] = textBox.GetLineText(i);
            }
            return erg;

        }
    }
}