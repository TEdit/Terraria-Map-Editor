using ColorMine.ColorSpaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using TEdit.Geometry;
using TEdit.Utility;

namespace TEdit.Common;

//
// Summary:
//     Describes a 32-bit packed color.
[DataContract]
[DebuggerDisplay("{DebugDisplayString,nq}")]
public struct TEditColor : IEquatable<TEditColor>
{
    public static TEditColor FromString(string hex)
    {
        var rgba = Convert.ToUInt32(hex.Substring(1), 16);

        float a = ((int)((rgba & 0xff000000) >> 24) / 255.0f);
        float r = ((int)((rgba & 0x00ff0000) >> 16) / 255.0f);
        float g = ((int)((rgba & 0x0000ff00) >> 8) / 255.0f);
        float b = ((int)(rgba & 0x000000ff) / 255.0f);

        return new TEditColor(r, g, b, a);
    }

    public static string ToHexString(TEditColor color) => $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

    private uint _packedValue;


    //
    // Summary:
    //     Gets or sets the blue component.
    [DataMember]
    public byte B
    {
        get
        {
            return (byte)(_packedValue >> 16);
        }
        set
        {
            _packedValue = (_packedValue & 0xFF00FFFFu) | (uint)(value << 16);
        }
    }

    //
    // Summary:
    //     Gets or sets the green component.
    [DataMember]
    public byte G
    {
        get
        {
            return (byte)(_packedValue >> 8);
        }
        set
        {
            _packedValue = (_packedValue & 0xFFFF00FFu) | (uint)(value << 8);
        }
    }

    //
    // Summary:
    //     Gets or sets the red component.
    [DataMember]
    public byte R
    {
        get
        {
            return (byte)_packedValue;
        }
        set
        {
            _packedValue = (_packedValue & 0xFFFFFF00u) | value;
        }
    }

    //
    // Summary:
    //     Gets or sets the alpha component.
    [DataMember]
    public byte A
    {
        get
        {
            return (byte)(_packedValue >> 24);
        }
        set
        {
            _packedValue = (_packedValue & 0xFFFFFFu) | (uint)(value << 24);
        }
    }

    //
    // Summary:
    //     TransparentBlack color (R:0,G:0,B:0,A:0).
    [Obsolete("Use Color.Transparent instead. In future versions this method can be removed.")]
    public static TEditColor TransparentBlack { get; private set; }

    //
    // Summary:
    //     Transparent color (R:0,G:0,B:0,A:0).
    public static TEditColor Transparent { get; private set; }

    //
    // Summary:
    //     AliceBlue color (R:240,G:248,B:255,A:255).
    public static TEditColor AliceBlue { get; private set; }

    //
    // Summary:
    //     AntiqueWhite color (R:250,G:235,B:215,A:255).
    public static TEditColor AntiqueWhite { get; private set; }

    //
    // Summary:
    //     Aqua color (R:0,G:255,B:255,A:255).
    public static TEditColor Aqua { get; private set; }

    //
    // Summary:
    //     Aquamarine color (R:127,G:255,B:212,A:255).
    public static TEditColor Aquamarine { get; private set; }

    //
    // Summary:
    //     Azure color (R:240,G:255,B:255,A:255).
    public static TEditColor Azure { get; private set; }

    //
    // Summary:
    //     Beige color (R:245,G:245,B:220,A:255).
    public static TEditColor Beige { get; private set; }

    //
    // Summary:
    //     Bisque color (R:255,G:228,B:196,A:255).
    public static TEditColor Bisque { get; private set; }

    //
    // Summary:
    //     Black color (R:0,G:0,B:0,A:255).
    public static TEditColor Black { get; private set; }

    //
    // Summary:
    //     BlanchedAlmond color (R:255,G:235,B:205,A:255).
    public static TEditColor BlanchedAlmond { get; private set; }

    //
    // Summary:
    //     Blue color (R:0,G:0,B:255,A:255).
    public static TEditColor Blue { get; private set; }

    //
    // Summary:
    //     BlueViolet color (R:138,G:43,B:226,A:255).
    public static TEditColor BlueViolet { get; private set; }

    //
    // Summary:
    //     Brown color (R:165,G:42,B:42,A:255).
    public static TEditColor Brown { get; private set; }

    //
    // Summary:
    //     BurlyWood color (R:222,G:184,B:135,A:255).
    public static TEditColor BurlyWood { get; private set; }

    //
    // Summary:
    //     CadetBlue color (R:95,G:158,B:160,A:255).
    public static TEditColor CadetBlue { get; private set; }

    //
    // Summary:
    //     Chartreuse color (R:127,G:255,B:0,A:255).
    public static TEditColor Chartreuse { get; private set; }

    //
    // Summary:
    //     Chocolate color (R:210,G:105,B:30,A:255).
    public static TEditColor Chocolate { get; private set; }

    //
    // Summary:
    //     Coral color (R:255,G:127,B:80,A:255).
    public static TEditColor Coral { get; private set; }

    //
    // Summary:
    //     CornflowerBlue color (R:100,G:149,B:237,A:255).
    public static TEditColor CornflowerBlue { get; private set; }

    //
    // Summary:
    //     Cornsilk color (R:255,G:248,B:220,A:255).
    public static TEditColor Cornsilk { get; private set; }

    //
    // Summary:
    //     Crimson color (R:220,G:20,B:60,A:255).
    public static TEditColor Crimson { get; private set; }

    //
    // Summary:
    //     Cyan color (R:0,G:255,B:255,A:255).
    public static TEditColor Cyan { get; private set; }

    //
    // Summary:
    //     DarkBlue color (R:0,G:0,B:139,A:255).
    public static TEditColor DarkBlue { get; private set; }

    //
    // Summary:
    //     DarkCyan color (R:0,G:139,B:139,A:255).
    public static TEditColor DarkCyan { get; private set; }

    //
    // Summary:
    //     DarkGoldenrod color (R:184,G:134,B:11,A:255).
    public static TEditColor DarkGoldenrod { get; private set; }

