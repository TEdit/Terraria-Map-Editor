using System;
using Microsoft.Xna.Framework.Graphics;

namespace TEdit.RenderWorld
{
    public class SimpleGraphicsDeviceService : IGraphicsDeviceService
    {
        private IntPtr handle;
        private GraphicsDevice graphicsDevice = null;

        public SimpleGraphicsDeviceService(IntPtr windowHandle)
        {
            handle = windowHandle;
        }
        public GraphicsDevice GraphicsDevice
        {
            get
            {
                if (graphicsDevice == null)
                {
                    PresentationParameters parms = new PresentationParameters();
                    parms.BackBufferFormat = SurfaceFormat.Color;
                    parms.BackBufferWidth = 480;
                    parms.BackBufferHeight = 320;
                    parms.DeviceWindowHandle = handle;
                    parms.DepthStencilFormat = DepthFormat.Depth24Stencil8;
                    parms.IsFullScreen = false;
                    if (GraphicsAdapter.DefaultAdapter.IsProfileSupported(GraphicsProfile.HiDef))
                    {
                        graphicsDevice = new GraphicsDevice(
                            GraphicsAdapter.DefaultAdapter,
                            GraphicsProfile.HiDef,
                            parms);
                    }
                    else
                    {
                        graphicsDevice = new GraphicsDevice(
                            GraphicsAdapter.DefaultAdapter,
                            GraphicsProfile.Reach,
                            parms);
                    }
                    if (DeviceCreated != null)
                        DeviceCreated(this, EventArgs.Empty);
                }
                return graphicsDevice;
            }
        }
        public event EventHandler<EventArgs> DeviceCreated;
        public event EventHandler<EventArgs> DeviceDisposing;
        public event EventHandler<EventArgs> DeviceReset;
        public event EventHandler<EventArgs> DeviceResetting;
    }

    public class SimpleProvider : IServiceProvider
    {
        private IntPtr handle;
        private SimpleGraphicsDeviceService graphicsDeviceService = null;
        public SimpleProvider(IntPtr windowHandle)
        {
            handle = windowHandle;
        }
        public Object GetService(Type type)
        {
            if (type == typeof(IGraphicsDeviceService))
            {
                if (graphicsDeviceService == null)
                    graphicsDeviceService = new SimpleGraphicsDeviceService(handle);
                return graphicsDeviceService;
            }
            return null;
        }
    }
}
