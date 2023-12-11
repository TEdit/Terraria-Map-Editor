using System;
using System.Reflection;

namespace TEdit.Common.Reactive;

//
// Summary:
//     This interface is meant for the GalaSoft.MvvmLight.Helpers.WeakAction`1 class
//     and can be useful if you store multiple WeakAction{T} instances but don't know
//     in advance what type T represents.
public interface IExecuteWithObject
{
    //
    // Summary:
    //     The target of the WeakAction.
    object Target { get; }

    //
    // Summary:
    //     Executes an action.
    //
    // Parameters:
    //   parameter:
    //     A parameter passed as an object, to be casted to the appropriate type.
    void ExecuteWithObject(object parameter);

    //
    // Summary:
    //     Deletes all references, which notifies the cleanup method that this entry must
    //     be deleted.
    void MarkForDeletion();
}

//
// Summary:
//     Stores an Action without causing a hard reference to be created to the Action's
//     owner. The owner can be garbage collected at any time.
//
// Type parameters:
//   T:
//     The type of the Action's parameter.
public class WeakAction<T> : WeakAction, IExecuteWithObject
{
    private Action<T> _staticAction;

    //
    // Summary:
    //     Gets the name of the method that this WeakAction represents.
    public override string MethodName
    {
        get
        {
            if (_staticAction != null)
            {
                return _staticAction.GetMethodInfo().Name;
            }

            return base.Method.Name;
        }
    }

    //
    // Summary:
    //     Gets a value indicating whether the Action's owner is still alive, or if it was
    //     collected by the Garbage Collector already.
    public override bool IsAlive
    {
        get
        {
            if (_staticAction == null && base.Reference == null)
            {
                return false;
            }

            if (_staticAction != null)
            {
                if (base.Reference != null)
                {
                    return base.Reference.IsAlive;
                }

                return true;
            }

            return base.Reference.IsAlive;
        }
    }

    //
    // Summary:
    //     Initializes a new instance of the WeakAction class.
    //
    // Parameters:
    //   action:
    //     The action that will be associated to this instance.
    //
    //   keepTargetAlive:
    //     If true, the target of the Action will be kept as a hard reference, which might
    //     cause a memory leak. You should only set this parameter to true if the action
    //     is using closures. See http://galasoft.ch/s/mvvmweakaction.
    public WeakAction(Action<T> action, bool keepTargetAlive = false)
        : this(action?.Target, action, keepTargetAlive)
    {
    }

    //
    // Summary:
    //     Initializes a new instance of the WeakAction class.
    //
    // Parameters:
    //   target:
    //     The action's owner.
    //
    //   action:
    //     The action that will be associated to this instance.
    //
    //   keepTargetAlive:
    //     If true, the target of the Action will be kept as a hard reference, which might
    //     cause a memory leak. You should only set this parameter to true if the action
    //     is using closures. See http://galasoft.ch/s/mvvmweakaction.
    public WeakAction(object target, Action<T> action, bool keepTargetAlive = false)
    {
        if (action.GetMethodInfo().IsStatic)
        {
            _staticAction = action;
            if (target != null)
            {
                base.Reference = new WeakReference(target);
            }
        }
        else
        {
            base.Method = action.GetMethodInfo();
            base.ActionReference = new WeakReference(action.Target);
            base.LiveReference = (keepTargetAlive ? action.Target : null);
            base.Reference = new WeakReference(target);
        }
    }

    //
    // Summary:
    //     Executes the action. This only happens if the action's owner is still alive.
    //     The action's parameter is set to default(T).
    public new void Execute()
    {
        Execute(default);
    }

    //
    // Summary:
    //     Executes the action. This only happens if the action's owner is still alive.
    //
    // Parameters:
    //   parameter:
    //     A parameter to be passed to the action.
    public void Execute(T parameter)
    {
        if (_staticAction != null)
        {
            _staticAction(parameter);
            return;
        }

        object actionTarget = base.ActionTarget;
        if (IsAlive && (object)base.Method != null && (base.LiveReference != null || base.ActionReference != null) && actionTarget != null)
        {
            base.Method.Invoke(actionTarget, new object[1] { parameter });
        }
    }

    //
    // Summary:
    //     Executes the action with a parameter of type object. This parameter will be casted
    //     to T. This method implements GalaSoft.MvvmLight.Helpers.IExecuteWithObject.ExecuteWithObject(System.Object)
    //     and can be useful if you store multiple WeakAction{T} instances but don't know
    //     in advance what type T represents.
    //
    // Parameters:
    //   parameter:
    //     The parameter that will be passed to the action after being casted to T.
    public void ExecuteWithObject(object parameter)
    {
        T parameter2 = (T)parameter;
        Execute(parameter2);
    }

    //
    // Summary:
    //     Sets all the actions that this WeakAction contains to null, which is a signal
    //     for containing objects that this WeakAction should be deleted.
    public new void MarkForDeletion()
    {
        _staticAction = null;
        base.MarkForDeletion();
    }
}
//
// Summary:
//     Stores an System.Action without causing a hard reference to be created to the
//     Action's owner. The owner can be garbage collected at any time.
public class WeakAction
{
    private Action _staticAction;

