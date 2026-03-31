using System;
using System.Threading;
using Velopack;

namespace mbottrilby
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private const string SingleInstanceMutexName = @"Local\mbot-trilby-single-instance";

        [STAThread]
        public static void Main()
        {
            using var singleInstanceMutex = new Mutex(initiallyOwned: true, SingleInstanceMutexName, out var createdNew);
            if (!createdNew)
            {
                return;
            }

            VelopackApp.Build().Run();

            var app = new App();
            app.InitializeComponent();
            var mainWindow = new MainWindow();
            app.MainWindow = mainWindow;
            app.Run(mainWindow);
        }
    }
}
