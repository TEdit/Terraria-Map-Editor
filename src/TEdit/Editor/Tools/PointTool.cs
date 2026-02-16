using System;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TEdit.Geometry;
using TEdit.Terraria;
using TEdit.UI;
using TEdit.ViewModel;
using Wpf.Ui.Controls;

namespace TEdit.Editor.Tools;

public sealed class PointTool : BaseTool
{
    public PointTool(WorldViewModel worldViewModel)
        : base(worldViewModel)
    {
        Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEdit;component/Images/Tools/point.png"));
        SymbolIcon = SymbolRegular.Location24;
        Name = "Point";
        IsActive = false;
        ToolType = ToolType.Npc;
    }

    public override void MouseDown(TileMouseState e)
    {
        var actions = GetActiveActions(e);
        if (actions.Contains("editor.draw"))
        {
            NPC npc = _wvm.CurrentWorld.NPCs.FirstOrDefault(n => n.Name == _wvm.SelectedPoint);

            if (npc != null)
            {
                npc.Home = e.Location;
                npc.Position = new Vector2FloatObservable(e.Location.X * 16, e.Location.Y * 16);
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
                else if (_wvm.SelectedPoint != null && _wvm.SelectedPoint.StartsWith("Team ", StringComparison.InvariantCultureIgnoreCase))
                {
                    string teamName = _wvm.SelectedPoint.Substring(5);
                    int teamIndex = Array.IndexOf(World.TeamNames, teamName);
                    if (teamIndex >= 0 && teamIndex < _wvm.CurrentWorld.TeamSpawns.Count)
                    {
                        _wvm.CurrentWorld.TeamSpawns[teamIndex].X = e.Location.X;
                        _wvm.CurrentWorld.TeamSpawns[teamIndex].Y = e.Location.Y;
                    }
                }
            }
        }
    }
}
