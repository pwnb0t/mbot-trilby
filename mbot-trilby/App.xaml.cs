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
            using System.Threading.Mutex singleInstanceMutex = new Mutex(initiallyOwned: true, SingleInstanceMutexName, out bool createdNew);
            if (!createdNew)
            {
                return;
            }

            VelopackApp.Build().Run();

            mbottrilby.App app = new App();
            app.InitializeComponent();
            mbottrilby.MainWindow mainWindow = new MainWindow();
            app.MainWindow = mainWindow;
            app.Run(mainWindow);
        }
    }
}
