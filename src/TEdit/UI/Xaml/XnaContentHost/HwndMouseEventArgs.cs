#region File Description
//-----------------------------------------------------------------------------
// HwndMouseEventArgs.cs
//
// Copyright 2011, Nick Gravelyn.
// Licensed under the terms of the Ms-PL: http://www.microsoft.com/opensource/licenses.mspx#Ms-PL
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Windows.Input;
using System.Windows;

namespace TEdit.UI.Xaml.XnaContentHost;

/// <summary>
/// Event arguments used for mouse events in the GraphicsDeviceControl.
/// </summary>
public class HwndMouseEventArgs : EventArgs
{
    /// <summary>
    /// Gets the state of the left mouse button.
    /// </summary>
    public MouseButtonState LeftButton { get; private set; }

    /// <summary>
    /// Gets the state of the right mouse button.
    /// </summary>
    public MouseButtonState RightButton { get; private set; }

    /// <summary>
    /// Gets the state of the middle mouse button.
    /// </summary>
    public MouseButtonState MiddleButton { get; private set; }

    /// <summary>
    /// Gets the state of the first extra mouse button.
    /// </summary>
    public MouseButtonState X1Button { get; private set; }

    /// <summary>
    /// Gets the state of the second extra mouse button.
    /// </summary>
    public MouseButtonState X2Button { get; private set; }

    /// <summary>
    /// Gets the button that was double clicked.
    /// </summary>
    public MouseButton? DoubleClickButton { get; private set; }

    /// <summary>
    /// Gets the mouse wheel delta.
    /// </summary>
    public int WheelDelta { get; private set; }

    /// <summary>
    /// Gets the horizontal mouse wheel delta.
    /// </summary>
    public int HorizontalWheelDelta { get; private set; }

    /// <summary>
    /// Gets the current position of the mouse.
    /// </summary>
    public Point Position { get; private set; }

    /// <summary>
    /// Gets the previous position of the mouse.
    /// </summary>
    public Point PreviousPosition { get; private set; }

    /// <summary>
    /// Initializes a new HwndMouseEventArgs.
    /// </summary>
    /// <param name="state">The state from which to initialize the properties.</param>
    public HwndMouseEventArgs(HwndMouseState state)
    {
        LeftButton = state.LeftButton;
        RightButton = state.RightButton;
        MiddleButton = state.MiddleButton;
        X1Button = state.X1Button;
        X2Button = state.X2Button;
        Position = state.Position;
        PreviousPosition = state.PreviousPosition;
    }

    /// <summary>
    /// Initializes a new HwndMouseEventArgs.
    /// </summary>
    /// <param name="state">The state from which to initialize the properties.</param>
    /// <param name="mouseWheelDelta">The mouse wheel rotation delta.</param>
    public HwndMouseEventArgs(HwndMouseState state, int mouseWheelDelta, int mouseHWheelDelta)
        : this(state)
    {
        WheelDelta = mouseWheelDelta;
        HorizontalWheelDelta = mouseHWheelDelta;
    }
    
    /// <summary>
    /// Initializes a new HwndMouseEventArgs.
    /// </summary>
    /// <param name="state">The state from which to initialize the properties.</param>
    /// <param name="doubleClickButton">The button that was double clicked.</param>
    public HwndMouseEventArgs(HwndMouseState state, MouseButton doubleClickButton)
        : this(state)
    {
        DoubleClickButton = doubleClickButton;
    }
}
