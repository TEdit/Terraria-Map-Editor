using System;
using System.Collections.Generic;
using System.IO;

namespace TEdit.Terraria;

/// <summary>
/// BitFlag Byte Structure
/// </summary>
public struct BitsByte
{
    private static bool Null;

    private byte @value;

    public byte Value => this.@value;

    public bool this[int key]
    {
        get
        {
            return (this.@value & 1 << (key & 31)) != 0;
        }
        set
        {
            if (value)
            {
                this.@value = (byte)(this.@value | (byte)(1 << (key & 31)));
                return;
            }
            this.@value = (byte)(this.@value & (byte)(~(1 << (key & 31))));
        }
    }

    public BitsByte(bool b1 = false, bool b2 = false, bool b3 = false, bool b4 = false, bool b5 = false, bool b6 = false, bool b7 = false, bool b8 = false)
    {
        this.@value = 0;
        this[0] = b1;
        this[1] = b2;
        this[2] = b3;
        this[3] = b4;
        this[4] = b5;
        this[5] = b6;
        this[6] = b7;
        this[7] = b8;
    }

    public void ClearAll()
    {
        this.@value = 0;
    }

    public static BitsByte[] ComposeBitsBytesChain(bool optimizeLength, params bool[] flags)
    {
        int j;
        int length = (int)flags.Length;
        int num = 0;
        while (length > 0)
        {
            num++;
            length -= 7;
        }
        BitsByte[] bitsByteArray = new BitsByte[num];
        int num1 = 0;
        int num2 = 0;
        for (int i = 0; i < (int)flags.Length; i++)
        {
            bitsByteArray[num2][num1] = flags[i];
            num1++;
            if (num1 == 7 && num2 < num - 1)
            {
                bitsByteArray[num2][num1] = true;
                num1 = 0;
                num2++;
            }
        }
        if (optimizeLength)
        {
            for (j = (int)bitsByteArray.Length - 1; bitsByteArray[j] == 0 && j > 0; j--)
            {
                bitsByteArray[j - 1][7] = false;
            }
            Array.Resize<BitsByte>(ref bitsByteArray, j + 1);
        }
        return bitsByteArray;
    }

    public static BitsByte[] DecomposeBitsBytesChain(BinaryReader reader)
    {
        BitsByte bitsByte;
        List<BitsByte> bitsBytes = new List<BitsByte>();
        do
        {
            bitsByte = reader.ReadByte();
            bitsBytes.Add(bitsByte);
        }
        while (bitsByte[7]);
        return bitsBytes.ToArray();
    }

    public static implicit operator Byte(BitsByte bb)
    {
        return bb.@value;
    }

    public static implicit operator BitsByte(byte b)
    {
        return new BitsByte()
        {
            @value = b
        };
    }

    public void Retrieve(ref bool b0)
    {
        this.Retrieve(ref b0, ref BitsByte.Null, ref BitsByte.Null, ref BitsByte.Null, ref BitsByte.Null, ref BitsByte.Null, ref BitsByte.Null, ref BitsByte.Null);
    }

    public void Retrieve(ref bool b0, ref bool b1)
    {
        this.Retrieve(ref b0, ref b1, ref BitsByte.Null, ref BitsByte.Null, ref BitsByte.Null, ref BitsByte.Null, ref BitsByte.Null, ref BitsByte.Null);
    }

    public void Retrieve(ref bool b0, ref bool b1, ref bool b2)
    {
        this.Retrieve(ref b0, ref b1, ref b2, ref BitsByte.Null, ref BitsByte.Null, ref BitsByte.Null, ref BitsByte.Null, ref BitsByte.Null);
    }

    public void Retrieve(ref bool b0, ref bool b1, ref bool b2, ref bool b3)
    {
        this.Retrieve(ref b0, ref b1, ref b2, ref b3, ref BitsByte.Null, ref BitsByte.Null, ref BitsByte.Null, ref BitsByte.Null);
    }

    public void Retrieve(ref bool b0, ref bool b1, ref bool b2, ref bool b3, ref bool b4)
    {
        this.Retrieve(ref b0, ref b1, ref b2, ref b3, ref b4, ref BitsByte.Null, ref BitsByte.Null, ref BitsByte.Null);
    }

    public void Retrieve(ref bool b0, ref bool b1, ref bool b2, ref bool b3, ref bool b4, ref bool b5)
    {
        this.Retrieve(ref b0, ref b1, ref b2, ref b3, ref b4, ref b5, ref BitsByte.Null, ref BitsByte.Null);
    }

    public void Retrieve(ref bool b0, ref bool b1, ref bool b2, ref bool b3, ref bool b4, ref bool b5, ref bool b6)
    {
        this.Retrieve(ref b0, ref b1, ref b2, ref b3, ref b4, ref b5, ref b6, ref BitsByte.Null);
    }

    public void Retrieve(ref bool b0, ref bool b1, ref bool b2, ref bool b3, ref bool b4, ref bool b5, ref bool b6, ref bool b7)
    {
        b0 = this[0];
        b1 = this[1];
        b2 = this[2];
        b3 = this[3];
        b4 = this[4];
        b5 = this[5];
        b6 = this[6];
        b7 = this[7];
    }

    public void SetAll()
    {
        this.@value = 255;
    }
}
