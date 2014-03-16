using System;
using System.Runtime.InteropServices;

namespace TsunamiGenerator
{
    public class Win7ProgressBar
    {
        private static ITaskbarList3 _taskbarList;  // We need two object to have our imported methods properly accessible to other classes,
        internal static ITaskbarList3 TBProgress    // a private one and an internal one that is "declared" using the private one and some
        {                                           // of the other imported methods
            get
            {
                if (_taskbarList == null)
                {
                    lock (typeof(Win7ProgressBar))  // This is a violation of Microsoft's design guidelines on the lock keyword, but there
                    {                               // is no other way to do it, we need the Win7ProgressBar class locked when we hook up
                        if (_taskbarList == null)   // with Windows API
                        {
                            _taskbarList = (ITaskbarList3)new CTaskbarList();
                            _taskbarList.HrInit();
                        }
                    }
                }
                return _taskbarList;
            }
        }
    }

    public enum ThumbnailProgressState  // A little handy enumerable containing all the possible progress states for
    {                                   // us so we don't need to remember the values for them
        NoProgress = 0,                 // No progress is displayed
        Indeterminate = 0x1,            // The progress is indeterminate (marquee)
        Normal = 0x2,                   // Normal progress is displayed
        Error = 0x4,                    // An error occurred (red)
        Paused = 0x8                    // The operation is paused (yellow)
    }

    [ComImportAttribute()]
    [GuidAttribute("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf")]     // COM GUID for ITaskbarList3, can be found on http://referencesource.microsoft.com
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ITaskbarList3                            // All these methods are required, but fourtunately, since ITaskbarList3 inherits
    {                                                           // everything from ITaskbarList2 and ITaskbarList, we don't need to import it thrice
        // ITaskbarList
        [PreserveSig]
        void HrInit();
        [PreserveSig]
        void AddTab(IntPtr hwnd);
        [PreserveSig]
        void DeleteTab(IntPtr hwnd);
        [PreserveSig]
        void ActivateTab(IntPtr hwnd);
        [PreserveSig]
        void SetActiveAlt(IntPtr hwnd);

        // ITaskbarList2
        [PreserveSig]
        void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

        // ITaskbarList3
        void SetProgressValue(IntPtr hwnd, UInt64 ullCompleted, UInt64 ullTotal);   // These are the two methods we're gonna be using in code
        void SetProgressState(IntPtr hwnd, ThumbnailProgressState tbpFlags);
    }

    [GuidAttribute("56FDF344-FD6D-11d0-958A-006097C9A090")]     // COM GUID for TaskbarList, can be found on http://referencesource.microsoft.com
    [ClassInterfaceAttribute(ClassInterfaceType.None)]
    [ComImportAttribute()]
    internal class CTaskbarList { }
}
