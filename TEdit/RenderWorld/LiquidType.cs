using System;
using System.Collections.Generic;

namespace TEdit.RenderWorld
{
    [Flags]
    public enum LiquidType
    {
        None  = 0x00,

        Water = 0x01,
        Lava  = 0x02, 
        
        All   = Water | Lava
    }
 }