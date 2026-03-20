using System;
using System.Drawing;
using System.IO;
using Forms = System.Windows.Forms;

namespace mbottrilby.Services
{
    internal sealed class TrayController : IDisposable
    {
        private readonly OverlayDiagnostics _diagnostics;
        private readonly Action _openSettings;
        private readonly Action _exitApp;
        private readonly string _appBaseDirectory;
        private Forms.NotifyIcon? _trayIcon;
        private Icon? _customTrayIcon;

        public TrayController(
            OverlayDiagnostics diagnostics,
            Action openSettings,
            Action exitApp,
            string appBaseDirectory
        )
        {
            _diagnostics = diagnostics;
            _openSettings = openSettings;
            _exitApp = exitApp;
            _appBaseDirectory = appBaseDirectory;
        }

        public void Initialize()
        {
            if (_trayIcon is not null)
            {
                return;
            }

            var trayIconImage = LoadTrayIcon();
            _trayIcon = new Forms.NotifyIcon
            {
                Icon = trayIconImage,
                Text = "mbot-trilby",
                Visible = true
            };

            var trayMenu = new Forms.ContextMenuStrip();
            trayMenu.Items.Add("Settings", null, (_, _) => _openSettings());
            trayMenu.Items.Add(new Forms.ToolStripSeparator());
            trayMenu.Items.Add("Exit", null, (_, _) => _exitApp());

            _trayIcon.ContextMenuStrip = trayMenu;
            _trayIcon.DoubleClick += (_, _) => _openSettings();
            if (trayIconImage == SystemIcons.Application)
            {
                _diagnostics.Info("tray", "Tray icon initialized with fallback icon (mbot.ico not available).");
            }
            else
            {
                _diagnostics.Info("tray", "Tray icon initialized from mbot.ico.");
            }
        }

        public void Dispose()
        {
            if (_trayIcon is not null)
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
                _trayIcon = null;
            }

            if (_customTrayIcon is not null)
            {
                _customTrayIcon.Dispose();
                _customTrayIcon = null;
            }
        }

        private Icon LoadTrayIcon()
        {
            var iconPath = Path.Combine(_appBaseDirectory, "mbot.ico");
            if (!File.Exists(iconPath))
            {
                return SystemIcons.Application;
            }

            try
            {
                _customTrayIcon = new Icon(iconPath);
                return _customTrayIcon;
            }
            catch
            {
                return SystemIcons.Application;
            }
        }
    }
}
