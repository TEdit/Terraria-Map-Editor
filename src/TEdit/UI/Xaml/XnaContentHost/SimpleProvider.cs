//-----------------------------------------------------------------------------
// SimpleProvider.cs
//
// Copyright 2011, BinaryConstruct.
// Licensed under the terms of the Ms-PL: http://www.microsoft.com/opensource/licenses.mspx#Ms-PL
//-----------------------------------------------------------------------------

using System;
using Microsoft.Xna.Framework.Graphics;

namespace TEdit.UI.Xaml.XnaContentHost
{
    public class SimpleProvider : IServiceProvider
    {
        private readonly IGraphicsDeviceService _graphicsDeviceService;
        public SimpleProvider(IGraphicsDeviceService graphicsDeviceService)
        {
            _graphicsDeviceService = graphicsDeviceService;
        }
        public Object GetService(Type type)
        {
            if (type == typeof(IGraphicsDeviceService))
            {
                if (_graphicsDeviceService != null)
                    return _graphicsDeviceService;
            }

            return null;
        }
    }
}