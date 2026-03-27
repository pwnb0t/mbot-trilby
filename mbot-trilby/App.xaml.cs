using System;
using Velopack;

namespace mbottrilby
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        [STAThread]
        public static void Main()
        {
            VelopackApp.Build().Run();

            var app = new App();
            app.InitializeComponent();
            var mainWindow = new MainWindow();
            app.MainWindow = mainWindow;
            app.Run(mainWindow);
        }
    }
}
