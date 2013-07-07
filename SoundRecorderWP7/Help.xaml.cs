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

namespace SoundRecorderWP8
{
    public partial class Help : PhoneApplicationPage
    {
        public Help()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(Help_Loaded);
        }

        void Help_Loaded(object sender, RoutedEventArgs e)
        {
            textBlock1.Text = @"Use this application to record and save your sound this app also includes a slider which you can use to manupulate the pitch of your voice. A Update With More Features is Coming Soon";  
        }
    }
}