using System;
using System.Linq;
using System.Windows.Media.Imaging;
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
                if (string.Equals(_wvm.SelectedPoint, Properties.Language.tool_point_spawn, StringComparison.InvariantCultureIgnoreCase))
                {
                    _wvm.CurrentWorld.SpawnX = e.Location.X;
                    _wvm.CurrentWorld.SpawnY = e.Location.Y;
                }
                else if (string.Equals(_wvm.SelectedPoint, Properties.Language.tool_point_dungeon, StringComparison.InvariantCultureIgnoreCase))
                {
                    _wvm.CurrentWorld.DungeonX = e.Location.X;
                    _wvm.CurrentWorld.DungeonY = e.Location.Y;
                }
                else if (_wvm.SelectedPoint != null)
                {
                    for (int teamIndex = 0; teamIndex < World.TeamCount; teamIndex++)
                    {
                        if (string.Equals(_wvm.SelectedPoint, WorldViewModel.GetTeamPointName(teamIndex), StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (teamIndex < _wvm.CurrentWorld.TeamSpawns.Count)
                            {
                                _wvm.CurrentWorld.TeamSpawns[teamIndex].X = e.Location.X;
                                _wvm.CurrentWorld.TeamSpawns[teamIndex].Y = e.Location.Y;
                            }
                            break;
                        }
                    }
                }
            }
        }
    }
}
