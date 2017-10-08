﻿#region SVN Info
/***************************************************************
 * $Author: fragman $
 * $Revision: 102 $
 * $Date: 2009-09-28 15:18:29 +0000 (Mon, 28 Sep 2009) $
 * $LastChangedBy: fragman $
 * $LastChangedDate: 2009-09-28 15:18:29 +0000 (Mon, 28 Sep 2009) $
 * $URL: http://seriesrenamer.googlecode.com/svn/trunk/Renamer/Program.cs $
 * 
 * License: GPLv3
 * 
****************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using Renamer.Classes;

namespace Renamer
{
    
        

        
    static class Program
    {
        [DllImport("kernel32.dll")]
        static extern Boolean AttachConsole(int dwProcessId);
        const int ATTACH_PARENT_PROCESS = -1;
        /// <summary>
        /// Program entry point.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if(args.Length>0){
                if (File.Exists(args[0]))
                {
                    args[0] = Filepath.goUpwards(args[0], 1);
                }
                if (!Directory.Exists(args[0]))
                {
                    AttachConsole(ATTACH_PARENT_PROCESS);
                    Console.Out.WriteLine();
                    Console.Out.WriteLine("Series Renamer command line argument(s):");
                    Console.Out.WriteLine("\"Series Renamer.exe [Path]\": Opens the program in the folder [Path].");
                    Console.Out.WriteLine("\"Series Renamer.exe /help\": Displays this help message.");
                    return;
                }
            }
            
            Console.Out.WriteLine("");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1.Instance = new Form1(args);
            Application.Run(Form1.Instance);
        }
    }
}
