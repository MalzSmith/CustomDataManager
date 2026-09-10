using System.Windows.Input;

namespace CustomDataManager;

public class AsyncCommand : ICommand
{
    private readonly Func<Task> _execute;
    private readonly Action<Task> _errorHandler;

    public AsyncCommand(Func<Task> execute, Action<Task> errorHandler)
    {
        _execute = execute;
        _errorHandler = errorHandler;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter) => _execute().ContinueWith(_errorHandler);
}