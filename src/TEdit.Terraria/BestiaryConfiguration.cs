using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TEdit.Terraria.DataModel;

namespace TEdit.Terraria;


public class BestiaryConfiguration
{
    public BestiaryNpcConfiguration Configuration { get; private set; } = new();

    public Dictionary<int, BestiaryNpcData> NpcById { get; private set; } = new();
    public Dictionary<string, BestiaryNpcData> NpcData { get; private set; } = new();

    public List<string> BestiaryTalkedIDs { get; private set; } = new();
    public List<string> BestiaryNearIDs { get; private set; } = new();
    public List<string> BestiaryKilledIDs { get; private set; } = new();

    public static BestiaryConfiguration LoadJson(string fileName)
    {
        using (StreamReader file = File.OpenText(fileName))
        using (JsonTextReader reader = new JsonTextReader(file))
        {
            JsonSerializer serializer = new JsonSerializer();
            var npcConfig = serializer.Deserialize<BestiaryNpcConfiguration>(reader);
            var bestiaryData = new BestiaryConfiguration();
            bestiaryData.Init(npcConfig);

            return bestiaryData;
        }
    }

    private void Init(BestiaryNpcConfiguration npcConfig)
    {
        Configuration = npcConfig;
        NpcData.Clear();
        foreach (var npc in npcConfig.NpcData)
        {
            NpcById[npc.Id] = npc;
            NpcData[npc.BestiaryId] = npc;
        }

        BestiaryTalkedIDs.Clear();
        BestiaryTalkedIDs.AddRange(NpcData.Where(npc => npc.Value.CanTalk).Select(npc => npc.Value.BestiaryId));

        BestiaryNearIDs.Clear();
        BestiaryNearIDs.AddRange(NpcData.Where(npc => npc.Value.IsCritter).Select(npc => npc.Value.BestiaryId));

        BestiaryKilledIDs.Clear();
        BestiaryKilledIDs.AddRange(NpcData.Where(npc => npc.Value.IsKillCredit).Select(npc => npc.Value.BestiaryId));
    }

    /// <summary>
    /// Create a BestiaryConfiguration from pre-populated lookup data (bridge pattern).
    /// </summary>
    public static BestiaryConfiguration FromBridge(
        BestiaryNpcConfiguration configuration,
        Dictionary<int, BestiaryNpcData> npcById,
        Dictionary<string, BestiaryNpcData> npcData,
        List<string> bestiaryTalkedIDs,
        List<string> bestiaryNearIDs,
        List<string> bestiaryKilledIDs)
    {
        var result = new BestiaryConfiguration
        {
            Configuration = configuration,
            NpcById = npcById,
            NpcData = npcData,
            BestiaryTalkedIDs = bestiaryTalkedIDs,
            BestiaryNearIDs = bestiaryNearIDs,
            BestiaryKilledIDs = bestiaryKilledIDs,
        };
        return result;
    }
}
