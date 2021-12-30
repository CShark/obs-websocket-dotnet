using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OBSWebsocketDotNet.Tests.Helper;

namespace OBSWebsocketDotNet.Tests.Requests
{
    [TestClass]
    public class Scenes
    {
        private static ObsHelper helper;
        private static OBSWebsocket obs;

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static async Task ClassInitialize(TestContext context)
        {
            helper = new ObsHelper(context);
            helper.LaunchObs();
            await helper.WaitForWebsocket();
            obs = await helper.GetConnection();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            helper.TerminateObs();
        }

        [TestCleanup]
        public async Task TestCleanup() {
            if (TestContext.CurrentTestOutcome == UnitTestOutcome.Failed) {
                helper.ResetObs();
                await helper.WaitForWebsocket();
                obs = await helper.GetConnection();
            }
        }


        [TestMethod]
        public void GetSceneList()
        {
            var info = obs.GetSceneList();

            Assert.IsNotNull(info, "No scene list returned");
            Assert.IsTrue(info.Scenes.Any(x => x.Name == "SceneA") &&
                          info.Scenes.Any(x => x.Name == "SceneB") &&
                          info.Scenes.Any(x => x.Name == "SceneC"), "Core scenes were not found");
        }

        [TestMethod]
        public void GetGroupList()
        {
            Assert.Fail("Groups are not implemented");
        }

        [TestMethod]
        public void CurrentProgramScene()
        {
            obs.SetCurrentScene("sceneB");
            var scene = obs.GetCurrentScene();

            Assert.AreEqual("SceneB", scene.Name, "Current scene was either not set or not received properly");
        }

        [TestMethod]
        public void CurrentPreviewScene()
        {
            Assert.IsTrue(obs.StudioModeEnabled());

            obs.SetPreviewScene("SceneC");
            var scene = obs.GetPreviewScene();

            Assert.AreEqual("SceneC", scene, "Preview scene was either not set or not received properly");
        }

        [TestMethod]
        public void AlterScenes()
        {
            obs.CreateScene("SceneF");
            var info = obs.GetSceneList();

            Assert.IsTrue(info.Scenes.Any(x => x.Name == "SceneF"), "Created scene was not found in scene list");

            // TODO: implement rename scene
            Assert.Fail("Renaming is not implemented");

            // TODO: implement delete scene
            Assert.Fail("Deletion is not implemented");
        }
    }
}