    //
    // Summary:
    //     DarkGray color (R:169,G:169,B:169,A:255).
    public static TEditColor DarkGray { get; private set; }

    //
    // Summary:
    //     DarkGreen color (R:0,G:100,B:0,A:255).
    public static TEditColor DarkGreen { get; private set; }

    //
    // Summary:
    //     DarkKhaki color (R:189,G:183,B:107,A:255).
    public static TEditColor DarkKhaki { get; private set; }

    //
    // Summary:
    //     DarkMagenta color (R:139,G:0,B:139,A:255).
    public static TEditColor DarkMagenta { get; private set; }

    //
    // Summary:
    //     DarkOliveGreen color (R:85,G:107,B:47,A:255).
    public static TEditColor DarkOliveGreen { get; private set; }

    //
    // Summary:
    //     DarkOrange color (R:255,G:140,B:0,A:255).
    public static TEditColor DarkOrange { get; private set; }

    //
    // Summary:
    //     DarkOrchid color (R:153,G:50,B:204,A:255).
    public static TEditColor DarkOrchid { get; private set; }

    //
    // Summary:
    //     DarkRed color (R:139,G:0,B:0,A:255).
    public static TEditColor DarkRed { get; private set; }

    //
    // Summary:
    //     DarkSalmon color (R:233,G:150,B:122,A:255).
    public static TEditColor DarkSalmon { get; private set; }

    //
    // Summary:
    //     DarkSeaGreen color (R:143,G:188,B:139,A:255).
    public static TEditColor DarkSeaGreen { get; private set; }

    //
    // Summary:
    //     DarkSlateBlue color (R:72,G:61,B:139,A:255).
    public static TEditColor DarkSlateBlue { get; private set; }

    //
    // Summary:
    //     DarkSlateGray color (R:47,G:79,B:79,A:255).
    public static TEditColor DarkSlateGray { get; private set; }

    //
    // Summary:
    //     DarkTurquoise color (R:0,G:206,B:209,A:255).
    public static TEditColor DarkTurquoise { get; private set; }

    //
    // Summary:
    //     DarkViolet color (R:148,G:0,B:211,A:255).
    public static TEditColor DarkViolet { get; private set; }

    //
    // Summary:
    //     DeepPink color (R:255,G:20,B:147,A:255).
    public static TEditColor DeepPink { get; private set; }

    //
    // Summary:
    //     DeepSkyBlue color (R:0,G:191,B:255,A:255).
    public static TEditColor DeepSkyBlue { get; private set; }

    //
    // Summary:
    //     DimGray color (R:105,G:105,B:105,A:255).
    public static TEditColor DimGray { get; private set; }

    //
    // Summary:
    //     DodgerBlue color (R:30,G:144,B:255,A:255).
    public static TEditColor DodgerBlue { get; private set; }

    //
    // Summary:
    //     Firebrick color (R:178,G:34,B:34,A:255).
    public static TEditColor Firebrick { get; private set; }

    //
    // Summary:
    //     FloralWhite color (R:255,G:250,B:240,A:255).
    public static TEditColor FloralWhite { get; private set; }

    //
    // Summary:
    //     ForestGreen color (R:34,G:139,B:34,A:255).
    public static TEditColor ForestGreen { get; private set; }

    //
    // Summary:
    //     Fuchsia color (R:255,G:0,B:255,A:255).
    public static TEditColor Fuchsia { get; private set; }

    //
    // Summary:
    //     Gainsboro color (R:220,G:220,B:220,A:255).
    public static TEditColor Gainsboro { get; private set; }

    //
    // Summary:
    //     GhostWhite color (R:248,G:248,B:255,A:255).
    public static TEditColor GhostWhite { get; private set; }

    //
    // Summary:
    //     Gold color (R:255,G:215,B:0,A:255).
    public static TEditColor Gold { get; private set; }

    //
    // Summary:
    //     Goldenrod color (R:218,G:165,B:32,A:255).
    public static TEditColor Goldenrod { get; private set; }

    //
    // Summary:
    //     Gray color (R:128,G:128,B:128,A:255).
    public static TEditColor Gray { get; private set; }

    //
    // Summary:
    //     Green color (R:0,G:128,B:0,A:255).
    public static TEditColor Green { get; private set; }

    //
    // Summary:
    //     GreenYellow color (R:173,G:255,B:47,A:255).
    public static TEditColor GreenYellow { get; private set; }

    //
    // Summary:
    //     Honeydew color (R:240,G:255,B:240,A:255).
    public static TEditColor Honeydew { get; private set; }

    //
    // Summary:
    //     HotPink color (R:255,G:105,B:180,A:255).
    public static TEditColor HotPink { get; private set; }

    //
    // Summary:
    //     IndianRed color (R:205,G:92,B:92,A:255).
    public static TEditColor IndianRed { get; private set; }

    //
    // Summary:
    //     Indigo color (R:75,G:0,B:130,A:255).
    public static TEditColor Indigo { get; private set; }

    //
    // Summary:
    //     Ivory color (R:255,G:255,B:240,A:255).
    public static TEditColor Ivory { get; private set; }

    //
    // Summary:
    //     Khaki color (R:240,G:230,B:140,A:255).
    public static TEditColor Khaki { get; private set; }

    //
    // Summary:
    //     Lavender color (R:230,G:230,B:250,A:255).
    public static TEditColor Lavender { get; private set; }

    //
    // Summary:
    //     LavenderBlush color (R:255,G:240,B:245,A:255).
    public static TEditColor LavenderBlush { get; private set; }

    //
    // Summary:
    //     LawnGreen color (R:124,G:252,B:0,A:255).
    public static TEditColor LawnGreen { get; private set; }

    //
    // Summary:
    //     LemonChiffon color (R:255,G:250,B:205,A:255).
    public static TEditColor LemonChiffon { get; private set; }

