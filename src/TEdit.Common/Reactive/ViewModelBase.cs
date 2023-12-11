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
}
