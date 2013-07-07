using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System.IO.IsolatedStorage;
using AgiliTrain.PhoneyTools.Microphone;
using System.Windows.Resources;
using Microsoft.Xna.Framework.Media;
using Coding4Fun.Phone.Controls;

namespace SoundRecorderWP8
{
    public partial class MainPage : PhoneApplicationPage
    {

        IApplicationBarIconButton recordButton;
        IApplicationBarIconButton playButton;
        IApplicationBarIconButton saveButton;

        private Microphone microphone;
        private byte[] buffer; 
        private MemoryStream stream;  
        private SoundEffectInstance soundInstance;              
        private bool soundIsPlaying;                   

        // Status images
        private BitmapImage blankImage;
        private BitmapImage microphoneImage;
        private BitmapImage speakerImage;

        /// <summary>
        /// Constructor 
        /// </summary>
        public MainPage()
        {
            Initialize();
        }
        private void Initialize()
        {

            InitializeComponent();

            recordButton = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
            playButton = ApplicationBar.Buttons[1] as ApplicationBarIconButton;
            saveButton = ApplicationBar.Buttons[2] as ApplicationBarIconButton;
            

   
            stream = new MemoryStream();
            microphone = Microphone.Default;
            soundIsPlaying = false;

            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromMilliseconds(33);
            dt.Tick += new EventHandler(dt_Tick);
            dt.Start();


            microphone.BufferReady +=
                new EventHandler<EventArgs>(microphone_BufferReady);

           blankImage = new BitmapImage(new Uri("Images/blank.png", UriKind.RelativeOrAbsolute));
           microphoneImage = new BitmapImage(new Uri("Images/microphone.png", UriKind.RelativeOrAbsolute));
           speakerImage = new BitmapImage(new Uri("Images/speaker.png", UriKind.RelativeOrAbsolute));
        }
        /// <summary>
        /// Updates the XNA FrameworkDispatcher and checks to see if a sound is playing.
        /// If sound has stopped playing, it updates the UI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void dt_Tick(object sender, EventArgs e)
        {
            try { FrameworkDispatcher.Update(); }
            catch { }

            if (soundIsPlaying)
            {
                if (soundInstance.State != SoundState.Playing)
                {
                    // Audio has finished playing
                    soundIsPlaying = false;

                    // Update the UI to reflect that the 
                    // sound has stopped playing
                    SetButtonStates(true, true, false, true);
                    UserHelp.Text = "press play\nor record";
                    StatusImage.Source = blankImage;
                }
            }
        }

