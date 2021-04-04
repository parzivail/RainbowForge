using System;
using System.Windows.Input;

namespace ForgeBrowser.Command
{
	public class DelegateCommand<T> : ICommand
	{
		private readonly Predicate<T> _canExecute;
		private readonly Action<T> _executeAction;

		public DelegateCommand(Action<T> executeAction, Predicate<T> canExecute)
		{
			_executeAction = executeAction;
			_canExecute = canExecute;
		}

		public DelegateCommand(Action<T> executeAction) : this(executeAction, null)
		{
			_executeAction = executeAction;
		}

		public event EventHandler CanExecuteChanged;

		public bool CanExecute(object parameter)
		{
			return _canExecute == null || parameter is T t && _canExecute(t);
		}

		public void Execute(object parameter)
		{
			if (parameter is T or null)
				_executeAction((T) parameter);
			CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}