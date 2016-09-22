using System.Windows;

namespace VisualCycloneGUI
{
    public partial class App
    {
        /*
         * Specifies things to do regarding window creation. 
         * We size the window according to the user's screensize and this is easier to do programmatically 
         * rather than in xaml.
        */
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
