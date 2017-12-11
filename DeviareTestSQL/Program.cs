using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Nektra.Deviare2;

namespace DeviareTestSQL
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        //[DllImport("DeviareCOM.dll", CharSet = CharSet.Unicode)]

        [MTAThread]
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                System.Console.WriteLine("Please enter a program name");
                return 1;
            }        
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(args[0]));
            return 0;
        }
    }
}
