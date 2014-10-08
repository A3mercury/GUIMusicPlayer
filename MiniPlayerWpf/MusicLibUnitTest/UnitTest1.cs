using System;
using MiniPlayerWpf;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MusicLibUnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestClass]
        public class MusicLibUnitTest
        {
            private Song defaultSong = new Song
            {
                Id = 9,
                Artist = "Bob",
                Album = "Fire",
                Filename = "test.mp3",
                Genre = "cool",
                Length = "123",
                Title = "Best Song"
            };

            [TestMethod]
            public void AddSongTest()
            {
                MusicLib musicLib = new MusicLib();

                // ID auto-increments
                int expectedId = 9;
                int actualId = musicLib.AddSong(defaultSong);
                Assert.AreEqual(expectedId, actualId, "ID of added song was unexpectedly " + actualId);

                // See if we can get back the song that was just added
                Song expectedSong = defaultSong;
                Song actualSong = musicLib.GetSong(expectedId);
                Assert.AreEqual(expectedSong, actualSong, "Got back unexpected song: " + actualSong.Title);
            }

            [TestMethod]
            public void DeleteSongTest()
            {
                MusicLib musicLib = new MusicLib();

                // Delete a song that already exists
                int songId = 8;
                bool expected = true;
                bool actual = musicLib.DeleteSong(songId);
                Assert.AreEqual(expected, actual, "Delete should have worked but got back " + actual);

                // Verify the song is not in the library anymore
                Song s = musicLib.GetSong(songId);
                Assert.IsNull(s, "Returned song should be null because it doesn't exist");

                // Delete a song that doesn't exist
                songId = 111;
                expected = false;
                actual = musicLib.DeleteSong(songId);
                Assert.AreEqual(expected, actual, "Delete should NOT have worked but got back " + actual);
            }

            [TestMethod]
            public void GetSongTest()
            {
                MusicLib musicLib = new MusicLib();

                // Add the default song and then retrieve it
                int songId = musicLib.AddSong(defaultSong);
                Song expected = defaultSong;
                Song actual = musicLib.GetSong(songId);
                Assert.AreEqual(expected, actual);

                // Get a song that doesn't exist
                songId = 111;
                actual = musicLib.GetSong(songId);
                Assert.IsNull(actual);
            }

            [TestMethod]
            public void UpdateSongTest()
            {
                MusicLib musicLib = new MusicLib();

                // Update a song's title
                int songId = musicLib.AddSong(defaultSong);
                defaultSong.Title = "Horrible Song";
                bool expected = true;
                bool actual = musicLib.UpdateSong(songId, defaultSong);
                Assert.AreEqual(expected, actual, "Update should have worked; got back " + actual);

                // Make sure the song's title was really changed
                Song s = musicLib.GetSong(songId);
                Assert.AreEqual(defaultSong, s, "Title was apparently not updated properly");

                // Try to update a song with a bad ID
                songId = 111;
                expected = false;
                actual = musicLib.UpdateSong(songId, defaultSong);
                Assert.AreEqual(expected, actual, "Update should NOT have worked; got back " + actual);
            }
        }
    }
}
