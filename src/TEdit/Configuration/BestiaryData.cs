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
        public bool IsKillCredit { get; set; }
        public int BestiaryDisplayIndex { get; set; }
    }


    public class BestiaryData
    {
        public Dictionary<int,NpcData> NpcById { get; private set; } = new();
        public Dictionary<string,NpcData> NpcData { get; private set; } = new();

        public List<string> BestiaryTalkedIDs { get; private set; } = new();
        public List<string> BestiaryNearIDs { get; private set; } = new();
        public List<string> BestiaryKilledIDs { get; private set; } = new();

        public static BestiaryData LoadJson(string fileName)
        {
            using (StreamReader file = File.OpenText(fileName))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                JsonSerializer serializer = new JsonSerializer();
                var npcData = serializer.Deserialize<List<NpcData>>(reader);
                var bestiaryData = new BestiaryData();
                bestiaryData.Init(npcData);

                return bestiaryData;
            }
        }

        private void Init(IEnumerable<NpcData> npcs)
        {
            NpcData.Clear();
            foreach (var npc in npcs)
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
