using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Globalization;
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
    /// Interaction logic for DisclaimerWindow.xaml
    /// </summary>
    public partial class DisclaimerWindow
    {
        public DisclaimerWindow()
        {
            InitializeComponent();
        }

        private void MetroWindow_ContentRendered(object sender, EventArgs e)
        {
            ThemeManager.ChangeTheme(this, new MahApps.Metro.Accent("Steel", new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Accents/Steel.xaml")), Theme.Dark);
        }                                                                                   // Change the accent of the title bar to a more fitting "Steel" color.

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var anim = new DoubleAnimation(0, (Duration)TimeSpan.FromMilliseconds(500));    // A simple way to display a fade out animation in three lines using System.Windows.Media.Animation
            anim.Completed += (s, _) => this.DialogResult = true;                           // Don't forget to set DialogResult to true so MainWindow knows we confirmed the disclaimer
            this.BeginAnimation(UIElement.OpacityProperty, anim);                           // Setting DialogResult closes the dialog, so we just start the animation and that's all we need
        }
    }
}
