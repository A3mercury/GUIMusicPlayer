using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MiniPlayerWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MediaPlayer mediaPlayer;
        private MusicLib musicLib;

        public MainWindow()
        {
            InitializeComponent();

            musicLib = new MusicLib();
            mediaPlayer = new MediaPlayer();

            // Put the ids to a ObservableCollection which has a Remove method for use later.
            // The UI will update itself automatically if any changes are made to this collection.
            ObservableCollection<string> items = new ObservableCollection<string>(musicLib.SongIds);

            // Bind the song IDs to the combo box
            songIdComboBox.ItemsSource = items;

            // Select the first item
            if (songIdComboBox.Items.Count > 0)
            {
                songIdComboBox.SelectedItem = songIdComboBox.Items[0];
                deleteButton.IsEnabled = true;
            }
        }        

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.FileName = ""; 
            openFileDialog.DefaultExt = "*.wma;*.wav;*mp3";
            openFileDialog.Filter = "Media files|*.wma;*.wav;*mp3|MP3 (*.mp3)|*.mp3|Wave files (*.wav)|*.wav|Windows Media Audio (*.wma)|*.wma|All files|*.*"; 
            
            // Show open file dialog box
            Nullable<bool> result = openFileDialog.ShowDialog();

            // Load the selected song
            if (result == true)
            {
                songIdComboBox.IsEnabled = false;
                Song s = GetSongDetails(openFileDialog.FileName);
                titleTextBox.Text = s.Title;
                artistTextBox.Text = s.Artist;
                albumTextBox.Text = s.Album;
                genreTextBox.Text = s.Genre;
                lengthTextBox.Text = s.Length;
                filenameTextBox.Text = s.Filename;
                mediaPlayer.Open(new Uri(s.Filename)); 
                addButton.IsEnabled = true;
            }
        }

        private Song GetSongDetails(string filename)
        {
            try
            {
                // PM> Install-Package taglib
                // http://stackoverflow.com/questions/1750464/how-to-read-and-write-id3-tags-to-an-mp3-in-c
                TagLib.File file = TagLib.File.Create(filename);

                Song s = new Song
                {
                    Title = file.Tag.Title,
                    Artist = file.Tag.AlbumArtists[0],
                    Album = file.Tag.Album,
                    Genre = file.Tag.Genres[0],
                    Length = file.Properties.Duration.Minutes + ":" + file.Properties.Duration.Seconds,
                    Filename = filename
                };

                return s;
            }
            catch (Exception ex)
            {
                DisplayError(ex.Message);
                return null;
            }
        }

        private void DisplayError(string errorMessage)
        {
            MessageBox.Show(errorMessage, "MiniPlayer", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            // Add the selected file to the music library
            Song s = new Song
            {
                Title = titleTextBox.Text,
                Artist = artistTextBox.Text,
                Album = albumTextBox.Text,
                Genre = genreTextBox.Text,
                Length = lengthTextBox.Text,
                Filename = filenameTextBox.Text
            };

            string id = musicLib.AddSong(s).ToString();

            // Add the song ID to the combo box
            songIdComboBox.IsEnabled = true;
            (songIdComboBox.ItemsSource as ObservableCollection<string>).Add(id);
            songIdComboBox.SelectedIndex = songIdComboBox.Items.Count - 1;

            // There is at least one song that can be deleted
            deleteButton.IsEnabled = true;
        }

        private void updateButton_Click(object sender, RoutedEventArgs e)
        {
            int songId = Convert.ToInt32(songIdComboBox.SelectedItem);
            Console.WriteLine("Updating song " + songId);

            Song s = new Song()
            {
                Title = titleTextBox.Text,
                Artist = artistTextBox.Text,
                Album = albumTextBox.Text,
                Genre = genreTextBox.Text,
                Length = lengthTextBox.Text,
                Filename = filenameTextBox.Text
            };

            if(musicLib.UpdateSong(songId, s))
            {
                titleTextBox.Text = s.Title;
                artistTextBox.Text = s.Artist;
                albumTextBox.Text = s.Album;
                genreTextBox.Text = s.Genre;
                lengthTextBox.Text = s.Length;
                filenameTextBox.Text = s.Filename;
            }
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this song?", "MiniPlayer", 
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                int songId = Convert.ToInt32(songIdComboBox.SelectedItem);
                Console.WriteLine("Deleting song " + songId);

                if (musicLib.DeleteSong(songId))
                {
                    // Remove the song from the list box and select the next item
                    (songIdComboBox.ItemsSource as ObservableCollection<string>).Remove(
                        songIdComboBox.SelectedItem.ToString());
                    if (songIdComboBox.Items.Count > 0)
                        songIdComboBox.SelectedItem = songIdComboBox.Items[0];
                    else
                    {
                        // No more songs to display
                        deleteButton.IsEnabled = false;
                        titleTextBox.Text = "";
                        artistTextBox.Text = "";
                        albumTextBox.Text = "";
                        genreTextBox.Text = "";
                        lengthTextBox.Text = "";
                        filenameTextBox.Text = "";
                    }
                }
                else
                {
                    Console.WriteLine("There was a problem deleting song with the id: " + songId);
                }
            }
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Play();
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
        }

        private void showDataButton_Click(object sender, RoutedEventArgs e)
        {
            musicLib.PrintAllTables();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            // Save music.xml in the same directory as the exe
            string filename = "music.xml";
            Console.WriteLine("Saving " + filename);
            musicLib.Save(filename);
        }

        private void songIdComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Display the selected song
            Console.WriteLine("Load song " + songIdComboBox.SelectedItem);
            int songId = Convert.ToInt32(songIdComboBox.SelectedItem);

            Song s = musicLib.GetSong(songId);

            if (s != null)
            {
                titleTextBox.Text = s.Title;
                artistTextBox.Text = s.Artist;
                albumTextBox.Text = s.Album;
                genreTextBox.Text = s.Genre;
                lengthTextBox.Text = s.Length;
                filenameTextBox.Text = s.Filename;
                if(filenameTextBox.Text != "")
                    mediaPlayer.Open(new Uri(filenameTextBox.Text));
            }
        }
    }
}