    //
    // Summary:
    //     LightBlue color (R:173,G:216,B:230,A:255).
    public static TEditColor LightBlue { get; private set; }

    //
    // Summary:
    //     LightCoral color (R:240,G:128,B:128,A:255).
    public static TEditColor LightCoral { get; private set; }

    //
    // Summary:
    //     LightCyan color (R:224,G:255,B:255,A:255).
    public static TEditColor LightCyan { get; private set; }

    //
    // Summary:
    //     LightGoldenrodYellow color (R:250,G:250,B:210,A:255).
    public static TEditColor LightGoldenrodYellow { get; private set; }

    //
    // Summary:
    //     LightGray color (R:211,G:211,B:211,A:255).
    public static TEditColor LightGray { get; private set; }

    //
    // Summary:
    //     LightGreen color (R:144,G:238,B:144,A:255).
    public static TEditColor LightGreen { get; private set; }

    //
    // Summary:
    //     LightPink color (R:255,G:182,B:193,A:255).
    public static TEditColor LightPink { get; private set; }

    //
    // Summary:
    //     LightSalmon color (R:255,G:160,B:122,A:255).
    public static TEditColor LightSalmon { get; private set; }

    //
    // Summary:
    //     LightSeaGreen color (R:32,G:178,B:170,A:255).
    public static TEditColor LightSeaGreen { get; private set; }

    //
    // Summary:
    //     LightSkyBlue color (R:135,G:206,B:250,A:255).
    public static TEditColor LightSkyBlue { get; private set; }

    //
    // Summary:
    //     LightSlateGray color (R:119,G:136,B:153,A:255).
    public static TEditColor LightSlateGray { get; private set; }

    //
    // Summary:
    //     LightSteelBlue color (R:176,G:196,B:222,A:255).
    public static TEditColor LightSteelBlue { get; private set; }

    //
    // Summary:
    //     LightYellow color (R:255,G:255,B:224,A:255).
    public static TEditColor LightYellow { get; private set; }

    //
    // Summary:
    //     Lime color (R:0,G:255,B:0,A:255).
    public static TEditColor Lime { get; private set; }

    //
    // Summary:
    //     LimeGreen color (R:50,G:205,B:50,A:255).
    public static TEditColor LimeGreen { get; private set; }

    //
    // Summary:
    //     Linen color (R:250,G:240,B:230,A:255).
    public static TEditColor Linen { get; private set; }

    //
    // Summary:
    //     Magenta color (R:255,G:0,B:255,A:255).
    public static TEditColor Magenta { get; private set; }

    //
    // Summary:
    //     Maroon color (R:128,G:0,B:0,A:255).
    public static TEditColor Maroon { get; private set; }

    //
    // Summary:
    //     MediumAquamarine color (R:102,G:205,B:170,A:255).
    public static TEditColor MediumAquamarine { get; private set; }

    //
    // Summary:
    //     MediumBlue color (R:0,G:0,B:205,A:255).
    public static TEditColor MediumBlue { get; private set; }

    //
    // Summary:
    //     MediumOrchid color (R:186,G:85,B:211,A:255).
    public static TEditColor MediumOrchid { get; private set; }

    //
    // Summary:
    //     MediumPurple color (R:147,G:112,B:219,A:255).
    public static TEditColor MediumPurple { get; private set; }

    //
    // Summary:
    //     MediumSeaGreen color (R:60,G:179,B:113,A:255).
    public static TEditColor MediumSeaGreen { get; private set; }

    //
    // Summary:
    //     MediumSlateBlue color (R:123,G:104,B:238,A:255).
    public static TEditColor MediumSlateBlue { get; private set; }

    //
    // Summary:
    //     MediumSpringGreen color (R:0,G:250,B:154,A:255).
    public static TEditColor MediumSpringGreen { get; private set; }

    //
    // Summary:
    //     MediumTurquoise color (R:72,G:209,B:204,A:255).
    public static TEditColor MediumTurquoise { get; private set; }

    //
    // Summary:
    //     MediumVioletRed color (R:199,G:21,B:133,A:255).
    public static TEditColor MediumVioletRed { get; private set; }

    //
    // Summary:
    //     MidnightBlue color (R:25,G:25,B:112,A:255).
    public static TEditColor MidnightBlue { get; private set; }

    //
    // Summary:
    //     MintCream color (R:245,G:255,B:250,A:255).
    public static TEditColor MintCream { get; private set; }

    //
    // Summary:
    //     MistyRose color (R:255,G:228,B:225,A:255).
    public static TEditColor MistyRose { get; private set; }

    //
    // Summary:
    //     Moccasin color (R:255,G:228,B:181,A:255).
    public static TEditColor Moccasin { get; private set; }

    //
    // Summary:
    //     MonoGame orange theme color (R:231,G:60,B:0,A:255).
    public static TEditColor MonoGameOrange { get; private set; }

    //
    // Summary:
    //     NavajoWhite color (R:255,G:222,B:173,A:255).
    public static TEditColor NavajoWhite { get; private set; }

    //
    // Summary:
    //     Navy color (R:0,G:0,B:128,A:255).
    public static TEditColor Navy { get; private set; }

    //
    // Summary:
    //     OldLace color (R:253,G:245,B:230,A:255).
    public static TEditColor OldLace { get; private set; }

    //
    // Summary:
    //     Olive color (R:128,G:128,B:0,A:255).
    public static TEditColor Olive { get; private set; }

    //
    // Summary:
    //     OliveDrab color (R:107,G:142,B:35,A:255).
    public static TEditColor OliveDrab { get; private set; }

    //
    // Summary:
    //     Orange color (R:255,G:165,B:0,A:255).
    public static TEditColor Orange { get; private set; }

    //
    // Summary:
    //     OrangeRed color (R:255,G:69,B:0,A:255).
    public static TEditColor OrangeRed { get; private set; }

    //
    // Summary:
    //     Orchid color (R:218,G:112,B:214,A:255).
    public static TEditColor Orchid { get; private set; }

