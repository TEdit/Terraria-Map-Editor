using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TEdit.ViewModel;

namespace TEdit.View;

/// <summary>
/// Thin wrapper around <see cref="SpriteBatch"/> that transparently applies
/// grayscale conversion when <see cref="ForceGrayscale"/> is set.
/// The non-grayscale fast path is aggressively inlined for zero overhead.
/// </summary>
internal sealed class FilteredSpriteBatch
{
    private readonly SpriteBatch _inner;
    private bool _beginCalled;

    public FilteredSpriteBatch(SpriteBatch inner) => _inner = inner;

    public GraphicsDevice GraphicsDevice => _inner.GraphicsDevice;

    /// <summary>
    /// When true, all Draw calls automatically convert the source texture region
    /// to grayscale and apply grayscale tinting to the color.
    /// </summary>
    public bool ForceGrayscale { get; set; }

    public void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState,
        DepthStencilState depthStencilState, RasterizerState rasterizerState)
    {
        _inner.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState);
        _beginCalled = true;
    }

    public void End()
    {
        _inner.End();
        _beginCalled = false;
    }

    /// <summary>
    /// Slow path for grayscale rendering. Kept separate so the fast path stays small for inlining.
    /// Falls back to normal draw if source rect is degenerate or out of texture bounds.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void DrawGrayscale(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle,
        Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth)
    {
        if (!_beginCalled || !sourceRectangle.HasValue) return;

        var src = sourceRectangle.Value;
        if (src.Width > 0 && src.Height > 0
            && src.X >= 0 && src.Y >= 0
            && src.Right <= texture.Width && src.Bottom <= texture.Height)
        {
            var grayTex = GrayscaleManager.GrayscaleCache.GetOrCreate(GraphicsDevice, texture, src);
            _inner.Draw(grayTex, destinationRectangle, new Rectangle(0, 0, src.Width, src.Height),
                GrayscaleManager.ToGrayscale(color), rotation, origin, effects, layerDepth);
        }
        else
        {
            _inner.Draw(texture, destinationRectangle, sourceRectangle, color, rotation, origin, effects, layerDepth);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void DrawGrayscale(Texture2D texture, Vector2 position, Rectangle? sourceRectangle,
        Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
    {
        if (!_beginCalled || !sourceRectangle.HasValue) return;

        var src = sourceRectangle.Value;
        if (src.Width > 0 && src.Height > 0
            && src.X >= 0 && src.Y >= 0
            && src.Right <= texture.Width && src.Bottom <= texture.Height)
        {
            var grayTex = GrayscaleManager.GrayscaleCache.GetOrCreate(GraphicsDevice, texture, src);
            _inner.Draw(grayTex, position, new Rectangle(0, 0, src.Width, src.Height),
                GrayscaleManager.ToGrayscale(color), rotation, origin, scale, effects, layerDepth);
        }
        else
        {
            _inner.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void DrawGrayscale(Texture2D texture, Vector2 position, Rectangle? sourceRectangle,
        Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
    {
        if (!_beginCalled || !sourceRectangle.HasValue) return;

        var src = sourceRectangle.Value;
        if (src.Width > 0 && src.Height > 0
            && src.X >= 0 && src.Y >= 0
            && src.Right <= texture.Width && src.Bottom <= texture.Height)
        {
            var grayTex = GrayscaleManager.GrayscaleCache.GetOrCreate(GraphicsDevice, texture, src);
            _inner.Draw(grayTex, position, new Rectangle(0, 0, src.Width, src.Height),
                GrayscaleManager.ToGrayscale(color), rotation, origin, scale, effects, layerDepth);
        }
        else
        {
            _inner.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
        }
    }

    // Overload 1: Rectangle destination (walls, tiles, wires, liquids — most calls)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle,
        Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth)
    {
        if (!ForceGrayscale)
        {
            _inner.Draw(texture, destinationRectangle, sourceRectangle, color, rotation, origin, effects, layerDepth);
            return;
        }
        DrawGrayscale(texture, destinationRectangle, sourceRectangle, color, rotation, origin, effects, layerDepth);
    }

    // Overload 2: Vector2 destination with float scale (slopes, tree entities, previews)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle,
        Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
    {
        if (!ForceGrayscale)
        {
            _inner.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
            return;
        }
        DrawGrayscale(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
    }

    // Overload 3: Vector2 destination with Vector2 scale (NPC overlays, spawn/dungeon markers)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle,
        Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
    {
        if (!ForceGrayscale)
        {
            _inner.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
            return;
        }
        DrawGrayscale(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
    }
}
