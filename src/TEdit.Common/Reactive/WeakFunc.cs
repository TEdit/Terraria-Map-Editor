using System;
using System.Reflection;

namespace TEdit.Common.Reactive;


//
// Summary:
//     Stores a Func<T> without causing a hard reference to be created to the Func's
//     owner. The owner can be garbage collected at any time.
//
// Type parameters:
//   TResult:
//     The type of the result of the Func that will be stored by this weak reference.
public class WeakFunc<TResult>
{
    private Func<TResult> _staticFunc;

    //
    // Summary:
    //     Gets or sets the System.Reflection.MethodInfo corresponding to this WeakFunc's
    //     method passed in the constructor.
    protected MethodInfo Method { get; set; }

    //
    // Summary:
    //     Get a value indicating whether the WeakFunc is static or not.
    public bool IsStatic => _staticFunc != null;

    //
    // Summary:
    //     Gets the name of the method that this WeakFunc represents.
    public virtual string MethodName
    {
        get
        {
            if (_staticFunc != null)
            {
                return _staticFunc.GetMethodInfo().Name;
            }

            return Method.Name;
        }
    }

    //
    // Summary:
    //     Gets or sets a WeakReference to this WeakFunc's action's target. This is not
    //     necessarily the same as TEdit.Common.Reactive.Helpers.WeakFunc`1.Reference, for
    //     example if the method is anonymous.
    protected WeakReference FuncReference { get; set; }

    //
    // Summary:
    //     Saves the TEdit.Common.Reactive.Helpers.WeakFunc`1.FuncReference as a hard reference.
    //     This is used in relation with this instance's constructor and only if the constructor's
    //     keepTargetAlive parameter is true.
    protected object LiveReference { get; set; }

    //
    // Summary:
    //     Gets or sets a WeakReference to the target passed when constructing the WeakFunc.
    //     This is not necessarily the same as TEdit.Common.Reactive.Helpers.WeakFunc`1.FuncReference,
    //     for example if the method is anonymous.
    protected WeakReference Reference { get; set; }

