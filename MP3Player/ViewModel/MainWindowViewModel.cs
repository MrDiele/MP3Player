using Microsoft.Win32;
using MP3Player.Commands;
using MP3Player.Model;
using MP3Player.Utility;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using WMPLib;

namespace MP3Player.ViewModel
{
    class MainWindowViewModel : ViewModelBase
    {
        #region Fields
        private ObservableCollection<MP3> m_listMP3 = new ObservableCollection<MP3>();
        private ObservableCollection<MP3> m_MP3file = new ObservableCollection<MP3>();

        WindowsMediaPlayer Player = new WindowsMediaPlayer();
        private DispatcherTimer timer = new DispatcherTimer();

        private double slidervalue;
        private double maximumLimit;
        private double minimumLimit;

        private string trackName;


        private MP3 selectedMP3;
        private bool Stopped = false;
        private bool Paused = false;
        private string filename = string.Empty;
        private string name = string.Empty;
        private string time_s = string.Empty;
        private int minute = 0;
        private int second = 0;
        private double time = 0;
        private double Position = 0;
        private bool flag = true;

        private ICommand window_About;
        private ICommand window_Closing;
        private ICommand onClickPrev;
        private ICommand onClickPlay;
        private ICommand onClickPause;
        private ICommand onClickStop;
        private ICommand onClickNext;
        private ICommand onClickBrowsButton;
        private ICommand menuItem_Click;
        private ICommand previewMouseLeftButtonUp;
        readonly ListViewDragDropManager<MP3> dragMgr;
        #endregion

        #region Конструктор
        /// <summary>
        /// Конструктор MainWindowViewModel.
        /// </summary>
        public MainWindowViewModel()
        {
            try
            {
                ObservableCollection<MP3> mp3col = MP3.DeserializeObject();

                for (int i = 0; i < mp3col.Count; i++)
                {
                    m_listMP3.Add(mp3col[i]);
                }
            }
            catch { }

            Player.settings.volume = 25;
            Player.PlayStateChange += OnStateChanged;
            timer.Interval = new TimeSpan(10000000);
            timer.Tick += OnTick;
            //dragMgr = new ListViewDragDropManager<MP3>(m_listMP3);
            //this.m_listBox.DragEnter += OnListViewDragEnter;
            //this.m_listBox.Drop += OnListViewDrop;

            //this.Loaded += Window1_Loaded;
        }
        #endregion

        #region Set/Get fields
           
            #region Время трэка
            /// <summary>
            /// Оставшееся время исполнения трэка.
            /// </summary>
            public string Time
            {
                get { return time_s; }
                set
                {
                    time_s = value;
                    base.RaisePropertyChangedEvent("Time");
                }
            }
            #endregion

            #region Громкость
            /// <summary>
            /// Громкость.
            /// </summary>
            public int Volume
            {
                get { return Player.settings.volume; }
                set
                {
                    Player.settings.volume = value;
                    base.RaisePropertyChangedEvent("Volume");
                }
            }
            #endregion

            #region Полоса прокрутки
            /// <summary>
            /// Максимальное значение полосы прокрутки трэка.
            /// </summary>
            public double SliderMaximum
            {
                get { return maximumLimit; }
                set
                {
                    maximumLimit = value;
                    base.RaisePropertyChangedEvent("SliderMaximum");
                }
            }

            /// <summary>
            /// Минимальное значение полосы прокрутки трэка.
            /// </summary>
            public double SliderMinimum
            {
                get { return minimumLimit; }
                set
                {
                    minimumLimit = value;
                    base.RaisePropertyChangedEvent("SliderMinimum");
                }
            } 

            /// <summary>
            /// Текущее значение на полосе прокрутки трэка.
            /// </summary>
            public double SliderValue
            {
                get { return slidervalue; }
                set
                {
                    slidervalue = value;
                    base.RaisePropertyChangedEvent("SliderValue");
                }
            }
        #endregion

            #region Выбранный трэк
        /// <summary>
        /// Выбранный трэк.
        /// </summary>
        public MP3 SelectedMP3
        {
            get { return selectedMP3; }

            set
            {
                selectedMP3 = value;
                RaisePropertyChangedEvent("SelectedMP3");
            }
        }
        #endregion

            #region Название трэка
        /// <summary>
        /// Название трэка.
        /// </summary>
        public string TrackName
        {
            get { return trackName; }

            set
            {
                trackName = value;
                RaisePropertyChangedEvent("TrackName");
            }
        }
        #endregion

        /// <summary>
        /// Полный список трэков.
        /// </summary>
        public ObservableCollection<MP3> ListMP3
        {
            get { return m_listMP3; }

            set
            {
                m_listMP3 = value;
                RaisePropertyChangedEvent("m_listBox");
            }
        }

        /// <summary>
        /// Выбранный трэк.                //такто нахер не надо      , попробовать поменять на   SelectedMP3
        /// </summary>
        public ObservableCollection<MP3> MP3file
        {
            get { return m_MP3file; }

            set
            {
                m_MP3file = value;
                RaisePropertyChangedEvent("m_mp3file");
            }
        }

        #endregion

