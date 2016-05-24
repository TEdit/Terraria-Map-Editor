using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace TerrariaSettingsUpdater
{
    public partial class Form1 : Form
    {
        private TerrariaWrapper terraria;
        public Form1()
        {
            InitializeComponent();
            terraria = new TerrariaWrapper();
        }

        private void itemBTN_Click(object sender, EventArgs e)
        {
            output.Text = terraria.GetWalls();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            output.Text = terraria.GetTiles();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string outputText = string.Empty;

            foreach (var item in terraria.GetItems().ToList().OrderBy(x => x.Id).ToList())
            {
                outputText += string.Format("<Item Id=\"{0}\" Name=\"{1}\"/>\r\n", item.Id, item.Name, item.Type);
            }

            var p = terraria.Prefixes();
            for (int i = 0; i < p.Count; i++)
            {
                outputText += string.Format("<Prefix Id=\"{0}\" Name=\"{1}\" />\r\n", i, p[i]);
            }
            //return;


            for (int i = 0; i < Terraria.Recipe.maxRecipes; i++)
            {
                outputText += string.Format("{0}: {1}\r\n", i, terraria.Recipes[i].createItem.name);

            }
            for (int i = 0; i < 1000; i++)
            {
                var item = terraria.GetItem(i);
                outputText += string.Format("{0}: {1}\r\n", i, item.name);
            }

            outputText += "\r\nITEMFRAMEIMPORTANT\r\n";
            for (int i = 0; i < Terraria.Main.tileFrameImportant.Length; i++)
            {
                if (Terraria.Main.tileFrameImportant[i])
                {
                    outputText += string.Format("{0} ", i);
                }
            }
            //
            output.Text = outputText;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string mergeText = string.Empty;
            var orgXML = XElement.Parse(input.Text);
            var newXML = XElement.Parse(output.Text);

            List<int> diffs = new List<int>();

            for (int i = 0; i <= 339; i++)
            {
                var otile = orgXML.Elements("Tile").FirstOrDefault(x => (int)x.Attribute("Id") == i);
                var ntile = newXML.Elements("Tile").FirstOrDefault(x => (int)x.Attribute("Id") == i);

                if (otile == null) otile = ntile;

                if (ntile == null)
                    continue;

                var oldAttributes = otile.Attributes().ToList();
                var newAttributes = ntile.Attributes().ToList();

                foreach (XAttribute attribute in newAttributes)
                {
                    var old = oldAttributes.FirstOrDefault(a => a.Name == attribute.Name);
                    if (old != null)
                    {
                        if (attribute.Name != "Name" || !string.IsNullOrWhiteSpace(attribute.Value))
                        {
                            oldAttributes.Remove(old);
                            oldAttributes.Add(attribute);
                        }
                    }
                    else
                    {
                        oldAttributes.Add(attribute);
                    } 
                    
                }
                otile.ReplaceAttributes(oldAttributes);

                mergeText += otile.ToString() + Environment.NewLine;

            }

            mergeText += Environment.NewLine + string.Join(",", diffs);

            merge.Text = mergeText;

        }

        private void button4_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 100; i++)
            {
                merge.Text += string.Format("{0}\r\n", 36 * i);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            var npcs = terraria.GetNpcs();

            foreach (var npc in npcs)
            {
                output.Text += string.Format("<Npc Id=\"{1}\" Name=\"{0}\" Frames=\"{2}\" />\r\n", npc.name, npc.netID, npc.width);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            string outtext = string.Empty;
            for (int y = 0; y < (int)frameGenTileHeight.Value; y += (int)frameGenPixelHeight.Value)
            
            {
                for (int x = 0; x < (int)frameGenTileWidth.Value; x += (int)frameGenPixelWidth.Value)
                {
                    outtext += string.Format("<Frame UV=\"{0},{1}\" Name=\"\" Variety=\"\" />\r\n",x,y);
                }
            }
            output.Text = outtext;
        }


    }

    public class TileProperties
    {
        public TileProperties()
        {
            Name = string.Empty;
            Values = new List<byte>();
        }
        public string Name { get; set; }
        public List<byte> Values { get; set; }
    }
}
