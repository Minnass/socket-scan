using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using WpfApp1.Models;
using System.Drawing.Printing;
using System.Net.Sockets;
using System.Net;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using MessageBox = System.Windows.MessageBox;
using Application = System.Windows.Application;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HttpServer _server;
        private int _selectedPort;
        bool _isWriteLog;
        private NotifyIcon _notifyIcon;
        public MainWindow()
        {
            InitializeComponent();
            InitializeNotifyIcon();
        }
        public void InitializeNotifyIcon()
        {
            _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = SystemIcons.Application;
            _notifyIcon.Visible = false;
            System.Windows.Forms.ContextMenu contextMenu = new System.Windows.Forms.ContextMenu();
            System.Windows.Forms.MenuItem exitMenuItem = new System.Windows.Forms.MenuItem();
            exitMenuItem.Text = "Exit";
            exitMenuItem.Click += ExitMenuItem_Click;
            _notifyIcon.MouseClick += NotifyIcon_MouseClick;
            contextMenu.MenuItems.Add(exitMenuItem);
            _notifyIcon.ContextMenu = contextMenu;
        }
        private void NotifyIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    return;
                }
            }
        }
        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _isWriteLog = false;
            _selectedPort = 14551;
            DataContext = this;
            PortComboBox.ItemsSource = GetAvailablePort();
        }

        List<int> GetAvailablePort()
        {
            return new List<int> { 14551, 26209, 26210, 26211, 27208, 27333 };
        }



        private bool IsPortInUse(int port)
        {
            try
            {
                var listener = new TcpListener(IPAddress.Loopback, port);
                listener.Start();
                listener.Stop();
                return false;
            }
            catch (SocketException)
            {
                return true;
            }
        }

        private void PortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.Controls.ComboBox comboBox = sender as System.Windows.Controls.ComboBox;
            if (comboBox != null && comboBox.SelectedIndex != -1)
            {
                _selectedPort = (int)comboBox.SelectedItem;
            }
        }

        private void StartServerButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsPortInUse(_selectedPort))
            {
                MessageBox.Show($@"Please choose another port. {_selectedPort} was used by another program");
            }
            
            if (_server != null)
            {
                _server.Stop();
            }
            _server = new HttpServer($@"http://localhost:{_selectedPort}/", _isWriteLog);
            _server.Start();
            this.Hide();
            _notifyIcon.Visible = true;
        }
        private void LogCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _isWriteLog = true;
        }
        private void LogCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _isWriteLog = false;
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_server != null)
            {
                _server.Stop();
            }
            base.OnClosed(e);
        }
    }
}

