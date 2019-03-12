using MP3Player.Model;
using MP3Player.ViewModel;
using MP3Player.Views;
using System.Windows;

namespace MP3Player
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        MainWindow mainView;
        MainWindowViewModel mainViewModel;

        /// <summary>
        /// Создаёт обьекты и делает связки на старте программы.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStartup(object sender, StartupEventArgs e)
        {
            mainView = new MainWindow(); // создали View
            mainViewModel = new MainWindowViewModel(); // Создали ViewModel
            mainView.DataContext = mainViewModel; // положили ViewModel во View в качестве DataContext
            mainView.Show();
        }

        /// <summary>
        /// Серилизует список трэков и записывает в файл.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveListMP3(object sender, ExitEventArgs e)
        { 
            MP3.SerializeObject(mainViewModel.ListMP3);
        }
    }
}
