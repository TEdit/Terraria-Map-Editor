using System;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TEdit.Geometry;
using TEdit.Terraria;
using TEdit.UI;
using TEdit.ViewModel;

namespace TEdit.Editor.Tools;

public sealed class PointTool : BaseTool
{
    public PointTool(WorldViewModel worldViewModel)
        : base(worldViewModel)
    {
        Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEdit;component/Images/Tools/point.png"));
        Name = "Point";
        IsActive = false;
        ToolType = ToolType.Npc;
    }

    public override void MouseDown(TileMouseState e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            NPC npc = _wvm.CurrentWorld.NPCs.FirstOrDefault(n => n.Name == _wvm.SelectedPoint);

            if (npc != null)
            {
                npc.Home = e.Location;
                npc.Position = new Vector2Float(e.Location.X * 16, e.Location.Y * 16);
            }
            else
            {
                if (string.Equals(_wvm.SelectedPoint, "Spawn", StringComparison.InvariantCultureIgnoreCase))
                {
                    _wvm.CurrentWorld.SpawnX = e.Location.X;
                    _wvm.CurrentWorld.SpawnY = e.Location.Y;
                }
                else if (string.Equals(_wvm.SelectedPoint, "Dungeon", StringComparison.InvariantCultureIgnoreCase))
                {
                    _wvm.CurrentWorld.DungeonX = e.Location.X;
                    _wvm.CurrentWorld.DungeonY = e.Location.Y;
                }
            }
        }
    }
}