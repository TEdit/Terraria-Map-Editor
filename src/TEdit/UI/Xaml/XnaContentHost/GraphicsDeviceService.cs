#region File Description
//-----------------------------------------------------------------------------
// GraphicsDeviceService.cs
//
// Copyright 2011, Nick Gravelyn.
// Licensed under the terms of the Ms-PL: http://www.microsoft.com/opensource/licenses.mspx#Ms-PL
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;

namespace TEdit.UI.Xaml.XnaContentHost;

/// <summary>
/// Helper class responsible for creating and managing the GraphicsDevice.
/// All GraphicsDeviceControl instances share the same GraphicsDeviceService,
/// so even though there can be many controls, there will only ever be a single
/// underlying GraphicsDevice. This implements the standard IGraphicsDeviceService
/// interface, which provides notification events for when the device is reset
/// or disposed.
/// </summary>
public class GraphicsDeviceService : IGraphicsDeviceService
{
    // Singleton device service instance.
    private static readonly GraphicsDeviceService singletonInstance = new GraphicsDeviceService();

    // Keep track of how many controls are sharing the singletonInstance.
    private static int referenceCount;

    private GraphicsDevice graphicsDevice;

    // Store the current device settings.
    private PresentationParameters parameters;

    /// <summary>
    /// Gets the current graphics device.
    /// </summary>
    public GraphicsDevice GraphicsDevice
    {
        get { return graphicsDevice; }
    }


    // IGraphicsDeviceService events.
    public event EventHandler<EventArgs> DeviceCreated;
    public event EventHandler<EventArgs> DeviceDisposing;
    public event EventHandler<EventArgs> DeviceReset;
    public event EventHandler<EventArgs> DeviceResetting;

    /// <summary>
    /// Constructor is private, because this is a singleton class:
    /// client controls should use the public AddRef method instead.
    /// </summary>
    private GraphicsDeviceService() { }

    private void CreateDevice(IntPtr windowHandle, int width, int height)
    {
        try
        {
            parameters = new PresentationParameters();

            parameters.BackBufferWidth = Math.Max(width, 1);
            parameters.BackBufferHeight = Math.Max(height, 1);
            parameters.BackBufferFormat = SurfaceFormat.Color;
            parameters.DepthStencilFormat = DepthFormat.Depth24;
            parameters.DeviceWindowHandle = windowHandle;
            parameters.PresentationInterval = PresentInterval.Immediate;
            parameters.IsFullScreen = false;

            graphicsDevice = new GraphicsDevice(
                GraphicsAdapter.DefaultAdapter,
                GraphicsProfile.HiDef,
                parameters);

            if (DeviceCreated != null)
                DeviceCreated(this, EventArgs.Empty);

        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to initialize GraphicsDeviceService. See inner exception for details.", ex);
        }
    }


    /// <summary>
    /// Gets a reference to the singleton instance.
    /// </summary>
    public static GraphicsDeviceService AddRef(IntPtr windowHandle, int width, int height)
    {
        // Increment the "how many controls sharing the device" reference count.
        if (Interlocked.Increment(ref referenceCount) == 1 || singletonInstance.GraphicsDevice == null || singletonInstance.GraphicsDevice.IsDisposed)
        {
            // If this is the first control to start using the
            // device, we must create the device.
            singletonInstance.CreateDevice(windowHandle, width, height);
        }

        return singletonInstance;
    }


    /// <summary>
    /// Releases a reference to the singleton instance.
    /// </summary>
    public void Release(bool disposing)
    {
        // Decrement the "how many controls sharing the device" reference count.
        if (Interlocked.Decrement(ref referenceCount) == 0)
        {
            // If this is the last control to finish using the
            // device, we should dispose the singleton instance.
            if (disposing)
            {
                if (DeviceDisposing != null)
                    DeviceDisposing(this, EventArgs.Empty);

                graphicsDevice.Dispose();
            }

            graphicsDevice = null;
        }
    }


    /// <summary>
    /// Resets the graphics device to whichever is bigger out of the specified
    /// resolution or its current size. This behavior means the device will
    /// demand-grow to the largest of all its GraphicsDeviceControl clients.
    /// </summary>
    public void ResetDevice(int width, int height)
    {
        if (DeviceResetting != null) { DeviceResetting(this, EventArgs.Empty); }

        parameters.BackBufferWidth = Math.Max(parameters.BackBufferWidth, width);
        parameters.BackBufferHeight = Math.Max(parameters.BackBufferHeight, height);

        // prevent backbuffer zero size
        if (parameters.BackBufferWidth <= 0) { parameters.BackBufferWidth = 640; }
        if (parameters.BackBufferHeight <= 0) { parameters.BackBufferHeight = 480; }

        graphicsDevice.Reset(parameters);

        if (DeviceReset != null) { DeviceReset(this, EventArgs.Empty); }
    }
}

