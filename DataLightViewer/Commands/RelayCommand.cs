using System;
using System.Windows.Input;

namespace DataLightViewer.Commands
{
    public class RelayCommand : ICommand
    {
        private Action _execute;
        private Predicate<object> _canExecute;

        public event EventHandler CanExecuteChanged = (sender, e) => { };
        public RelayCommand(Action execute, Predicate<object> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute();
    }
}
