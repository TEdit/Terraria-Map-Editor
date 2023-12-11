using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace TEdit.Common.Reactive;

//
// Summary:
//     A base class for objects of which the properties must be observable.
public class ObservableObject : INotifyPropertyChanged
{
    //
    // Summary:
    //     Provides access to the PropertyChanged event handler to derived classes.
    protected PropertyChangedEventHandler PropertyChangedHandler => this.PropertyChanged;

    //
    // Summary:
    //     Occurs after a property value changes.
    public event PropertyChangedEventHandler PropertyChanged;

    //
    // Summary:
    //     Verifies that a property name exists in this ViewModel. This method can be called
    //     before the property is used, for instance before calling RaisePropertyChanged.
    //     It avoids errors when a property name is changed but some places are missed.
    //
    // Parameters:
    //   propertyName:
    //     The name of the property that will be checked.
    //
    // Remarks:
    //     This method is only active in DEBUG mode.
    [Conditional("DEBUG")]
    [DebuggerStepThrough]
    public void VerifyPropertyName(string propertyName)
    {
        TypeInfo typeInfo = GetType().GetTypeInfo();
        if (string.IsNullOrEmpty(propertyName) || (object)typeInfo.GetDeclaredProperty(propertyName) != null)
        {
            return;
        }

        bool flag = false;
        while ((object)typeInfo.BaseType != typeof(object))
        {
            typeInfo = typeInfo.BaseType.GetTypeInfo();
            if ((object)typeInfo.GetDeclaredProperty(propertyName) != null)
            {
                flag = true;
                break;
            }
        }

        if (!flag)
        {
            throw new ArgumentException("Property not found", propertyName);
        }
    }

    //
    // Summary:
    //     Raises the PropertyChanged event if needed.
    //
    // Parameters:
    //   propertyName:
    //     (optional) The name of the property that changed.
    //
    // Remarks:
    //     If the propertyName parameter does not correspond to an existing property on
    //     the current class, an exception is thrown in DEBUG configuration only.
    public virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    //
    // Summary:
    //     Raises the PropertyChanged event if needed.
    //
    // Parameters:
    //   propertyExpression:
    //     An expression identifying the property that changed.
    //
    // Type parameters:
    //   T:
    //     The type of the property that changed.
    public virtual void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
    {
        if (this.PropertyChanged != null)
        {
            string propertyName = GetPropertyName(propertyExpression);
            if (!string.IsNullOrEmpty(propertyName))
            {
                RaisePropertyChanged(propertyName);
            }
        }
    }

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
    public virtual void RaisePropertyChanged<T>([CallerMemberName] string propertyName = null, T oldValue = default, T newValue = default)
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
    //     Extracts the name of a property from an expression.
    //
    // Parameters:
    //   propertyExpression:
    //     An expression returning the property's name.
    //
    // Type parameters:
    //   T:
    //     The type of the property.
    //
    // Returns:
    //     The name of the property returned by the expression.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     If the expression is null.
    //
    //   T:System.ArgumentException:
    //     If the expression does not represent a property.
    protected static string GetPropertyName<T>(Expression<Func<T>> propertyExpression)
    {
        if (propertyExpression == null)
        {
            throw new ArgumentNullException("propertyExpression");
        }

        return ((((propertyExpression.Body as MemberExpression) ?? throw new ArgumentException("Invalid argument", "propertyExpression")).Member as PropertyInfo) ?? throw new ArgumentException("Argument is not a property", "propertyExpression")).Name;
    }

    //
    // Summary:
    //     Assigns a new value to the property. Then, raises the PropertyChanged event if
    //     needed.
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
    // Type parameters:
    //   T:
    //     The type of the property that changed.
    //
    // Returns:
    //     True if the PropertyChanged event has been raised, false otherwise. The event
    //     is not raised if the old value is equal to the new value.
    protected bool Set<T>(Expression<Func<T>> propertyExpression, ref T field, T newValue)
    {
        if (EqualityComparer<T>.Default.Equals(field, newValue))
        {
            return false;
        }

        field = newValue;
        RaisePropertyChanged(propertyExpression);
        return true;
    }

    //
    // Summary:
    //     Assigns a new value to the property. Then, raises the PropertyChanged event if
    //     needed.
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
    // Type parameters:
    //   T:
    //     The type of the property that changed.
    //
    // Returns:
    //     True if the PropertyChanged event has been raised, false otherwise. The event
    //     is not raised if the old value is equal to the new value.
    protected bool Set<T>(string propertyName, ref T field, T newValue)
    {
        if (EqualityComparer<T>.Default.Equals(field, newValue))
        {
            return false;
        }

        field = newValue;
        RaisePropertyChanged(propertyName);
        return true;
    }

    //
    // Summary:
    //     Assigns a new value to the property. Then, raises the PropertyChanged event if
    //     needed.
    //
    // Parameters:
    //   field:
    //     The field storing the property's value.
    //
    //   newValue:
    //     The property's value after the change occurred.
    //
    //   propertyName:
    //     (optional) The name of the property that changed.
    //
    // Type parameters:
    //   T:
    //     The type of the property that changed.
    //
    // Returns:
    //     True if the PropertyChanged event has been raised, false otherwise. The event
    //     is not raised if the old value is equal to the new value.
    protected bool Set<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
    {
        return Set(propertyName, ref field, newValue);
    }
}
