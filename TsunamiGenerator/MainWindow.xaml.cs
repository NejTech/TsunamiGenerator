using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Resources;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Threading;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using SKYPE4COMLib;
using MahApps.Metro;

namespace TsunamiGenerator
{
    public partial class MainWindow
    {                                   // Here we define a few objects that we need to pass between methods or threads
        static Skype skp;               // The Skype object, needs to be declared as static, because we need to access it from a BackgroundWorker
        BackgroundWorker floodWorker;   // BackgroundWorker used for the actual flood, so we can have our GUI responsive at all times
        IUserCollection iuc;            // Similar to a List<>, but made of the IUser objects from Skype4COM
        int messagesSent = 0;           // An integer to store the amount of messages sent
        bool tbProgressSupported;       // A boolean to let us know if we can call the Windows API functions to display progress in takbar
        ResourceManager rm;             // Lastly, a ResourceManager to fetch all the strings we need in proper localization

        public MainWindow()
        {
            string[,] langs = SettingsWindow.langs;

            if (File.Exists("lang.cfg"))        // If the file with language settings exists
            {
                string line1;
                using (StreamReader sr = new StreamReader("lang.cfg"))  // Then we read its first line
                    line1 = sr.ReadLine();
                for (int l = 0; l < langs.GetLength(0); l++)            // And go through all the supported languages
                {
                    if (langs[l, 0] == line1)
                    {                                                   // If we find the first line of the file is one of our supported languages, 
                        CultureInfo ci = new CultureInfo(line1);        // We set it as the new culture info
                        Thread.CurrentThread.CurrentUICulture = ci;     // NOTE: Any fooling around with cultures must be done before InitializeComponent()
                    }                                                   //       or it's not going to have any effect!
                }
            }

            tbProgressSupported = (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 1) || (Environment.OSVersion.Version.Major > 6);
                                                                        // To be able to display progress in taskbar we need to be running atleast Windows 7, so we check for it

            InitializeComponent();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            skp = new Skype();
            ThemeManager.ChangeTheme(this, new MahApps.Metro.Accent("Steel", new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Accents/Steel.xaml")), Theme.Light);
                                                                        // Change the accent of the title bar to a more fitting "Steel" color.

            rm = Languages.Text.ResourceManager;                        // Initialize the ResourceManager

            var dc = new DisclaimerWindow();                            // Display the Disclaimer Window
            dc.WindowStartupLocation = WindowStartupLocation.Manual;    // And PROPERLY center it - WindowStartupLocation.CenterOwner doesn't do it properly for some reason
            dc.Left = this.Left + ((this.Width - dc.Width) / 2);        // Basically : position + ( size difference / 2 )
            dc.Top = this.Top + ((this.Height - dc.Height) / 2);
            dc.ShowDialog();

            if (dc.DialogResult != true)                    // If the dislaimer is not confirmed (the window is "just" closed)
                Environment.Exit(0);                        // Then exit.

            if (!skp.Client.IsRunning)                      // Check if the Skype client is running
            {
                //MessageBox.Show(rm.GetString("errSkypeNotFound"), rm.GetString("titleError"), MessageBoxButton.OK, MessageBoxImage.Error);
                var mmb = new ModernMsgBox(rm.GetString("errSkypeNotFound"), MessageBoxImage.Error);    // Since MahApps.Metro doesn't contain a way to
                mmb.Left = this.Left + ((this.Width - mmb.Width) / 2);                                  // display a message box fitting into the design
                mmb.Top = this.Top + ((this.Height - mmb.Height) / 2);                                  // I made one myself, more info in its code
                mmb.ShowDialog();                                                                       // I'm including the MessageBox equivalent in comment
                Environment.Exit(0);                                                                    // every time we need to invoke it for refference...
            }

            try
            {
                skp.Attach(6, true);            // Try to attach to Skype, and prepare for problems...
            }
            catch (COMException)
            {
                //MessageBox.Show(rm.GetString("errSkypeTookTooLong"), rm.GetString("titleError"), MessageBoxButton.OK, MessageBoxImage.Error);
                var mmb = new ModernMsgBox(rm.GetString("errSkypeTookTooLong"), MessageBoxImage.Error); // If it screws up, display an error message
                mmb.Left = this.Left + ((this.Width - mmb.Width) / 2);
                mmb.Top = this.Top + ((this.Height - mmb.Height) / 2);
                mmb.ShowDialog();
                Environment.Exit(0);
            }

            statusLabel.Content = rm.GetString("statusConnected");
        }

        private void button1_Click(object sender, RoutedEventArgs e)    // This is for the "search" button
        {
            if (userSearchTextBox.Text == "")                           // If the textBox is empty, warn the user
            {
                //MessageBox.Show(rm.GetString("warnInputUsername"), rm.GetString("titleWarning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                var mmb = new ModernMsgBox(rm.GetString("warnInputUsername"), MessageBoxImage.Warning); // If you pass the MessageBoxImage as warning,
                mmb.Left = this.Left + ((this.Width - mmb.Width) / 2);                                  // then ModernMsgBox displays differently
                mmb.Top = this.Top + ((this.Height - mmb.Height) / 2);
                mmb.ShowDialog();
                return;
            }

            userSearchListBox.Items.Clear();                        // First, we clean the listBox
            iuc = skp.SearchForUsers(userSearchTextBox.Text);       // Then we define the Skype4COM IUserCollection using a SearchForUsers method with the input from the name

            foreach (IUser u in iuc)                                // Even though it's an independent class, IUserCollection can be looped through with a foreach statement
            {
                userSearchListBox.Items.Add(u.FullName + " (" + u.Handle + ")");    // We add every user as a new item to the listBox using both his 
            }                                                                       // FullName and Handle property to allow for certain identification
        }

        private void button2_Click(object sender, RoutedEventArgs e)    // This is for the "clear" button
        {
            userSearchListBox.Items.Clear();                            // All we need to do is to clear the listBox, because the IUserCollection gets fully 
        }                                                               // overwritten when you perform a new search
        

        private void userSearchListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (userSearchListBox.SelectedIndex != -1)      // If nothing is selected in the listBox (or if it's empty) 
            {
                launchButton.IsEnabled = true;              // Then disable the launch button (a nifty little practice to save ourselves from
            }                                               // displaying another warning ModernMsgBox)
            else
            {
                launchButton.IsEnabled = false;
            }

        }

        private void btnFlood_Click(object sender, RoutedEventArgs e)   // This is for the launch button, this is where the fun begins!
        {
            string message = messageTextBox.Text;   // We define a few variables to store the parameters of the work
            int _count = 0;
            int _delay = 0;

            if (message == "")      // If the message to send is empty, then cuss out- *ahem* I meant warn the user and return from the void
            {
                //MessageBox.Show(rm.GetString("warnInputMessage"), rm.GetString("titleWarning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                var mmb = new ModernMsgBox(rm.GetString("warnInputMessage"), MessageBoxImage.Warning);
                mmb.Left = this.Left + ((this.Width - mmb.Width) / 2);
                mmb.Top = this.Top + ((this.Height - mmb.Height) / 2);
                mmb.ShowDialog();
                return;
            }

            bool msgAmountParsable = Int32.TryParse(countTextBox.Text, out _count);     // For some reason try/catch blocks don't work reliably inside WPF...
            if (msgAmountParsable == false)                                             // So we have to make due without it! Int32.TryParse is perfect for it,
                                                                                        // since in the event of a successful conversion we can save the result
            {                                                                           // but if it fails nothing happens, and that's awesome
                //MessageBox.Show(rm.GetString("warnMsgAmountInvalid"), rm.GetString("titleWarning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                var mmb = new ModernMsgBox(rm.GetString("warnMsgAmountInvalid"), MessageBoxImage.Warning);  // So anyway, it it fails, show a simple polite warning to the user
                mmb.Left = this.Left + ((this.Width - mmb.Width) / 2);                                      // and return from this void
                mmb.Top = this.Top + ((this.Height - mmb.Height) / 2);
                mmb.ShowDialog();
                return;
            }

            if (_count <= 0)        // If the amount of messages to send is too low, then once again warn the user and return...
            {
                //MessageBox.Show(rm.GetString("warnMsgAmountTooSmall"), rm.GetString("titleWarning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                var mmb = new ModernMsgBox(rm.GetString("warnMsgAmountTooSmall"), MessageBoxImage.Warning);
                mmb.Left = this.Left + ((this.Width - mmb.Width) / 2);
                mmb.Top = this.Top + ((this.Height - mmb.Height) / 2);
                mmb.ShowDialog();
                return;
            }

            bool msgSendIntervalParsable = Int32.TryParse(delayTextBox.Text, out _delay);   // If we got this far, we go and try to parse the sending interval
            if (msgSendIntervalParsable == false)                                           // Exactly the same thing as before...
            {
                //MessageBox.Show(rm.GetString("warnSendingIntervalInvalid"), rm.GetString("titleWarning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                var mmb = new ModernMsgBox(rm.GetString("warnSendingIntervalInvalid"), MessageBoxImage.Warning);
                mmb.Left = this.Left + ((this.Width - mmb.Width) / 2);
                mmb.Top = this.Top + ((this.Height - mmb.Height) / 2);
                mmb.ShowDialog();
                return;
            }

            if (_delay <= 0)
            {
                //MessageBox.Show(rm.GetString("warnSendingIntervalTooSmall"), rm.GetString("titleWarning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                var mmb = new ModernMsgBox(rm.GetString("warnSendingIntervalTooSmall"), MessageBoxImage.Warning);
                mmb.Left = this.Left + ((this.Width - mmb.Width) / 2);
                mmb.Top = this.Top + ((this.Height - mmb.Height) / 2);
                mmb.ShowDialog();
                return;
            }
                                                        // If we got here, it means all the user input is correct and we can go 'make some noise'!
            stopButton.IsEnabled = true;                // We enable the stop button
            launchButton.IsEnabled = false;             // And disable the launch button
            statusLabel.Content = rm.GetString("statusWorking");    // Change the status label to notify the user we're working
            messagesSent = 0;                           // Since we're about to start, we set the amount of messages sent (used outside this void) to 0

                                                        // We prepare the taskbar "progress" bar by setting the progress state to normal. We need to ALWAYS
                                                        // check if we're running atleast Windows 7, because calling Windows API functions that aren't there
            if (tbProgressSupported)                    // will most likely lead to severe program issues... Luckily, we have a boolean for that
                Win7ProgressBar.TBProgress.SetProgressState(new WindowInteropHelper(this).Handle, ThumbnailProgressState.Normal);

            string[] floodData_ = { message, countTextBox.Text, delayTextBox.Text, iuc[userSearchListBox.SelectedIndex + 1].Handle };
                                                        // We prepare a little four string array to pass to the worker
            floodWorker = new BackgroundWorker();       // Then we define the BackgroundWorker and set up it's properties...
            floodWorker.WorkerReportsProgress = true;
            floodWorker.WorkerSupportsCancellation = true;
            floodWorker.DoWork += floodWorker_DoWork;   // ..enabling us to define the three events it raises
            floodWorker.ProgressChanged += floodWorker_ProgressChanged;
            floodWorker.RunWorkerCompleted += floodWorker_Completed;
            floodWorker.RunWorkerAsync(floodData_);     // And we start it, passing the work data to it

        }

        void floodWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string[] floodData = (string[])e.Argument;  // First, we "receive" the data needed for the work from the event's arguments
            int count = Convert.ToInt32(floodData[1]);  // We can just "Convert.ToInt32" it because we already know it's parsable
            int delay = Convert.ToInt32(floodData[2]);

            for (int i = 0; i < count; i++)             // And here is the "extremely complicated" algorithm itself
            {
                messagesSent = i;                       // We "save" i to messagesSent to use it outside of this loop
                if (floodWorker.CancellationPending)    // We check if someone didn't hit the stop button (more about it later)
                    break;
                skp.SendMessage(floodData[3], floodData[0]);    // We send the message itself!
                (sender as BackgroundWorker).ReportProgress((int)(((float)i / (float)count) * 100));    // And report progress, and do some float division and then convert it
                Thread.Sleep(delay);                                                                    // to a procentual amount rounded up to an integer (some optimalization
            }                                                                                           // could be done here) and afterwards pause the thread for the specified
        }                                                                                               // amount of milliseconds...

