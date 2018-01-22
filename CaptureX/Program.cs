using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CaptureX
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new MainForm());

            //Get   the   running   instance.     
            Process instance = RunningInstance();
            if (instance == null)
            {
                System.Windows.Forms.Application.EnableVisualStyles();   //这两行实现   XP   可视风格     
                System.Windows.Forms.Application.DoEvents();
                //There   isn't   another   instance,   show   our   form.     
                Application.Run(new MainForm());
            }
            else
            {
                //There   is   another   instance   of   this   process.     
                HandleRunningInstance(instance);
            }
            //Application.Run(new Form1());
        }

        public static Process RunningInstance()
        {
            Process current = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcessesByName(current.ProcessName);
            //Loop   through   the   running   processes   in   with   the   same   name     
            foreach (Process process in processes)
            {
                //Ignore   the   current   process     
                if (process.Id != current.Id)
                {
                    //Make   sure   that   the   process   is   running   from   the   exe   file.     
                    if (Assembly.GetExecutingAssembly().Location.Replace("/", "//") == current.MainModule.FileName)
                    {
                        //Return   the   other   process   instance.     
                        return process;
                    }
                }
            }
            //No   other   instance   was   found,   return   null.   
            return null;
        }
        public static void HandleRunningInstance(Process instance)
        {
            //Make   sure   the   window   is   not   minimized   or   maximized     
            Win32API.ShowWindowAsync(instance.MainWindowHandle, WS_SHOWNORMAL);
            //Set   the   real   intance   to   foreground   window  
            Win32API.SetForegroundWindow(instance.MainWindowHandle);
        }

        private const int WS_SHOWNORMAL = 1;
    }
}
