namespace TEdit.Common.Reactive;

//
// Summary:
//     This interface is meant for the GalaSoft.MvvmLight.Helpers.WeakFunc`1 class and
//     can be useful if you store multiple WeakFunc{T} instances but don't know in advance
//     what type T represents.
public interface IExecuteWithObjectAndResult
{
    //
    // Summary:
    //     Executes a Func and returns the result.
    //
    // Parameters:
    //   parameter:
    //     A parameter passed as an object, to be casted to the appropriate type.
    //
    // Returns:
    //     The result of the operation.
    object ExecuteWithObject(object parameter);
}
