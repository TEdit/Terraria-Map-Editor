using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using TEditXna;

namespace TEditXNA.Terraria
{
    public class Textures
    {
        private readonly GraphicsDevice _gdDevice;
        private readonly Dictionary<int, Texture2D> _tiles = new Dictionary<int, Texture2D>();
        private readonly Dictionary<int, Texture2D> _backgrounds = new Dictionary<int, Texture2D>();
        private readonly Dictionary<int, Texture2D> _underworld = new Dictionary<int, Texture2D>();
        private readonly Dictionary<int, Texture2D> _walls = new Dictionary<int, Texture2D>();
        private readonly Dictionary<int, Texture2D> _trees = new Dictionary<int, Texture2D>();
        private readonly Dictionary<int, Texture2D> _treeTops = new Dictionary<int, Texture2D>();
        private readonly Dictionary<int, Texture2D> _treeBranches = new Dictionary<int, Texture2D>();
        private readonly Dictionary<int, Texture2D> _shrooms = new Dictionary<int, Texture2D>();
        private readonly Dictionary<int, Texture2D> _npcs = new Dictionary<int, Texture2D>();
        private readonly Dictionary<int, Texture2D> _liquids = new Dictionary<int, Texture2D>(); /* Heathtech */
        private readonly Dictionary<string, Texture2D> _misc = new Dictionary<string, Texture2D>(); /* Heathtech */
        private readonly Dictionary<int, Texture2D> _armorHead = new Dictionary<int, Texture2D>();
        private readonly Dictionary<int, Texture2D> _armorBody = new Dictionary<int, Texture2D>();
        private readonly Dictionary<int, Texture2D> _armorFemale = new Dictionary<int, Texture2D>();
        private readonly Dictionary<int, Texture2D> _armorLegs = new Dictionary<int, Texture2D>();
        private readonly Dictionary<int, Texture2D> _item = new Dictionary<int, Texture2D>();

        public Dictionary<int, Texture2D> Tiles { get { return _tiles; } }
        public Dictionary<int, Texture2D> Underworld { get { return _underworld; } }
        public Dictionary<int, Texture2D> Backgrounds { get { return _backgrounds; } }
        public Dictionary<int, Texture2D> Walls { get { return _walls; } }
        public Dictionary<int, Texture2D> Trees { get { return _trees; } }
        public Dictionary<int, Texture2D> TreeTops { get { return _treeTops; } }
        public Dictionary<int, Texture2D> TreeBranches { get { return _treeBranches; } }
        public Dictionary<int, Texture2D> Shrooms { get { return _shrooms; } }
        public Dictionary<int, Texture2D> Npcs { get { return _npcs; } }
        public Dictionary<int, Texture2D> Liquids { get { return _liquids; } } /* Heathtech */
        public Dictionary<string, Texture2D> Misc { get { return _misc; } } /* Heathtech */
        public Dictionary<int, Texture2D> ArmorHead { get { return _armorHead; } }
        public Dictionary<int, Texture2D> ArmorBody { get { return _armorBody; } }
        public Dictionary<int, Texture2D> ArmorFemale { get { return _armorFemale; } }
        public Dictionary<int, Texture2D> ArmorLegs { get { return _armorLegs; } }
        public Dictionary<int, Texture2D> Item { get { return _item; } }
        public Texture2D Actuator { get { return _actuator ?? (_actuator = (Texture2D)GetMisc("Actuator")); } }

