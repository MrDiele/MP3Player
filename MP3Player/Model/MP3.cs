using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MP3Player.Model
{
    [Serializable]
    class MP3
    {
        public MP3()
        {
            HasID3Tag = false;
            Id3Album = string.Empty;
            Name = string.Empty;
            Path = string.Empty;
            Id3Commnet = string.Empty;
            Id3Title = string.Empty;
            Id3Year = string.Empty;
            Id3Artist = string.Empty;
            Id3Genre = 0;
            Id3TrackNumber = 0;
            MPtime = string.Empty;
            Duration = 0;
            Genre = Genres.None;
        }
        
        #region Get/Set
        public Genres Genre { get; set; }

        public double Duration { get; set; }

        public string MPtime { get; set; }

        public bool HasID3Tag { get; set; }

        public string Id3Album { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public string Id3Commnet { get; set; }

        public string Id3Title { get; set; }

        public string Id3Artist { get; set; }

        public string Id3Year { get; set; }

        public byte Id3Genre { get; set; }

        public byte Id3TrackNumber { get; set; }
        #endregion

        public static void SerializeObject(ObservableCollection<MP3> mp3col)
        {
            BinaryFormatter binary = new BinaryFormatter();
            using (FileStream fstream = new FileStream("listmp3.dat", FileMode.OpenOrCreate, FileAccess.Write))
            {
                binary.Serialize(fstream, mp3col);
            }
        }

        public static ObservableCollection<MP3> DeserializeObject()
        {
            BinaryFormatter binary = new BinaryFormatter();
            ObservableCollection<MP3> mp3col = new ObservableCollection<MP3>();
            using (FileStream fstream = new FileStream("listmp3.dat", FileMode.Open, FileAccess.Read))
            {
                mp3col = (ObservableCollection<MP3>)binary.Deserialize(fstream);
            }
            return mp3col;
        }
    }
}
