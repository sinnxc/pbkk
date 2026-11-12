using System.Windows.Input;

namespace StudentManagementApp.ViewModels;

public sealed class AsyncCommand(Func<Task> execute, Func<bool> canExecute) : ICommand
{
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) => canExecute();
    public async void Execute(object? parameter) => await ExecuteAsync();
    public Task ExecuteAsync() => CanExecute(null) ? execute() : Task.CompletedTask;
    public void NotifyCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
