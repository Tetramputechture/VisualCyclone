using System.Windows;

namespace VisualCycloneGUI
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private void AppStart(object sender, StartupEventArgs e)
        {
            // create main window
            var width = (int) SystemParameters.PrimaryScreenWidth;
            var height = (int) SystemParameters.PrimaryScreenHeight;

            const double mainWindowScalar = 0.7;
            var wnd = new MainWindow
            {
                Width = width*mainWindowScalar,
                Height = height*mainWindowScalar
            };

            wnd.Show();
        }
    }
}
