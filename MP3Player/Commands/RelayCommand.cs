using System;
using System.Windows.Input;

namespace MP3Player.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> execute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute)
        {
            this.execute = execute;
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            execute(parameter);
        }
    }
}