        #region Command

        /// <summary>
        /// Команда вызова информации о программе.
        /// </summary>
        public ICommand Window_About
        {
            get
            {
                return window_About ?? (window_About = new RelayCommand((o) => 
                {
                    MessageBox.Show(@"Музыкальный проигрыватель разработан с целью ознакомления с возможностями WPF

Автор: Александр Печенюк", "Сообщение", MessageBoxButton.OK);
                }));                           
            }
        }

        /// <summary>
        /// Команда срабатывающая на перемещение ползунка проигрывания трэка
        /// </summary>
        public ICommand PreviewMouseLeftButtonUp
        {
            get
            {
                return previewMouseLeftButtonUp ?? (previewMouseLeftButtonUp = new RelayCommand((o) =>
                {
                    Player.controls.currentPosition = SliderValue;
                }));
            }
        }

        /// <summary>
        /// Команда закрытия программы.
        /// </summary>
        public ICommand Window_Closing                                                                        
        {                                                                                                                                 
            get
            {
                return window_Closing ?? (window_Closing = new RelayCommand((o) => 
                {
                    MP3.SerializeObject(m_listMP3);
                }));
            }
        }

        /// <summary>
        /// Команда на включение тредыдущего трэка.
        /// </summary>
        public ICommand OnClickPrev
        {
            get
            {
                return onClickPrev ?? (onClickPrev = new RelayCommand((o) =>
                {
                    OnClickPrevButton();
                }));
            }
        }

        /// <summary>
        /// Команда на проигрывание выбранного трэка из списка.
        /// </summary>
        public ICommand OnClickPlay
        {
            get
            {
                return onClickPlay ?? (onClickPlay = new RelayCommand((o) =>
                {
                    OnClickPlayButton();
                }));
            }
        }

        /// <summary>
        /// Команда на приостановку проигрывания выбранного трэка.
        /// </summary>
        public ICommand OnClickPause
        {
            get
            {
                return onClickPause ?? (onClickPause = new RelayCommand((o) =>
                {
                    OnClickPauseButton();
                }));
            }
        }

        /// <summary>
        /// Команда на остановку прогрывания выбранного трэка.
        /// </summary>
        public ICommand OnClickStop
        {
            get
            {
                return onClickStop ?? (onClickStop = new RelayCommand((o) => 
                {
                    OnClickStopButton();
                }));
            }
        }

        /// <summary>
        /// Команда на проигрывание следующего трэка из списка.
        /// </summary>
        public ICommand OnClickNext
        {
            get
            {
                return onClickNext ?? (onClickNext = new RelayCommand((o) =>
                {
                    OnClickNextButton();
                }));
            }
        }

        /// <summary>
        /// Команда на добавление новаого трэка в список проирывателя.
        /// </summary>
        public ICommand OnClickBrows
        {
            get
            {
                return onClickBrowsButton ?? (onClickBrowsButton = new RelayCommand((o) => 
                {
                    OnClickBrowsButton();
                }));
            }
        }

        /// <summary>
        /// Команда на удаление трэка из списка.
        /// </summary>
        public ICommand MenuItem_Click
        {
            get
            {
                return menuItem_Click ?? (menuItem_Click = new RelayCommand((o) => 
                {
                    m_listMP3.Remove(SelectedMP3);
                }));
            }
        }

        #endregion

        #region Кнопки

        /// <summary>
        /// Обработка нажатия клавиши предыдущего трэка. 
        /// </summary>
        private void OnClickPrevButton()
        {
            Stopped = true;
            Player.controls.stop();
            Stopped = false;
            MP3 previndex = null;
            try
            {
                previndex = ListMP3[(ListMP3.IndexOf(SelectedMP3) - 1)];
            }
            catch (Exception)
            {
                MessageBox.Show("Выбран первый трэк в плэйлисте", "Сообщение", MessageBoxButton.OK);
            }
            if (previndex != null)
            {
                SelectedMP3 = previndex;
                Player.URL = (previndex).Path;
                AddToListMP3();
                time = (SelectedMP3).Duration;
                SliderMaximum = time;
                SliderMinimum = 0;
                SliderValue = 0;
                TrackName = SelectedMP3.Name;
                Player.controls.play();
                timer.Start();
            }
        }

        /// <summary>
        /// Обработка нажатия клавиши проигрывания трэка.  
        /// </summary>
        private void OnClickPlayButton()
        {
            if (SelectedMP3 != null)
            {
                if (!Paused)
                {
                    Player.URL = SelectedMP3.Path;
                    time = Math.Round(SelectedMP3.Duration);
                    AddToListMP3();
                    SliderMaximum = time;
                    SliderMinimum = 0;
                    SliderValue = 0;
                    TrackName = SelectedMP3.Name;
                    Player.controls.play();

                    timer.Start();
                }
                else
                    OnClickPauseButton();

                if (Stopped)
                    Stopped = false;
            }
            else
                MessageBox.Show("Выберите трек для проигрывания!", "Выбор трека", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Обработка нажатия клавиши паузы. 
        /// </summary>
        private void OnClickPauseButton()
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

        /// <summary>
        /// Обработка нажатия клавиши стоп.
        /// </summary>
        private void OnClickStopButton()
        {
            Paused = false;
            Stopped = true;
            MP3file.Clear();
            Player.controls.stop();
        }

        /// <summary>
        /// Обработка нажатия клавиши включения следующего трэка.
        /// </summary>
        private void OnClickNextButton()
        {
            Stopped = true;
            Player.controls.stop();
            Stopped = false;
            MP3 nextrack = null;
            try
            {
                nextrack = ListMP3[(ListMP3.IndexOf(SelectedMP3) + 1)];
            }
            catch (Exception)
            {
                MessageBox.Show("Выбран последний трэк в плэйлисте", "Сообщение", MessageBoxButton.OK);
            }
            if (nextrack != null)
            {
                SelectedMP3 = nextrack;
                Player.URL = (SelectedMP3).Path;
                AddToListMP3();
                time = (SelectedMP3).Duration;
                SliderMaximum = time;
                SliderMinimum = 0;
                SliderValue = 0;
                TrackName = SelectedMP3.Name;
                Player.controls.play();
                timer.Start();
            }
        }

        /// <summary>
        /// Обработка нажатия клавиши добавления нового трэка.
        /// </summary>
        private void OnClickBrowsButton()
        {
            OpenFileDialog m_openfiledlg = new OpenFileDialog
            {
                Filter = "File (*.mp3)|*.mp3"
            };

            if (m_openfiledlg.ShowDialog() == true)
            {
                bool flag = true;
                this.filename = m_openfiledlg.FileName;
                this.name = System.IO.Path.GetFileNameWithoutExtension(this.filename);
                for (int i = 0; i < m_listMP3.Count; i++)
                {
                    if (m_listMP3[i].Path == this.filename)
                        flag = false;
                }
                if (flag)
                {
                    m_listMP3.Add(TrackCreate(this.filename, this.name));
                }
                else
                    MessageBox.Show("Этот трэк уже добавлен!", "Ошибка добавления", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Обработка нажатия клавиши удаление трэка из списка.
        /// </summary>
        private void MenuItem_ClickButton()
        {
            m_listMP3.Remove(ListMP3.GetEnumerator().Current);
        }
        #endregion

        #region Внутренняя логика 

        /// <summary>
        /// Читает данные из файла.
        /// </summary>
        /// <param name="name">Полное название файла.</param>
        /// <param name="filename">Полный путь к файлу.</param> 
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

        /// <summary>
        /// Вычисляет колличество полных минут.
        /// </summary>
        /// <param name="time">Колличество секунд.</param>
        /// <returns>Колличество минут.</returns>
        private int GetMinutes(double time)
        {
            return minute = (int)(time / 60);
        }

        /// <summary>
        /// Вычисляет остаток за вычитом колличества полных минут.
        /// </summary>
        /// <param name="time">Колличество секунд.</param>
        /// <param name="minute">Колличество минут.</param>
        /// <returns>Колличество секунд.</returns>
        private int GetSeconds(double time, int minute)
        {
            return second = (int)(time - minute * 60);
        }

        private void AddToListMP3()
        {
            if (MP3file.Count != 0)
                MP3file.Clear();
            m_MP3file.Add(SelectedMP3);      
        }

        /// <summary>
        /// Событие отслеживающее изменение состояния воспроизведения.
        /// </summary>
        /// <param name="state">Состояние.</param>
        private void OnStateChanged(int state)
        {
            if (state == 1)
            {
                if (Stopped)
                {
                    timer.Stop();

                    Time = "";
                    SliderValue = 0;
                }
                else if (state == 1 && !Stopped)
                {
                    if (flag)
                    {
                        timer.Stop();
                        MP3 nextrack = null;

                        try
                        {
                            nextrack = ListMP3[(ListMP3.IndexOf(SelectedMP3) + 1)];
                        }
                        catch (Exception) { }

                        if (nextrack != null)
                        {
                            SelectedMP3 = nextrack;
                            Player.URL = (SelectedMP3).Path;
                            AddToListMP3();
                            time = Math.Round(SelectedMP3.Duration);
                            SliderMaximum = time;
                            SliderMinimum = 0;
                            SliderValue = 0;
                            TrackName = SelectedMP3.Name;

                            timer.Interval = new TimeSpan(10000000);

                            timer.Start();
                            flag = false;
                        }
                    }
                }
            }
            else if (state == 2)
            {
                timer.Stop();
            }
        }

        /// <summary>
        /// Формирует строку остатка времени проигрывания трэка.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTick(object sender, EventArgs e)
        {
            SliderValue++;
            double dif = SliderMaximum - SliderValue;
            int m = GetMinutes(dif);
            int s = GetSeconds(dif, m);
            Time = string.Format("{0}:{1}", m, s);

            if (!flag)
            {
                flag = !flag;
                Player.controls.play();
            }
        }

        /// <summary>
        /// Создаёт новый трэк в программе
        /// </summary>
        /// <param name="filename">Полный путь к файлу.</param>
        /// <param name="name">Полное название файла.</param>
        /// <returns>Трэк</returns>
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