    //
    // Summary:
    //     PaleGoldenrod color (R:238,G:232,B:170,A:255).
    public static TEditColor PaleGoldenrod { get; private set; }

    //
    // Summary:
    //     PaleGreen color (R:152,G:251,B:152,A:255).
    public static TEditColor PaleGreen { get; private set; }

    //
    // Summary:
    //     PaleTurquoise color (R:175,G:238,B:238,A:255).
    public static TEditColor PaleTurquoise { get; private set; }

    //
    // Summary:
    //     PaleVioletRed color (R:219,G:112,B:147,A:255).
    public static TEditColor PaleVioletRed { get; private set; }

    //
    // Summary:
    //     PapayaWhip color (R:255,G:239,B:213,A:255).
    public static TEditColor PapayaWhip { get; private set; }

    //
    // Summary:
    //     PeachPuff color (R:255,G:218,B:185,A:255).
    public static TEditColor PeachPuff { get; private set; }

    //
    // Summary:
    //     Peru color (R:205,G:133,B:63,A:255).
    public static TEditColor Peru { get; private set; }

    //
    // Summary:
    //     Pink color (R:255,G:192,B:203,A:255).
    public static TEditColor Pink { get; private set; }

    //
    // Summary:
    //     Plum color (R:221,G:160,B:221,A:255).
    public static TEditColor Plum { get; private set; }

    //
    // Summary:
    //     PowderBlue color (R:176,G:224,B:230,A:255).
    public static TEditColor PowderBlue { get; private set; }

    //
    // Summary:
    //     Purple color (R:128,G:0,B:128,A:255).
    public static TEditColor Purple { get; private set; }

    //
    // Summary:
    //     Red color (R:255,G:0,B:0,A:255).
    public static TEditColor Red { get; private set; }

    //
    // Summary:
    //     RosyBrown color (R:188,G:143,B:143,A:255).
    public static TEditColor RosyBrown { get; private set; }

    //
    // Summary:
    //     RoyalBlue color (R:65,G:105,B:225,A:255).
    public static TEditColor RoyalBlue { get; private set; }

    //
    // Summary:
    //     SaddleBrown color (R:139,G:69,B:19,A:255).
    public static TEditColor SaddleBrown { get; private set; }

    //
    // Summary:
    //     Salmon color (R:250,G:128,B:114,A:255).
    public static TEditColor Salmon { get; private set; }

    //
    // Summary:
    //     SandyBrown color (R:244,G:164,B:96,A:255).
    public static TEditColor SandyBrown { get; private set; }

    //
    // Summary:
    //     SeaGreen color (R:46,G:139,B:87,A:255).
    public static TEditColor SeaGreen { get; private set; }

    //
    // Summary:
    //     SeaShell color (R:255,G:245,B:238,A:255).
    public static TEditColor SeaShell { get; private set; }

    //
    // Summary:
    //     Sienna color (R:160,G:82,B:45,A:255).
    public static TEditColor Sienna { get; private set; }

    //
    // Summary:
    //     Silver color (R:192,G:192,B:192,A:255).
    public static TEditColor Silver { get; private set; }

    //
    // Summary:
    //     SkyBlue color (R:135,G:206,B:235,A:255).
    public static TEditColor SkyBlue { get; private set; }

    //
    // Summary:
    //     SlateBlue color (R:106,G:90,B:205,A:255).
    public static TEditColor SlateBlue { get; private set; }

    //
    // Summary:
    //     SlateGray color (R:112,G:128,B:144,A:255).
    public static TEditColor SlateGray { get; private set; }

    //
    // Summary:
    //     Snow color (R:255,G:250,B:250,A:255).
    public static TEditColor Snow { get; private set; }

    //
    // Summary:
    //     SpringGreen color (R:0,G:255,B:127,A:255).
    public static TEditColor SpringGreen { get; private set; }

    //
    // Summary:
    //     SteelBlue color (R:70,G:130,B:180,A:255).
    public static TEditColor SteelBlue { get; private set; }

    //
    // Summary:
    //     Tan color (R:210,G:180,B:140,A:255).
    public static TEditColor Tan { get; private set; }

    //
    // Summary:
    //     Teal color (R:0,G:128,B:128,A:255).
    public static TEditColor Teal { get; private set; }

    //
    // Summary:
    //     Thistle color (R:216,G:191,B:216,A:255).
    public static TEditColor Thistle { get; private set; }

    //
    // Summary:
    //     Tomato color (R:255,G:99,B:71,A:255).
    public static TEditColor Tomato { get; private set; }

    //
    // Summary:
    //     Turquoise color (R:64,G:224,B:208,A:255).
    public static TEditColor Turquoise { get; private set; }

    //
    // Summary:
    //     Violet color (R:238,G:130,B:238,A:255).
    public static TEditColor Violet { get; private set; }

    //
    // Summary:
    //     Wheat color (R:245,G:222,B:179,A:255).
    public static TEditColor Wheat { get; private set; }

    //
    // Summary:
    //     White color (R:255,G:255,B:255,A:255).
    public static TEditColor White { get; private set; }

    //
    // Summary:
    //     WhiteSmoke color (R:245,G:245,B:245,A:255).
    public static TEditColor WhiteSmoke { get; private set; }

    //
    // Summary:
    //     Yellow color (R:255,G:255,B:0,A:255).
    public static TEditColor Yellow { get; private set; }

    //
    // Summary:
    //     YellowGreen color (R:154,G:205,B:50,A:255).
    public static TEditColor YellowGreen { get; private set; }

    //
    // Summary:
    //     Gets or sets packed value of this Microsoft.Xna.Framework.Color.
    [CLSCompliant(false)]
    public uint PackedValue
    {
        get
        {
            return _packedValue;
        }
        set
        {
            _packedValue = value;
        }
    }

    public static explicit operator uint(TEditColor color) => color.PackedValue;

    internal string DebugDisplayString => R + "  " + G + "  " + B + "  " + A;

