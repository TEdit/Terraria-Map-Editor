using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TEdit.Geometry;
using TEdit.Terraria;
using TEdit.Terraria.Objects;

namespace TEdit.Render;


public class Textures
{
    private readonly GraphicsDevice _gdDevice;

    // Deferred texture loading infrastructure
    private readonly TextureLoadingState _loadingState = new();
    private readonly ConcurrentQueue<Action> _graphicsThreadQueue = new();

    /// <summary>
    /// State for tracking async texture loading progress.
    /// </summary>
    public TextureLoadingState LoadingState => _loadingState;

    /// <summary>
    /// Whether async texture loading is complete.
    /// </summary>
    public bool TexturesFullyLoaded => _loadingState.IsComplete;

    public Dictionary<int, Texture2D> Gore { get; } = new Dictionary<int, Texture2D>();
    public Dictionary<int, Texture2D> Extra { get; } = new Dictionary<int, Texture2D>();
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
    public Dictionary<TownNpcKey, Texture2D> TownNpcs { get; } = new();
    public Dictionary<int, Texture2D> Liquids { get; } = new Dictionary<int, Texture2D>();
    public Dictionary<string, Texture2D> Misc { get; } = new Dictionary<string, Texture2D>();
    public Dictionary<int, Texture2D> ArmorHead { get; } = new Dictionary<int, Texture2D>();
    public Dictionary<int, Texture2D> ArmorBody { get; } = new Dictionary<int, Texture2D>();
    public Dictionary<int, Texture2D> ArmorFemale { get; } = new Dictionary<int, Texture2D>();
    public Dictionary<int, Texture2D> ArmorLegs { get; } = new Dictionary<int, Texture2D>();
    public Dictionary<int, Texture2D> Item { get; } = new Dictionary<int, Texture2D>();
    public Texture2D Actuator { get { return _actuator ??= (Texture2D)GetMisc("Actuator"); } }

    public ContentManager ContentManager { get; }

