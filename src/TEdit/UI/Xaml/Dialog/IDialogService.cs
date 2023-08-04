using System;

namespace TEdit.UI.Xaml.Dialog;

/// <summary>
/// This is a very bare bones implementation of an IDialog service.
/// There should be many more overloads for showing messages to make programming easier and to provide support for TaskDialog.
/// You should also have File Open, File Save, Folder Browswer operations.
/// </summary>    
public interface IDialogService
{
    DialogResponse ShowException(String message, DialogImage image = DialogImage.Error);
    DialogResponse ShowMessage(String message, String caption, DialogButton button, DialogImage image);
    DialogResponse ShowMessageOverlay(String message, String caption, DialogButton button, DialogImage image);
}