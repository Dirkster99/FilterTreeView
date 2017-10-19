namespace FilterTreeViewRxVis.Commands
{
    using System;
    using System.Diagnostics;
    using System.Reactive.Subjects;
    using System.Windows.Input;

    public class ReactiveRelayCommand : ICommand
    {
        private readonly Action<object> execute;
        private readonly Predicate<object> canExecute;

        private readonly Subject<object> executed = new Subject<object>();

        public ReactiveRelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        public ReactiveRelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            this.execute = execute;
            this.canExecute = canExecute;
        }

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            execute(parameter);
            executed.OnNext(parameter);
        }

        public IObservable<object> Executed
        {
            get { return executed; }
        }
    }
}
