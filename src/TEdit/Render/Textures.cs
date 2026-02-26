using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TEdit.Terraria;
using TEdit.Terraria.Objects;

namespace TEdit.Render;


public class Textures : IDisposable
{
    private bool _disposed;
    private readonly GraphicsDevice _gdDevice;
    private readonly IServiceProvider _serviceProvider;

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
    public Dictionary<int, Texture2D> GlowMasks { get; } = new Dictionary<int, Texture2D>();
    public Dictionary<int, Texture2D> Liquids { get; } = new Dictionary<int, Texture2D>();
    public Dictionary<string, Texture2D> Misc { get; } = new Dictionary<string, Texture2D>();
    public Dictionary<int, Texture2D> ArmorHead { get; } = new Dictionary<int, Texture2D>();
    public Dictionary<int, Texture2D> ArmorBody { get; } = new Dictionary<int, Texture2D>();
    public Dictionary<int, Texture2D> ArmorFemale { get; } = new Dictionary<int, Texture2D>();
    public Dictionary<int, Texture2D> ArmorLegs { get; } = new Dictionary<int, Texture2D>();
    public Dictionary<int, Texture2D> Item { get; } = new Dictionary<int, Texture2D>();
    public Dictionary<string, Texture2D> PlayerBody { get; } = new Dictionary<string, Texture2D>();
    public Dictionary<int, Texture2D> PlayerHair { get; } = new Dictionary<int, Texture2D>();

    // Accessory texture dictionaries
    public Dictionary<int, Texture2D> AccWings { get; } = new();
    public Dictionary<int, Texture2D> AccBack { get; } = new();
    public Dictionary<int, Texture2D> AccBalloon { get; } = new();
    public Dictionary<int, Texture2D> AccShoes { get; } = new();
    public Dictionary<int, Texture2D> AccWaist { get; } = new();
    public Dictionary<int, Texture2D> AccNeck { get; } = new();
    public Dictionary<int, Texture2D> AccFace { get; } = new();
    public Dictionary<int, Texture2D> AccShield { get; } = new();
    public Dictionary<int, Texture2D> AccHandsOn { get; } = new();
    public Dictionary<int, Texture2D> AccHandsOff { get; } = new();
    public Dictionary<int, Texture2D> AccFront { get; } = new();
    public Texture2D Actuator { get { return _actuator ??= (Texture2D)GetMisc("Actuator"); } }

    public ContentManager ContentManager { get; }

    // Separate ContentManager for preview-only textures (items, NPCs, armor, player, accessories).
    // These are loaded to extract pixel data for preview bitmaps, then the entire CM is unloaded
    // to free GPU memory. The main ContentManager is never poisoned, so the renderer can
    // lazy-reload any texture it needs on demand.
    private ContentManager _previewContentManager;