    static TEditColor()
    {
        TransparentBlack = new TEditColor(0u);
        Transparent = new TEditColor(0u);
        AliceBlue = new TEditColor(4294965488u);
        AntiqueWhite = new TEditColor(4292340730u);
        Aqua = new TEditColor(4294967040u);
        Aquamarine = new TEditColor(4292149119u);
        Azure = new TEditColor(4294967280u);
        Beige = new TEditColor(4292670965u);
        Bisque = new TEditColor(4291093759u);
        Black = new TEditColor(4278190080u);
        BlanchedAlmond = new TEditColor(4291685375u);
        Blue = new TEditColor(4294901760u);
        BlueViolet = new TEditColor(4293012362u);
        Brown = new TEditColor(4280953509u);
        BurlyWood = new TEditColor(4287084766u);
        CadetBlue = new TEditColor(4288716383u);
        Chartreuse = new TEditColor(4278255487u);
        Chocolate = new TEditColor(4280183250u);
        Coral = new TEditColor(4283465727u);
        CornflowerBlue = new TEditColor(4293760356u);
        Cornsilk = new TEditColor(4292671743u);
        Crimson = new TEditColor(4282127580u);
        Cyan = new TEditColor(4294967040u);
        DarkBlue = new TEditColor(4287299584u);
        DarkCyan = new TEditColor(4287335168u);
        DarkGoldenrod = new TEditColor(4278945464u);
        DarkGray = new TEditColor(4289309097u);
        DarkGreen = new TEditColor(4278215680u);
        DarkKhaki = new TEditColor(4285249469u);
        DarkMagenta = new TEditColor(4287299723u);
        DarkOliveGreen = new TEditColor(4281297749u);
        DarkOrange = new TEditColor(4278226175u);
        DarkOrchid = new TEditColor(4291572377u);
        DarkRed = new TEditColor(4278190219u);
        DarkSalmon = new TEditColor(4286224105u);
        DarkSeaGreen = new TEditColor(4287347855u);
        DarkSlateBlue = new TEditColor(4287315272u);
        DarkSlateGray = new TEditColor(4283387695u);
        DarkTurquoise = new TEditColor(4291939840u);
        DarkViolet = new TEditColor(4292018324u);
        DeepPink = new TEditColor(4287829247u);
        DeepSkyBlue = new TEditColor(4294950656u);
        DimGray = new TEditColor(4285098345u);
        DodgerBlue = new TEditColor(4294938654u);
        Firebrick = new TEditColor(4280427186u);
        FloralWhite = new TEditColor(4293982975u);
        ForestGreen = new TEditColor(4280453922u);
        Fuchsia = new TEditColor(4294902015u);
        Gainsboro = new TEditColor(4292664540u);
        GhostWhite = new TEditColor(4294965496u);
        Gold = new TEditColor(4278245375u);
        Goldenrod = new TEditColor(4280329690u);
        Gray = new TEditColor(4286611584u);
        Green = new TEditColor(4278222848u);
        GreenYellow = new TEditColor(4281335725u);
        Honeydew = new TEditColor(4293984240u);
        HotPink = new TEditColor(4290013695u);
        IndianRed = new TEditColor(4284243149u);
        Indigo = new TEditColor(4286709835u);
        Ivory = new TEditColor(4293984255u);
        Khaki = new TEditColor(4287424240u);
        Lavender = new TEditColor(4294633190u);
        LavenderBlush = new TEditColor(4294308095u);
        LawnGreen = new TEditColor(4278254716u);
        LemonChiffon = new TEditColor(4291689215u);
        LightBlue = new TEditColor(4293318829u);
        LightCoral = new TEditColor(4286611696u);
        LightCyan = new TEditColor(4294967264u);
        LightGoldenrodYellow = new TEditColor(4292016890u);
        LightGray = new TEditColor(4292072403u);
        LightGreen = new TEditColor(4287688336u);
        LightPink = new TEditColor(4290885375u);
        LightSalmon = new TEditColor(4286226687u);
        LightSeaGreen = new TEditColor(4289376800u);
        LightSkyBlue = new TEditColor(4294626951u);
        LightSlateGray = new TEditColor(4288252023u);
        LightSteelBlue = new TEditColor(4292789424u);
        LightYellow = new TEditColor(4292935679u);
        Lime = new TEditColor(4278255360u);
        LimeGreen = new TEditColor(4281519410u);
        Linen = new TEditColor(4293325050u);
        Magenta = new TEditColor(4294902015u);
        Maroon = new TEditColor(4278190208u);
        MediumAquamarine = new TEditColor(4289383782u);
        MediumBlue = new TEditColor(4291624960u);
        MediumOrchid = new TEditColor(4292040122u);
        MediumPurple = new TEditColor(4292571283u);
        MediumSeaGreen = new TEditColor(4285641532u);
        MediumSlateBlue = new TEditColor(4293814395u);
        MediumSpringGreen = new TEditColor(4288346624u);
        MediumTurquoise = new TEditColor(4291613000u);
        MediumVioletRed = new TEditColor(4286911943u);
        MidnightBlue = new TEditColor(4285536537u);
        MintCream = new TEditColor(4294639605u);
        MistyRose = new TEditColor(4292994303u);
        Moccasin = new TEditColor(4290110719u);
        MonoGameOrange = new TEditColor(4278205671u);
        NavajoWhite = new TEditColor(4289584895u);
        Navy = new TEditColor(4286578688u);
        OldLace = new TEditColor(4293326333u);
        Olive = new TEditColor(4278222976u);
        OliveDrab = new TEditColor(4280520299u);
        Orange = new TEditColor(4278232575u);
        OrangeRed = new TEditColor(4278207999u);
        Orchid = new TEditColor(4292243674u);
        PaleGoldenrod = new TEditColor(4289390830u);
        PaleGreen = new TEditColor(4288215960u);
        PaleTurquoise = new TEditColor(4293848751u);
        PaleVioletRed = new TEditColor(4287852763u);
        PapayaWhip = new TEditColor(4292210687u);
        PeachPuff = new TEditColor(4290370303u);
        Peru = new TEditColor(4282353101u);
        Pink = new TEditColor(4291543295u);
        Plum = new TEditColor(4292714717u);
        PowderBlue = new TEditColor(4293320880u);
        Purple = new TEditColor(4286578816u);
        Red = new TEditColor(4278190335u);
        RosyBrown = new TEditColor(4287598524u);
        RoyalBlue = new TEditColor(4292962625u);
        SaddleBrown = new TEditColor(4279453067u);
        Salmon = new TEditColor(4285694202u);
        SandyBrown = new TEditColor(4284523764u);
        SeaGreen = new TEditColor(4283927342u);
        SeaShell = new TEditColor(4293850623u);
        Sienna = new TEditColor(4281160352u);
        Silver = new TEditColor(4290822336u);
        SkyBlue = new TEditColor(4293643911u);
        SlateBlue = new TEditColor(4291648106u);
        SlateGray = new TEditColor(4287660144u);
        Snow = new TEditColor(4294638335u);
        SpringGreen = new TEditColor(4286578432u);
        SteelBlue = new TEditColor(4290019910u);
        Tan = new TEditColor(4287411410u);
        Teal = new TEditColor(4286611456u);
        Thistle = new TEditColor(4292394968u);
        Tomato = new TEditColor(4282868735u);
        Turquoise = new TEditColor(4291878976u);
        Violet = new TEditColor(4293821166u);
        Wheat = new TEditColor(4289978101u);
        White = new TEditColor(uint.MaxValue);
        WhiteSmoke = new TEditColor(4294309365u);
        Yellow = new TEditColor(4278255615u);
        YellowGreen = new TEditColor(4281519514u);
    }

