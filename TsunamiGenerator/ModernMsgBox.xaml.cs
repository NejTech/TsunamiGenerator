using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using MahApps.Metro;

namespace TsunamiGenerator
{
    /// <summary>
    /// Interaction logic for ModernMsgBox.xaml
    /// </summary>
    public partial class ModernMsgBox
    {
        public ModernMsgBox(string text, MessageBoxImage img)
        {
            InitializeComponent();
            ThemeManager.ChangeTheme(this, new MahApps.Metro.Accent("Steel", new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Accents/Steel.xaml")), Theme.Light);
            ResourceManager rm = Languages.Text.ResourceManager;    // Once again, we change the theme and get ourselves a ResourceManager to fetch the window labels
                                                    // As I said, the look of this is decided by the MessageBoxImage you pass to it, right now it is coded quite crudely,
            if (img == MessageBoxImage.Warning)     // but I want to release this as a little separate "project" later
            {
                this.Background = new SolidColorBrush(Color.FromArgb(0xff, 0xee, 0xee, 0x2d)); // So anyway, if the MessageBoxImage passed is the Warning image,
                this.Title = rm.GetString("titleWarning");                                     // change the background to #FFEEEE2D and change the title and the icon to "warning"
                IconVisualBrush.Visual = (Visual)this.Resources["appbar_warning"];             // Took me a bit of time to figure out, but accesing the MahApps.Metro resources
            }                                                                                  // IS possible from code :)
            msgTextBlock.Text = text;                                                          // And we set the textBlock text to the passed text...
        }

        private void Button_Click(object sender, RoutedEventArgs e)     // When the "OK" button is pressed
        {
            this.Close();                                               // Close the window (NOTE: Here we use Close() because it fires the Closing event, unlike Hide())
        }

        private void ModernMsgBoxWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            var anim = new DoubleAnimation(0, (Duration)TimeSpan.FromMilliseconds(500));    // Esentially the same animation as the other non-Main Windows...
            anim.Completed += (s, _) => this.Hide();
            this.BeginAnimation(UIElement.OpacityProperty, anim);
        }
    }
}
