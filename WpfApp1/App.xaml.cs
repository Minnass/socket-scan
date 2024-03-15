using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
  

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Process MyProc = Process.GetCurrentProcess();

            if ((Process.GetProcessesByName(MyProc.ProcessName).Length > 1))
            {
                MessageBox.Show("Application is already running"); 
                Environment.Exit(-2);
                return;
            }
        }
    }
    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
