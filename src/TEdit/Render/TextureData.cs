using System;

namespace TEdit.Render;

public class TextureData
{
    public UInt32[] Pixel;
    public int Width;
    public int Height;

    public TextureData(int width, int height)
    {
        Width = width;
        Height = height;
        Pixel = new UInt32[width*height];
    }

    public void SetSize(int width, int height)
    {
        Width = width;
        Height = height;
        Pixel = new UInt32[width*height];
        Update();
    }

    public event EventHandler Updated;
    
    protected virtual void OnUpdated(object sender, EventArgs e)
    {
        if (Updated != null) Updated(sender, e);
    }

    public void Update()
    {
        OnUpdated(this, new EventArgs());
    }
}