    //
    // Summary:
    //     Constructs an RGBA color from a packed value. The value is a 32-bit unsigned
    //     integer, with R in the least significant octet.
    //
    // Parameters:
    //   packedValue:
    //     The packed value.
    [CLSCompliant(false)]
    public TEditColor(uint packedValue)
    {
        _packedValue = packedValue;
    }

    //
    // Summary:
    //     Constructs an RGBA color from the XYZW unit length components of a vector.
    //
    // Parameters:
    //   color:
    //     A Microsoft.Xna.Framework.Vector4 representing color.
    public TEditColor(Vector4Float color)
        : this((int)(color.X * 255f), (int)(color.Y * 255f), (int)(color.Z * 255f), (int)(color.W * 255f))
    {
    }

    //
    // Summary:
    //     Constructs an RGBA color from the XYZ unit length components of a vector. Alpha
    //     value will be opaque.
    //
    // Parameters:
    //   color:
    //     A Microsoft.Xna.Framework.Vector3 representing color.
    public TEditColor(Vector3Float color)
        : this((int)(color.X * 255f), (int)(color.Y * 255f), (int)(color.Z * 255f))
    {
    }

    //
    // Summary:
    //     Constructs an RGBA color from a Microsoft.Xna.Framework.Color and an alpha value.
    //
    // Parameters:
    //   color:
    //     A Microsoft.Xna.Framework.Color for RGB values of new Microsoft.Xna.Framework.Color
    //     instance.
    //
    //   alpha:
    //     The alpha component value from 0 to 255.
    public TEditColor(TEditColor color, int alpha)
    {
        if ((alpha & 0xFFFFFF00u) != 0L)
        {
            uint num = (uint)Calc.Clamp(alpha, 0, 255);
            _packedValue = (color._packedValue & 0xFFFFFFu) | (num << 24);
        }
        else
        {
            _packedValue = (color._packedValue & 0xFFFFFFu) | (uint)(alpha << 24);
        }
    }

    //
    // Summary:
    //     Constructs an RGBA color from color and alpha value.
    //
    // Parameters:
    //   color:
    //     A Microsoft.Xna.Framework.Color for RGB values of new Microsoft.Xna.Framework.Color
    //     instance.
    //
    //   alpha:
    //     Alpha component value from 0.0f to 1.0f.
    public TEditColor(TEditColor color, float alpha)
        : this(color, (int)(alpha * 255f))
    {
    }

    //
    // Summary:
    //     Constructs an RGBA color from scalars representing red, green and blue values.
    //     Alpha value will be opaque.
    //
    // Parameters:
    //   r:
    //     Red component value from 0.0f to 1.0f.
    //
    //   g:
    //     Green component value from 0.0f to 1.0f.
    //
    //   b:
    //     Blue component value from 0.0f to 1.0f.
    public TEditColor(float r, float g, float b)
        : this((int)(r * 255f), (int)(g * 255f), (int)(b * 255f))
    {
    }

    //
    // Summary:
    //     Constructs an RGBA color from scalars representing red, green, blue and alpha
    //     values.
    //
    // Parameters:
    //   r:
    //     Red component value from 0.0f to 1.0f.
    //
    //   g:
    //     Green component value from 0.0f to 1.0f.
    //
    //   b:
    //     Blue component value from 0.0f to 1.0f.
    //
    //   alpha:
    //     Alpha component value from 0.0f to 1.0f.
    public TEditColor(float r, float g, float b, float alpha)
        : this((int)(r * 255f), (int)(g * 255f), (int)(b * 255f), (int)(alpha * 255f))
    {
    }

    //
    // Summary:
    //     Constructs an RGBA color from scalars representing red, green and blue values.
    //     Alpha value will be opaque.
    //
    // Parameters:
    //   r:
    //     Red component value from 0 to 255.
    //
    //   g:
    //     Green component value from 0 to 255.
    //
    //   b:
    //     Blue component value from 0 to 255.
    public TEditColor(int r, int g, int b)
    {
        _packedValue = 4278190080u;
        if (((r | g | b) & 0xFFFFFF00u) != 0L)
        {
            uint num = (uint)Calc.Clamp(r, 0, 255);
            uint num2 = (uint)Calc.Clamp(g, 0, 255);
            uint num3 = (uint)Calc.Clamp(b, 0, 255);
            _packedValue |= (num3 << 16) | (num2 << 8) | num;
        }
        else
        {
            _packedValue |= (uint)((b << 16) | (g << 8) | r);
        }
    }

