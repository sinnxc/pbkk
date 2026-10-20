using System.Windows;

namespace Sibumi.Core;

public enum ToastKind { Success, Error, Info }

public static class Toast
{
    public static event Action<string, ToastKind> Raised;

    public static void Success(string m) => Raise(m, ToastKind.Success);
    public static void Error(string m) => Raise(m, ToastKind.Error);
    public static void Info(string m) => Raise(m, ToastKind.Info);

    static void Raise(string m, ToastKind k)
    {
        var d = Application.Current?.Dispatcher;
        if (d == null || d.CheckAccess()) Raised?.Invoke(m, k);
        else d.Invoke(() => Raised?.Invoke(m, k));
    }
}