        void floodWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)     // This is the event that gets fired when the worker reports progress...
        {                                                                               // Which means more or less with every message... As said, this could be optimized a bit
            if (tbProgressSupported)                                                    // Anyway, display the procentual progress on both the progress bars and the actual amount
                Win7ProgressBar.TBProgress.SetProgressValue(new WindowInteropHelper(this).Handle, (ulong)e.ProgressPercentage, (ulong)100); // in the status message
            progressBar1.Value = e.ProgressPercentage;
            statusLabel.Content = rm.GetString("statusWorking")+ " " + messagesSent.ToString() + " " + rm.GetString("msgSent");
        }

        void floodWorker_Completed(object sender, RunWorkerCompletedEventArgs e)        // This gets fired when the work gets finished...
        {
            //tbmanager.SetProgressValue(100, 100);                                       // We set both the progress bars to one hundred percent
            if (tbProgressSupported)
                Win7ProgressBar.TBProgress.SetProgressValue(new WindowInteropHelper(this).Handle, (ulong)100, (ulong)100);
            progressBar1.Value = 100;

            stopButton.IsEnabled = false;                                               // Disable the stop button and enable the launch button
            launchButton.IsEnabled = true;
            statusLabel.Content = rm.GetString("statusDone") + ", " + (messagesSent + 1).ToString() + " " + rm.GetString("msgSent");    // And change the status message
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)     // This if for the "stop" button
        {
            floodWorker.CancelAsync();          // Send a cancel "note" to the worker
            stopButton.IsEnabled = false;       // And once again, disable the stop button and enable the launch button
            launchButton.IsEnabled = true;
        }

