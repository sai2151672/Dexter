using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using WpfApplication = System.Windows.Application;

namespace Dexter
{
    public partial class App : WpfApplication
    {
        private NotifyIcon? _notifyIcon;
        private MainWindow? _mainWindow;
        private bool _isExitRequested;
        private Icon? _trayIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _mainWindow = new MainWindow();
            _mainWindow.Hide();
            _mainWindow.Closing += MainWindow_Closing;

            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "dexter.ico");

            _trayIcon = File.Exists(iconPath)
                ? new Icon(iconPath)
                : SystemIcons.Application;

            _notifyIcon = new NotifyIcon
            {
                Icon = _trayIcon,
                Visible = true,
                Text = "Dexter"
            };

            _notifyIcon.DoubleClick += NotifyIcon_DoubleClick;

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Open Dexter", null, (_, _) => ShowMainWindow());
            contextMenu.Items.Add("Exit", null, (_, _) => ExitApplication());

            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void NotifyIcon_DoubleClick(object? sender, EventArgs e)
        {
            ShowMainWindow();
        }

        private void ShowMainWindow()
        {
            if (_mainWindow == null)
                return;

            _mainWindow.Show();
            _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Activate();
            _mainWindow.Topmost = true;
            _mainWindow.Topmost = false;
            _mainWindow.Focus();
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isExitRequested)
            {
                e.Cancel = true;
                _mainWindow?.Hide();
            }
        }

        private void ExitApplication()
        {
            _isExitRequested = true;

            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }

            _trayIcon?.Dispose();
            _trayIcon = null;

            _mainWindow?.Close();
            Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }

            _trayIcon?.Dispose();
            _trayIcon = null;

            base.OnExit(e);
        }
    }
}