// Copyright (c) 2011 Jeffrey Richter

using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace TEdit.Framework.Threading
{
    public sealed class OneManyLock : IDisposable
    {
        #region Lock State Management

#if false
      private struct BitField {
         private Int32 m_mask, m_1, m_startBit;
         public BitField(Int32 startBit, Int32 numBits) {
            m_startBit = startBit;
            m_mask = unchecked((Int32)((1 << numBits) - 1) << startBit);
            m_1 = unchecked((Int32)1 << startBit);
         }
         public void Increment(ref Int32 value) { value += m_1; }
         public void Decrement(ref Int32 value) { value -= m_1; }
         public void Decrement(ref Int32 value, Int32 amount) { value -= m_1 * amount; }
         public Int32 Get(Int32 value) { return (value & m_mask) >> m_startBit; }
         public Int32 Set(Int32 value, Int32 fieldValue) { return (value & ~m_mask) | (fieldValue << m_startBit); }
      }

      private static BitField s_state = new BitField(0, 3);
      private static BitField s_readersReading = new BitField(3, 9);
      private static BitField s_readersWaiting = new BitField(12, 9);
      private static BitField s_writersWaiting = new BitField(21, 9);
      private static OneManyLockStates State(Int32 value) { return (OneManyLockStates)s_state.Get(value); }
      private static void State(ref Int32 ls, OneManyLockStates newState) {
         ls = s_state.Set(ls, (Int32)newState);
      }
#endif

        private enum OneManyLockStates
        {
            Free = 0x00000000,
            OwnedByWriter = 0x00000001,
            OwnedByReaders = 0x00000002,
            OwnedByReadersAndWriterPending = 0x00000003,
            ReservedForWriter = 0x00000004,
        }

        private const Int32 c_lsStateStartBit = 0;
        private const Int32 c_lsReadersReadingStartBit = 3;
        private const Int32 c_lsReadersWaitingStartBit = 12;
        private const Int32 c_lsWritersWaitingStartBit = 21;

        // Mask = unchecked((Int32) ((1 << numBits) - 1) << startBit);
        private const Int32 c_lsStateMask = unchecked(((1 << 3) - 1) << c_lsStateStartBit);
        private const Int32 c_lsReadersReadingMask = unchecked(((1 << 9) - 1) << c_lsReadersReadingStartBit);
        private const Int32 c_lsReadersWaitingMask = unchecked(((1 << 9) - 1) << c_lsReadersWaitingStartBit);
        private const Int32 c_lsWritersWaitingMask = unchecked(((1 << 9) - 1) << c_lsWritersWaitingStartBit);

        // FirstBit = unchecked((Int32) 1 << startBit);
        private const Int32 c_ls1ReaderReading = unchecked(1 << c_lsReadersReadingStartBit);
        private const Int32 c_ls1ReaderWaiting = unchecked(1 << c_lsReadersWaitingStartBit);
        private const Int32 c_ls1WriterWaiting = unchecked(1 << c_lsWritersWaitingStartBit);

        private static OneManyLockStates State(Int32 ls)
        {
            return (OneManyLockStates) (ls & c_lsStateMask);
        }

        private static void State(ref Int32 ls, OneManyLockStates newState)
        {
            ls = (ls & ~c_lsStateMask) | ((Int32) newState);
        }

        private static Int32 NumReadersReading(Int32 ls)
        {
            return (ls & c_lsReadersReadingMask) >> c_lsReadersReadingStartBit;
        }

        private static void IncReadersReading(ref Int32 ls)
        {
            ls += c_ls1ReaderReading;
        }

        private static void DecReadersReading(ref Int32 ls)
        {
            ls -= c_ls1ReaderReading;
        }

        private static Int32 NumReadersWaiting(Int32 ls)
        {
            return (ls & c_lsReadersWaitingMask) >> c_lsReadersWaitingStartBit;
        }

        private static void IncReadersWaiting(ref Int32 ls)
        {
            ls += c_ls1ReaderWaiting;
        }

        private static void DecReadersWaiting(ref Int32 ls, Int32 amount)
        {
            ls -= c_ls1ReaderWaiting*amount;
        }

        private static Int32 NumWritersWaiting(Int32 ls)
        {
            return (ls & c_lsWritersWaitingMask) >> c_lsWritersWaitingStartBit;
        }

        private static void IncWritersWaiting(ref Int32 ls)
        {
            ls += c_ls1WriterWaiting;
        }

        private static void DecWritersWaiting(ref Int32 ls)
        {
            ls -= c_ls1WriterWaiting;
        }

        private enum WakeUp
        {
            None,
            Writer,
            Readers
        }

        private Int32 NumWritersToWake()
        {
            Int32 ls = m_LockState;

            // If lock is RFW && WW>0, try to subtract 1 writer
            while ((State(ls) == OneManyLockStates.ReservedForWriter) && (NumWritersWaiting(ls) > 0))
            {
                Int32 desired = ls;
                DecWritersWaiting(ref desired);
                if (InterlockedEx.IfThen(ref m_LockState, ls, desired, out ls))
                {
                    // We sucessfully subtracted 1 waiting writer, wake it up
                    return 1;
                }
            }
            return 0;
        }

        private static class InterlockedEx
        {
            public static Boolean IfThen(ref Int32 value, Int32 @if, Int32 then, out Int32 previousValue)
            {
                previousValue = Interlocked.CompareExchange(ref value, then, @if);
                return (previousValue == @if);
            }
        }

        private Int32 NumReadersToWake()
        {
            Int32 ls = m_LockState, numReadersWaiting;

            // If lock is Free && RW>0, try to subtract all readers
            while ((State(ls) == OneManyLockStates.Free) && ((numReadersWaiting = NumReadersWaiting(ls)) > 0))
            {
                Int32 desired = ls;
                DecReadersWaiting(ref desired, numReadersWaiting);
                if (InterlockedEx.IfThen(ref m_LockState, ls, desired, out ls))
                {
                    // We sucessfully subtracted all waiting readers, wake them up
                    return numReadersWaiting;
                }
            }
            return 0;
        }

        /// <summary>
        /// Returns a string representing the state of the object.
        /// </summary>
        /// <returns>The string representing the state of the object.</returns>
        public override string ToString()
        {
            Int32 ls = m_LockState;
            return String.Format(CultureInfo.InvariantCulture,
                                 "State={0}, RR={1}, RW={2}, WW={3}", State(ls),
                                 NumReadersReading(ls), NumReadersWaiting(ls),
                                 NumWritersWaiting(ls));
        }

        #endregion

        #region State Fields

        private Int32 m_LockState = (Int32) OneManyLockStates.Free;

        // Readers wait on this if a writer owns the lock
        private Semaphore m_ReadersLock = new Semaphore(0, Int32.MaxValue);

        // Writers wait on this if a reader owns the lock
        private AutoResetEvent m_WritersLock = new AutoResetEvent(false);

        #endregion

        #region Construction and Dispose

        /// <summary>Allow the object to clean itself up.</summary>
        /// <param name="disposing">true if the object is being disposed; false if it is being finalzied.</param>
        public void Dispose()
        {
            m_WritersLock.Close();
            m_WritersLock = null;
            m_ReadersLock.Close();
            m_ReadersLock = null;
        }

        #endregion

        #region Writer members

        /// <summary>
        /// Implements the ResourceLock's WaitToWrite behavior.
        /// </summary>
        public void Enter(Boolean exclusive)
        {
            if (exclusive)
            {
                while (WaitToWrite(ref m_LockState)) m_WritersLock.WaitOne();
            }
            else
            {
                while (WaitToRead(ref m_LockState)) m_ReadersLock.WaitOne();
            }
        }

        private static Boolean WaitToWrite(ref Int32 target)
        {
            Int32 i, j = target;
            Boolean wait;
            do
            {
                i = j;
                Int32 desired = i;
                wait = false;

                switch (State(desired))
                {
                    case OneManyLockStates.Free: // If Free -> OBW, return
                    case OneManyLockStates.ReservedForWriter: // If RFW -> OBW, return
                        State(ref desired, OneManyLockStates.OwnedByWriter);
                        break;

                    case OneManyLockStates.OwnedByWriter: // If OBW -> WW++, wait & loop around
                        IncWritersWaiting(ref desired);
                        wait = true;
                        break;

                    case OneManyLockStates.OwnedByReaders: // If OBR or OBRAWP -> OBRAWP, WW++, wait, loop around
                    case OneManyLockStates.OwnedByReadersAndWriterPending:
                        State(ref desired, OneManyLockStates.OwnedByReadersAndWriterPending);
                        IncWritersWaiting(ref desired);
                        wait = true;
                        break;
                    default:
                        Debug.Assert(false, "Invalid Lock state");
                        break;
                }
                j = Interlocked.CompareExchange(ref target, desired, i);
            } while (i != j);
            return wait;
        }

        /// <summary>
        /// Implements the ResourceLock's OnDone behavior.
        /// </summary>
        public void Leave(Boolean write)
        {
            if (write)
            {
                Debug.Assert((State(m_LockState) == OneManyLockStates.OwnedByWriter) && (NumReadersReading(m_LockState) == 0));
                // Pre-condition:  Lock's state must be OBW (not Free/OBR/OBRAWP/RFW)
                // Post-condition: Lock's state must become Free or RFW (the lock is never passed)

                // Phase 1: Release the lock
                WakeUp wakeup = DoneWriting(ref m_LockState);

                // Phase 2: Possibly wake waiters
                switch (wakeup)
                {
                    case WakeUp.None:
                        break;
                    case WakeUp.Readers:
                        Int32 numReadersToWake = NumReadersToWake();
                        if (numReadersToWake > 0) m_ReadersLock.Release(numReadersToWake);
                        break;
                    case WakeUp.Writer:
                        Int32 numWritersToWake = NumWritersToWake();
                        Debug.Assert(numWritersToWake < 2);
                        if (numWritersToWake > 0) m_WritersLock.Set();
                        break;
                }
            }
            else
            {
                Debug.Assert((State(m_LockState) == OneManyLockStates.OwnedByReaders) || (State(m_LockState) == OneManyLockStates.OwnedByReadersAndWriterPending));
                // Pre-condition:  Lock's state must be OBR/OBRAWP (not Free/OBW/RFW)
                // Post-condition: Lock's state must become unchanged, Free or RFW (the lock is never passed)

                // Phase 1: Release the lock
                WakeUp wakeup = DoneReading(ref m_LockState);

                // Phase 2: Possibly wake a waiting writer
                switch (wakeup)
                {
                    case WakeUp.None:
                        break;
                    case WakeUp.Readers:
                        Debug.Assert(false);
                        break;
                    case WakeUp.Writer:
                        Int32 numWritersToWake = NumWritersToWake();
                        Debug.Assert(numWritersToWake < 2); // Must be 0 or 1
                        if (numWritersToWake > 0) m_WritersLock.Set();
                        break;
                }
            }
        }

        private static WakeUp DoneWriting(ref Int32 target)
        {
            Int32 i, j = target;
            WakeUp wakeup = WakeUp.None;
            do
            {
                i = j;
                Int32 desired = i;

                // if WW=0 && RW=0 -> Free
                if ((NumWritersWaiting(desired) == 0) && (NumReadersWaiting(desired) == 0))
                {
                    State(ref desired, OneManyLockStates.Free);
                }
                else
                {
                    // if WW>0 && RW=0 -> RFW, possibly release a writer
                    if ((NumWritersWaiting(desired) > 0) && (NumReadersWaiting(desired) == 0))
                    {
                        State(ref desired, OneManyLockStates.ReservedForWriter);
                        wakeup = WakeUp.Writer;
                    }
                    else
                    {
                        // if WW>0 && RW>0 -> RFW, possibly release a writer
                        if ((NumWritersWaiting(desired) > 0) && (NumReadersWaiting(desired) > 0))
                        {
                            // JMR: Merge with above - is this even possible?
                            State(ref desired, OneManyLockStates.ReservedForWriter);
                            wakeup = WakeUp.Writer;
                        }
                        else
                        {
                            // if WW=0 && RW>0 -> Free, possibly release readers
                            if ((NumWritersWaiting(desired) == 0) && (NumReadersWaiting(desired) > 0))
                            {
                                State(ref desired, OneManyLockStates.Free);
                                wakeup = WakeUp.Readers;
                            }
                            else
                            {
                                Debug.Assert(false, "Invalid Lock state");
                            }
                        }
                    }
                }
                j = Interlocked.CompareExchange(ref target, desired, i);
            } while (i != j);
            return wakeup;
        }

        #endregion

        #region Reader members

        private static Boolean WaitToRead(ref Int32 target)
        {
            Int32 i, j = target;
            Boolean wait;
            do
            {
                i = j;
                Int32 desired = i;
                wait = false;

                switch (State(desired))
                {
                    case OneManyLockStates.Free: // If Free->OBR, RR=1, return
                        State(ref desired, OneManyLockStates.OwnedByReaders);
                        IncReadersReading(ref desired);
                        break;

                    case OneManyLockStates.OwnedByReaders: // If OBR -> RR++, return
                        IncReadersReading(ref desired);
                        break;

                    case OneManyLockStates.OwnedByWriter: // If OBW/OBRAWP/RFW -> RW++, wait, loop around
                    case OneManyLockStates.OwnedByReadersAndWriterPending:
                    case OneManyLockStates.ReservedForWriter:
                        IncReadersWaiting(ref desired);
                        wait = true;
                        break;

                    default:
                        Debug.Assert(false, "Invalid Lock state");
                        break;
                }
                j = Interlocked.CompareExchange(ref target, desired, i);
            } while (i != j);
            return wait;
        }

        private static WakeUp DoneReading(ref Int32 target)
        {
            Int32 i, j = target;
            WakeUp wakeup = WakeUp.None;
            do
            {
                i = j;
                Int32 desired = i;
                DecReadersReading(ref desired); // RR--

                // if RR>0 -> readers still reading, return
                if (NumReadersReading(desired) > 0)
                {
                }
                else
                {
                    // No more readers reading
                    // if WW>0 -> RFW, return
                    if (NumWritersWaiting(desired) > 0)
                    {
                        State(ref desired, OneManyLockStates.ReservedForWriter);
                        wakeup = WakeUp.Writer;
                    }
                    else
                    {
                        // All readers left and No waiting writers
                        State(ref desired, OneManyLockStates.Free);
                    }
                }
                j = Interlocked.CompareExchange(ref target, desired, i);
            } while (i != j);
            return wakeup;
        }

        #endregion
    }
}