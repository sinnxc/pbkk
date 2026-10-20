using System.Windows.Input;

namespace Sibumi.Core;

public class RelayCommand : ICommand
{
    readonly Action<object> _exec;
    readonly Predicate<object> _can;

    public RelayCommand(Action<object> exec, Predicate<object> can = null) { _exec = exec; _can = can; }

    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object p) => _can?.Invoke(p) ?? true;
    public void Execute(object p) => _exec(p);
}

// Sama seperti RelayCommand, tapi untuk method async dan tombol tidak bisa diklik dua kali
public class AsyncCommand : ICommand
{
    readonly Func<object, Task> _exec;
    readonly Predicate<object> _can;
    bool _running;

    public AsyncCommand(Func<object, Task> exec, Predicate<object> can = null) { _exec = exec; _can = can; }

    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object p) => !_running && (_can?.Invoke(p) ?? true);

    public async void Execute(object p)
    {
        _running = true;
        CommandManager.InvalidateRequerySuggested();
        try { await _exec(p); }
        catch (Exception ex) { ErrorHandler.Handle(ex); }
        finally
        {
            _running = false;
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
