using Microsoft.Win32;
using MP3Player.Commands;
using MP3Player.Model;
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
        private ObservableCollection<MP3> _listMP3 = new ObservableCollection<MP3>();
        private ObservableCollection<MP3> _mp3file = new ObservableCollection<MP3>();
        readonly WindowsMediaPlayer player = new WindowsMediaPlayer();
        private readonly DispatcherTimer _timer = new DispatcherTimer();

        private double _slidervalue;
        private double _maximumLimit;
        private double _minimumLimit;

        private string _trackName;

        private MP3 _selectedMP3;
        private bool _stopped = false;
        private bool _paused = false;
        private string _filename = string.Empty;
        private string _name = string.Empty;
        private string _time_s = string.Empty;
        private double _time = 0;
        private bool _flag = true;

        private ICommand window_About;
        private ICommand window_Closing;
        private ICommand onClickPrev;
        private ICommand onClickPlay;
        private ICommand onClickNext;
        private ICommand onClickBrowsButton;
        private ICommand menuItem_Click;
        private ICommand previewMouseLeftButtonUp;
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
                    _listMP3.Add(mp3col[i]);
                }
            }
            catch { }

            player.settings.volume = 25;
            player.PlayStateChange += OnStateChanged;
            player.PositionChange += PositionChange;
            _timer.Interval = new TimeSpan(10000000);
            _timer.Tick += OnTick;
        }
        #endregion

        #region Set/Get fields

        #region Время трэка
        /// <summary>
        /// Оставшееся время исполнения трэка.
        /// </summary>
        public string Time
            {
                get { return _time_s; }
                set
                {
                    _time_s = value;
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
                get { return player.settings.volume; }
                set
                {
                    player.settings.volume = value;
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
                get { return _maximumLimit; }
                set
                {
                    _maximumLimit = value;
                    base.RaisePropertyChangedEvent("SliderMaximum");
                }
            }

            /// <summary>
            /// Минимальное значение полосы прокрутки трэка.
            /// </summary>
            public double SliderMinimum
            {
                get { return _minimumLimit; }
                set
                {
                    _minimumLimit = value;
                    base.RaisePropertyChangedEvent("SliderMinimum");
                }
            } 

            /// <summary>
            /// Текущее значение на полосе прокрутки трэка.
            /// </summary>
            public double SliderValue
            {
                get { return _slidervalue; }
                set
                {
                    _slidervalue = value;
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
            get { return _selectedMP3; }

            set
            {
                _selectedMP3 = value;
                _stopped = true;
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
            get { return _trackName; }

            set
            {
                _trackName = value;
                RaisePropertyChangedEvent("TrackName");
            }
        }
        #endregion

        /// <summary>
        /// Полный список трэков.
        /// </summary>
        public ObservableCollection<MP3> ListMP3
        {
            get { return _listMP3; }

            set
            {
                _listMP3 = value;
                RaisePropertyChangedEvent("m_listBox");
            }
        }

        /// <summary>
        /// Выбранный трэк.                //такто нахер не надо      , попробовать поменять на   SelectedMP3
        /// </summary>
        public ObservableCollection<MP3> MP3file
        {
            get { return _mp3file; }

            set
            {
                _mp3file = value;
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
                    player.controls.currentPosition = SliderValue;
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
                    MP3.SerializeObject(_listMP3);
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
                    _listMP3.Remove(SelectedMP3);
                }));
            }
        }

        public int Minute { get; set; } = 0;
        public int Second { get; set; } = 0;

        #endregion

        #region Кнопки

        /// <summary>
        /// Обработка нажатия клавиши предыдущего трэка. 
        /// </summary>
        private void OnClickPrevButton()
        {
            _stopped = true;
            player.controls.stop();
            _stopped = false;
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
                player.URL = (previndex).Path;
                AddToListMP3();
                _time = (SelectedMP3).Duration;
                SliderMaximum = _time;
                SliderMinimum = 0;
                SliderValue = 0;
                TrackName = SelectedMP3.Name;
                player.controls.play();
                _timer.Start();
            }
        }

        /// <summary>
        /// Обработка нажатия клавиши проигрывания трэка.  // допилить обработку в соответсвии с новым интерфейсом
        /// </summary>
        private void OnClickPlayButton()
        {
            if (SelectedMP3 != null)
            {
                if (_stopped)
                {
                    player.URL = SelectedMP3.Path;
                    _time = Math.Round(SelectedMP3.Duration);
                    AddToListMP3();
                    SliderMaximum = _time;
                    SliderMinimum = 0;
                    SliderValue = 0;
                    TrackName = SelectedMP3.Name;
                    player.controls.play();
                    _stopped = false;
                    _timer.Start();
                }
                else
                {
                    _paused = !_paused;
                    if (_paused)
                    {
                        player.controls.pause();
                    }
                    else
                    {
                        player.controls.play();
                    }
                }
            }
            else
                MessageBox.Show("Выберите трек для проигрывания!", "Выбор трека", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Обработка нажатия клавиши включения следующего трэка.
        /// </summary>
        private void OnClickNextButton()
        {
            _stopped = true;
            player.controls.stop();
            _stopped = false;
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
                player.URL = (SelectedMP3).Path;
                AddToListMP3();
                _time = (SelectedMP3).Duration;
                SliderMaximum = _time;
                SliderMinimum = 0;
                SliderValue = 0;
                TrackName = SelectedMP3.Name;
                player.controls.play();
                _timer.Start();
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
                _filename = m_openfiledlg.FileName;
                _name = Path.GetFileNameWithoutExtension(_filename);
                for (int i = 0; i < _listMP3.Count; i++)
                {
                    if (_listMP3[i].Path == _filename)
                        flag = false;
                }
                if (flag)
                {
                    _listMP3.Add(TrackCreate(_filename, _name));
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
            _listMP3.Remove(ListMP3.GetEnumerator().Current);
        }
        #endregion

        #region Внутренняя логика 

        private void PositionChange(double oldPosition, double newPosition)
        {
            double dif = SliderMaximum - SliderValue;
            int m = GetMinutes(dif);
            int s = GetSeconds(dif, m);
            Time = string.Format("{0}:{1}", m, s);
        }

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
            return Minute = (int)(time / 60);
        }

        /// <summary>
        /// Вычисляет остаток за вычитом колличества полных минут.
        /// </summary>
        /// <param name="time">Колличество секунд.</param>
        /// <param name="minute">Колличество минут.</param>
        /// <returns>Колличество секунд.</returns>
        private int GetSeconds(double time, int minute)
        {
            return Second = (int)(time - minute * 60);
        }

        private void AddToListMP3()
        {
            if (MP3file.Count != 0)
                MP3file.Clear();
            _mp3file.Add(SelectedMP3);      
        }

        /// <summary>
        /// Событие отслеживающее изменение состояния воспроизведения.
        /// </summary>
        /// <param name="state">Состояние.</param>
        private void OnStateChanged(int state)
        {
            switch (state)
            {
                case 1:
                case 2:
                    _timer.Stop();
                    break;
                case 3:
                    _timer.Start();
                    break;
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

            if (!_flag)
            {
                _flag = !_flag;
                player.controls.play();
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
            file.MPtime = player.newMedia(this._filename).durationString;
            file.Duration = player.newMedia(this._filename).duration;
            GetMinutes(file.Duration);
            return file;
        }
        #endregion                                                                                                       
    }
}