    //
    // Summary:
    //     Constructs an RGBA color from scalars representing red, green, blue and alpha
    //     values.
    //
    // Parameters:
    //   r:
    //     Red component value from 0 to 255.
    //
    //   g:
    //     Green component value from 0 to 255.
    //
    //   b:
    //     Blue component value from 0 to 255.
    //
    //   alpha:
    //     Alpha component value from 0 to 255.
    public TEditColor(int r, int g, int b, int alpha)
    {
        if (((r | g | b | alpha) & 0xFFFFFF00u) != 0L)
        {
            uint num = (uint)Calc.Clamp(r, 0, 255);
            uint num2 = (uint)Calc.Clamp(g, 0, 255);
            uint num3 = (uint)Calc.Clamp(b, 0, 255);
            uint num4 = (uint)Calc.Clamp(alpha, 0, 255);
            _packedValue = (num4 << 24) | (num3 << 16) | (num2 << 8) | num;
        }
        else
        {
            _packedValue = (uint)((alpha << 24) | (b << 16) | (g << 8) | r);
        }
    }

    //
    // Summary:
    //     Constructs an RGBA color from scalars representing red, green, blue and alpha
    //     values.
    //
    // Parameters:
    //   r:
    //
    //   g:
    //
    //   b:
    //
    //   alpha:
    //
    // Remarks:
    //     This overload sets the values directly without clamping, and may therefore be
    //     faster than the other overloads.
    public TEditColor(byte r, byte g, byte b, byte alpha)
    {
        _packedValue = (uint)((alpha << 24) | (b << 16) | (g << 8) | r);
    }

    //
    // Summary:
    //     Compares whether two Microsoft.Xna.Framework.Color instances are equal.
    //
    // Parameters:
    //   a:
    //     Microsoft.Xna.Framework.Color instance on the left of the equal sign.
    //
    //   b:
    //     Microsoft.Xna.Framework.Color instance on the right of the equal sign.
    //
    // Returns:
    //     true if the instances are equal; false otherwise.
    public static bool operator ==(TEditColor a, TEditColor b)
    {
        return a._packedValue == b._packedValue;
    }

    //
    // Summary:
    //     Compares whether two Microsoft.Xna.Framework.Color instances are not equal.
    //
    // Parameters:
    //   a:
    //     Microsoft.Xna.Framework.Color instance on the left of the not equal sign.
    //
    //   b:
    //     Microsoft.Xna.Framework.Color instance on the right of the not equal sign.
    //
    // Returns:
    //     true if the instances are not equal; false otherwise.
    public static bool operator !=(TEditColor a, TEditColor b)
    {
        return a._packedValue != b._packedValue;
    }

    //
    // Summary:
    //     Gets the hash code of this Microsoft.Xna.Framework.Color.
    //
    // Returns:
    //     Hash code of this Microsoft.Xna.Framework.Color.
    public override int GetHashCode()
    {
        return _packedValue.GetHashCode();
    }

    //
    // Summary:
    //     Compares whether current instance is equal to specified object.
    //
    // Parameters:
    //   obj:
    //     The Microsoft.Xna.Framework.Color to compare.
    //
    // Returns:
    //     true if the instances are equal; false otherwise.
    public override bool Equals(object obj)
    {
        if (obj is TEditColor)
        {
            return Equals((TEditColor)obj);
        }

        return false;
    }

    //
    // Summary:
    //     Performs linear interpolation of Microsoft.Xna.Framework.Color.
    //
    // Parameters:
    //   value1:
    //     Source Microsoft.Xna.Framework.Color.
    //
    //   value2:
    //     Destination Microsoft.Xna.Framework.Color.
    //
    //   amount:
    //     Interpolation factor.
    //
    // Returns:
    //     Interpolated Microsoft.Xna.Framework.Color.
    public static TEditColor Lerp(TEditColor value1, TEditColor value2, float amount)
    {
        amount = Calc.Clamp(amount, 0f, 1f);
        return new TEditColor((int)Calc.Lerp((int)value1.R, (int)value2.R, amount), (int)Calc.Lerp((int)value1.G, (int)value2.G, amount), (int)Calc.Lerp((int)value1.B, (int)value2.B, amount), (int)Calc.Lerp((int)value1.A, (int)value2.A, amount));
    }

    //
    // Summary:
    //     Microsoft.Xna.Framework.Color.Lerp(Microsoft.Xna.Framework.Color,Microsoft.Xna.Framework.Color,System.Single)
    //     should be used instead of this function.
    //
    // Returns:
    //     Interpolated Microsoft.Xna.Framework.Color.
    [Obsolete("Color.Lerp should be used instead of this function.")]
    public static TEditColor LerpPrecise(TEditColor value1, TEditColor value2, float amount)
    {
        amount = Calc.Clamp(amount, 0f, 1f);
        return new TEditColor((int)Calc.LerpPrecise((int)value1.R, (int)value2.R, amount), (int)Calc.LerpPrecise((int)value1.G, (int)value2.G, amount), (int)Calc.LerpPrecise((int)value1.B, (int)value2.B, amount), (int)Calc.LerpPrecise((int)value1.A, (int)value2.A, amount));
    }

    //
    // Summary:
    //     Multiply Microsoft.Xna.Framework.Color by value.
    //
    // Parameters:
    //   value:
    //     Source Microsoft.Xna.Framework.Color.
    //
    //   scale:
    //     Multiplicator.
    //
    // Returns:
    //     Multiplication result.
    public static TEditColor Multiply(TEditColor value, float scale)
    {
        return new TEditColor((int)((float)(int)value.R * scale), (int)((float)(int)value.G * scale), (int)((float)(int)value.B * scale), (int)((float)(int)value.A * scale));
    }

