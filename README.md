# Tsunami Generator
## A program for sending large amounts of messages on Skype

The idea for this program was conceived after some fooling around with Skype4COM.dll (which is, unfortunately getting phased out) and using it to repeatedly send messages. It is this simple.

###Used libraries:
* MahApps.Metro (included as NuGet package)
* System.Windows.Interactivity (from Blend SDK)
* Skype4COM.dll (exists on any computer with Skype installed)

###License:
This program is licensed under the BSD 2-Clause License. See LICENSE for its full text.

###Localization:
This program currently supports the English and Czech languages. The support for localization is done through .resx files (found in the Languages folder of the project) and a list of supported languages is hardcoded at the beginning of SettingsWindow.xaml.cs in *string[,] langs*.

###Binaries:
A zip archive with compiled binaries can be found in the bin folder of this repository.