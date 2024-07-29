using System.Threading;
using TEdit.Editor.Plugins;
using TEdit.Editor.Tools;

namespace TEdit.ViewModel;

public static class ViewModelLocator
{
    static ViewModelLocator()
    {
        _worldViewModel = CreateWorldViewModel();
    }

    private static WorldViewModel _worldViewModel;
    public static WorldViewModel WorldViewModel
    {
        get
        {
            if (_worldViewModel != null) return _worldViewModel;

            WorldViewModel temp = CreateWorldViewModel();
            Interlocked.CompareExchange(ref _worldViewModel, temp, null);
            return _worldViewModel;
        }
    }
    
    public static BestiaryViewModel GetBestiaryViewModel() => new BestiaryViewModel();
    public static CreativePowersViewModel GetCreativePowersViewModel() => new CreativePowersViewModel();

    private static WorldViewModel CreateWorldViewModel()
    {
        var wvm = new WorldViewModel();
        wvm.Tools.Add(new PasteTool(wvm));
        var defaultTool = new ArrowTool(wvm);
        wvm.Tools.Add(defaultTool);
        wvm.Tools.Add(new SelectionTool(wvm));
        wvm.Tools.Add(new PickerTool(wvm));
        wvm.Tools.Add(new PencilTool(wvm));
        wvm.Tools.Add(new BrushTool(wvm));
        wvm.Tools.Add(new HammerAreaTool(wvm));
        //wvm.Tools.Add(new BiomeTool(wvm));
        wvm.Tools.Add(new FillTool(wvm));
        wvm.Tools.Add(new PointTool(wvm));
        //wvm.Tools.Add(new SpriteTool(wvm));
        wvm.Tools.Add(new SpriteTool2(wvm));
        wvm.Tools.Add(new MorphTool(wvm));
        wvm.ActiveTool = defaultTool;

        //Sorted by Plugin-Name
        wvm.Plugins.Add(new BlockShufflePlugin(wvm));
        wvm.Plugins.Add(new CleanseWorldPlugin(wvm));
        wvm.Plugins.Add(new FindChestWithPlugin(wvm));
        wvm.Plugins.Add(new FindPlanteraBulbPlugin(wvm));
        wvm.Plugins.Add(new FindTileWithPlugin(wvm));
        wvm.Plugins.Add(new HouseGenPlugin(wvm));
        wvm.Plugins.Add(new ImageToPixelartEditor(wvm));
        wvm.Plugins.Add(new PlayerMapRenderer(wvm));
        wvm.Plugins.Add(new RandomizerPlugin(wvm));
        wvm.Plugins.Add(new RemoveAllChestsPlugin(wvm));
        wvm.Plugins.Add(new RemoveAllUnlockedChestsPlugin(wvm));
        wvm.Plugins.Add(new RemoveTileWithPlugin(wvm));
        wvm.Plugins.Add(new ReplaceAllPlugin(wvm));
        wvm.Plugins.Add(new SandSettlePlugin(wvm));
        wvm.Plugins.Add(new SimpleOreGeneratorPlugin(wvm));
        wvm.Plugins.Add(new SpriteDebuggerPlugin(wvm));
        wvm.Plugins.Add(new TextStatuePlugin(wvm));
        wvm.Plugins.Add(new UnlockAllChestsPlugin(wvm));
        
        return wvm;
    }
}
