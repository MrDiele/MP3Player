using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MP3Player.Model
{
    public enum Genres : byte
    {
        Blues,
        ClassicRock,
        Country,
        Dance,
        Disco,
        Funk,
        Grunge,
        HipHop,
        Jazz,
        Metal,
        NewAge,
        Oldies,
        Other,
        Pop,
        RnB,
        Rap,
        Reggae,
        Rock,
        Techno,
        Industrial,
        Alternative,
        Ska,
        DeathMetal,
        Pranks,
        Soundtrack,
        EuroTechno,
        Ambient,
        TripHop,
        Vocal,
        JazzFunk,
        Fusion,
        Trance,
        Classical,
        Instrumental,
        Acid,
        House,
        Game,
        SoundClip,
        Gospel,
        Noise,
        AlternRock,
        Bass,
        Soul,
        Punk,
        Space,
        Mediative,
        InstrumentalPop,
        InstrumentalRock,
        Ethnic,
        Gothic,
        Darkwave,
        TechnoIndustrial,
        Electronic,
        PopFolk,
        Eurodance,
        Dream,
        SouthernRock,
        Comedy,
        Cult,
        Gangsta,
        Top40,
        ChristianRap,
        PopFunk,
        Jungle,
        NativeAmerican,
        Cabaret,
        NewWave,
        Psychadelic,
        Rave,
        Showtunes,
        Trailer,
        LoFi,
        Tribal,
        AcidPunk,
        AcidJazz,
        Polka,
        Retro,
        Musical,
        RocknRoll,
        HardRock,
        None = 255
    };

    [Serializable]
    class MP3
    {
        private string name;
        private string path;
        private bool hasID3Tag;
        private string id3Album;
        private string id3Artist;
        private string id3Comment;
        private byte id3Genre;
        private Genres genre;
        private string id3Title;
        private byte id3TrackNumber;
        private string id3Year;
        private string mptime;
        private double duration;

        public MP3()
        {

            this.hasID3Tag = false;
            this.id3Album = string.Empty;
            this.name = string.Empty;
            this.path = string.Empty;
            this.id3Comment = string.Empty;
            this.id3Title = string.Empty;
            this.id3Year = string.Empty;
            this.id3Artist = string.Empty;
            this.id3Genre = 0;
            this.id3TrackNumber = 0;
            this.mptime = string.Empty;
            this.duration = 0;
            this.genre = Genres.None;
        }

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

        public Genres Genre
        {
            get
            {
                return this.genre;
            }
            set
            {
                this.genre = value;
            }
        }

        public double Duration
        {
            get
            {
                return this.duration;
            }
            set
            {
                this.duration = value;
            }
        }

        public string MPtime
        {
            get
            {
                return this.mptime;
            }
            set
            {
                this.mptime = value;
            }
        }

        public bool HasID3Tag
        {
            get
            {
                return this.hasID3Tag;
            }
            set
            {
                this.hasID3Tag = value;
            }
        }

        public string Id3Album
        {
            get
            {
                return this.id3Album;
            }
            set
            {
                this.id3Album = value;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        public string Path
        {
            get
            {
                return this.path;
            }
            set
            {
                this.path = value;
            }
        }

        public string Id3Commnet
        {
            get
            {
                return this.id3Comment;
            }
            set
            {
                this.id3Comment = value;
            }
        }

        public string Id3Title
        {
            get
            {
                return this.id3Title;
            }
            set
            {
                this.id3Title = value;
            }

        }

        public string Id3Artist
        {
            get
            {
                return this.id3Artist;
            }
            set
            {
                this.id3Artist = value;
            }
        }

        public string Id3Year
        {
            get
            {
                return this.id3Year;
            }
            set
            {
                this.id3Year = value;
            }
        }

        public byte Id3Genre
        {
            get
            {
                return this.id3Genre;
            }
            set
            {
                this.id3Genre = value;
            }
        }

        public byte Id3TrackNumber
        {
            get
            {
                return this.id3TrackNumber;
            }
            set
            {
                this.id3TrackNumber = value;
            }
        }
    }
}
