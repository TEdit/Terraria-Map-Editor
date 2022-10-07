using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace TEdit.Configuration
{
    public class BestiaryData
    {
        public List<string> BestiaryTalkedIDs { get; set; } = new List<string>();
        public List<string> BestiaryNearIDs { get; set; } = new List<string>();
        public List<string> BestiaryKilledIDs { get; set; } = new List<string>();

        public static BestiaryData LoadJson(string fileName)
        {
            using (StreamReader file = File.OpenText(fileName))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                JsonSerializer serializer = new JsonSerializer();
                return serializer.Deserialize<BestiaryData>(reader);
            }
        }
    }
}
