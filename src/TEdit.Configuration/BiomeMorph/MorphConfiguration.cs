using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace TEdit.Configuration.BiomeMorph;

public class MorphConfiguration
{
    // this could probably be move to settings file and loaded as global someday. this works for now
    public static HashSet<int> NotSolidTiles = new HashSet<int>(new int[] { 3, 4, 5, 11, 12, 13, 14, 15, 16, 17, 18, 20, 21, 24, 26, 27, 28, 29, 31, 32, 33, 34, 35, 36, 42, 49, 50, 51, 52, 55, 61, 62, 69, 71, 72, 73, 74, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 110, 113, 114, 115, 124, 125, 126, 128, 129, 131, 132, 133, 134, 135, 136, 139, 141, 142, 143, 144, 149, 165, 171, 172, 173, 174, 178, 184, 185, 186, 187, 201, 205, 207, 209, 210, 212, 213, 214, 215, 216, 217, 218, 219, 220, 227, 228, 231, 233, 236, 237, 238, 240, 241, 242, 243, 244, 245, 246, 247, 254, 269, 270, 271, 275, 276, 277, 278, 279, 280, 281, 282, 283, 285, 286, 287, 288, 289, 290, 291, 292, 293, 294, 295, 296, 297, 298, 299, 300, 301, 302, 303, 304, 305, 306, 307, 308, 309, 310, 314, 316, 317, 318, 319, 320, 323, 324, 330, 331, 332, 333, 334, 335, 336, 337, 338, 339, 340, 341, 342, 343, 344, 349, 351, 352, 353, 354, 355, 356, 358, 359, 360, 361, 362, 363, 364, 365, 366, 372, 373, 374, 375, 376, 377, 378, 379, 382, 386, 389, 390, 391, 392, 393, 394, 395, 405, 406, 410, 411, 412, 413, 414, 419, 420, 423, 424, 425, 428, 429, 440, 441, 442, 443, 444, 445, 449, 450, 451, 452, 453, 454, 455, 456, 457, 461, 462, 463, 464, 465, 466, 467, 468, 469, 470, 471, 475, 480, 485, 486, 487, 488, 489, 490, 491, 493, 494, 497, 499, 504, 505, 506, 509, 510, 511, 518, 519, 520, 521, 522, 523, 524, 525, 526, 527, 528, 529, 530, 531, 532, 533, 538, 542, 543, 544, 545, 547, 548, 549, 550, 551, 552, 553, 554, 555, 556, 558, 559, 560, 561, 564, 565, 567, 568, 569, 570, 571, 572, 573, 574, 575, 576, 577, 578, 579, 580, 581, 582, 583, 584, 585, 586, 587, 588, 589, 590, 591, 592, 593, 594, 595, 596, 597, 598, 599, 600, 601, 602, 603, 604, 605, 606, 607, 608, 609, 610, 611, 612, 613, 614, 615, 616, 617, 619, 620, 621, 622, 623, 624, 629, 630, 631, 632, 634, 636, 637, 638, 639, 640, 642, 643, 644, 645, 646, 647, 648, 649, 650, 651, 652, 653, 654, 655, 656, 657, 658, 660, 663, 665, });

    public Dictionary<string, MorphBiomeData> Biomes { get; set; } = new();
    public Dictionary<string, int> MossTypes { get; set; } = new();

    private readonly HashSet<int> _mossTypes = new HashSet<int>();

    public bool IsMoss(ushort type) => _mossTypes.Contains(type);

    public void Save(string fileName)
    {
        using (StreamWriter file = File.CreateText(fileName))
        using (JsonTextWriter writer = new JsonTextWriter(file))
        {
            JsonSerializer serializer = new JsonSerializer()
            {
                Formatting = Formatting.Indented,
            };
            serializer.Serialize(writer, this);
        }
    }

    public static MorphConfiguration LoadJson(string fileName)
    {
        using (StreamReader file = File.OpenText(fileName))
        using (JsonTextReader reader = new JsonTextReader(file))
        {
            JsonSerializer serializer = new JsonSerializer();
            var morphConfig = serializer.Deserialize<MorphConfiguration>(reader);
            morphConfig.InitCache();
            return morphConfig;
        }
    }

    public void InitCache()
    {
        _mossTypes.Clear();
        foreach (var id in MossTypes.Values)
        {
            _mossTypes.Add(id);
        }
    }
}
