using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.IO;
using System.Threading;
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
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        //         -------------- HARDCODED LIST OF SELECTED LANGUAGES HERE --------------
        public static string[,] langs = new string[2, 2] { { "en-US", "english" }, { "cs-CZ", "czech" } };      // TODO: French, Spanish, ...
                                                                                                                // Accessed from MainWindow
        private void MetroWindow_ContentRendered(object sender, EventArgs e)
        {
            ThemeManager.ChangeTheme(this, new MahApps.Metro.Accent("Steel", new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Accents/Steel.xaml")), Theme.Dark);
                                                                                                // Change the accent of the title bar to a more fitting "Steel" color.
            langs = new string[2, 2] { { "en-US", "english" }, { "cs-CZ", "czech" } };          // List of all selected languages, TODO: French, Spanish, ...

            if (File.Exists("lang.cfg"))                                    // Same as MainWindow: If the file with language settings exists...
            {
                string line1;
                using (StreamReader sr = new StreamReader("lang.cfg"))      // Then we read its first line
                    line1 = sr.ReadLine();
                for (int l = 0; l < langs.GetLength(0); l++)                // And go through all the supported languages
                {
                    if (langs[l, 0] == line1)
                    {                                                       // If we find the first line of the file is one of our supported languages, 
                        languageSelectBox.SelectedIndex = l;                // We set it as the selection in the combobox
                    }
                }
            }

            for (int l = 0; l < langs.GetLength(0); l++)                    // We loop through the list for the second time
            {
                languageSelectBox.Items.Add(langs[l, 1]);                   // And fill the combobox with available languages

                if (!File.Exists("lang.cfg") && CultureInfo.CurrentUICulture.Equals(new CultureInfo(langs[l, 0])))  // If the settings file doesn't exist and the current UICulture
                    languageSelectBox.SelectedIndex = l;                                // equals the UICulture we are currently looking at in the loop, we set the selected index to it
            }
        }

        private void languageSelectBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            File.WriteAllText("lang.cfg", langs[languageSelectBox.SelectedIndex, 0]);       // When a new language is selected, we write it to a file with WriteAllText
        }                                                                                   // Because it doesn't matter if it exists or not, because it gets overwritten anyway

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;                                                                // Stop the event so we can animate the closing
            var anim = new DoubleAnimation(0, (Duration)TimeSpan.FromMilliseconds(500));    // A simple way to display a fade out animation in three lines using System.Windows.Media.Animation
            anim.Completed += (s, _) => this.Hide();                                        // Set the end of the animation to close the window (for some weird reason, Hide actually closes it
            this.BeginAnimation(UIElement.OpacityProperty, anim);                           // while Close makes the window stay in the takbar... wtf.)
        }                                                                                   // Oh and, start the animation
    }
}
