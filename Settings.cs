using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CustomDataManager;

public class Settings : INotifyPropertyChanged
{
    public string SubtypeIdFilter
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string TypeIdFilter
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    // Lines that match this regex will be omitted
    public string LineFilter
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string TargetDir
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public bool ForceRestartBlockAfter
    {
        get;
        set => SetField(ref field, value);
    } = true;
    
    
    #region INPC implementation

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion
}