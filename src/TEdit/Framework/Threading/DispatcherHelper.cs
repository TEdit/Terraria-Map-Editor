// ****************************************************************************
// <copyright file="DispatcherHelper.cs" company="GalaSoft Laurent Bugnion">
// Copyright © GalaSoft Laurent Bugnion 2009-2011
// </copyright>
// ****************************************************************************
// <author>Laurent Bugnion</author>
// <email>laurent@galasoft.ch</email>
// <date>29.11.2009</date>
// <project>TEdit.Common.Reactive</project>
// <web>http://www.galasoft.ch</web>
// <license>
// See license.txt in this solution or http://www.galasoft.ch/license_MIT.txt
// </license>
// <LastBaseLevel>BL0002</LastBaseLevel>
// ****************************************************************************

using System;
using System.Text;
using System.Windows.Threading;


////using GalaSoft.Utilities.Attributes;

namespace TEdit.Framework.Threading;

//
// Summary:
//     Helper class for dispatcher operations on the UI thread.
public static class DispatcherHelper
{
    //
    // Summary:
    //     Gets a reference to the UI thread's dispatcher, after the GalaSoft.MvvmLight.Threading.DispatcherHelper.Initialize
    //     method has been called on the UI thread.
    public static Dispatcher UIDispatcher { get; private set; }

    //
    // Summary:
    //     Executes an action on the UI thread. If this method is called from the UI thread,
    //     the action is executed immendiately. If the method is called from another thread,
    //     the action will be enqueued on the UI thread's dispatcher and executed asynchronously.
    //     For additional operations on the UI thread, you can get a reference to the UI
    //     thread's dispatcher thanks to the property GalaSoft.MvvmLight.Threading.DispatcherHelper.UIDispatcher
    //     .
    //
    // Parameters:
    //   action:
    //     The action that will be executed on the UI thread.
    public static void CheckBeginInvokeOnUI(Action action)
    {
        if (action != null)
        {
            CheckDispatcher();
            if (UIDispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                UIDispatcher.BeginInvoke(action);
            }
        }
    }

    private static void CheckDispatcher()
    {
        if (UIDispatcher == null)
        {
            StringBuilder stringBuilder = new StringBuilder("The DispatcherHelper is not initialized.");
            stringBuilder.AppendLine();
            stringBuilder.Append("Call DispatcherHelper.Initialize() in the static App constructor.");
            throw new InvalidOperationException(stringBuilder.ToString());
        }
    }

    //
    // Summary:
    //     Invokes an action asynchronously on the UI thread.
    //
    // Parameters:
    //   action:
    //     The action that must be executed.
    //
    // Returns:
    //     An object, which is returned immediately after BeginInvoke is called, that can
    //     be used to interact with the delegate as it is pending execution in the event
    //     queue.
    public static DispatcherOperation RunAsync(Action action)
    {
        CheckDispatcher();
        return UIDispatcher.BeginInvoke(action);
    }

    //
    // Summary:
    //     This method should be called once on the UI thread to ensure that the GalaSoft.MvvmLight.Threading.DispatcherHelper.UIDispatcher
    //     property is initialized.
    //     In a Silverlight application, call this method in the Application_Startup event
    //     handler, after the MainPage is constructed.
    //     In WPF, call this method on the static App() constructor.
    public static void Initialize()
    {
        if (UIDispatcher == null || !UIDispatcher.Thread.IsAlive)
        {
            UIDispatcher = Dispatcher.CurrentDispatcher;
        }
    }

    //
    // Summary:
    //     Resets the class by deleting the GalaSoft.MvvmLight.Threading.DispatcherHelper.UIDispatcher
    public static void Reset()
    {
        UIDispatcher = null;
    }
}
