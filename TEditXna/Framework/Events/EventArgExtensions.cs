using System;
using System.Threading;

namespace TEdit.Framework.Events
{
    public static class EventArgExtensions
    {
        public static void Raise<TEventArgs>(this TEventArgs e, Object sender, ref EventHandler<TEventArgs> eventDelegate)
            where TEventArgs : EventArgs
        {
            // Copy a reference to the delegate field now into a temp field for thread safety
            EventHandler<TEventArgs> temp = Interlocked.CompareExchange(ref eventDelegate, null, null);

            // If any methods registered interest with our event, notify them
            if (temp != null) temp(sender, e);
        }
    }
}