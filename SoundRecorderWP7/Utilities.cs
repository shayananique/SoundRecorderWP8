using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO.IsolatedStorage;

namespace SoundRecorderWP8
{
    public static class Utilities
    {
        public static string SelectedSound = string.Empty;
        private static IsolatedStorageFile _IsoStore;
        public static IsolatedStorageFile IsoStore
        {
            get
            {
                if (_IsoStore == null)
                {
                    _IsoStore = IsolatedStorageFile.GetUserStoreForApplication();
                }
                return _IsoStore;
            }
        }
    }
    
    public class MyFileInfo
        {
            public long Length { get; set; }
            public string Name { get; set; }
        }
}


