using System.Linq;
using TEdit.Common.Reactive;

namespace TEdit.Terraria.Objects;

public class NpcName : ObservableObject
{
    public NpcName()
    {
        
    }
    public NpcName(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; set; }
    public string Name { get; set; }
}
