#region File Description
//-----------------------------------------------------------------------------
// NativeMethods.cs
//
// Copyright 2011, Nick Gravelyn.
// Licensed under the terms of the Ms-PL: http://www.microsoft.com/opensource/licenses.mspx#Ms-PL
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Xna.Framework.Graphics;
using TEditXna;

namespace TEdit.UI.Xaml.XnaContentHost
{
    /// <summary>
    /// A control that enables XNA graphics rendering inside a WPF control through
    /// the use of a hosted child Hwnd.
    /// </summary>
    public class GraphicsDeviceControl : HwndHost
    {
        #region Fields

        // The name of our window class
        private const string windowClass = "GraphicsDeviceControlHostWindowClass";

        // The HWND we present to when rendering
        private IntPtr hWnd;

        // For holding previous hWnd focus
        private IntPtr hWndprev;

        // The GraphicsDeviceService that provides and manages our GraphicsDevice
        private GraphicsDeviceService graphicsService;
        public GraphicsDeviceService GraphicsService
        {
            get { return graphicsService; }
        }

        // Track if the application has focus
        private bool applicationHasFocus = false;

        // Track if the mouse is in the window
        private bool mouseInWindow = false;

        // Track the mouse state
        private HwndMouseState mouseState = new HwndMouseState();

        // Tracking whether we've "capture" the mouse
        private bool isMouseCaptured = false;

        // The screen coordinates of the mouse when captured
        private int capturedMouseX;
        private int capturedMouseY;

        // The client coordinates of the mouse when captured
        private int capturedMouseClientX;
        private int capturedMouseClientY;

        #endregion

        #region Events

        /// <summary>
        /// Invoked when the control has initialized the GraphicsDevice.
        /// </summary>
        public event EventHandler<GraphicsDeviceEventArgs> LoadContent;

        /// <summary>
        /// Invoked when the control is ready to render XNA content
        /// </summary>
        public event EventHandler<GraphicsDeviceEventArgs> RenderXna;

        /// <summary>
        /// Invoked when the control receives a left mouse down message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndLButtonDown;

        /// <summary>
        /// Invoked when the control receives a left mouse up message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndLButtonUp;

        /// <summary>
        /// Invoked when the control receives a left mouse double click message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndLButtonDblClick;

        /// <summary>
        /// Invoked when the control receives a right mouse down message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndRButtonDown;

        /// <summary>
        /// Invoked when the control receives a right mouse up message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndRButtonUp;

        /// <summary>
        /// Invoked when the control receives a rigt mouse double click message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndRButtonDblClick;

        /// <summary>
        /// Invoked when the control receives a middle mouse down message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMButtonDown;

        /// <summary>
        /// Invoked when the control receives a middle mouse up message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMButtonUp;

        /// <summary>
        /// Invoked when the control receives a middle mouse double click message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMButtonDblClick;

        /// <summary>
        /// Invoked when the control receives a mouse down message for the first extra button.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndX1ButtonDown;

        /// <summary>
        /// Invoked when the control receives a mouse up message for the first extra button.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndX1ButtonUp;

        /// <summary>
        /// Invoked when the control receives a double click message for the first extra mouse button.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndX1ButtonDblClick;

        /// <summary>
        /// Invoked when the control receives a mouse down message for the second extra button.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndX2ButtonDown;

        /// <summary>
        /// Invoked when the control receives a mouse up message for the second extra button.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndX2ButtonUp;

        /// <summary>
        /// Invoked when the control receives a double click message for the first extra mouse button.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndX2ButtonDblClick;

        /// <summary>
        /// Invoked when the control receives a mouse move message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMouseMove;

        /// <summary>
        /// Invoked when the control first gets a mouse move message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMouseEnter;

        /// <summary>
        /// Invoked when the control gets a mouse leave message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMouseLeave;

        /// <summary>
        /// Invoked when the control recieves a mouse wheel delta.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMouseWheel;

        #endregion

        #region Construction and Disposal

