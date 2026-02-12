using System;
using System.Globalization;
using System.Windows.Data;
using System.Collections.Generic;

namespace TEdit.Converters
{
    public class BiomeNameConverter : IValueConverter
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

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string key && _cn.TryGetValue(key, out var name))
                return name;

            return value;
        }

        // 关键：反向转换！！！
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var kv in _cn)
            {
                if (kv.Value == (string)value)
                    return kv.Key;
            }
            return value;
        }
    }
}
