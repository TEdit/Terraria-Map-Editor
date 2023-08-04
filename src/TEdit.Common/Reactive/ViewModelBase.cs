using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace TEdit.Common.Reactive;

// Summary:
//     A base class for the ViewModel classes in the MVVM pattern.
public abstract class ViewModelBase : ObservableObject
{
    //
    // Summary:
    //     Initializes a new instance of the ViewModelBase class.
    public ViewModelBase()
    {
    }

    public static bool IsInDesignModeStatic => false;

    //
    // Summary:
    //     Raises the PropertyChanged event if needed, and broadcasts a PropertyChangedMessage
    //     using the Messenger instance (or the static default instance if no Messenger
    //     instance is available).
    //
    // Parameters:
    //   propertyName:
    //     The name of the property that changed.
    //
    //   oldValue:
    //     The property's value before the change occurred.
    //
    //   newValue:
    //     The property's value after the change occurred.
    //
    //   broadcast:
    //     If true, a PropertyChangedMessage will be broadcasted. If false, only the event
    //     will be raised.
    //
    // Type parameters:
    //   T:
    //     The type of the property that changed.
    //
    // Remarks:
    //     If the propertyName parameter does not correspond to an existing property on
    //     the current class, an exception is thrown in DEBUG configuration only.
    public virtual void RaisePropertyChanged<T>([CallerMemberName] string propertyName = null, T oldValue = default(T), T newValue = default(T))
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            throw new ArgumentException("This method cannot be called with an empty string", "propertyName");
        }

        RaisePropertyChanged(propertyName);
    }

    //
    // Summary:
    //     Raises the PropertyChanged event if needed, and broadcasts a PropertyChangedMessage
    //     using the Messenger instance (or the static default instance if no Messenger
    //     instance is available).
    //
    // Parameters:
    //   propertyExpression:
    //     An expression identifying the property that changed.
    //
    //   oldValue:
    //     The property's value before the change occurred.
    //
    //   newValue:
    //     The property's value after the change occurred.
    //
    //   broadcast:
    //     If true, a PropertyChangedMessage will be broadcasted. If false, only the event
    //     will be raised.
    //
    // Type parameters:
    //   T:
    //     The type of the property that changed.
    public virtual void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression, T oldValue, T newValue)
    {
        RaisePropertyChanged(propertyExpression);
    }

    //
    // Summary:
    //     Assigns a new value to the property. Then, raises the PropertyChanged event if
    //     needed, and broadcasts a PropertyChangedMessage using the Messenger instance
    //     (or the static default instance if no Messenger instance is available).
    //
    // Parameters:
    //   propertyExpression:
    //     An expression identifying the property that changed.
    //
    //   field:
    //     The field storing the property's value.
    //
    //   newValue:
    //     The property's value after the change occurred.
    //
    //   broadcast:
    //     If true, a PropertyChangedMessage will be broadcasted. If false, only the event
    //     will be raised.
    //
    // Type parameters:
    //   T:
    //     The type of the property that changed.
    //
    // Returns:
    //     True if the PropertyChanged event was raised, false otherwise.
    protected bool Set<T>(Expression<Func<T>> propertyExpression, ref T field, T newValue)
    {
        if (EqualityComparer<T>.Default.Equals(field, newValue))
        {
            return false;
        }

        T oldValue = field;
        field = newValue;
        RaisePropertyChanged(propertyExpression, oldValue, field);
        return true;
    }

    //
    // Summary:
    //     Assigns a new value to the property. Then, raises the PropertyChanged event if
    //     needed, and broadcasts a PropertyChangedMessage using the Messenger instance
    //     (or the static default instance if no Messenger instance is available).
    //
    // Parameters:
    //   propertyName:
    //     The name of the property that changed.
    //
    //   field:
    //     The field storing the property's value.
    //
    //   newValue:
    //     The property's value after the change occurred.
    //
    //   broadcast:
    //     If true, a PropertyChangedMessage will be broadcasted. If false, only the event
    //     will be raised.
    //
    // Type parameters:
    //   T:
    //     The type of the property that changed.
    //
    // Returns:
    //     True if the PropertyChanged event was raised, false otherwise.
    protected bool Set<T>(string propertyName, ref T field, T newValue = default(T))
    {
        if (EqualityComparer<T>.Default.Equals(field, newValue))
        {
            return false;
        }

        T oldValue = field;
        field = newValue;
        RaisePropertyChanged(propertyName, oldValue, field);
        return true;
    }

    //
    // Summary:
    //     Assigns a new value to the property. Then, raises the PropertyChanged event if
    //     needed, and broadcasts a PropertyChangedMessage using the Messenger instance
    //     (or the static default instance if no Messenger instance is available).
    //
    // Parameters:
    //   field:
    //     The field storing the property's value.
    //
    //   newValue:
    //     The property's value after the change occurred.
    //
    //   broadcast:
    //     If true, a PropertyChangedMessage will be broadcasted. If false, only the event
    //     will be raised.
    //
    //   propertyName:
    //     (optional) The name of the property that changed.
    //
    // Type parameters:
    //   T:
    //     The type of the property that changed.
    //
    // Returns:
    //     True if the PropertyChanged event was raised, false otherwise.
    protected bool Set<T>(ref T field, T newValue = default(T), [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, newValue))
        {
            return false;
        }

        T oldValue = field;
        field = newValue;
        RaisePropertyChanged(propertyName, oldValue, field);
        return true;
    }
}