        public GraphicsDeviceControl()
        {
            // We must be notified of the control finishing loading so we can get the GraphicsDeviceService
            Loaded += new RoutedEventHandler(XnaWindowHost_Loaded);

            // We must be notified of the control changing sizes so we can resize the GraphicsDeviceService
            SizeChanged += new SizeChangedEventHandler(XnaWindowHost_SizeChanged);

            // We must be notified of the application foreground status for our mouse input events
            Application.Current.Activated += new EventHandler(Current_Activated);
            Application.Current.Deactivated += new EventHandler(Current_Deactivated);

            // We use the CompositionTarget.Rendering event to trigger the control to draw itself
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        protected override void Dispose(bool disposing)
        {
            // Release our reference to the GraphicsDeviceService if we have one
            if (graphicsService != null)
            {
                graphicsService.Release(disposing);
                graphicsService = null;
            }

            // Unhook the Rendering event so we no longer attempt to draw
            CompositionTarget.Rendering -= CompositionTarget_Rendering;

            base.Dispose(disposing);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Captures the mouse, hiding it and trapping it inside the window bounds.
        /// </summary>
        /// <remarks>
        /// This method is useful for tooling scenarios where you only care about the mouse deltas
        /// and want the user to be able to continue interacting with the window while they move
        /// the mouse. A good example of this is rotating an object based on the mouse deltas where
        /// through capturing you can spin and spin without having the cursor leave the window.
        /// </remarks>
        public new void CaptureMouse()
        {
            // Don't do anything if the mouse is already captured
            if (isMouseCaptured)
                return;

            NativeMethods.ShowCursor(false);
            isMouseCaptured = true;

            // Store the current cursor position so we can reset the cursor back
            // whenever we get a move message
            NativeMethods.POINT p = new NativeMethods.POINT();
            NativeMethods.GetCursorPos(ref p);
            capturedMouseX = p.X;
            capturedMouseY = p.Y;

            // Get the client position of this point
            NativeMethods.ScreenToClient(hWnd, ref p);
            capturedMouseClientX = p.X;
            capturedMouseClientY = p.Y;
        }

        private IntPtr _cursorPtr = IntPtr.Zero;

        public void SetCursor(Cursor cursor)
        {
            // TODO: this doesn't quite work...
            Cursor = cursor;
            int cursorID = 0;
            if (Cursor == Cursors.Arrow) cursorID = NativeMethods.IDC_ARROW;
            else if (Cursor == Cursors.IBeam) cursorID = NativeMethods.IDC_IBEAM;
            else if (Cursor == Cursors.Wait) cursorID = NativeMethods.IDC_WAIT;
            else if (Cursor == Cursors.Cross) cursorID = NativeMethods.IDC_CROSS;
            else if (Cursor == Cursors.UpArrow) cursorID = NativeMethods.IDC_UPARROW;
            else if (Cursor == Cursors.SizeNWSE) cursorID = NativeMethods.IDC_SIZENWSE;
            else if (Cursor == Cursors.SizeNESW) cursorID = NativeMethods.IDC_SIZENESW;
            else if (Cursor == Cursors.SizeWE) cursorID = NativeMethods.IDC_SIZEWE;
            else if (Cursor == Cursors.SizeNS) cursorID = NativeMethods.IDC_SIZENS;
            else if (Cursor == Cursors.SizeAll) cursorID = NativeMethods.IDC_SIZEALL;
            else if (Cursor == Cursors.No) cursorID = NativeMethods.IDC_NO;
            else if (Cursor == Cursors.Hand) cursorID = NativeMethods.IDC_HAND;
            else if (Cursor == Cursors.AppStarting) cursorID = NativeMethods.IDC_APPSTARTING;
            else if (Cursor == Cursors.Help) cursorID = NativeMethods.IDC_HELP;
            else
            {
                cursorID = NativeMethods.IDC_ARROW;
            }

            IntPtr newCursor = NativeMethods.LoadCursor(IntPtr.Zero, cursorID);
            NativeMethods.SetCursor(newCursor);

            if (_cursorPtr != IntPtr.Zero)
                NativeMethods.DestroyCursor(_cursorPtr);
            _cursorPtr = newCursor;
        }

        /// <summary>
        /// Releases the capture of the mouse which makes it visible and allows it to leave the window bounds.
        /// </summary>
        public new void ReleaseMouseCapture()
        {
            // Don't do anything if the mouse isn't captured
            if (!isMouseCaptured)
                return;

            NativeMethods.ShowCursor(true);
            isMouseCaptured = false;
        }

        #endregion

        #region Graphics Device Control Implementation

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            // If we've captured the mouse, reset the cursor back to the captured position
            if (isMouseCaptured &&
                (int)mouseState.Position.X != capturedMouseX &&
                (int)mouseState.Position.Y != capturedMouseY)
            {
                NativeMethods.SetCursorPos(capturedMouseX, capturedMouseY);

                mouseState.Position = mouseState.PreviousPosition = new Point(capturedMouseClientX, capturedMouseClientY);
            }

            // If we have no graphics service, we can't draw
            if (graphicsService == null)
                return;

            // Get the current width and height of the control
            int width = (int)ActualWidth;
            int height = (int)ActualHeight;

            // If the control has no width or no height, skip drawing since it's not visible
            if (width < 1 || height < 1)
                return;

            // Create the active viewport to which we'll render our content
            Viewport viewport = new Viewport(0, 0, width, height);
            graphicsService.GraphicsDevice.Viewport = viewport;

            // Invoke the event to render this control
            if (RenderXna != null)
                RenderXna(this, new GraphicsDeviceEventArgs(graphicsService.GraphicsDevice));

            // If lost control of graphics device, skip rendering
            if (graphicsService.GraphicsDevice.GraphicsDeviceStatus == GraphicsDeviceStatus.Lost)
            {
                graphicsService.ResetDevice(width, height);
                return;
            }

            if (graphicsService.GraphicsDevice.GraphicsDeviceStatus == GraphicsDeviceStatus.NotReset)
            {
                graphicsService.ResetDevice(width, height);
                return;
            }

            // Present to the screen, but only use the visible area of the back buffer
            if (graphicsService.GraphicsDevice.GraphicsDeviceStatus == GraphicsDeviceStatus.Normal)
                GraphicsService.GraphicsDevice.Present();
            //graphicsService.GraphicsDevice.Present(viewport.Bounds, null, hWnd);
        }

        void XnaWindowHost_Loaded(object sender, RoutedEventArgs e)
        {
            InitGraphicsDevice();
        }

        bool InitGraphicsDevice()
        {
            // If we don't yet have a GraphicsDeviceService reference, we must add one for this control
            if (graphicsService == null || graphicsService.GraphicsDevice == null || graphicsService.GraphicsDevice.IsDisposed)
            {
                graphicsService = GraphicsDeviceService.AddRef(hWnd, (int)ActualWidth, (int)ActualHeight);
                // Invoke the LoadContent event
                if (LoadContent != null)
                    LoadContent(this, new GraphicsDeviceEventArgs(graphicsService.GraphicsDevice));

                return true;
            }

            return false;
        }

        void XnaWindowHost_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!InitGraphicsDevice() && graphicsService != null)
                graphicsService.ResetDevice((int)ActualWidth, (int)ActualHeight);
        }