    //
    // Summary:
    //     Gets or sets the System.Reflection.MethodInfo corresponding to this WeakAction's
    //     method passed in the constructor.
    protected MethodInfo Method { get; set; }

    //
    // Summary:
    //     Gets the name of the method that this WeakAction represents.
    public virtual string MethodName
    {
        get
        {
            if (_staticAction != null)
            {
                return _staticAction.GetMethodInfo().Name;
            }

            return Method.Name;
        }
    }

    //
    // Summary:
    //     Gets or sets a WeakReference to this WeakAction's action's target. This is not
    //     necessarily the same as TEdit.Common.Reactive.Helpers.WeakAction.Reference, for
    //     example if the method is anonymous.
    protected WeakReference ActionReference { get; set; }

    //
    // Summary:
    //     Saves the TEdit.Common.Reactive.Helpers.WeakAction.ActionReference as a hard reference.
    //     This is used in relation with this instance's constructor and only if the constructor's
    //     keepTargetAlive parameter is true.
    protected object LiveReference { get; set; }

    //
    // Summary:
    //     Gets or sets a WeakReference to the target passed when constructing the WeakAction.
    //     This is not necessarily the same as TEdit.Common.Reactive.Helpers.WeakAction.ActionReference,
    //     for example if the method is anonymous.
    protected WeakReference Reference { get; set; }

    //
    // Summary:
    //     Gets a value indicating whether the WeakAction is static or not.
    public bool IsStatic => _staticAction != null;

    //
    // Summary:
    //     Gets a value indicating whether the Action's owner is still alive, or if it was
    //     collected by the Garbage Collector already.
    public virtual bool IsAlive
    {
        get
        {
            if (_staticAction == null && Reference == null && LiveReference == null)
            {
                return false;
            }

            if (_staticAction != null)
            {
                if (Reference != null)
                {
                    return Reference.IsAlive;
                }

                return true;
            }

            if (LiveReference != null)
            {
                return true;
            }

            if (Reference != null)
            {
                return Reference.IsAlive;
            }

            return false;
        }
    }

    //
    // Summary:
    //     Gets the Action's owner. This object is stored as a System.WeakReference.
    public object Target
    {
        get
        {
            if (Reference == null)
            {
                return null;
            }

            return Reference.Target;
        }
    }

    //
    // Summary:
    //     The target of the weak reference.
    protected object ActionTarget
    {
        get
        {
            if (LiveReference != null)
            {
                return LiveReference;
            }

            if (ActionReference == null)
            {
                return null;
            }

            return ActionReference.Target;
        }
    }

    //
    // Summary:
    //     Initializes an empty instance of the TEdit.Common.Reactive.Helpers.WeakAction class.
    protected WeakAction()
    {
    }

    //
    // Summary:
    //     Initializes a new instance of the TEdit.Common.Reactive.Helpers.WeakAction class.
    //
    // Parameters:
    //   action:
    //     The action that will be associated to this instance.
    //
    //   keepTargetAlive:
    //     If true, the target of the Action will be kept as a hard reference, which might
    //     cause a memory leak. You should only set this parameter to true if the action
    //     is using closures. See http://galasoft.ch/s/mvvmweakaction.
    public WeakAction(Action action, bool keepTargetAlive = false)
        : this(action?.Target, action, keepTargetAlive)
    {
    }

    //
    // Summary:
    //     Initializes a new instance of the TEdit.Common.Reactive.Helpers.WeakAction class.
    //
    // Parameters:
    //   target:
    //     The action's owner.
    //
    //   action:
    //     The action that will be associated to this instance.
    //
    //   keepTargetAlive:
    //     If true, the target of the Action will be kept as a hard reference, which might
    //     cause a memory leak. You should only set this parameter to true if the action
    //     is using closures. See http://galasoft.ch/s/mvvmweakaction.
    public WeakAction(object target, Action action, bool keepTargetAlive = false)
    {
        if (action.GetMethodInfo().IsStatic)
        {
            _staticAction = action;
            if (target != null)
            {
                Reference = new WeakReference(target);
            }
        }
        else
        {
            Method = action.GetMethodInfo();
            ActionReference = new WeakReference(action.Target);
            LiveReference = (keepTargetAlive ? action.Target : null);
            Reference = new WeakReference(target);
        }
    }

    //
    // Summary:
    //     Executes the action. This only happens if the action's owner is still alive.
    public void Execute()
    {
        if (_staticAction != null)
        {
            _staticAction();
            return;
        }

        object actionTarget = ActionTarget;
        if (IsAlive && (object)Method != null && (LiveReference != null || ActionReference != null) && actionTarget != null)
        {
            Method.Invoke(actionTarget, null);
        }
    }

    //
    // Summary:
    //     Sets the reference that this instance stores to null.
    public void MarkForDeletion()
    {
        Reference = null;
        ActionReference = null;
        LiveReference = null;
        Method = null;
        _staticAction = null;
    }
}
