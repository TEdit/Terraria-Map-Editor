using Xunit;
using TEdit.Terraria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using System.Reflection;
using System.IO;

namespace TEdit.Terraria.Tests;

public class WorldTests
{
    [Theory]
    [InlineData(".\\WorldFiles\\v1.0.wld")]
    public void LoadWorldV0_Test(string fileName)
    {
        var (w, er) = World.LoadWorld(fileName);
    }

    [Theory]
    [InlineData(".\\WorldFiles\\android.wld")]
    public void LoadWorldAndroid_Test(string fileName)
    {
        var (w, er) = World.LoadWorld(fileName);
    }

    [Theory]
    [InlineData(".\\WorldFiles\\console.wld")]
    public void LoadWorldConsole_Test(string fileName)
    {
        var (w, er) = World.LoadWorld(fileName);
    }

    [Theory]
    [InlineData(".\\WorldFiles\\v1.2.2.wld")]
    [InlineData(".\\WorldFiles\\v1.2.1.wld")]
    [InlineData(".\\WorldFiles\\v1.2.1.2.wld")]
    [InlineData(".\\WorldFiles\\v1.2.1.1.wld")]
    [InlineData(".\\WorldFiles\\v1.2.0.3.wld")]
    [InlineData(".\\WorldFiles\\v1.2.0.3.1.wld")]
    [InlineData(".\\WorldFiles\\v1.2.0.2.wld")]
    [InlineData(".\\WorldFiles\\v1.2.0.1.wld")]
    [InlineData(".\\WorldFiles\\v1.2.wld")]
    [InlineData(".\\WorldFiles\\v1.1.wld")]
    [InlineData(".\\WorldFiles\\v1.1.2.wld")]
    [InlineData(".\\WorldFiles\\v1.1.1.wld")]
    [InlineData(".\\WorldFiles\\v1.0.6.wld")]
    [InlineData(".\\WorldFiles\\v1.0.6.1.wld")]
    [InlineData(".\\WorldFiles\\v1.0.5.wld")]
    [InlineData(".\\WorldFiles\\v1.0.4.wld")]
    [InlineData(".\\WorldFiles\\v1.0.3.wld")]
    [InlineData(".\\WorldFiles\\v1.0.2.wld")]
    [InlineData(".\\WorldFiles\\v1.0.1.wld")]
    public void LoadWorldV1_Test(string fileName)
    {
        var (w, er) = World.LoadWorld(fileName);
    }

    [Theory]
    [InlineData(".\\WorldFiles\\v1.3.0.1.wld")]
    [InlineData(".\\WorldFiles\\v1.3.0.2.wld")]
    [InlineData(".\\WorldFiles\\v1.3.0.3.wld")]
    [InlineData(".\\WorldFiles\\v1.3.0.4.wld")]
    [InlineData(".\\WorldFiles\\v1.3.0.5.wld")]
    [InlineData(".\\WorldFiles\\v1.3.0.6.wld")]
    [InlineData(".\\WorldFiles\\v1.3.0.7.wld")]
    [InlineData(".\\WorldFiles\\v1.3.0.8.wld")]
    [InlineData(".\\WorldFiles\\v1.3.1.1.wld")]
    [InlineData(".\\WorldFiles\\v1.3.1.wld")]
    [InlineData(".\\WorldFiles\\v1.3.2.1.wld")]
    [InlineData(".\\WorldFiles\\v1.3.2.wld")]
    [InlineData(".\\WorldFiles\\v1.3.3.1.wld")]
    [InlineData(".\\WorldFiles\\v1.3.3.2.wld")]
    [InlineData(".\\WorldFiles\\v1.3.3.3.wld")]
    [InlineData(".\\WorldFiles\\v1.3.3.wld")]
    [InlineData(".\\WorldFiles\\v1.3.4.1.wld")]
    [InlineData(".\\WorldFiles\\v1.3.4.2.wld")]
    [InlineData(".\\WorldFiles\\v1.3.4.3.wld")]
    [InlineData(".\\WorldFiles\\v1.3.4.4.wld")]
    [InlineData(".\\WorldFiles\\v1.3.4.wld")]
    [InlineData(".\\WorldFiles\\v1.3.5.1.wld")]
    [InlineData(".\\WorldFiles\\v1.3.5.2.wld")]
    [InlineData(".\\WorldFiles\\v1.3.5.3.wld")]
    [InlineData(".\\WorldFiles\\v1.3.5.wld")]
    [InlineData(".\\WorldFiles\\v1.4.0.1.wld")]
    [InlineData(".\\WorldFiles\\v1.4.0.2.wld")]
    [InlineData(".\\WorldFiles\\v1.4.0.3.wld")]
    [InlineData(".\\WorldFiles\\v1.4.0.4.wld")]
    [InlineData(".\\WorldFiles\\v1.4.0.5.wld")]
    [InlineData(".\\WorldFiles\\v1.4.1.1.wld")]
    [InlineData(".\\WorldFiles\\v1.4.1.2.wld")]
    [InlineData(".\\WorldFiles\\v1.4.1.wld")]
    [InlineData(".\\WorldFiles\\v1.4.2.1.wld")]
    [InlineData(".\\WorldFiles\\v1.4.2.2.wld")]
    [InlineData(".\\WorldFiles\\v1.4.2.3.wld")]
    [InlineData(".\\WorldFiles\\v1.4.2.wld")]
    [InlineData(".\\WorldFiles\\v1.4.3.1.wld")]
    [InlineData(".\\WorldFiles\\v1.4.3.2.wld")]
    [InlineData(".\\WorldFiles\\v1.4.3.3.wld")]
    [InlineData(".\\WorldFiles\\v1.4.3.4.wld")]
    [InlineData(".\\WorldFiles\\v1.4.3.5.wld")]
    [InlineData(".\\WorldFiles\\v1.4.3.6.wld")]
    [InlineData(".\\WorldFiles\\v1.4.3.wld")]
    [InlineData(".\\WorldFiles\\v1.2.4.wld")]
    [InlineData(".\\WorldFiles\\v1.2.4.1.wld")]
    [InlineData(".\\WorldFiles\\v1.2.3.wld")]
    [InlineData(".\\WorldFiles\\v1.2.3.1.wld")]
    public void LoadWorldV2_Test(string fileName)
    {
        var (w, er) = World.LoadWorld(fileName);
    }

