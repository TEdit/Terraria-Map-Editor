using System;

namespace TEdit.RenderWorld
{
    [Flags]
    public enum ImageType
    {
        Undefined 	  = 0x00000000,

        RGBOrderMask  = 0x7f000000,
        AttributeMask = 0x00ff0000,
        BPPMask       = 0x0000ff00,
        AlphaBPCMask  = 0x000000ff,

        Max           = 0x7fffffff,

        // (NOTE: Only one can exist at a time, so it's not binary bits.)
        ARGB          = 0x01000000,  // System.Drawing.Imaging & XNA
        RGBA          = 0x02000000,  
        ABGR          = 0x03000000,  // System.Windows.Media.Imaging
        BGRA          = 0x04000000,
        // (NOTE: the A in the order doesn't exactly mean it has alpha...
        //  Check the AlphaBPCMask for that.)
        Indexed 	  = 0x05000000,
        Grayscale     = 0x06000000,
        NoRGB         = 0x07000000,
        RG            = 0x08000000,
        CMYK          = 0x09000000,

        PAlpha 		  = 0x00010000,
        GreenUpBit    = 0x00020000,

        // (NOTE: The last two masks should not be OR-ed for values, as 24/48 are multi-bit.
        //  Use bitmask AND + equality for verification.)
        BPP1 		  = 0x00000100,
        BPP2 		  = 0x00000200,
        BPP4 		  = 0x00000400,
        BPP8 		  = 0x00000800,
        BPP16 		  = 0x00001000,
        BPP24 		  = 0x00001800,
        BPP32 		  = 0x00002000,
        BPP48 		  = 0x00002800,
        BPP64 		  = 0x00004000,
        BPP128 		  = 0x00008000,

        AlphaB1		  = 0x00000001,
        AlphaB2  	  = 0x00000002,
        AlphaB4		  = 0x00000004,
        AlphaB8		  = 0x00000008,
        AlphaB16	  = 0x00000010,
        AlphaB24	  = 0x00000018,
        AlphaB32	  = 0x00000020,

        // RGB_BPC = (int)(BPP - BAlpha) / 3 + (GreenUpBit && G ? 1 : 0);

        // System.Drawing.Imaging.PixelFormat
        Format1bppIndexed    = BPP1  | Indexed,
        Format4bppIndexed 	 = BPP4  | Indexed,
        Format8bppIndexed 	 = BPP8  | Indexed,
        Format16bppGrayScale = BPP16 | Grayscale,
        Format16bppRgb555 	 = BPP16 | ARGB,
        Format16bppRgb565 	 = BPP16 | ARGB | GreenUpBit,
        Format16bppArgb1555  = BPP16 | ARGB | AlphaB1,
        Format24bppRgb 		 = BPP24 | ARGB,
        Format32bppRgb 		 = BPP32 | ARGB,
        Format32bppArgb 	 = BPP32 | ARGB | AlphaB8,
        Format32bppPArgb 	 = BPP32 | ARGB | AlphaB8  | PAlpha,
        Format48bppRgb 		 = BPP48 | ARGB,
        Format64bppArgb 	 = BPP64 | ARGB | AlphaB16,
        Format64bppPArgb     = BPP64 | ARGB | AlphaB16 | PAlpha,

        // Microsoft.Xna.Framework.Graphics.SurfaceFormat
        //   (minus the truly bizarre formats...)
        Color                = Format32bppArgb,
        Bgr565               = BPP16  | BGRA  | GreenUpBit,
        Bgra5551			 = BPP16  | BGRA  | AlphaB1,
        Bgra4444             = BPP16  | BGRA  | AlphaB4,
        Rgba1010102          = BPP32  | RGBA  | AlphaB2,
        Rg32                 = BPP32  | RG,
        Rgba64               = BPP64  | RGBA  | AlphaB16,
        Alpha8               = BPP8   | NoRGB | AlphaB8,
        Single               = BPP32  | Grayscale,
        Vector2              = BPP64  | RG,
        Vector4              = BPP128 | ABGR  | AlphaB32,
        HalfSingle           = Format16bppGrayScale,
        HalfVector2          = BPP32  | RG,
        HalfVector4          = BPP64  | ABGR  | AlphaB16,

        // System.Windows.Media.PixelFormats
        Bgr101010            = BPP32  | BGRA,
        Bgr24                = BPP24  | BGRA,
        Bgr32                = BPP32  | BGRA  | AlphaB8,  // technically, the Alpha is unused
        Bgr555               = BPP16  | BGRA,
        //Bgr565             = Already defined above
        Bgra32               = BPP32  | BGRA  | AlphaB8,
        BlackWhite           = BPP1   | Grayscale,
        Cmyk32               = BPP32  | CMYK,
        Gray2                = BPP2   | Grayscale,
        Gray4                = BPP4   | Grayscale,
        Gray8                = BPP8   | Grayscale,
        Gray16               = Format16bppGrayScale,
        Gray32Float          = BPP32  | Grayscale,
        Indexed1             = Format1bppIndexed,
        Indexed2             = BPP2   | Indexed,
        Indexed4             = Format4bppIndexed,
        Indexed8             = Format8bppIndexed,
        Pbgra32              = BPP32  | BGRA  | AlphaB8  | PAlpha,
        Prgba64              = BPP64  | BGRA  | AlphaB8  | PAlpha,
        Pbgra128Float        = BPP128 | BGRA  | AlphaB32 | PAlpha,
        Rgb24                = BPP24  | RGBA,
        Rgb48                = BPP48  | RGBA,
        Rgb128Float          = BPP128 | RGBA  | AlphaB32,  // technically, the Alpha is unused
        //Rgba64             = Already defined above
        Rgba128Float         = BPP128 | RGBA  | AlphaB32,

        // Unlisted, but used for WriteableBitmap / System.Windows.Media.Imaging / etc.
        Abgr32               = BPP32  | ABGR  | AlphaB8,
    }
}