    public Textures(IServiceProvider serviceProvider, GraphicsDevice gdDevice)
    {
        _gdDevice = gdDevice;
        _serviceProvider = serviceProvider;
        string path = DependencyChecker.PathToContent;

        _defaultTexture = new Texture2D(_gdDevice, 1, 1);
        _defaultTexture.SetData(new Color[] { Color.Transparent });

        _whitePixelTexture = new Texture2D(_gdDevice, 1, 1);
        _whitePixelTexture.SetData(new Color[] { Color.White });

        // Generate 64x64 radial gradient for light halo effects
        const int haloSize = 64;
        _glowHaloTexture = new Texture2D(_gdDevice, haloSize, haloSize);
        var haloPixels = new Color[haloSize * haloSize];
        float center = (haloSize - 1) / 2f;
        for (int py = 0; py < haloSize; py++)
        {
            for (int px = 0; px < haloSize; px++)
            {
                float dx = (px - center) / center;
                float dy = (py - center) / center;
                float distSq = dx * dx + dy * dy;
                float alpha = Math.Max(0f, 1f - distSq);
                haloPixels[py * haloSize + px] = new Color(alpha, alpha, alpha, alpha);
            }
        }
        _glowHaloTexture.SetData(haloPixels);

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
                _previewContentManager = new ContentManager(serviceProvider, path);

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

    public Texture2D GetNPC(int num)
    {
        if (!Npcs.ContainsKey(num))
        {
            string name = string.Format("Images\\NPC_{0}", num);
            Npcs[num] = LoadTextureFrom(_previewContentManager ?? ContentManager, name);
        }
        return Npcs[num];
    }

    public Texture2D GetTownNPC(string name, int npcId, int variant = 0, bool partying = false, bool shimmered = false)
    {
        var key = new TownNpcKey(name, variant, partying, shimmered);

        if (!TownNpcs.ContainsKey(key))
        {
            var cm = _previewContentManager ?? ContentManager;

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

            var tex = LoadTextureFrom(cm, texturePath);

            // Fallback chain: party/shimmer variant → base town NPC texture → NPC_{id}
            // Note: LoadTextureFrom returns _defaultTexture for missing files, never null
            if ((tex == null || tex == _defaultTexture) && texturePath != basePath)
            {
                tex = LoadTextureFrom(cm, basePath);
            }
            if (tex == null || tex == _defaultTexture)
            {
                tex = LoadTextureFrom(cm, $"Images\\NPC_{npcId}");
            }

            TownNpcs[key] = tex;
        }

        return TownNpcs[key];
    }


    public Texture2D GetGlowMask(int id) => GetTextureById(GlowMasks, id, "Images\\Glow_{0}");

    public Texture GetLiquid(int num) => GetTextureById(Liquids, num, "Images\\Liquid_{0}");

    public Texture GetMisc(string name) => GetTextureById(Misc, name, "Images\\{0}");

    public Texture GetArmorHead(int num) => GetPreviewTextureById(ArmorHead, num, "Images\\Armor_Head_{0}");

    public Texture GetArmorBody(int num) => GetPreviewTextureById(ArmorBody, num, "Images\\Armor\\Armor_{0}");

    public Texture GetArmorFemale(int num) => GetPreviewTextureById(ArmorFemale, num, "Images\\Armor\\Armor_Female_{0}");

    public Texture GetArmorLegs(int num) => GetPreviewTextureById(ArmorLegs, num, "Images\\Armor_Legs_{0}");

    // Accessory texture getters
    public Texture2D GetAccWings(int num) => GetPreviewTextureById(AccWings, num, "Images\\Wings_{0}");
    public Texture2D GetAccBack(int num) => GetPreviewTextureById(AccBack, num, "Images\\Acc_Back_{0}");
    public Texture2D GetAccBalloon(int num) => GetPreviewTextureById(AccBalloon, num, "Images\\Acc_Balloon_{0}");
    public Texture2D GetAccShoes(int num) => GetPreviewTextureById(AccShoes, num, "Images\\Acc_Shoes_{0}");
    public Texture2D GetAccWaist(int num) => GetPreviewTextureById(AccWaist, num, "Images\\Acc_Waist_{0}");
    public Texture2D GetAccNeck(int num) => GetPreviewTextureById(AccNeck, num, "Images\\Acc_Neck_{0}");
    public Texture2D GetAccFace(int num) => GetPreviewTextureById(AccFace, num, "Images\\Acc_Face_{0}");
    public Texture2D GetAccShield(int num) => GetPreviewTextureById(AccShield, num, "Images\\Acc_Shield_{0}");
    public Texture2D GetAccHandsOn(int num) => GetPreviewTextureById(AccHandsOn, num, "Images\\Acc_HandsOn_{0}");
    public Texture2D GetAccHandsOff(int num) => GetPreviewTextureById(AccHandsOff, num, "Images\\Acc_HandsOff_{0}");
    public Texture2D GetAccFront(int num) => GetPreviewTextureById(AccFront, num, "Images\\Acc_Front_{0}");

    public Texture2D GetItem(int num)
    {
        if (!Item.ContainsKey(num))
        {
            string name = string.Format("Images\\Item_{0}", num);
            Item[num] = LoadTextureFrom(_previewContentManager ?? ContentManager, name);
        }
        return Item[num];
    }

    public Texture2D GetPlayerBody(int skinVariant, int partIndex)
    {
        // Try the specific variant first
        var key = $"{skinVariant}_{partIndex}";
        var texture = GetPreviewTextureById(PlayerBody, key, "Images\\Player_{0}");
        if (texture != DefaultTexture) return texture;

        // Fallback chain matching Terraria's PlayerDataInitializer CopyVariant logic:
        // Variants 1,2,3,8 → fall back to variant 0 (male base)
        // Variants 5,6,7,9 → fall back to variant 4 (female base)
        // Variant 4 → falls back to variant 0 for head parts (0,1,2,15)
        // Variants 10,11 (display dolls) → 10 falls back to 0, 11 falls back to 10
        int fallback = skinVariant switch
        {
            1 or 2 or 3 or 8 => 0,
            5 or 6 or 7 or 9 => 4,
            4 => 0,
            11 => 10,
            10 => 0,
            _ => -1
        };

        if (fallback >= 0)
        {
            var fbKey = $"{fallback}_{partIndex}";
            texture = GetPreviewTextureById(PlayerBody, fbKey, "Images\\Player_{0}");
            if (texture != DefaultTexture) return texture;

            // Second-level fallback: variant 4 → 0, variant 10 → 0, variant 11 → 10 → 0
            if (fallback == 4 || fallback == 10)
            {
                var fb2Key = $"0_{partIndex}";
                texture = GetPreviewTextureById(PlayerBody, fb2Key, "Images\\Player_{0}");
                if (texture != DefaultTexture) return texture;
            }
        }

        return DefaultTexture;
    }

    public Texture2D GetPlayerHair(int hairIndex)
    {
        if (!PlayerHair.ContainsKey(hairIndex))
        {
            // Terraria hair textures are 1-indexed: Player_Hair_1.xnb = hair style 0
            string name = $"Images\\Player_Hair_{hairIndex + 1}";
            PlayerHair[hairIndex] = LoadTextureFrom(_previewContentManager ?? ContentManager, name);
        }
        return PlayerHair[hairIndex];
    }

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

    private Texture2D GetPreviewTextureById<T>(Dictionary<T, Texture2D> collection, T id, string path)
    {
        if (!collection.ContainsKey(id))
        {
            string name = string.Format(path, id);
            collection[id] = LoadTextureFrom(_previewContentManager ?? ContentManager, name);
        }

        return collection[id];
    }

    private static Color ColorKey = Color.FromNonPremultiplied(247, 119, 249, 255);
    private Texture2D _defaultTexture;
    private Texture2D _whitePixelTexture;
    private Texture2D _actuator;
    private Texture2D _glowHaloTexture;
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
    /// Get a 64x64 radial gradient texture (white center, transparent edge) for light halo effects.
    /// </summary>
    public Texture2D GlowHaloTexture => _glowHaloTexture;

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
                ErrorLogging.LogDebug($"Missing texture: {path}");
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
    /// Load a texture using a specific ContentManager instance.
    /// </summary>
    private Texture2D LoadTextureFrom(ContentManager cm, string path)
    {
        try
        {
            string texturePath = Path.Join(cm.RootDirectory, path + ".xnb");
            if (!File.Exists(texturePath))
            {
                ErrorLogging.LogDebug($"Missing texture: {path}");
                return _defaultTexture;
            }

            var loadTexture = cm.Load<Texture2D>(path);
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
    /// Computes and caches WrapThreshold values for all tiles with TextureWrap configured.
    /// Call this after tile textures have been loaded.
    /// </summary>
    public void CacheTextureWrapThresholds()
    {
    }

    /// <summary>
    /// Unload all preview-only textures (items, NPCs, armor, player, accessories).
    /// Call after all previews have been generated.
    /// Clears dictionaries and disposes the preview ContentManager to fully release GPU resources.
    /// Future renderer access will lazy-reload via the main ContentManager.
    /// </summary>
    public void UnloadPreviewTextures()
    {
        int count = Item.Count + Npcs.Count + TownNpcs.Count
            + ArmorHead.Count + ArmorBody.Count + ArmorFemale.Count + ArmorLegs.Count
            + PlayerBody.Count + PlayerHair.Count
            + AccWings.Count + AccBack.Count + AccBalloon.Count + AccShoes.Count
            + AccWaist.Count + AccNeck.Count + AccFace.Count + AccShield.Count
            + AccHandsOn.Count + AccHandsOff.Count + AccFront.Count;

        Item.Clear();
        Npcs.Clear();
        TownNpcs.Clear();
        ArmorHead.Clear();
        ArmorBody.Clear();
        ArmorFemale.Clear();
        ArmorLegs.Clear();
        PlayerBody.Clear();
        PlayerHair.Clear();
        AccWings.Clear();
        AccBack.Clear();
        AccBalloon.Clear();
        AccShoes.Clear();
        AccWaist.Clear();
        AccNeck.Clear();
        AccFace.Clear();
        AccShield.Clear();
        AccHandsOn.Clear();
        AccHandsOff.Clear();
        AccFront.Clear();

        _previewContentManager?.Unload();
        _previewContentManager?.Dispose();
        _previewContentManager = null;
        ErrorLogging.LogDebug($"Unloaded {count} preview textures to free memory");
    }

    /// <summary>
    /// Dispose and clear a texture dictionary, skipping the default texture sentinel.
    /// </summary>
    private void DisposeAndClearDictionary<T>(Dictionary<T, Texture2D> dict)
    {
        foreach (var kvp in dict)
        {
            if (kvp.Value != null && kvp.Value != _defaultTexture && !kvp.Value.IsDisposed)
            {
                kvp.Value.Dispose();
            }
        }
        dict.Clear();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        DisposeAndClearDictionary(Tiles);
        DisposeAndClearDictionary(Walls);
        DisposeAndClearDictionary(Npcs);
        DisposeAndClearDictionary(TownNpcs);
        DisposeAndClearDictionary(Item);
        DisposeAndClearDictionary(Gore);
        DisposeAndClearDictionary(Extra);
        DisposeAndClearDictionary(Moon);
        DisposeAndClearDictionary(Underworld);
        DisposeAndClearDictionary(Backgrounds);
        DisposeAndClearDictionary(Trees);
        DisposeAndClearDictionary(TreeTops);
        DisposeAndClearDictionary(TreeBranches);
        DisposeAndClearDictionary(Shrooms);
        DisposeAndClearDictionary(GlowMasks);
        DisposeAndClearDictionary(Liquids);
        DisposeAndClearDictionary(Misc);
        DisposeAndClearDictionary(ArmorHead);
        DisposeAndClearDictionary(ArmorBody);
        DisposeAndClearDictionary(ArmorFemale);
        DisposeAndClearDictionary(ArmorLegs);
        DisposeAndClearDictionary(PlayerBody);
        DisposeAndClearDictionary(PlayerHair);
        DisposeAndClearDictionary(AccWings);
        DisposeAndClearDictionary(AccBack);
        DisposeAndClearDictionary(AccBalloon);
        DisposeAndClearDictionary(AccShoes);
        DisposeAndClearDictionary(AccWaist);
        DisposeAndClearDictionary(AccNeck);
        DisposeAndClearDictionary(AccFace);
        DisposeAndClearDictionary(AccShield);
        DisposeAndClearDictionary(AccHandsOn);
        DisposeAndClearDictionary(AccHandsOff);
        DisposeAndClearDictionary(AccFront);

        _defaultTexture?.Dispose();
        _whitePixelTexture?.Dispose();
        _glowHaloTexture?.Dispose();
        _actuator?.Dispose();

        _previewContentManager?.Unload();
        _previewContentManager?.Dispose();
        ContentManager?.Unload();
        ContentManager?.Dispose();
    }
}