        readonly ContentManager _cm;
        public ContentManager ContentManager
        {
            get { return _cm; }
        }
        public Textures(IServiceProvider serviceProvider, GraphicsDevice gdDevice)
        {
            _gdDevice = gdDevice;
            string path = TEditXna.DependencyChecker.PathToContent;

            _defaultTexture = new Texture2D(_gdDevice, 1, 1);
            _defaultTexture.SetData(new Color[] { Color.Transparent });

            if (Directory.Exists(path))
            {
                _cm = new ContentManager(serviceProvider, path);
            }
        }
        public bool Valid
        {
            get { return _cm != null; }
        }
        public Texture2D GetTile(int num)
        {
            if (!Tiles.ContainsKey(num))
            {
                try
                {
                    string name = String.Format("Images\\Tiles_{0}", num);
                    Tiles[num] = LoadTexture(name);
                }
                catch
                {
                    string name = String.Format("CustomGFX\\Tiles_{0}", num + 50);
                    Tiles[num] = LoadTexture(name);
                }
            }
            return Tiles[num];
        }
        public Texture2D GetUnderworld(int num)
        {
            if (!Underworld.ContainsKey(num))
            {
                string name = String.Format("Images\\Backgrounds\\Underworld {0}", num);
                Underworld[num] = LoadTexture(name);
            }
            return Underworld[num];
        }
        public Texture2D GetBackground(int num)
        {
            if (!Backgrounds.ContainsKey(num))
            {
                string name = String.Format("Images\\Background_{0}", num);
                Backgrounds[num] = LoadTexture(name);
            }
            return Backgrounds[num];
        }
        public Texture2D GetWall(int num)
        {
            if (!Walls.ContainsKey(num))
            {
                string name = String.Format("Images\\Wall_{0}", num);
                Walls[num] = LoadTexture(name);
            }
            return Walls[num];
        }
        public Texture2D GetTree(int num)
        {
            if (!Trees.ContainsKey(num))
            {
                if (num >= 0)
                {
                    string name = String.Format("Images\\Tiles_5_{0}", num);
                    Trees[num] = LoadTexture(name);
                }
                else
                {
                    string name = "Images\\Tiles_5";
                    Trees[num] = LoadTexture(name);
                }
            }
            return Trees[num];
        }
        public Texture GetTreeTops(int num)
        {
            if (!TreeTops.ContainsKey(num))
            {
                string name = String.Format("Images\\Tree_Tops_{0}", num);
                TreeTops[num] = LoadTexture(name);
            }
            return TreeTops[num];
        }
        public Texture GetTreeBranches(int num)
        {
            if (!TreeBranches.ContainsKey(num))
            {
                string name = String.Format("Images\\Tree_Branches_{0}", num);
                TreeBranches[num] = LoadTexture(name);
            }
            return TreeBranches[num];
        }
        public Texture GetShroomTop(int num)
        {
            if (!Shrooms.ContainsKey(num))
            {
                string name = String.Format("Images\\Shroom_Tops");
                Shrooms[num] = LoadTexture(name);
            }
            return Shrooms[num];
        }
        public Texture GetNPC(int num)
        {
            if (!Npcs.ContainsKey(num))
            {
                string name = String.Format("Images\\NPC_{0}", num);
                Npcs[num] = LoadTexture(name);
            }
            return Npcs[num];
        }
        /* Heathtech */
        public Texture GetLiquid(int num)
        {
            if (!Liquids.ContainsKey(num))
            {
                string name = String.Format("Images\\Liquid_{0}", num);
                Liquids[num] = LoadTexture(name);
            }
            return Liquids[num];
        }
        /* Heathtech */
        public Texture GetMisc(string name)
        {
            if (!Misc.ContainsKey(name))
            {
                string texName = String.Format("Images\\{0}", name);
                Misc[name] = LoadTexture(texName);
            }
            return Misc[name];
        }
        public Texture GetArmorHead(int num)
        {
            if (!ArmorHead.ContainsKey(num))
            {
                string name = String.Format("Images\\Armor_Head_{0}", num);
                ArmorHead[num] = LoadTexture(name);
            }
            return ArmorHead[num];
        }
        public Texture GetArmorBody(int num)
        {
            if (!ArmorBody.ContainsKey(num))
            {
                string name = String.Format("Images\\Armor_Body_{0}", num);
                ArmorBody[num] = LoadTexture(name);
            }
            return ArmorBody[num];
        }
        public Texture GetArmorFemale(int num)
        {
            if (!ArmorFemale.ContainsKey(num))
            {
                string name = String.Format("Images\\Female_Body_{0}", num);
                ArmorFemale[num] = LoadTexture(name);
            }
            return ArmorFemale[num];
        }
        public Texture GetArmorLegs(int num)
        {
            if (!ArmorLegs.ContainsKey(num))
            {
                string name = String.Format("Images\\Armor_Legs_{0}", num);
                ArmorLegs[num] = LoadTexture(name);
            }
            return ArmorLegs[num];
        }
        public Texture2D GetItem(int num)
        {
            if (!Item.ContainsKey(num))
            {
                string name = String.Format("Images\\Item_{0}", num);
                Item[num] = LoadTexture(name);
            }
            return Item[num];
        }

        private static Color ColorKey = Color.FromNonPremultiplied(247, 119, 249, 255);
        private Texture2D _defaultTexture;
        private Texture2D _actuator;
        private readonly Rectangle _zeroSixteenRectangle = new Rectangle(0, 0, 16, 16);
        public Rectangle ZeroSixteenRectangle { get { return _zeroSixteenRectangle; } }

        private Texture2D LoadTexture(string path)
        {
            try
            {
                var loadTexture = _cm.Load<Texture2D>(path);
                var pixels = new Color[loadTexture.Height * loadTexture.Width];
                loadTexture.GetData(pixels);
                for (int i = 0; i < pixels.Length; i++)
                {
                    if (pixels[i] == Color.Magenta || pixels[i] == ColorKey)
                    {
                        pixels[i] = Color.Transparent;
                    }
                }
                loadTexture.SetData(pixels);
                return loadTexture;
            }
            catch (Exception err)
            {
                ErrorLogging.Log(string.Format("Failed to load texture: {0}", path));
                ErrorLogging.Log(err.Message);
            }

            return _defaultTexture;
        }
    }
}