        void Current_Activated(object sender, EventArgs e)
        {
            applicationHasFocus = true;
        }

        void Current_Deactivated(object sender, EventArgs e)
        {
            applicationHasFocus = false;
            ResetMouseState();

            if (mouseInWindow)
            {
                mouseInWindow = false;
                if (HwndMouseLeave != null)
                    HwndMouseLeave(this, new HwndMouseEventArgs(mouseState));
            }

            ReleaseMouseCapture();
        }

        private void ResetMouseState()
        {
            // We need to invoke events for any buttons that were pressed
            bool fireL = mouseState.LeftButton == MouseButtonState.Pressed;
            bool fireM = mouseState.MiddleButton == MouseButtonState.Pressed;
            bool fireR = mouseState.RightButton == MouseButtonState.Pressed;
            bool fireX1 = mouseState.X1Button == MouseButtonState.Pressed;
            bool fireX2 = mouseState.X2Button == MouseButtonState.Pressed;

            // Update the state of all of the buttons
            mouseState.LeftButton = MouseButtonState.Released;
            mouseState.MiddleButton = MouseButtonState.Released;
            mouseState.RightButton = MouseButtonState.Released;
            mouseState.X1Button = MouseButtonState.Released;
            mouseState.X2Button = MouseButtonState.Released;

            // Fire any events
            HwndMouseEventArgs args = new HwndMouseEventArgs(mouseState);
            if (fireL && HwndLButtonUp != null)
                HwndLButtonUp(this, args);
            if (fireM && HwndMButtonUp != null)
                HwndMButtonUp(this, args);
            if (fireR && HwndRButtonUp != null)
                HwndRButtonUp(this, args);
            if (fireX1 && HwndX1ButtonUp != null)
                HwndX1ButtonUp(this, args);
            if (fireX2 && HwndX2ButtonUp != null)
                HwndX2ButtonUp(this, args);
            // The mouse is no longer considered to be in our window
            mouseInWindow = false;
        }

