using System.IO;
using GalaSoft.MvvmLight;

namespace TEditXNA.Terraria
{
    public class MapTile : ObservableObject
    {
        private byte _type;
        private byte _light;
        private byte _misc;
        private byte _misc2;

        public byte Misc2
        {
            get { return _misc2; }
            set { Set("Misc2", ref _misc2, value); }
        }

        public byte Misc
        {
            get { return _misc; }
            set { Set("Misc", ref _misc, value); }
        }

        public byte Light
        {
            get { return _light; }
            set { Set("Light", ref _light, value); }
        }

        public byte Type
        {
            get { return _type; }
            set { Set("Type", ref _type, value); }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public MapTile Copy()
        {
            return (MapTile)MemberwiseClone();
        }

        public bool IsSameAs(MapTile other)
        {
            return _type == other._type &&
                   _light == other._light &&
                   _misc == other._misc &&
                   _misc2 == other._misc2;
        }
    }

    public class Map : ObservableObject
    {
        private string _worldName;
        private int _worldId;
        private int _maxTilesX;
        private int _maxTilesY;
        private int _version;
        public MapTile[,] Tiles;

        public MapTile this[int x, int y]
        {
            get { return Tiles[x, y]; }
            set { Tiles[x, y] = value; }
        }

        public int Version
        {
            get { return _version; }
            set { Set("Version", ref _version, value); }
        }

        public int MaxTilesY
        {
            get { return _maxTilesY; }
            set { Set("MaxTilesY", ref _maxTilesY, value); }
        }

        public int MaxTilesX
        {
            get { return _maxTilesX; }
            set { Set("MaxTilesX", ref _maxTilesX, value); }
        }

        public int WorldId
        {
            get { return _worldId; }
            set { Set("WorldId", ref _worldId, value); }
        }

        public string WorldName
        {
            get { return _worldName; }
            set { Set("WorldName", ref _worldName, value); }
        }

        public static Map Load(string filename)
        {
            var m = new Map();

            using (var b = new BinaryReader(File.OpenRead(filename)))
            {
                m.Version = b.ReadInt32();

                if (m.Version <= World.CompatibleVersion)
                {
                    m.WorldName = b.ReadString();
                    m.WorldId = b.ReadInt32();
                    m.MaxTilesY = b.ReadInt32();
                    m.MaxTilesX = b.ReadInt32();

                    m.Tiles = new MapTile[m.MaxTilesX, m.MaxTilesY];
                    for (int x = 0; x < m.MaxTilesX; x++)
                    {
                        // status
                        for (int y = 0; y < m.MaxTilesY; y++)
                        {
                            if (!b.ReadBoolean())
                            {
                                int rle = b.ReadInt16();

                                if (rle > 0)
                                {
                                    y = y + rle;
                                    if (m[x, y] != null)
                                    {
                                        m[x, y] = new MapTile();
                                    }
                                }
                            }
                            else
                            {
                                var curTile = new MapTile();
                                curTile.Type = b.ReadByte();
                                curTile.Light = b.ReadByte();
                                curTile.Misc = b.ReadByte();

                                if (m.Version < 50)
                                    curTile.Misc2 = 0;
                                else
                                    curTile.Misc2 = b.ReadByte();

                                m[x, y] = curTile;

                                int rle = b.ReadInt16();
                                if (curTile.Light == 255)
                                {
                                    if (rle > 0)
                                    {
                                        for (int k = y + 1; k < y + rle + 1; k++)
                                        {
                                            m[x, k] = curTile.Copy();
                                        }
                                        y = y + rle;
                                    }
                                }
                                else if (rle > 0)
                                {
                                    for (int k = y + 1; k < y + rle + 1; k++)
                                    {
                                        byte light = b.ReadByte();

                                        if (light > 18)
                                        {
                                            m[x, k] = curTile.Copy();
                                            m[x, k].Light = light;
                                        }
                                    }
                                    y = y + rle;
                                }
                            }
                        }
                    }
                    b.Close();
                }
            }

            return m;
        }

        public void Save(string filename)
        {
            string tempfileName = filename + ".tedittmp";

            using (var fs = new FileStream(tempfileName, FileMode.Create))
            using (var bw = new BinaryWriter(fs))
            {
                bw.Write(World.CompatibleVersion);
                bw.Write(WorldName);
                bw.Write(WorldId);
                bw.Write(MaxTilesY);
                bw.Write(MaxTilesX);

                for (int x = 0; x < MaxTilesX; x++)
                {
                    for (int y = 0; y < MaxTilesY; y++)
                    {
                        bool active = false;

                        if (Tiles[x, y] == null || Tiles[x, y].Light <= 18)
                        {
                            // save dark tiles
                            bw.Write(active);

                            // rle compression
                            int rle = 1;
                            while (y + rle < MaxTilesY && (Tiles[x, y + rle] == null || Tiles[x, y + rle].Light == 0))
                            {
                                rle++;
                            }
                            rle--;
                            bw.Write((short)rle);
                            y = y + rle;
                        }
                        else
                        {
                            // save revealed tiles
                            active = true;
                            bw.Write(active);
                            bw.Write(Tiles[x, y].Type);
                            bw.Write(Tiles[x, y].Light);
                            bw.Write(Tiles[x, y].Misc);
                            bw.Write(Tiles[x, y].Misc2);

                            // rle compression
                            int rle = 1;
                            if (Tiles[x, y].Light != 255)
                            {
                                while (y + rle < MaxTilesY &&
                                       Tiles[x, y + rle] != null &&
                                       Tiles[x, y + rle].Light > 18 &&
                                       Tiles[x, y + rle].IsSameAs(Tiles[x, y]))
                                {
                                    rle++;
                                }
                                rle--;
                                bw.Write((short)rle);

                                // save light if it isn't maxed
                                if (rle > 0)
                                {
                                    for (int k = y + 1; k < y + rle + 1; k++)
                                    {
                                        bw.Write(Tiles[x, k].Light);
                                    }
                                }
                                y = y + rle;
                            }
                            else
                            {
                                while (y + rle < MaxTilesY &&
                                       Tiles[x, y + rle] != null &&
                                       Tiles[x, y + rle].Light > 18 &&
                                       Tiles[x, y + rle].IsSameAs(Tiles[x, y]))
                                {
                                    rle++;
                                }
                                rle--;
                                bw.Write((short)rle);
                                y = y + rle;
                            }
                        }
                    }
                }
                bw.Close();
            }

            File.Copy(tempfileName, filename, true);
            File.Delete(tempfileName);
        }
    }
}