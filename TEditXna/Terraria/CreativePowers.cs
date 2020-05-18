using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace TEditXNA.Terraria
{
    public class CreativePowers
    {
        public enum CreativePowerId : ushort
        {
            time_setfrozen = 0,
            // time_setdawn = 1,
            // time_setnoon = 2,
            // time_setdusk = 3,
            // time_setmidnight = 4,
            godmode = 5,
            // wind_setstrength = 6,
            // rain_setstrength = 7,
            time_setspeed = 8,
            rain_setfrozen = 9,
            wind_setfrozen = 10,
            increaseplacementrange = 11,
            setdifficulty = 12,
            biomespread_setfrozen = 13,
            setspawnrate = 14,
        }

        Dictionary<CreativePowerId, object> _powers = new Dictionary<CreativePowerId, object>();


        public void Save(BinaryWriter w)
        {
            foreach (var item in _powers)
            {
                w.Write(true);
                w.Write((ushort)item.Key);

                switch (item.Key)
                {
                    case CreativePowerId.time_setfrozen:
                         w.Write((bool)item.Value);
                        break;
                    case CreativePowerId.godmode:
                        w.Write((bool)item.Value);
                        break;
                    case CreativePowerId.time_setspeed:
                        w.Write(MathHelper.Clamp((float)item.Value, 0, 1f));
                        break;
                    case CreativePowerId.rain_setfrozen:
                        w.Write((bool)item.Value);
                        break;
                    case CreativePowerId.wind_setfrozen:
                        w.Write((bool)item.Value);
                        break;
                    case CreativePowerId.increaseplacementrange:
                        w.Write((bool)item.Value);
                        break;
                    case CreativePowerId.setdifficulty:
                        w.Write(MathHelper.Clamp((float)item.Value, 0, 1f));
                        break;
                    case CreativePowerId.biomespread_setfrozen:
                        w.Write((bool)item.Value);
                        break;
                    case CreativePowerId.setspawnrate:
                        w.Write(MathHelper.Clamp((float)item.Value, 0, 1f));
                        break;
                }
            }
            w.Write(false);
        }

        public void Load(BinaryReader r, uint WorldVersion)
        {
            while (r.ReadBoolean())
            {
                var powerId = (CreativePowerId)r.ReadInt16();

                switch (powerId)
                {
                    case CreativePowerId.time_setfrozen:
                        _powers[powerId] = r.ReadBoolean();
                        break;
                    case CreativePowerId.godmode:
                        _powers[powerId] = r.ReadBoolean();
                        break;;
                    case CreativePowerId.time_setspeed:
                        _powers[powerId] = r.ReadSingle();
                        break;
                    case CreativePowerId.rain_setfrozen:
                        _powers[powerId] = r.ReadBoolean();
                        break;
                    case CreativePowerId.wind_setfrozen:
                        _powers[powerId] = r.ReadBoolean();
                        break;
                    case CreativePowerId.increaseplacementrange:
                        _powers[powerId] = r.ReadBoolean();
                        break;
                    case CreativePowerId.setdifficulty:
                        _powers[powerId] = r.ReadSingle();
                        break;
                    case CreativePowerId.biomespread_setfrozen:
                        _powers[powerId] = r.ReadBoolean();
                        break;
                    case CreativePowerId.setspawnrate:
                        _powers[powerId] = r.ReadSingle();
                        break;
                }
            }
        }


    }
}
