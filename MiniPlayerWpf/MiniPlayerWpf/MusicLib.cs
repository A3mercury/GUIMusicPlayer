using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPlayerWpf
{
    class MusicLib
    {
        private DataSet musicDataSet;

        // The constructor reads the music.xsd and music.xml files into the DataSet
        public MusicLib()
        {
            musicDataSet = new DataSet();
            musicDataSet.ReadXmlSchema("music.xsd");
            musicDataSet.ReadXml("music.xml");

            PrintAllTables();

            // Get a list of all song IDs
            DataTable songs = musicDataSet.Tables["song"];
            var ids = from row in songs.AsEnumerable()
                      orderby row["id"]
                      select row["id"].ToString();
        }

        public void PrintAllTables()
        {
            foreach (DataTable table in musicDataSet.Tables)
            {
                Console.WriteLine("Table name = " + table.TableName);
                foreach (DataRow row in table.Rows)
                {
                    Console.WriteLine("Row:");
                    int i = 0;
                    foreach (Object item in row.ItemArray)
                    {
                        Console.WriteLine(" " + table.Columns[i].Caption + "=" + item);
                        i++;
                    }
                }
                Console.WriteLine();
            }
        }

        public EnumerableRowCollection<string> SongIds
        {
            get
            {
                var ids = from row in musicDataSet.Tables["song"].AsEnumerable()
                          orderby row["id"]
                          select row["id"].ToString();
                return ids;
            }
        } 

        // Adds a song to the music library and returns the song's ID. The song 
        // parameter's ID is also set to the auto-generated ID.
        public int AddSong(Song s)
        {
            DataTable table = musicDataSet.Tables["song"];
            DataRow row = table.NewRow();

            row["title"] = s.Title;
            row["artist"] = s.Artist;
            row["album"] = s.Album;
            row["filename"] = s.Filename;
            row["length"] = s.Length;
            row["genre"] = s.Genre;
            table.Rows.Add(row); 

            // Update this song's ID
            s.Id = Convert.ToInt32(row["id"]);

            return s.Id;
        }

        // Return a Song for the given song ID or null if no song was not found.
        public Song GetSong(int songId) 
        {
            DataTable table = musicDataSet.Tables["song"];
            Song s = null;

            foreach (DataRow row in table.Select("id=" + songId))
            {
                if(row["id"].ToString() == songId.ToString())
                {
                    s = new Song();
                    s.Title = row["title"].ToString();
                    s.Artist = row["artist"].ToString();
                    s.Album = row["album"].ToString();
                    s.Filename = row["filename"].ToString();
                    s.Length = row["length"].ToString();
                    s.Genre = row["genre"].ToString();
                }
            }

            return s;
        }

        // Update the given song with the given song ID. Returns true if the song 
        // was updated, false if it could not because the song ID was not found.
        public bool UpdateSong(int songId, Song song)
        {
            //DataTable table = musicDataSet.Tables["song"];
            bool result = false;

            DataTable table = musicDataSet.Tables["song"];

            foreach (DataRow row in table.Select("id=" + songId))
            {
                if (row["id"].ToString() == songId.ToString())
                {
                    row["title"] = song.Title;
                    row["artist"] = song.Artist;
                    row["album"] = song.Album;
                    row["filename"] = song.Filename;
                    row["genre"] = song.Genre;
                    row["length"] = song.Length;

                    result = true;
                }
            }

            return result;
        }

        // Delete a song given the song's ID. Return true if the song was 
        // successfully deleted, false if the song ID was not found. 
        public bool DeleteSong(int songId) 
        {
            bool result = false;
            DataTable table = musicDataSet.Tables["song"];
            table.Rows.Remove(table.Rows.Find(songId));

            // Remove from playlist_song every occurance of songId.
            // Add rows to a separate list before deleting because we'll get an exception
            // if we try to delete more than one row while looping through table.Rows

            List<DataRow> rows = new List<DataRow>();
            table = musicDataSet.Tables["playlist_song"];
            foreach (DataRow row in table.Rows)
                if (row["song_id"].ToString() == songId.ToString())
                    rows.Add(row);

            foreach (DataRow row in rows)
                row.Delete();

            if(GetSong(songId) == null)
                result = true;

            return result;
        }

        // Save the song database to the music.xml file 
        public void Save(string filename)
        {
            musicDataSet.WriteXml(filename);
        }
    }
}
