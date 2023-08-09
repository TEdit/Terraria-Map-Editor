using System.IO;
using System.Collections.Generic;
using TEdit.Utility;

namespace TEdit.Terraria;

public enum CreativePowerId : ushort
{
    time_setfrozen = 0,
    // time_setdawn = 1,
    // time_setnoon = 2,
    // time_setdusk = 3,
    // time_setmidnight = 4,
    godmode = 5,                  // player only
                                  // wind_setstrength = 6,
                                  // rain_setstrength = 7,
    time_setspeed = 8,
    rain_setfrozen = 9,
    wind_setfrozen = 10,
    increaseplacementrange = 11, // player only
    setdifficulty = 12,
    biomespread_setfrozen = 13,
    setspawnrate = 14,
}

public class CreativePowerArgs
{
    public CreativePowerId Id { get; set; }
    public float Value { get; set; }
    public bool IsActive { get; set; }
}


public class CreativePowers
{
    Dictionary<CreativePowerId, object> _powers = new Dictionary<CreativePowerId, object>();

    public void SetPowerState(CreativePowerArgs args)
    {
        switch (args.Id)
        {
            case CreativePowerId.time_setspeed:
            case CreativePowerId.setdifficulty:
            case CreativePowerId.setspawnrate:
                _powers[args.Id] = Calc.Clamp((float)args.Value, 0, 1f);
                return;
            default:
                _powers[args.Id] = args.IsActive;
                break;
        }
    }

    public void SetPowerStateSafe(CreativePowerId id, float? value = null, bool? isEnabled = null)
    {
        switch (id)
        {
            case CreativePowerId.time_setspeed:
                if (_powers.ContainsKey(id) && value != null)
                {
                    _powers[id] = Calc.Clamp((float)value, 0, 1f);
                }
                return;
            case CreativePowerId.setdifficulty:
                if (_powers.ContainsKey(id) && value != null)
                {
                    _powers[id] = Calc.Clamp((float)value, 0, 1f);
                }
                return;
            case CreativePowerId.setspawnrate:
                if (_powers.ContainsKey(id) && value != null)
                {
                    _powers[id] = Calc.Clamp((float)value, 0, 1f);
                }
                return;
            default:
                if (_powers.ContainsKey(id) && isEnabled != null)
                {
                    _powers[id] = isEnabled.Value;
                }
                break;
        }
    }

    public void SetPowerStateSafe(CreativePowerArgs args)
    {
        switch (args.Id)
        {
            case CreativePowerId.time_setspeed:
            case CreativePowerId.setdifficulty:
            case CreativePowerId.setspawnrate:
            default:
                if (_powers.ContainsKey(args.Id))
                {
                    _powers[args.Id] = args.IsActive;
                }
                break;
        }
    }

    public bool? GetPowerBool(CreativePowerId id)
    {
        switch (id)
        {
            case CreativePowerId.time_setspeed:
            case CreativePowerId.setdifficulty:
            case CreativePowerId.setspawnrate:
                throw new System.ArgumentOutOfRangeException(nameof(id), $"Power {id.ToString()} is not type of boolean.");
            default:
                if (_powers.TryGetValue(id, out object v))
                {
                    return (bool)v;
                }
                return null;
        }
    }

    public float? GetPowerFloat(CreativePowerId id)
    {
        switch (id)
        {
            case CreativePowerId.time_setspeed:
                if (_powers.TryGetValue(id, out object t))
                {
                    return (float)t;
                }
                return null;
            case CreativePowerId.setdifficulty:
                if (_powers.TryGetValue(id, out object u))
                {
                    return (float)u;
                }
                return null;
            case CreativePowerId.setspawnrate:
                if (_powers.TryGetValue(id, out object v))
                {
                    return (float)v;
                }
                return null;
            default:
                throw new System.ArgumentOutOfRangeException(nameof(id), $"Power {id.ToString()} is not type of float.");
        }
    }

    // public bool DisablePower(CreativePowerId id) => _powers.Remove(id);

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
                    w.Write((float)item.Value);
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
                    w.Write(Calc.Clamp((float)item.Value, 0, 1f));
                    break;
                case CreativePowerId.biomespread_setfrozen:
                    w.Write((bool)item.Value);
                    break;
                case CreativePowerId.setspawnrate:
                    w.Write(Calc.Clamp((float)item.Value, 0, 1f));
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
                    break; ;
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
