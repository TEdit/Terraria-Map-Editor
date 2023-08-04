using TEdit.Common.Reactive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TEdit.Terraria;


public class Bestiary : ObservableObject
{
    const int KillMax = 9999;
    public Dictionary<string,int> NPCKills = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> NPCNear = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> NPCChat = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public Bestiary Copy(uint worldVersion)
    {
        var copy = new Bestiary();
        using (MemoryStream ms = new MemoryStream())
        {
            var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
            using (BinaryWriter w = new BinaryWriter(ms, encoding, true))
            {
                this.Save(w);
            }
            ms.Position = 0;
            using (BinaryReader r = new BinaryReader(ms, encoding, true))
            {
                copy.Load(r, worldVersion);
            }
        }
        return copy;
    }

    public void Save(BinaryWriter w)
    {
        // Kill Counts
        w.Write(NPCKills.Count);
        foreach (var item in NPCKills)
        {
            w.Write(item.Key);
            w.Write(item.Value);
        }

        // Seen
        w.Write(NPCNear.Count);
        foreach (var item in NPCNear)
        {
            w.Write(item);
        }

        // Chatted
        w.Write(NPCChat.Count);
        foreach (var item in NPCChat)
        {
            w.Write(item);
        }
    }

    public void Load(BinaryReader r, uint version)
    {
        NPCKills.Clear();
        NPCNear.Clear();
        NPCChat.Clear();

        int numKills = r.ReadInt32();
        for (int i = 0; i < numKills; i++)
        {
            NPCKills[r.ReadString()] = r.ReadInt32();
        }

        int numSeen = r.ReadInt32();
        for (int i = 0; i < numSeen; i++)
        {
            NPCNear.Add(r.ReadString());
        }

        int numChat = r.ReadInt32();
        for (int i = 0; i < numChat; i++)
        {
            NPCChat.Add(r.ReadString());
        }
    }

}