    [Theory]
    [InlineData(".\\WorldFiles\\v1.4.4.1.wld")]
    [InlineData(".\\WorldFiles\\v1.4.4.2.wld")]
    [InlineData(".\\WorldFiles\\v1.4.4.3.wld")]
    [InlineData(".\\WorldFiles\\v1.4.4.4.wld")]
    public void LoadWorldV2_144x_Test(string fileName)
    {
        var (w, er) = World.LoadWorld(fileName);
    }

    [Theory]
    [InlineData(".\\WorldFiles\\console.wld")]
    public void SaveWorld_Console_Test(string fileName)
    {
        var (w,er) = World.LoadWorld(fileName);

        var saveTest = fileName + ".test";
        World.Save(w, saveTest, incrementRevision: false);

        // essentially, just a save and load test
        var w2 = World.LoadWorld(saveTest);
    }

    [Theory]
    [InlineData(".\\WorldFiles\\v1.0.wld")]

    public void SaveWorldV0_Test(string fileName)
    {
        var (w, er) = World.LoadWorld(fileName);

        var saveTest = fileName + ".test";
        World.Save(w, saveTest, versionOverride: -38, incrementRevision: false);

        // essentially, just a save and load test
        var w2 = World.LoadWorld(saveTest);
    }

    [Theory]
    [InlineData(".\\WorldFiles\\v1.2.wld")]
    [InlineData(".\\WorldFiles\\v1.2.2.wld")]
    [InlineData(".\\WorldFiles\\v1.2.1.wld")]
    [InlineData(".\\WorldFiles\\v1.2.1.2.wld")]
    [InlineData(".\\WorldFiles\\v1.2.1.1.wld")]
    [InlineData(".\\WorldFiles\\v1.2.0.3.wld")]
    [InlineData(".\\WorldFiles\\v1.2.0.3.1.wld")]
    [InlineData(".\\WorldFiles\\v1.2.0.2.wld")]
    [InlineData(".\\WorldFiles\\v1.2.0.1.wld")]

    public void SaveWorldV1_Terraria1_2_Test(string fileName)
    {
        var (w, er) = World.LoadWorld(fileName);

        var saveTest = fileName + ".test";
        World.Save(w, saveTest, incrementRevision: false);

        // essentially, just a save and load test
        var w2 = World.LoadWorld(saveTest);
    }

    [Theory]
    [InlineData(".\\WorldFiles\\v1.1.wld")]
    [InlineData(".\\WorldFiles\\v1.1.2.wld")]
    [InlineData(".\\WorldFiles\\v1.1.1.wld")]
    public void SaveWorldV1_Terraria1_1_Test(string fileName)
    {
        var (w, er) = World.LoadWorld(fileName);

        var saveTest = fileName + ".test";
        World.Save(w, saveTest, incrementRevision: false);

        // essentially, just a save and load test
        var w2 = World.LoadWorld(saveTest);
    }

    [Theory]
    [InlineData(".\\WorldFiles\\v1.0.6.wld")]
    [InlineData(".\\WorldFiles\\v1.0.6.1.wld")]
    [InlineData(".\\WorldFiles\\v1.0.5.wld")]
    [InlineData(".\\WorldFiles\\v1.0.4.wld")]
    [InlineData(".\\WorldFiles\\v1.0.3.wld")]
    [InlineData(".\\WorldFiles\\v1.0.2.wld")]
    [InlineData(".\\WorldFiles\\v1.0.1.wld")]
    public void SaveWorldV1_Terraria1_0_Test(string fileName)
    {
        var (w, er) = World.LoadWorld(fileName);

        var saveTest = fileName + ".test";
        World.Save(w, saveTest, incrementRevision: false);

        // essentially, just a save and load test
        var w2 = World.LoadWorld(saveTest);
    }

    [Theory]
    [InlineData(".\\WorldFiles\\v1.0.5.wld")]

