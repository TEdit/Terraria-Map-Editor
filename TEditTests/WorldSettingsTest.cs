using TEdit.RenderWorld;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace TEditTests
{
    
    
    /// <summary>
    ///This is a test class for WorldSettingsTest and is intended
    ///to contain all WorldSettingsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class WorldSettingsTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for LoadXMLSettings
        ///</summary>
        [TestMethod()]
        [DeploymentItem("TEdit.exe")]
        public void LoadXMLSettingsTest()
        {
            string file = "settings.xml"; // TODO: Initialize to an appropriate value
            WorldSettings_Accessor.LoadXMLSettings(file);

            int[] tileShine = new[] { 6, 7, 8, 9, 12, 21, 22, 45, 46, 47, 63, 64, 65, 66, 67, 68 };
            int[] tileMergeDirt = new[] { 1, 6, 7, 8, 9, 22, 25, 37, 40, 53, 56, 59 };
            int[] tileCut = new[] { 3, 24, 28, 32, 51, 52, 61, 62, 69, 71, 73, 74, 82, 83, 84 };
            int[] tileAlch = new[] { 82, 83, 84 };
            int[] tileFrameImportant = new[] { 3, 5, 10, 11, 12, 13, 14, 15, 16, 17, 18, 20, 21, 24, 26, 27, 28, 29, 31, 33, 34, 35, 36, 42, 50, 55, 61, 71, 72, 73, 74, 77, 78, 79, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106 };
            int[] tileLavaDeath = new[] { 3, 5, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 27, 28, 29, 32, 33, 34, 35, 36, 42, 49, 50, 52, 55, 61, 62, 69, 71, 72, 73, 74, 79, 80, 81, 86, 87, 88, 89, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106 };
            int[] tileSolid = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 19, 22, 23, 25, 30, 37, 38, 39, 40, 41, 43, 44, 45, 46, 47, 48, 53, 54, 56, 57, 58, 59, 60, 63, 64, 65, 66, 67, 68, 70, 75, 76 };
            int[] tileBlockLight = new[] { 0, 1, 2, 6, 7, 8, 9, 10, 10, 22, 23, 25, 30, 32, 37, 38, 39, 40, 41, 43, 44, 45, 46, 47, 48, 51, 52, 53, 56, 57, 58, 59, 60, 62, 63, 64, 65, 66, 67, 68, 70, 75, 76 };
            int[] tileNoAttach = new[] { 3, 4, 10, 13, 14, 15, 16, 17, 18, 19, 19, 20, 21, 27, 50, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 101, 102 };
            int[] tileNoFail = new[] { 3, 4, 24, 32, 32, 50, 61, 69, 73, 74, 82, 83, 84 };
            int[] tileSolidTop = new[] { 14, 16, 18, 19, 87, 88, 101 };
            int[] tileStone = new[] { 63, 64, 65, 66, 67, 68 };
            int[] tileDungeon = new[] { 41, 43, 44 };
            int[] tileTable = new[] { 14, 18, 19, 87, 88, 101 };
            int[] tileWaterDeath = new[] { 4, 51, 93, 98 };
            int[] wallHouse = new[] { 1, 4, 5, 6, 10, 11, 12, 16, 17, 18, 19, 20 };

            for (int i = 0; i < byte.MaxValue; i++)
            {
                Assert.IsTrue(WorldSettings_Accessor.Tiles[i].IsFramed == tileFrameImportant.Contains(i));
                Assert.IsTrue(WorldSettings_Accessor.Tiles[i].IsSolid == tileSolid.Contains(i));
                Assert.IsTrue(WorldSettings_Accessor.Tiles[i].IsSolidTop == tileSolidTop.Contains(i));
            }
        }
    }
}
