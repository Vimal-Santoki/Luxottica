using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Luxottica
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string[] args = Environment.GetCommandLineArgs();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //string argsdata = "";
            //foreach (string arg in args)
            //{
            //    argsdata += arg + "#";
            //}
            string str=args.Length<=1?"":args[1];
            Application.Run(new Form1(str));
        }
    }
}
