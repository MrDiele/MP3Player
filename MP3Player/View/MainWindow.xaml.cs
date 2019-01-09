using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Microsoft.Win32;
using MP3Player.Model;
using MP3Player.Utility;
using WMPLib;

namespace MP3Player
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields
        private ObservableCollection<MP3> m_listVideo = new ObservableCollection<MP3>();
        private ObservableCollection<MP3> m_Videofile = new ObservableCollection<MP3>();
        private ObservableCollection<MP3> m_boxVideo = new ObservableCollection<MP3>();

        WindowsMediaPlayer Player = new WindowsMediaPlayer();
        private DispatcherTimer timer = new DispatcherTimer();

        private bool Stopped = false;
        private bool Paused = false;
        private string filename = string.Empty;
        private string name = string.Empty;
        private int minute = 0;
        private int second = 0;
        private double time = 0;
        private double Position = 0;
        private bool flag = true;

        ListViewDragDropManager<MP3> dragMgr;
        #endregion

        #region Конструктор
        public MainWindow()
        {
            InitializeComponent();

            try
            {
                ObservableCollection<MP3> mp3col = MP3.DeserializeObject();

                for (int i = 0; i < mp3col.Count; i++)
                {
                    m_listVideo.Add(mp3col[i]);
                    m_boxVideo.Add(mp3col[i]);
                }
            }
            catch { }

            m_listmp.ItemsSource = m_Videofile;
            m_listBox.ItemsSource = m_listVideo;
            Player.PlayStateChange += OnStateChanged;
            timer.Interval = new TimeSpan(10000000);
            timer.Tick += OnTick;
            m_Volume.Maximum = 40;
            m_Volume.Minimum = 0;
            m_Volume.Value = 25;
            Player.settings.volume = (int)m_Volume.Value;
            this.Loaded += Window1_Loaded;
        }
        #endregion

        #region Логика для формы
        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            this.dragMgr = new ListViewDragDropManager<MP3>(this.m_listBox);
            this.m_listBox.DragEnter += OnListViewDragEnter;
            this.m_listBox.Drop += OnListViewDrop;
        }

        void OnListViewDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
        }

        void OnListViewDrop(object sender, DragEventArgs e)
        {
            if (e.Effects == DragDropEffects.None)
                return;

            MP3 task = e.Data.GetData(typeof(MP3)) as MP3;
            if (sender == this.m_listBox)
            {
                if (this.dragMgr.IsDragInProgress)
                    return;
            }
        }
        #endregion

        #region Полоса проигрывания

        private void m_Slider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
            {
                var item = e.GetPosition(m_Slider);
                Player.controls.currentPosition = m_Slider.Value;
            }
        #endregion

        #region Громкость

        private void OnValueChange(object sender, RoutedPropertyChangedEventArgs<double> e)
            {
                Player.settings.volume = (int)m_Volume.Value;
            }
        #endregion

        #region Кнопки
        private void OnClickPrev(object sender, RoutedEventArgs e)
        {
            Stopped = true;
            Player.controls.stop();
            Stopped = false;

            var previndex = m_listBox.SelectedIndex - 1;
            if (previndex < 0)
            {
                previndex = m_listBox.Items.Count - 1;
            }

            m_listBox.SelectedIndex = previndex;
            Player.URL = (m_listBox.Items[previndex] as MP3).Path;
            AddToListMP3();
            time = (m_listBox.Items[previndex] as MP3).Duration;
            m_Slider.Maximum = time;
            m_Slider.Minimum = 0;
            m_Slider.Value = 0;
            Player.controls.play();
            timer.Start();

            AnimationMp3Name((this.m_listBox.Items[this.m_listBox.SelectedIndex] as MP3).Name);
        }

        private void OnClickButtonPlay(object sender, RoutedEventArgs e)
        {
            if (this.m_listBox.SelectedItems.Count != 0)
            {
                if (!Paused)
                {
                    Player.URL = (this.m_listBox.Items[this.m_listBox.SelectedIndex] as MP3).Path;
                    time = Math.Round((this.m_listBox.Items[this.m_listBox.SelectedIndex] as MP3).Duration);
                    AddToListMP3();
                    m_Slider.Maximum = time;
                    m_Slider.Minimum = 0;
                    m_Slider.Value = 0;
                    Player.controls.play();

                    timer.Start();

                    AnimationMp3Name((this.m_listBox.Items[this.m_listBox.SelectedIndex] as MP3).Name);
                }
                else
                    OnClickPause(this, new RoutedEventArgs());

                if (Stopped)
                    Stopped = false;
            }
            else
                MessageBox.Show("Выберите трек для проигрывания!", "Выбор трека", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnClickPause(object sender, RoutedEventArgs e)
        {
            if (!Paused)
            {
                Player.controls.pause();
                Position = Player.controls.currentPosition;
                if (Position != 0)
                    Paused = !Paused;
            }
            else
            {
                Player.controls.currentPosition = Position;

                if (Player.controls.currentPosition != 0)
                {
                    Player.controls.play();
                    timer.Start();
                    Paused = !Paused;
                }
            }
        }

        private void OnClickStop(object sender, RoutedEventArgs e)
        {
            Paused = false;
            Stopped = true;
            m_Videofile.Clear();
            Player.controls.stop();
        }

        private void OnClickNext(object sender, RoutedEventArgs e)
        {
            Stopped = true;
            Player.controls.stop();
            Stopped = false;

            var nextrack = m_listBox.SelectedIndex + 1;
            if (nextrack >= m_listBox.Items.Count)
            {
                nextrack = 0;
            }

            m_listBox.SelectedIndex = nextrack;
            Player.URL = (m_listBox.Items[nextrack] as MP3).Path;
            AddToListMP3();
            time = (m_listBox.Items[nextrack] as MP3).Duration;
            m_Slider.Maximum = time;
            m_Slider.Minimum = 0;
            m_Slider.Value = 0;
            Player.controls.play();
            timer.Start();
            AnimationMp3Name((this.m_listBox.Items[this.m_listBox.SelectedIndex] as MP3).Name);
        }

        private void OnClickBrowsButton(object sender, RoutedEventArgs e)
        {
            OpenFileDialog m_openfiledlg = new OpenFileDialog();
            m_openfiledlg.Filter = "File (*.mp3)|*.mp3";

            if (m_openfiledlg.ShowDialog() == true)
            {
                bool flag = true;
                this.filename = m_openfiledlg.FileName;
                this.name = System.IO.Path.GetFileNameWithoutExtension(this.filename);
                for (int i = 0; i < m_listVideo.Count; i++)
                {
                    if (m_listVideo[i].Path == this.filename)
                        flag = false;
                }
                if (flag)
                {
                    m_listVideo.Add(TrackCreate(this.filename, this.name));
                    m_boxVideo.Add(TrackCreate(this.filename, this.name));

                }
                else
                    MessageBox.Show("Этот трэк уже добавлен!", "Ошибка добавления", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            m_listVideo.Remove(this.m_listBox.Items[this.m_listBox.SelectedIndex] as MP3);
        }
        #endregion

        #region Внутренняя логика 

        private void Window_Closing(object sender, RoutedEventArgs e)
        {
            MP3.SerializeObject(m_listVideo);
        }

        private void Window_About(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(@"Первое пробное приложение разработанное с целью ознакомления с возможностями WPF

Автор: Александр Печенюк", "Сообщение", MessageBoxButton.OK);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MP3.SerializeObject(m_listVideo);
        }

        private void ReadMP3File(ref MP3 file, string filename)
        {
            byte[] buffer = new byte[128];

            using (FileStream fstream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                fstream.Seek(-128, SeekOrigin.End);
                fstream.Read(buffer, 0, 128);
            }

            string id3Tag = Encoding.GetEncoding(1251).GetString(buffer);

            if (id3Tag.Substring(0, 3) == "TAG")
            {
                file.Id3Album = id3Tag.Substring(63, 30).Trim();
                file.Id3Artist = id3Tag.Substring(33, 30).Trim();
                file.Id3Commnet = id3Tag.Substring(97, 28).Trim();
                file.Id3Title = id3Tag.Substring(3, 30).Trim();
                file.Id3Year = id3Tag.Substring(93, 4).Trim();

                if (id3Tag[125] == 0)
                {
                    file.Id3TrackNumber = buffer[126];
                }
                else
                {
                    file.Id3TrackNumber = 0;
                }
                file.Id3Genre = buffer[127];
                file.Genre = (Genres)file.Id3Genre;
                file.HasID3Tag = true;
            }
        }

        private void AnimationMp3Name(string name)
        {
            ThicknessAnimation ta = new ThicknessAnimation();
            ta.From = new Thickness(600, 0, 0, 0);
            ta.To = new Thickness(-600, 0, 0, 0);
            ta.Duration = TimeSpan.FromMilliseconds(6000);
            ta.RepeatBehavior = RepeatBehavior.Forever;
            //mp3_name.Content = (this.m_listBox.Items[this.m_listBox.SelectedIndex] as MP3).Name;
            //mp3_name.BeginAnimation(MarginProperty, ta);
        }

        private int GetMinutes(double time)
        {
            return minute = (int)(time / 60);
        }

        private int GetSeconds(double time, int minute)
        {
            return second = (int)(time - minute * 60);
        }

        private void AddToListMP3()
        {
            if (m_Videofile.Count != 0)
                m_Videofile.Clear();
            m_Videofile.Add(m_boxVideo[this.m_listBox.SelectedIndex]);
        }

        private void OnStateChanged(int state)
        {
            if (state == 1)
            {
                if (Stopped)
                {
                    timer.Stop();

                    m_TextBlock.Text = "";
                    m_Slider.Value = 0;
                }
                else if (state == 1 && !Stopped)
                {
                    if (flag)
                    {
                        timer.Stop();

                        var currentindex = m_listBox.SelectedIndex + 1;

                        if (currentindex >= m_listBox.Items.Count)
                        {
                            currentindex = 0;
                        }

                        m_listBox.SelectedIndex = currentindex;
                        Player.URL = (this.m_listBox.Items[currentindex] as MP3).Path;
                        AddToListMP3();
                        time = Math.Round((this.m_listBox.Items[currentindex] as MP3).Duration);
                        m_Slider.Maximum = time;
                        m_Slider.Minimum = 0;
                        m_Slider.Value = 0;

                        timer.Interval = new TimeSpan(10000000);

                        timer.Start();
                        flag = false;
                    }
                }
            }
            else if (state == 2)
            {
                timer.Stop();
            }
        }

        private void OnTick(object sender, EventArgs e)
        {
            m_Slider.Value++;
            double dif = m_Slider.Maximum - m_Slider.Value;
            int m = GetMinutes(dif);
            int s = GetSeconds(dif, m);
            m_TextBlock.Text = string.Format("{0}:{1}", m, s);

            if (!flag)
            {
                flag = !flag;
                Player.controls.play();
            }
        }

        private MP3 TrackCreate(string filename, string name)
        {
            MP3 file = new MP3();
            ReadMP3File(ref file, filename);
            file.Path = filename;
            file.Name = name;
            file.MPtime = Player.newMedia(this.filename).durationString;
            file.Duration = Player.newMedia(this.filename).duration;
            GetMinutes(file.Duration);
            return file;
        }
        #endregion
    }
}
