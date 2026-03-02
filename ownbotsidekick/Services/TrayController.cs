using System;
using System.Drawing;
using System.IO;
using Forms = System.Windows.Forms;

namespace ownbotsidekick.Services
{
    internal sealed class TrayController : IDisposable
    {
        private readonly OverlayDiagnostics _diagnostics;
        private readonly Action _showOverlay;
        private readonly Action _hideOverlay;
        private readonly Action _toggleOverlay;
        private readonly Action _exitApp;
        private readonly string _appBaseDirectory;
        private Forms.NotifyIcon? _trayIcon;
        private Icon? _customTrayIcon;

        public TrayController(
            OverlayDiagnostics diagnostics,
            Action showOverlay,
            Action hideOverlay,
            Action toggleOverlay,
            Action exitApp,
            string appBaseDirectory
        )
        {
            _diagnostics = diagnostics;
            _showOverlay = showOverlay;
            _hideOverlay = hideOverlay;
            _toggleOverlay = toggleOverlay;
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
                Text = "ownbotsidekick",
                Visible = true
            };

            var trayMenu = new Forms.ContextMenuStrip();
            trayMenu.Items.Add("Show Overlay", null, (_, _) => _showOverlay());
            trayMenu.Items.Add("Hide Overlay", null, (_, _) => _hideOverlay());
            trayMenu.Items.Add(new Forms.ToolStripSeparator());
            trayMenu.Items.Add("Exit", null, (_, _) => _exitApp());

            _trayIcon.ContextMenuStrip = trayMenu;
            _trayIcon.DoubleClick += (_, _) => _toggleOverlay();
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
