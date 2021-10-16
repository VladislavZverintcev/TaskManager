using GalaSoft.MvvmLight.Command;
using System;
using System.Windows.Input;

namespace TaskManager.ViewModel.Commands
{
    public class VMCommands : ICommand
    {
        private Action<object> execute;
        private Func<object, bool> canExecute;

        public VMCommands(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            this.execute(parameter);
        }

        public static implicit operator RelayCommand(VMCommands v)
        {
            throw new NotImplementedException();
        }
    }
}