    //
    // Summary:
    //     Multiply Microsoft.Xna.Framework.Color by value.
    //
    // Parameters:
    //   value:
    //     Source Microsoft.Xna.Framework.Color.
    //
    //   scale:
    //     Multiplicator.
    //
    // Returns:
    //     Multiplication result.
    public static TEditColor operator *(TEditColor value, float scale)
    {
        return new TEditColor((int)((float)(int)value.R * scale), (int)((float)(int)value.G * scale), (int)((float)(int)value.B * scale), (int)((float)(int)value.A * scale));
    }

    public static TEditColor operator *(float scale, TEditColor value)
    {
        return new TEditColor((int)((float)(int)value.R * scale), (int)((float)(int)value.G * scale), (int)((float)(int)value.B * scale), (int)((float)(int)value.A * scale));
    }

    //
    // Summary:
    //     Gets a Microsoft.Xna.Framework.Vector3 representation for this object.
    //
    // Returns:
    //     A Microsoft.Xna.Framework.Vector3 representation for this object.
    public Vector3Float ToVector3()
    {
        return new Vector3Float((float)(int)R / 255f, (float)(int)G / 255f, (float)(int)B / 255f);
    }

    //
    // Summary:
    //     Gets a Microsoft.Xna.Framework.Vector4 representation for this object.
    //
    // Returns:
    //     A Microsoft.Xna.Framework.Vector4 representation for this object.
    public Vector4Float ToVector4()
    {
        return new Vector4Float((float)(int)R / 255f, (float)(int)G / 255f, (float)(int)B / 255f, (float)(int)A / 255f);
    }

    //
    // Summary:
    //     Returns a System.String representation of this Microsoft.Xna.Framework.Color
    //     in the format: {R:[red] G:[green] B:[blue] A:[alpha]}
    //
    // Returns:
    //     System.String representation of this Microsoft.Xna.Framework.Color.
    public override string ToString() => ToHexString(this);

    //
    // Summary:
    //     Translate a non-premultipled alpha Microsoft.Xna.Framework.Color to a Microsoft.Xna.Framework.Color
    //     that contains premultiplied alpha.
    //
    // Parameters:
    //   vector:
    //     A Microsoft.Xna.Framework.Vector4 representing color.
    //
    // Returns:
    //     A Microsoft.Xna.Framework.Color which contains premultiplied alpha data.
    public static TEditColor FromNonPremultiplied(Vector4Float vector)
    {
        return new TEditColor(vector.X * vector.W, vector.Y * vector.W, vector.Z * vector.W, vector.W);
    }

    //
    // Summary:
    //     Translate a non-premultipled alpha Microsoft.Xna.Framework.Color to a Microsoft.Xna.Framework.Color
    //     that contains premultiplied alpha.
    //
    // Parameters:
    //   r:
    //     Red component value.
    //
    //   g:
    //     Green component value.
    //
    //   b:
    //     Blue component value.
    //
    //   a:
    //     Alpha component value.
    //
    // Returns:
    //     A Microsoft.Xna.Framework.Color which contains premultiplied alpha data.
    public static TEditColor FromNonPremultiplied(int r, int g, int b, int a)
    {
        return new TEditColor(r * a / 255, g * a / 255, b * a / 255, a);
    }

    //
    // Summary:
    //     Compares whether current instance is equal to specified Microsoft.Xna.Framework.Color.
    //
    // Parameters:
    //   other:
    //     The Microsoft.Xna.Framework.Color to compare.
    //
    // Returns:
    //     true if the instances are equal; false otherwise.
    public bool Equals(TEditColor other)
    {
        return PackedValue == other.PackedValue;
    }

    //
    // Summary:
    //     Deconstruction method for Microsoft.Xna.Framework.Color.
    //
    // Parameters:
    //   r:
    //     Red component value from 0 to 255.
    //
    //   g:
    //     Green component value from 0 to 255.
    //
    //   b:
    //     Blue component value from 0 to 255.
    public void Deconstruct(out byte r, out byte g, out byte b)
    {
        r = R;
        g = G;
        b = B;
    }

    //
    // Summary:
    //     Deconstruction method for Microsoft.Xna.Framework.Color.
    //
    // Parameters:
    //   r:
    //     Red component value from 0.0f to 1.0f.
    //
    //   g:
    //     Green component value from 0.0f to 1.0f.
    //
    //   b:
    //     Blue component value from 0.0f to 1.0f.
    public void Deconstruct(out float r, out float g, out float b)
    {
        r = (float)(int)R / 255f;
        g = (float)(int)G / 255f;
        b = (float)(int)B / 255f;
    }

    //
    // Summary:
    //     Deconstruction method for Microsoft.Xna.Framework.Color with Alpha.
    //
    // Parameters:
    //   r:
    //     Red component value from 0 to 255.
    //
    //   g:
    //     Green component value from 0 to 255.
    //
    //   b:
    //     Blue component value from 0 to 255.
    //
    //   a:
    //     Alpha component value from 0 to 255.
    public void Deconstruct(out byte r, out byte g, out byte b, out byte a)
    {
        r = R;
        g = G;
        b = B;
        a = A;
    }

    //
    // Summary:
    //     Deconstruction method for Microsoft.Xna.Framework.Color with Alpha.
    //
    // Parameters:
    //   r:
    //     Red component value from 0.0f to 1.0f.
    //
    //   g:
    //     Green component value from 0.0f to 1.0f.
    //
    //   b:
    //     Blue component value from 0.0f to 1.0f.
    //
    //   a:
    //     Alpha component value from 0.0f to 1.0f.
    public void Deconstruct(out float r, out float g, out float b, out float a)
    {
        r = (float)(int)R / 255f;
        g = (float)(int)G / 255f;
        b = (float)(int)B / 255f;
        a = (float)(int)A / 255f;
    }
}