    public void SaveWorldV1_Terraria1_0_5_Test(string fileName)
    {
        var (w, er) = World.LoadWorld(fileName);

        var saveTest = fileName + ".test";
        World.Save(w, saveTest, incrementRevision: false);

        // essentially, just a save and load test
        var w2 = World.LoadWorld(saveTest);
    }

    [Theory]
    [InlineData(".\\WorldFiles\\v1.3.0.1.wld")]
    [InlineData(".\\WorldFiles\\v1.3.0.2.wld")]
    [InlineData(".\\WorldFiles\\v1.3.0.3.wld")]
    [InlineData(".\\WorldFiles\\v1.3.0.4.wld")]
    [InlineData(".\\WorldFiles\\v1.3.0.5.wld")]
    [InlineData(".\\WorldFiles\\v1.3.0.6.wld")]
    [InlineData(".\\WorldFiles\\v1.3.0.7.wld")]
    [InlineData(".\\WorldFiles\\v1.3.0.8.wld")]
    [InlineData(".\\WorldFiles\\v1.3.1.1.wld")]
    [InlineData(".\\WorldFiles\\v1.3.1.wld")]
    [InlineData(".\\WorldFiles\\v1.3.2.1.wld")]
    [InlineData(".\\WorldFiles\\v1.3.2.wld")]
    [InlineData(".\\WorldFiles\\v1.3.3.1.wld")]
    [InlineData(".\\WorldFiles\\v1.3.3.2.wld")]
    [InlineData(".\\WorldFiles\\v1.3.3.3.wld")]
    [InlineData(".\\WorldFiles\\v1.3.3.wld")]
    [InlineData(".\\WorldFiles\\v1.3.4.1.wld")]
    [InlineData(".\\WorldFiles\\v1.3.4.2.wld")]
    [InlineData(".\\WorldFiles\\v1.3.4.3.wld")]
    [InlineData(".\\WorldFiles\\v1.3.4.4.wld")]
    [InlineData(".\\WorldFiles\\v1.3.4.wld")]
    [InlineData(".\\WorldFiles\\v1.3.5.1.wld")]
    [InlineData(".\\WorldFiles\\v1.3.5.2.wld")]
    [InlineData(".\\WorldFiles\\v1.3.5.3.wld")]
    [InlineData(".\\WorldFiles\\v1.3.5.wld")]
    [InlineData(".\\WorldFiles\\v1.4.0.1.wld")]
    [InlineData(".\\WorldFiles\\v1.4.0.2.wld")]
    [InlineData(".\\WorldFiles\\v1.4.0.3.wld")]
    [InlineData(".\\WorldFiles\\v1.4.0.4.wld")]
    [InlineData(".\\WorldFiles\\v1.4.0.5.wld")]
    [InlineData(".\\WorldFiles\\v1.4.1.1.wld")]
    [InlineData(".\\WorldFiles\\v1.4.1.2.wld")]
    [InlineData(".\\WorldFiles\\v1.4.1.wld")]
    [InlineData(".\\WorldFiles\\v1.4.2.1.wld")]
    [InlineData(".\\WorldFiles\\v1.4.2.2.wld")]
    [InlineData(".\\WorldFiles\\v1.4.2.3.wld")]
    [InlineData(".\\WorldFiles\\v1.4.2.wld")]
    [InlineData(".\\WorldFiles\\v1.4.3.1.wld")]
    [InlineData(".\\WorldFiles\\v1.4.3.2.wld")]
    [InlineData(".\\WorldFiles\\v1.4.3.3.wld")]
    [InlineData(".\\WorldFiles\\v1.4.3.4.wld")]
    [InlineData(".\\WorldFiles\\v1.4.3.5.wld")]
    [InlineData(".\\WorldFiles\\v1.4.3.6.wld")]
    [InlineData(".\\WorldFiles\\v1.4.3.wld")]
    [InlineData(".\\WorldFiles\\v1.2.4.wld")]
    [InlineData(".\\WorldFiles\\v1.2.4.1.wld")]
    [InlineData(".\\WorldFiles\\v1.2.3.wld")]
    [InlineData(".\\WorldFiles\\v1.2.3.1.wld")]
    public void SaveWorldV2_Test(string fileName)
    {
        var (w, er) = World.LoadWorld(fileName);

        var saveTest = fileName + ".test";
        World.Save(w, saveTest, incrementRevision: false);

        // essentially, just a save and load test
        var w2 = World.LoadWorld(saveTest);
    }

    [Theory]
    [InlineData(".\\WorldFiles\\v1.4.4.1.wld")]
    [InlineData(".\\WorldFiles\\v1.4.4.2.wld")]
    [InlineData(".\\WorldFiles\\v1.4.4.3.wld")]
    [InlineData(".\\WorldFiles\\v1.4.4.4.wld")]
    public void SaveWorldV2_144x_Test(string fileName)
    {
        var (w, er) = World.LoadWorld(fileName);

        var saveTest = fileName + ".test";
        World.Save(w, saveTest, incrementRevision: false);

        // essentially, just a save and load test
        var w2 = World.LoadWorld(saveTest);
    }
}
