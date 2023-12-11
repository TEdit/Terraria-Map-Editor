using System;
using System.Reflection;
using System.Threading;
using System.Windows.Input;

namespace TEdit.Common.Reactive.Command;

//
// Summary:
//     A generic command whose sole purpose is to relay its functionality to other objects
//     by invoking delegates. The default return value for the CanExecute method is
//     'true'. This class allows you to accept command parameters in the Execute and
//     CanExecute callback methods.
//
// Type parameters:
//   T:
//     The type of the command parameter.
//
// Remarks:
//     If you are using this class in WPF4.5 or above, you need to use the GalaSoft.MvvmLight.CommandWpf
//     namespace (instead of GalaSoft.MvvmLight.Command). This will enable (or restore)
//     the CommandManager class which handles automatic enabling/disabling of controls
//     based on the CanExecute delegate.
public class RelayCommand<T> : ICommand
{
    private readonly WeakAction<T> _execute;

    private readonly WeakFunc<T, bool> _canExecute;

    //
    // Summary:
    //     Occurs when changes occur that affect whether the command should execute.
    public event EventHandler CanExecuteChanged;

    //
    // Summary:
    //     Initializes a new instance of the RelayCommand class that can always execute.
    //
    // Parameters:
    //   execute:
    //     The execution logic. IMPORTANT: If the action causes a closure, you must set
    //     keepTargetAlive to true to avoid side effects.
    //
    //   keepTargetAlive:
    //     If true, the target of the Action will be kept as a hard reference, which might
    //     cause a memory leak. You should only set this parameter to true if the action
    //     is causing a closure. See http://galasoft.ch/s/mvvmweakaction.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     If the execute argument is null.
    public RelayCommand(Action<T> execute, bool keepTargetAlive = false)
        : this(execute, (Func<T, bool>)null, keepTargetAlive)
    {
    }

    //
    // Summary:
    //     Initializes a new instance of the RelayCommand class.
    //
    // Parameters:
    //   execute:
    //     The execution logic. IMPORTANT: If the action causes a closure, you must set
    //     keepTargetAlive to true to avoid side effects.
    //
    //   canExecute:
    //     The execution status logic. IMPORTANT: If the func causes a closure, you must
    //     set keepTargetAlive to true to avoid side effects.
    //
    //   keepTargetAlive:
    //     If true, the target of the Action will be kept as a hard reference, which might
    //     cause a memory leak. You should only set this parameter to true if the action
    //     is causing a closure. See http://galasoft.ch/s/mvvmweakaction.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     If the execute argument is null.
    public RelayCommand(Action<T> execute, Func<T, bool> canExecute, bool keepTargetAlive = false)
    {
        if (execute == null)
        {
            throw new ArgumentNullException("execute");
        }

        _execute = new WeakAction<T>(execute, keepTargetAlive);
        if (canExecute != null)
        {
            _canExecute = new WeakFunc<T, bool>(canExecute, keepTargetAlive);
        }
    }

    //
    // Summary:
    //     Raises the GalaSoft.MvvmLight.Command.RelayCommand`1.CanExecuteChanged event.
    public void RaiseCanExecuteChanged()
    {
        this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    //
    // Summary:
    //     Defines the method that determines whether the command can execute in its current
    //     state.
    //
    // Parameters:
    //   parameter:
    //     Data used by the command. If the command does not require data to be passed,
    //     this object can be set to a null reference
    //
    // Returns:
    //     true if this command can be executed; otherwise, false.
    public bool CanExecute(object parameter)
    {
        if (_canExecute == null)
        {
            return true;
        }

        if (_canExecute.IsStatic || _canExecute.IsAlive)
        {
            if (parameter == null && typeof(T).GetTypeInfo().IsValueType)
            {
                return _canExecute.Execute(default);
            }

            if (parameter == null || parameter is T)
            {
                return _canExecute.Execute((T)parameter);
            }
        }

        return false;
    }

    //
    // Summary:
    //     Defines the method to be called when the command is invoked.
    //
    // Parameters:
    //   parameter:
    //     Data used by the command. If the command does not require data to be passed,
    //     this object can be set to a null reference
    public virtual void Execute(object parameter)
    {
        if (!CanExecute(parameter) || _execute == null || (!_execute.IsStatic && !_execute.IsAlive))
        {
            return;
        }

        if (parameter == null)
        {
            if (typeof(T).GetTypeInfo().IsValueType)
            {
                _execute.Execute(default);
            }
            else
            {
                _execute.Execute((T)parameter);
            }
        }
        else
        {
            _execute.Execute((T)parameter);
        }
    }
}
public class RelayCommand : ICommand
{
    private readonly WeakAction _execute;