        private void statusLabel_MouseDown(object sender, MouseButtonEventArgs e)   // This happens when somebody clicks on the status message
        {
            if (e.RightButton == MouseButtonState.Pressed)                          // If it was a right-click
            {
                progressBar1.Value = 0;                                             // Then practically wipe all the user input. Hidden feature.
                if (tbProgressSupported)
                    Win7ProgressBar.TBProgress.SetProgressState(new WindowInteropHelper(this).Handle, ThumbnailProgressState.NoProgress);
                statusLabel.Content = rm.GetString("statusConnected");
                delayTextBox.Text = "";
                countTextBox.Text = "";
                messageTextBox.Text = "";
                userSearchTextBox.Text = "";
                userSearchListBox.Items.Clear();
            }
        }

        private void titleBarSettingsButton_Click(object sender, RoutedEventArgs e)     // This is for the nifty settings button in the title bar
        {
            var sw = new SettingsWindow();                              // Display the settings window... That's it
            sw.WindowStartupLocation = WindowStartupLocation.Manual;
            sw.Left = this.Left + ((this.Width - sw.Width)/2);
            sw.Top = this.Top + 30;                                     // The alignment is slightly different here
            sw.ShowDialog();
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();              // We need to make sure the application fully closes, because without this
        }                                                               // the sometimes process doesn't close when you close the main window. WPF?
    }
}