        #endregion

        #region HWND Management

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            // Create the host window as a child of the parent
            hWnd = CreateHostWindow(hwndParent.Handle);
            return new HandleRef(this, hWnd);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            // Destroy the window and reset our hWnd value
            NativeMethods.DestroyWindow(hwnd.Handle);
            hWnd = IntPtr.Zero;
        }

        /// <summary>
        /// Creates the host window as a child of the parent window.
        /// </summary>
        private IntPtr CreateHostWindow(IntPtr hWndParent)
        {
            // Register our window class
            RegisterWindowClass();

            // Create the window
            return NativeMethods.CreateWindowEx(0, windowClass, "",
               NativeMethods.WS_CHILD | NativeMethods.WS_VISIBLE,
               0, 0, (int)Width, (int)Height, hWndParent, IntPtr.Zero, IntPtr.Zero, 0);
        }

        /// <summary>
        /// Registers the window class.
        /// </summary>
        private void RegisterWindowClass()
        {
            NativeMethods.WNDCLASSEX wndClass = new NativeMethods.WNDCLASSEX();
            wndClass.cbSize = (uint)Marshal.SizeOf(wndClass);
            wndClass.hInstance = NativeMethods.GetModuleHandle(null);
            wndClass.lpfnWndProc = NativeMethods.DefaultWindowProc;
            wndClass.lpszClassName = windowClass;
            wndClass.hCursor = NativeMethods.LoadCursor(IntPtr.Zero, NativeMethods.IDC_ARROW);

            NativeMethods.RegisterClassEx(ref wndClass);
        }

        #endregion

        #region WndProc Implementation

        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            try
            {



                switch (msg)
                {
                    case NativeMethods.WM_MOUSEWHEEL:
                        if (mouseInWindow)
                        {
                            int delta = 0;
                            
                            try { delta = wParam.ToInt32(); }
                            catch (Exception) { /*supress error*/ }

                            if (delta == 0)
                            {
                                try { delta = (int) wParam.ToInt64(); }
                                catch (Exception) { /*supress error*/ }
                            }

                            if (HwndMouseWheel != null)
                                HwndMouseWheel(this, new HwndMouseEventArgs(mouseState, delta, 0));
                        }
                        break;
                    case NativeMethods.WM_LBUTTONDOWN:
                        mouseState.LeftButton = MouseButtonState.Pressed;
                        if (HwndLButtonDown != null)
                            HwndLButtonDown(this, new HwndMouseEventArgs(mouseState));
                        break;
                    case NativeMethods.WM_LBUTTONUP:
                        mouseState.LeftButton = MouseButtonState.Released;
                        if (HwndLButtonUp != null)
                            HwndLButtonUp(this, new HwndMouseEventArgs(mouseState));
                        break;
                    case NativeMethods.WM_LBUTTONDBLCLK:
                        if (HwndLButtonDblClick != null)
                            HwndLButtonDblClick(this, new HwndMouseEventArgs(mouseState, MouseButton.Left));
                        break;
                    case NativeMethods.WM_RBUTTONDOWN:
                        mouseState.RightButton = MouseButtonState.Pressed;
                        if (HwndRButtonDown != null)
                            HwndRButtonDown(this, new HwndMouseEventArgs(mouseState));
                        break;
                    case NativeMethods.WM_RBUTTONUP:
                        mouseState.RightButton = MouseButtonState.Released;
                        if (HwndRButtonUp != null)
                            HwndRButtonUp(this, new HwndMouseEventArgs(mouseState));
                        break;
                    case NativeMethods.WM_RBUTTONDBLCLK:
                        if (HwndRButtonDblClick != null)
                            HwndRButtonDblClick(this, new HwndMouseEventArgs(mouseState, MouseButton.Right));
                        break;
                    case NativeMethods.WM_MBUTTONDOWN:
                        mouseState.MiddleButton = MouseButtonState.Pressed;
                        if (HwndMButtonDown != null)
                            HwndMButtonDown(this, new HwndMouseEventArgs(mouseState));
                        break;
                    case NativeMethods.WM_MBUTTONUP:
                        mouseState.MiddleButton = MouseButtonState.Released;
                        if (HwndMButtonUp != null)
                            HwndMButtonUp(this, new HwndMouseEventArgs(mouseState));
                        break;
                    case NativeMethods.WM_MBUTTONDBLCLK:
                        if (HwndMButtonDblClick != null)
                            HwndMButtonDblClick(this, new HwndMouseEventArgs(mouseState, MouseButton.Middle));
                        break;
                    case NativeMethods.WM_XBUTTONDOWN:
                        if (((int)wParam & NativeMethods.MK_XBUTTON1) != 0)
                        {
                            mouseState.X1Button = MouseButtonState.Pressed;
                            if (HwndX1ButtonDown != null)
                                HwndX1ButtonDown(this, new HwndMouseEventArgs(mouseState));
                        }
                        else if (((int)wParam & NativeMethods.MK_XBUTTON2) != 0)
                        {
                            mouseState.X2Button = MouseButtonState.Pressed;
                            if (HwndX2ButtonDown != null)
                                HwndX2ButtonDown(this, new HwndMouseEventArgs(mouseState));
                        }
                        break;
                    case NativeMethods.WM_XBUTTONUP:
                        if (((int)wParam & NativeMethods.MK_XBUTTON1) != 0)
                        {
                            mouseState.X1Button = MouseButtonState.Released;
                            if (HwndX1ButtonUp != null)
                                HwndX1ButtonUp(this, new HwndMouseEventArgs(mouseState));
                        }
                        else if (((int)wParam & NativeMethods.MK_XBUTTON2) != 0)
                        {
                            mouseState.X2Button = MouseButtonState.Released;
                            if (HwndX2ButtonUp != null)
                                HwndX2ButtonUp(this, new HwndMouseEventArgs(mouseState));
                        }
                        break;
                    case NativeMethods.WM_XBUTTONDBLCLK:
                        if (((int)wParam & NativeMethods.MK_XBUTTON1) != 0)
                        {
                            if (HwndX1ButtonDblClick != null)
                                HwndX1ButtonDblClick(this, new HwndMouseEventArgs(mouseState, MouseButton.XButton1));
                        }
                        else if (((int)wParam & NativeMethods.MK_XBUTTON2) != 0)
                        {
                            if (HwndX2ButtonDblClick != null)
                                HwndX2ButtonDblClick(this, new HwndMouseEventArgs(mouseState, MouseButton.XButton2));
                        }
                        break;
                    case NativeMethods.WM_MOUSEMOVE:
                        // If the application isn't in focus, we don't handle this message
                        if (!applicationHasFocus)
                            break;

                        // record the prevous and new position of the mouse
                        mouseState.PreviousPosition = mouseState.Position;
                        mouseState.Position = new Point(
                            NativeMethods.GetXLParam((int)lParam),
                            NativeMethods.GetYLParam((int)lParam));

                        if (!mouseInWindow)
                        {
                            mouseInWindow = true;

                            // if the mouse is just entering, use the same position for the previous state
                            // so we don't get weird deltas happening when the move event fires
                            mouseState.PreviousPosition = mouseState.Position;

                            if (HwndMouseEnter != null)
                                HwndMouseEnter(this, new HwndMouseEventArgs(mouseState));
                            hWndprev = NativeMethods.GetFocus();
                            NativeMethods.SetFocus(hWnd);
                            // send the track mouse event so that we get the WM_MOUSELEAVE message
                            NativeMethods.TRACKMOUSEEVENT tme = new NativeMethods.TRACKMOUSEEVENT();
                            tme.cbSize = Marshal.SizeOf(typeof(NativeMethods.TRACKMOUSEEVENT));
                            tme.dwFlags = NativeMethods.TME_LEAVE;
                            tme.hWnd = hwnd;
                            NativeMethods.TrackMouseEvent(ref tme);
                        }

                        // Only fire the mouse move if the position actually changed
                        if (mouseState.Position != mouseState.PreviousPosition)
                        {
                            if (HwndMouseMove != null)
                                HwndMouseMove(this, new HwndMouseEventArgs(mouseState));
                        }

                        break;
                    case NativeMethods.WM_MOUSELEAVE:

                        // If we have capture, we ignore this message because we're just
                        // going to reset the cursor position back into the window
                        if (isMouseCaptured)
                            break;

                        // Reset the state which releases all buttons and 
                        // marks the mouse as not being in the window.
                        ResetMouseState();

                        if (HwndMouseLeave != null)
                            HwndMouseLeave(this, new HwndMouseEventArgs(mouseState));

                        NativeMethods.SetFocus(hWndprev);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                // log this
                ErrorLogging.LogException(ex);
            }

            return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
        }



        #endregion
    }
}