    //
    // Summary:
    //     Gets a value indicating whether the Func's owner is still alive, or if it was
    //     collected by the Garbage Collector already.
    public virtual bool IsAlive
    {
        get
        {
            if (_staticFunc == null && Reference == null && LiveReference == null)
            {
                return false;
            }

            if (_staticFunc != null)
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
    //     Gets the Func's owner. This object is stored as a System.WeakReference.
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
    //     Gets the owner of the Func that was passed as parameter. This is not necessarily
    //     the same as TEdit.Common.Reactive.Helpers.WeakFunc`1.Target, for example if the
    //     method is anonymous.
    protected object FuncTarget
    {
        get
        {
            if (LiveReference != null)
            {
                return LiveReference;
            }

            if (FuncReference == null)
            {
                return null;
            }

            return FuncReference.Target;
        }
    }

    //
    // Summary:
    //     Initializes an empty instance of the WeakFunc class.
    protected WeakFunc()
    {
    }

    //
    // Summary:
    //     Initializes a new instance of the WeakFunc class.
    //
    // Parameters:
    //   func:
    //     The Func that will be associated to this instance.
    //
    //   keepTargetAlive:
    //     If true, the target of the Action will be kept as a hard reference, which might
    //     cause a memory leak. You should only set this parameter to true if the action
    //     is using closures. See http://galasoft.ch/s/mvvmweakaction.
    public WeakFunc(Func<TResult> func, bool keepTargetAlive = false)
        : this(func?.Target, func, keepTargetAlive)
    {
    }

    //
    // Summary:
    //     Initializes a new instance of the WeakFunc class.
    //
    // Parameters:
    //   target:
    //     The Func's owner.
    //
    //   func:
    //     The Func that will be associated to this instance.
    //
    //   keepTargetAlive:
    //     If true, the target of the Action will be kept as a hard reference, which might
    //     cause a memory leak. You should only set this parameter to true if the action
    //     is using closures. See http://galasoft.ch/s/mvvmweakaction.
    public WeakFunc(object target, Func<TResult> func, bool keepTargetAlive = false)
    {
        if (func.GetMethodInfo().IsStatic)
        {
            _staticFunc = func;
            if (target != null)
            {
                Reference = new WeakReference(target);
            }
        }
        else
        {
            Method = func.GetMethodInfo();
            FuncReference = new WeakReference(func.Target);
            LiveReference = (keepTargetAlive ? func.Target : null);
            Reference = new WeakReference(target);
        }
    }

    //
    // Summary:
    //     Executes the action. This only happens if the Func's owner is still alive.
    //
    // Returns:
    //     The result of the Func stored as reference.
    public TResult Execute()
    {
        if (_staticFunc != null)
        {
            return _staticFunc();
        }

        object funcTarget = FuncTarget;
        if (IsAlive && (object)Method != null && (LiveReference != null || FuncReference != null) && funcTarget != null)
        {
            return (TResult)Method.Invoke(funcTarget, null);
        }

        return default;
    }

    //
    // Summary:
    //     Sets the reference that this instance stores to null.
    public void MarkForDeletion()
    {
        Reference = null;
        FuncReference = null;
        LiveReference = null;
        Method = null;
        _staticFunc = null;
    }
}

//
// Summary:
//     Stores an Func without causing a hard reference to be created to the Func's owner.
//     The owner can be garbage collected at any time.
//
// Type parameters:
//   T:
//     The type of the Func's parameter.
//
//   TResult:
//     The type of the Func's return value.
public class WeakFunc<T, TResult> : WeakFunc<TResult>, IExecuteWithObjectAndResult
{
    private Func<T, TResult> _staticFunc;

    //
    // Summary:
    //     Gets or sets the name of the method that this WeakFunc represents.
    public override string MethodName
    {
        get
        {
            if (_staticFunc != null)
            {
                return _staticFunc.GetMethodInfo().Name;
            }

            return base.Method.Name;
        }
    }

    //
    // Summary:
    //     Gets a value indicating whether the Func's owner is still alive, or if it was
    //     collected by the Garbage Collector already.
    public override bool IsAlive
    {
        get
        {
            if (_staticFunc == null && base.Reference == null)
            {
                return false;
            }

            if (_staticFunc != null)
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
    //     Initializes a new instance of the WeakFunc class.
    //
    // Parameters:
    //   func:
    //     The Func that will be associated to this instance.
    //
    //   keepTargetAlive:
    //     If true, the target of the Action will be kept as a hard reference, which might
    //     cause a memory leak. You should only set this parameter to true if the action
    //     is using closures. See http://galasoft.ch/s/mvvmweakaction.
    public WeakFunc(Func<T, TResult> func, bool keepTargetAlive = false)
        : this(func?.Target, func, keepTargetAlive)
    {
    }

    //
    // Summary:
    //     Initializes a new instance of the WeakFunc class.
    //
    // Parameters:
    //   target:
    //     The Func's owner.
    //
    //   func:
    //     The Func that will be associated to this instance.
    //
    //   keepTargetAlive:
    //     If true, the target of the Action will be kept as a hard reference, which might
    //     cause a memory leak. You should only set this parameter to true if the action
    //     is using closures. See http://galasoft.ch/s/mvvmweakaction.
    public WeakFunc(object target, Func<T, TResult> func, bool keepTargetAlive = false)
    {
        if (func.GetMethodInfo().IsStatic)
        {
            _staticFunc = func;
            if (target != null)
            {
                base.Reference = new WeakReference(target);
            }
        }
        else
        {
            base.Method = func.GetMethodInfo();
            base.FuncReference = new WeakReference(func.Target);
            base.LiveReference = (keepTargetAlive ? func.Target : null);
            base.Reference = new WeakReference(target);
        }
    }

    //
    // Summary:
    //     Executes the Func. This only happens if the Func's owner is still alive. The
    //     Func's parameter is set to default(T).
    //
    // Returns:
    //     The result of the Func stored as reference.
    public new TResult Execute()
    {
        return Execute(default);
    }

    //
    // Summary:
    //     Executes the Func. This only happens if the Func's owner is still alive.
    //
    // Parameters:
    //   parameter:
    //     A parameter to be passed to the action.
    //
    // Returns:
    //     The result of the Func stored as reference.
    public TResult Execute(T parameter)
    {
        if (_staticFunc != null)
        {
            return _staticFunc(parameter);
        }

        object funcTarget = base.FuncTarget;
        if (IsAlive && (object)base.Method != null && (base.LiveReference != null || base.FuncReference != null) && funcTarget != null)
        {
            return (TResult)base.Method.Invoke(funcTarget, new object[1] { parameter });
        }

        return default;
    }

    //
    // Summary:
    //     Executes the Func with a parameter of type object. This parameter will be casted
    //     to T. This method implements GalaSoft.MvvmLight.Helpers.IExecuteWithObject.ExecuteWithObject(System.Object)
    //     and can be useful if you store multiple WeakFunc{T} instances but don't know
    //     in advance what type T represents.
    //
    // Parameters:
    //   parameter:
    //     The parameter that will be passed to the Func after being casted to T.
    //
    // Returns:
    //     The result of the execution as object, to be casted to T.
    public object ExecuteWithObject(object parameter)
    {
        T parameter2 = (T)parameter;
        return Execute(parameter2);
    }

    //
    // Summary:
    //     Sets all the funcs that this WeakFunc contains to null, which is a signal for
    //     containing objects that this WeakFunc should be deleted.
    public new void MarkForDeletion()
    {
        _staticFunc = null;
        base.MarkForDeletion();
    }
}
