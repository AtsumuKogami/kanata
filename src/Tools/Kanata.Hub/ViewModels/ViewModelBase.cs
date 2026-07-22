using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Kanata.Hub.ViewModels;

/// <summary>
/// Provides property change notification for Kanata Hub view models.
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Sets a backing field and raises <see cref="PropertyChanged"/> when the value changes.
    /// </summary>
    /// <typeparam name="T">The field value type.</typeparam>
    /// <param name="field">The backing field reference.</param>
    /// <param name="value">The new value.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns><see langword="true"/> when the value changed.</returns>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Raises <see cref="PropertyChanged"/> for the specified property.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
