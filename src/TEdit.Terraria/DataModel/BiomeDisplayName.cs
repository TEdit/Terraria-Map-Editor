using System.Collections.Generic;

namespace TEdit.Terraria
{
    public static class BiomeDisplayName
    {
        private static readonly Dictionary<string, string> _cn = new()
        {
            { "Purify", "净化" },
            { "Corruption", "腐化" },
            { "Crimson", "猩红" },
            { "Hallow", "神圣" },
            { "GlowingMushroom", "发光蘑菇" },
            { "Forest", "森林" },
            { "Snow", "雪地" },
            { "Desert", "沙漠" }
        };

        public static string Get(string key)
        {
            if (key == null)
                return "";

            if (_cn.TryGetValue(key, out var value))
                return value;

            return key; // 没翻译就显示原文
        }
    }
}