    private readonly WeakFunc<bool> _canExecute;

    private EventHandler _requerySuggestedLocal;


    //private EventHandler _canExecuteChanged;

    ///// <inheritdoc/>
    //event EventHandler ICommand.CanExecuteChanged
    //{
    //    add => _canExecuteChanged += value;
    //    remove => _canExecuteChanged -= value;
    //}

    //
    // Summary:
    //     Occurs when changes occur that affect whether the command should execute.
    public event EventHandler CanExecuteChanged
    {
        add
        {
            if (_canExecute != null)
            {
                EventHandler eventHandler = _requerySuggestedLocal;
                EventHandler eventHandler2;
                do
                {
                    eventHandler2 = eventHandler;
                    EventHandler value2 = (EventHandler)Delegate.Combine(eventHandler2, value);
                    eventHandler = Interlocked.CompareExchange(ref _requerySuggestedLocal, value2, eventHandler2);
                }
                while (eventHandler != eventHandler2);
            }
        }
        remove
        {
            if (_canExecute != null)
            {
                EventHandler eventHandler = _requerySuggestedLocal;
                EventHandler eventHandler2;
                do
                {
                    eventHandler2 = eventHandler;
                    EventHandler value2 = (EventHandler)Delegate.Remove(eventHandler2, value);
                    eventHandler = Interlocked.CompareExchange(ref _requerySuggestedLocal, value2, eventHandler2);
                }
                while (eventHandler != eventHandler2);
            }
        }
    }

    //
    // Summary:
    //     Initializes a new instance of the RelayCommand class that can always execute.
    //
    // Parameters:
    //   execute:
    //     The execution logic. IMPORTANT: If the action causes a closure, you must set
    //     keepTargetAlive to true to avoid side effects.
    //
    //   keepTargetAlive:
    //     If true, the target of the Action will be kept as a hard reference, which might
    //     cause a memory leak. You should only set this parameter to true if the action
    //     is causing a closure. See http://galasoft.ch/s/mvvmweakaction.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     If the execute argument is null.
    public RelayCommand(Action execute, bool keepTargetAlive = false)
        : this(execute, null, keepTargetAlive)
    {
    }

    //
    // Summary:
    //     Initializes a new instance of the RelayCommand class.
    //
    // Parameters:
    //   execute:
    //     The execution logic. IMPORTANT: If the action causes a closure, you must set
    //     keepTargetAlive to true to avoid side effects.
    //
    //   canExecute:
    //     The execution status logic. IMPORTANT: If the func causes a closure, you must
    //     set keepTargetAlive to true to avoid side effects.
    //
    //   keepTargetAlive:
    //     If true, the target of the Action will be kept as a hard reference, which might
    //     cause a memory leak. You should only set this parameter to true if the action
    //     is causing a closures. See http://galasoft.ch/s/mvvmweakaction.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     If the execute argument is null.
    public RelayCommand(Action execute, Func<bool> canExecute, bool keepTargetAlive = false)
    {
        if (execute == null)
        {
            throw new ArgumentNullException("execute");
        }

        _execute = new WeakAction(execute, keepTargetAlive);
        if (canExecute != null)
        {
            _canExecute = new WeakFunc<bool>(canExecute, keepTargetAlive);
        }
    }

    //
    // Summary:
    //     Defines the method that determines whether the command can execute in its current
    //     state.
    //
    // Parameters:
    //   parameter:
    //     This parameter will always be ignored.
    //
    // Returns:
    //     true if this command can be executed; otherwise, false.
    public bool CanExecute(object parameter)
    {
        if (_canExecute != null)
        {
            if (_canExecute.IsStatic || _canExecute.IsAlive)
            {
                return _canExecute.Execute();
            }

            return false;
        }

        return true;
    }

    //
    // Summary:
    //     Defines the method to be called when the command is invoked.
    //
    // Parameters:
    //   parameter:
    //     This parameter will always be ignored.
    public virtual void Execute(object parameter)
    {
        if (CanExecute(parameter) && _execute != null && (_execute.IsStatic || _execute.IsAlive))
        {
            _execute.Execute();
        }
    }
}
