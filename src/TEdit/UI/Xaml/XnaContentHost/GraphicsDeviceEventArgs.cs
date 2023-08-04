#region File Description
//-----------------------------------------------------------------------------
// GraphicsDeviceEventArgs.cs
//
// Copyright 2011, Nick Gravelyn.
// Licensed under the terms of the Ms-PL: http://www.microsoft.com/opensource/licenses.mspx#Ms-PL
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework.Graphics;

namespace TEdit.UI.Xaml.XnaContentHost;

/// <summary>
/// Arguments used for GraphicsDevice related events.
/// </summary>
public class GraphicsDeviceEventArgs : EventArgs
{
    /// <summary>
    /// Gets the GraphicsDevice.
    /// </summary>
    public GraphicsDevice GraphicsDevice { get; private set; }

    /// <summary>
    /// Initializes a new GraphicsDeviceEventArgs.
    /// </summary>
    /// <param name="device">The GraphicsDevice associated with the event.</param>
    public GraphicsDeviceEventArgs(GraphicsDevice device)
    {
        GraphicsDevice = device;
    }
}
