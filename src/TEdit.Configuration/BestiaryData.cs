using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TEdit.Configuration
{
    public class NpcData
    {
        public int Id { get; set; }
        public int BannerId { get; set; }
        public string FullName { get; set; }
        public string Name { get; set; }
        public string BestiaryId { get; set; }
        public bool CanTalk { get; set; }
        public bool IsCritter { get; set; }
        public bool IsTownNpc { get; set; }
        public bool IsKillCredit { get; set; }
        public int BestiaryDisplayIndex { get; set; }
    }

    public class NpcConfiguration
    {
        public List<string> Cat { get; private set; } = new();
        public List<string> Dog { get; private set; } = new();
        public List<string> Bunny { get; private set; } = new();
        public List<NpcData> NpcData { get; private set; } = new();
    }


    public class BestiaryData
    {
        public NpcConfiguration Configuration { get; private set; } = new();

        public Dictionary<int, NpcData> NpcById { get; private set; } = new();
        public Dictionary<string, NpcData> NpcData { get; private set; } = new();

        public List<string> BestiaryTalkedIDs { get; private set; } = new();
        public List<string> BestiaryNearIDs { get; private set; } = new();
        public List<string> BestiaryKilledIDs { get; private set; } = new();

        public static BestiaryData LoadJson(string fileName)
        {
            using (StreamReader file = File.OpenText(fileName))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                JsonSerializer serializer = new JsonSerializer();
                var npcConfig = serializer.Deserialize<NpcConfiguration>(reader);
                var bestiaryData = new BestiaryData();
                bestiaryData.Init(npcConfig);

                return bestiaryData;
            }
        }

        private void Init(NpcConfiguration npcConfig)
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
    }
}
