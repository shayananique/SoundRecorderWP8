using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.IO.IsolatedStorage;
using System.IO;

namespace SoundRecorderWP8
{
    public partial class SavedSounds : PhoneApplicationPage
    {
        public SavedSounds()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(SavedSounds_Loaded);
        }

        void SavedSounds_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSounds();
        }
        private void LoadSounds()
        {
            Utilities.SelectedSound = null;

            IsolatedStorageFile isf = Utilities.IsoStore;
            if (!isf.DirectoryExists("sounds")) isf.CreateDirectory("sounds");

            string[] filenames = isf.GetFileNames("sounds//*.wav");

            List<MyFileInfo> files = new List<MyFileInfo>();
            foreach (var item in filenames)
            {
                IsolatedStorageFileStream fi = new IsolatedStorageFileStream(System.IO.Path.Combine("sounds", item), FileMode.Open, isf);
                files.Add(new MyFileInfo { Length = fi.Length, Name = item });

            }
            listBox1.ItemsSource = files;
        }


        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this file?", "Confirm file deletion", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                IsolatedStorageFile isf = Utilities.IsoStore;
                isf.DeleteFile("sounds//" + (sender as MenuItem).Tag.ToString());
                LoadSounds();
            }
   
        }


        private void PlayMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Utilities.SelectedSound = (sender as MenuItem).Tag.ToString();

            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

    }
}