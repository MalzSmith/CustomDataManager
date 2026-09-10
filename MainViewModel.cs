using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace CustomDataManager;

public class MainViewModel : INotifyPropertyChanged
{
    public Settings Settings { get; }
    public ICommand ExportCommand { get; }
    public ICommand ImportCommand { get; }
    
    
    public MainViewModel(Settings settings, ICommand exportCommand, ICommand importCommand)
    {
        Settings = settings;
        ExportCommand = exportCommand;
        ImportCommand = importCommand;
    }

    #region INPC

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion
}