    public Textures(IServiceProvider serviceProvider, GraphicsDevice gdDevice)
    {
        _gdDevice = gdDevice;
        string path = DependencyChecker.PathToContent;

        _defaultTexture = new Texture2D(_gdDevice, 1, 1);
        _defaultTexture.SetData(new Color[] { Color.Transparent });

        _whitePixelTexture = new Texture2D(_gdDevice, 1, 1);
        _whitePixelTexture.SetData(new Color[] { Color.White });

        if (Directory.Exists(path))
        {
            try
            {
                // try and load a single file, this checks for read access
                using (StreamReader sr = new StreamReader(Path.Combine(path, "Images\\NPC_0.xnb")))
                {
                    sr.BaseStream.ReadByte();
                }

                ContentManager = new ContentManager(serviceProvider, path);

            }
            catch (Exception ex)
            {
                ErrorLogging.LogException(ex);
                System.Windows.Forms.MessageBox.Show($"Error loading textures from {path}.\r\nPlease check that this folder exists and TEdit has read access.\r\n\r\n{ex.Message}", "Error Loading Textures", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }

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

    public Texture2D GetNPC(int num) => GetTextureById(Npcs, num, "Images\\NPC_{0}");

    public Texture2D GetTownNPC(string name, int npcId, int variant = 0, bool partying = false, bool shimmered = false)
    {
        var key = new TownNpcKey(name, variant, partying, shimmered);


        if (!TownNpcs.ContainsKey(key))
        {
            string variantName = "Default";
            if (WorldConfiguration.NpcById.TryGetValue(npcId, out var npcData)
                && npcData.Variants != null
                && variant >= 0
                && variant < npcData.Variants.Count)
            {
                variantName = npcData.Variants[variant];
            }
            if (name == "DD2Bartender")
            {
                name = "Tavernkeep";
            }
            else if (name == "SantaClaus")
            {
                name = "Santa";
            }
            else if (name.StartsWith("Town"))
            {
                name = name.Substring(4);
            }



            string basePath = $"Images\\TownNPCs\\{name}_{variantName}";
            string texturePath = basePath;

            if (partying && shimmered)
            {
                texturePath = $"Images\\TownNPCs\\Shimmered\\{name}_{variantName}_Party";
            }
            else if (shimmered)
            {
                texturePath = $"Images\\TownNPCs\\Shimmered\\{name}_{variantName}";
            }
            else if (partying)
            {
                texturePath = $"Images\\TownNPCs\\{name}_{variantName}_Party";
            }

            var tex = LoadTexture(texturePath);

            // Fallback chain: party/shimmer variant → base town NPC texture → NPC_{id}
            // Note: LoadTexture returns _defaultTexture (1x1 transparent) for missing files, never null
            if ((tex == null || tex == _defaultTexture) && texturePath != basePath)
            {
                tex = LoadTexture(basePath);
            }
            if (tex == null || tex == _defaultTexture)
            {
                tex = LoadTexture($"Images\\NPC_{npcId}");
            }

            TownNpcs[key] = tex;
        }

        return TownNpcs[key];
    }


    public Texture GetLiquid(int num) => GetTextureById(Liquids, num, "Images\\Liquid_{0}");

    public Texture GetMisc(string name) => GetTextureById(Misc, name, "Images\\{0}");

    public Texture GetArmorHead(int num) => GetTextureById(ArmorHead, num, "Images\\Armor_Head_{0}");

    public Texture GetArmorBody(int num) => GetTextureById(ArmorBody, num, "Images\\Armor\\Armor_{0}");

    public Texture GetArmorFemale(int num) => GetTextureById(ArmorFemale, num, "Images\\Armor\\Armor_Female_{0}");

    public Texture GetArmorLegs(int num) => GetTextureById(ArmorLegs, num, "Images\\Armor_Legs_{0}");

    public Texture2D GetItem(int num) => GetTextureById(Item, num, "Images\\Item_{0}");

    public Texture2D GetMoon(int num) => GetTextureById(Moon, num, "Images\\Moon_{0}");

    public Texture2D GetExtra(int num) => GetTextureById(Extra, num, "Images\\Extra_{0}");

    public Texture2D GetGore(int num) => GetTextureById(Gore, num, "Images\\Gore_{0}");

    /// <summary>
    /// Get texture for preview based on PreviewConfig settings.
    /// </summary>
    public Texture2D GetPreviewTexture(PreviewConfig config, int tileId)
    {
        if (config == null)
            return GetTile(tileId);

        return config.TextureType switch
        {
            PreviewTextureType.Tree => GetTree(config.TextureStyle),
            PreviewTextureType.TreeTops => (Texture2D)GetTreeTops(config.TextureStyle),
            PreviewTextureType.TreeBranch => (Texture2D)GetTreeBranches(config.TextureStyle),
            PreviewTextureType.PalmTree => GetTile(323), // Palm tree trunk texture
            PreviewTextureType.PalmTreeTop => (Texture2D)GetTreeTops(15), // Palm tree top texture
            PreviewTextureType.Item => GetItem(config.TextureStyle),
            _ => GetTile(tileId)
        };
    }

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
    private Texture2D _whitePixelTexture;
    private Texture2D _actuator;
    private readonly Rectangle _zeroSixteenRectangle = new Rectangle(0, 0, 16, 16);
    public Rectangle ZeroSixteenRectangle { get { return _zeroSixteenRectangle; } }

    /// <summary>
    /// Get the default transparent texture (used when textures are not yet loaded).
    /// </summary>
    public Texture2D DefaultTexture => _defaultTexture;

    /// <summary>
    /// Get a 1x1 white pixel texture (used for solid color drawing).
    /// </summary>
    public Texture2D WhitePixelTexture => _whitePixelTexture;

    /// <summary>
    /// Queue a texture creation action to be executed on the graphics thread.
    /// </summary>
    /// <param name="createAction">Action that creates a texture (must be called on graphics thread)</param>
    public void QueueTextureCreation(Action createAction)
    {
        _graphicsThreadQueue.Enqueue(createAction);
    }

    /// <summary>
    /// Process queued texture creations on the graphics thread.
    /// Call this from the render loop.
    /// </summary>
    /// <param name="maxOperationsPerFrame">Maximum number of texture creations per frame</param>
    /// <returns>Number of operations processed</returns>
    public int ProcessTextureQueue(int maxOperationsPerFrame = 5)
    {
        int processed = 0;

        while (processed < maxOperationsPerFrame && _graphicsThreadQueue.TryDequeue(out var action))
        {
            try
            {
                action();
                processed++;
            }
            catch (Exception ex)
            {
                ErrorLogging.LogException(ex);
            }
        }

        return processed;
    }

    /// <summary>
    /// Load a texture immediately (for use during deferred loading).
    /// Must be called on the graphics thread.
    /// </summary>
    /// <param name="path">Texture path (without extension)</param>
    /// <returns>Loaded texture or default texture on failure</returns>
    public Texture2D LoadTextureImmediate(string path)
    {
        try
        {
            string texturePath = Path.Join(ContentManager.RootDirectory, path + ".xnb");

            if (!File.Exists(texturePath))
            {
                ErrorLogging.LogWarn($"Missing texture: {path}");
                return _defaultTexture;
            }

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
            ErrorLogging.LogWarn($"Failed to load texture: {path}");
            ErrorLogging.LogWarn(err.Message);
        }

        return _defaultTexture;
    }

    /// <summary>
    /// Private wrapper for backward compatibility with existing code.
    /// </summary>
    private Texture2D LoadTexture(string path) => LoadTextureImmediate(path);

    /// <summary>
    /// Computes and caches WrapThreshold values for all tiles with TextureWrap configured.
    /// Call this after tile textures have been loaded.
    /// </summary>
    public void CacheTextureWrapThresholds()
    {
    }
}