        /// <summary>
        /// The Microphone.BufferReady event handler.
        /// Gets the audio data from the microphone and stores it in a buffer,
        /// then writes that buffer to a stream for later playback.
        /// Any action in this event handler should be quick!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void microphone_BufferReady(object sender, EventArgs e)
        {
            // Retrieve audio data
            microphone.GetData(buffer);

            // Store the audio data in a stream
            stream.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Handles the Click event for the record button.
        /// Sets up the microphone and data buffers to collect audio data,
        /// then starts the microphone. Also, updates the UI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void recordButton_Click(object sender, EventArgs e)
        {
            // Get audio data in 1/2 second chunks
            microphone.BufferDuration = TimeSpan.FromMilliseconds(500);

            // Allocate memory to hold the audio data
            buffer = new byte[microphone.GetSampleSizeInBytes(microphone.BufferDuration)];

            // Set the stream back to zero in case there is already something in it
            stream.SetLength(0);

            // Start recording
            microphone.Start();

            SetButtonStates(false, false, true, false);
            UserHelp.Text = "record";
            StatusImage.Source = microphoneImage;
        }

        /// <summary>
        /// Handles the Click event for the stop button.
        /// Stops the microphone from collecting audio and updates the UI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stopButton_Click(object sender, EventArgs e)
        {
            if (microphone.State == MicrophoneState.Started)
            {
                // In RECORD mode, user clicked the 
                // stop button to end recording
                microphone.Stop();
            }
            else if (soundInstance != null && soundInstance.State == SoundState.Playing)
            {
                // In PLAY mode, user clicked the 
                // stop button to end playing back
                soundInstance.Stop();
            }

            SetButtonStates(true, true, false, true);
            UserHelp.Text = "ready";
            StatusImage.Source = blankImage;

        }

        /// <summary>
        /// Handles the Click event for the play button.
        /// Plays the audio collected from the microphone and updates the UI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void playButton_Click(object sender, EventArgs e)
        {
            if (stream.Length > 0)
            {
                // Update the UI to reflect that
                // sound is playing
                SetButtonStates(false, false, true, false);
                UserHelp.Text = "play";
                StatusImage.Source = speakerImage;

                // Play the audio in a new thread so the UI can update.
                Thread soundThread = new Thread(new ThreadStart(playSound));
                soundThread.Start();
            }
        }

        /// <summary>
        /// Plays the audio using SoundEffectInstance 
        /// so we can monitor the playback status.
        /// </summary>
        private void playSound()
        {
            // Play audio using SoundEffectInstance so we can monitor it's State 
            // and update the UI in the dt_Tick handler when it is done playing.
            SoundEffect sound = new SoundEffect(stream.ToArray(), microphone.SampleRate, AudioChannels.Mono);
            soundInstance = sound.CreateInstance();
            soundIsPlaying = true;
            Dispatcher.BeginInvoke(() => soundInstance.Pitch = (float)slider1.Value);
            soundInstance.Play();
        }

        /// <summary>
        /// Helper method to change the IsEnabled property for the ApplicationBarIconButtons.
        /// </summary>
        /// <param name="recordEnabled">New state for the record button.</param>
        /// <param name="playEnabled">New state for the play button.</param>
        /// <param name="saveEnabled">New state for the save button.</param>
        /// <param name="ringtoneEnabled">New state for ringtone button</param>
        private void SetButtonStates(bool recordEnabled, bool playEnabled, bool stopEnabled, bool saveEnabled)
        {
           recordButton.IsEnabled = recordEnabled;
           playButton.IsEnabled = playEnabled;
           saveButton.IsEnabled = saveEnabled;
            if (playEnabled)
            {
                 playButton.Text = "play";
                 playButton.IconUri = new Uri("/Images/play.png",UriKind.Relative);

                 playButton.Click -=  playButton_Click;
                 playButton.Click -= stopButton_Click;
                 playButton.Click +=new EventHandler( playButton_Click);
            }
            if (stopEnabled)
            {
                
                 playButton.Text = "stop";
                 playButton.IconUri = new Uri("/Images/stop.png", UriKind.Relative);
                playButton.IsEnabled = true;
                 playButton.Click -=  playButton_Click;
                playButton.Click -= stopButton_Click;
                 playButton.Click += new EventHandler(stopButton_Click);
            }
        }
        
        private void saveButton_Click(object sender, System.EventArgs e)
        {

            stopButton_Click(null,null);

            // TODO: Add event handler implementation here.
            if (stream.GetBuffer().Length == 0)
            {
                MessageBox.Show("You must first record a sound before you try to save it");
                return;
            }
            IsolatedStorageFile isf = Utilities.IsoStore;

            if (!isf.DirectoryExists("sounds")) isf.CreateDirectory("sounds");

            InputPrompt input = new InputPrompt();
            input.Completed += (s, e2) =>
            {
                IsolatedStorageFileStream localstream = isf.CreateFile(Path.Combine("sounds", e2.Result + ".wav"));
                localstream.Write(stream.GetBuffer(), 0, stream.GetBuffer().Length);
                localstream.Flush();
                localstream.Close();
            };
            input.Title = "Filename";
            input.Message = "Write the filename of the sound";
            input.Show();

        }

        private void navigateToSavedSoundsMenuItem_Click(object sender, System.EventArgs e)
        {
            // TODO: Add event handler implementation here.
            NavigationService.Navigate(new Uri("/SavedSounds.xaml", UriKind.Relative));
        }


        private void LoadSound(string filename)
        {
            using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                IsolatedStorageFileStream localstream = iso.OpenFile(Path.Combine("sounds", filename), FileMode.Open);
                buffer = new byte[localstream.Length];
                localstream.Read(buffer, 0, (int)localstream.Length);
                localstream.Flush();
                localstream.Close();
                stream = new MemoryStream(buffer,0,buffer.Length,true,true);
                SetButtonStates(true, true, false, false);
            }
        }


        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (!string.IsNullOrEmpty(Utilities.SelectedSound))
            {
                LoadSound(Utilities.SelectedSound);
            }           
        }

        private void aboutMenuItem_Click(object sender, EventArgs e)
        {
            AboutPrompt ap = new AboutPrompt();
            ap.Show("Shayan Anique", "shayan_anique", "support@shayananique.com", "http://www.shayananique.com");
        }

        private void helpMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Help.xaml", UriKind.Relative));
        }

        private void RoundButton_Click(object sender, RoutedEventArgs e)
        {
            slider1.Value = 0;
        }

        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (soundInstance != null && soundIsPlaying)
                soundInstance.Pitch = (float)slider1.Value;
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void PhoneApplicationPage_Loaded_1(object sender, RoutedEventArgs e)
        {
       
 
        }
    }
}


      


