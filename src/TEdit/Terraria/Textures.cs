using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TEdit;

namespace TEdit.Terraria
{
    public class Textures
    {
        private readonly GraphicsDevice _gdDevice;

        public Dictionary<int, Texture2D> Moon { get; } = new Dictionary<int, Texture2D>();
        public Dictionary<int, Texture2D> Tiles { get; } = new Dictionary<int, Texture2D>();
        public Dictionary<int, Texture2D> Underworld { get; } = new Dictionary<int, Texture2D>();
        public Dictionary<int, Texture2D> Backgrounds { get; } = new Dictionary<int, Texture2D>();
        public Dictionary<int, Texture2D> Walls { get; } = new Dictionary<int, Texture2D>();
        public Dictionary<int, Texture2D> Trees { get; } = new Dictionary<int, Texture2D>();
        public Dictionary<int, Texture2D> TreeTops { get; } = new Dictionary<int, Texture2D>();
        public Dictionary<int, Texture2D> TreeBranches { get; } = new Dictionary<int, Texture2D>();
        public Dictionary<int, Texture2D> Shrooms { get; } = new Dictionary<int, Texture2D>();
        public Dictionary<int, Texture2D> Npcs { get; } = new Dictionary<int, Texture2D>();
        public Dictionary<int, Texture2D> Liquids { get; } = new Dictionary<int, Texture2D>(); 
        public Dictionary<string, Texture2D> Misc { get; } = new Dictionary<string, Texture2D>();
        public Dictionary<int, Texture2D> ArmorHead { get; } = new Dictionary<int, Texture2D>();
        public Dictionary<int, Texture2D> ArmorBody { get; } = new Dictionary<int, Texture2D>();
        public Dictionary<int, Texture2D> ArmorFemale { get; } = new Dictionary<int, Texture2D>();
        public Dictionary<int, Texture2D> ArmorLegs { get; } = new Dictionary<int, Texture2D>();
        public Dictionary<int, Texture2D> Item { get; } = new Dictionary<int, Texture2D>();
        public Texture2D Actuator { get { return _actuator ?? (_actuator = (Texture2D)GetMisc("Actuator")); } }

        public ContentManager ContentManager { get; }

        public Textures(IServiceProvider serviceProvider, GraphicsDevice gdDevice)
        {
            _gdDevice = gdDevice;
            string path = DependencyChecker.PathToContent;

            _defaultTexture = new Texture2D(_gdDevice, 1, 1);
            _defaultTexture.SetData(new Color[] { Color.Transparent });

            if (Directory.Exists(path))
            {
                ContentManager = new ContentManager(serviceProvider, path);
            }
        }

        public bool Valid => ContentManager != null;

        public Texture2D GetTile(int num) => GetTextureById(Tiles, num, "Images\\Tiles_{0}");

        public Texture2D GetUnderworld(int num) => GetTextureById(Underworld, num, "Images\\Backgrounds\\Underworld {0}");

        public Texture2D GetBackground(int num) => GetTextureById(Backgrounds, num, "Images\\Background_{0}");

        public Texture2D GetWall(int num) => GetTextureById(Walls, num, "Images\\Wall_{0}");

        public Texture2D GetTree(int num)
        {
            if (!Trees.ContainsKey(num))
            {
                if (num >= 0)
                {
                    string name = $"Images\\Tiles_5_{num}";
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
        public Texture GetTreeTops(int num) => GetTextureById(TreeTops, num, "Images\\Tree_Tops_{0}");

        public Texture GetTreeBranches(int num) => GetTextureById(TreeBranches, num, "Images\\Tree_Branches_{0}");

        public Texture GetShroomTop(int num) => GetTextureById(Shrooms, num, "Images\\Shroom_Tops");

        public Texture GetNPC(int num) => GetTextureById(Npcs, num, "Images\\NPC_{0}");

        public Texture GetLiquid(int num) => GetTextureById(Liquids, num, "Images\\Liquid_{0}");

        public Texture GetMisc(string name) => GetTextureById(Misc, name, "Images\\{0}");

        public Texture GetArmorHead(int num) => GetTextureById(ArmorHead, num, "Images\\Armor_Head_{0}");

        public Texture GetArmorBody(int num) => GetTextureById(ArmorBody, num, "Images\\Armor_Body_{0}");

        public Texture GetArmorFemale(int num) => GetTextureById(ArmorFemale, num, "Images\\Female_Body_{0}");

        public Texture GetArmorLegs(int num) => GetTextureById(ArmorLegs, num, "Images\\Armor_Legs_{0}");

        public Texture2D GetItem(int num) => GetTextureById(Item, num, "Images\\Item_{0}");

        public Texture2D GetMoon(int num) => GetTextureById(Moon, num, "Images\\Moon_{0}");

        private Texture2D GetTextureById<T>(Dictionary<T, Texture2D> collection, T id, string path)
        {
            if (!collection.ContainsKey(id))
            {
                string name = string.Format(path, id);
                collection[id] = LoadTexture(name);                
            }

            return collection[id];
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
                var loadTexture = ContentManager.Load<Texture2D>(path);
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
                ErrorLogging.Log($"Failed to load texture: {path}");
                ErrorLogging.Log(err.Message);
            }

            return _defaultTexture;
        }
    }
}