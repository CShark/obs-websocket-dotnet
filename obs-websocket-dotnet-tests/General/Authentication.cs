using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OBSWebsocketDotNet.Tests.Helper;

namespace OBSWebsocketDotNet.Tests.General
{
    [TestClass]
    public class Authentication
    {
        private static ObsHelper helper;

        [ClassInitialize]
        public static async Task ClassInitialize(TestContext context)
        {
            helper = new ObsHelper(context);
            helper.LaunchObs();
            await helper.WaitForWebsocket();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            helper.TerminateObs();
        }

        [TestMethod]
        public async Task NormalAuthentication()
        {
            var obs = new OBSWebsocket();
            var connectEvent = new TaskCompletionSource<bool>();

            obs.Connected += (sender, args) =>
            {
                connectEvent.SetResult(true);
            };
            obs.Connect(ObsHelper.WebsocketConnection, ObsHelper.WebsocketPassword);


            await Task.WhenAny(connectEvent.Task, Task.Delay(500));

            Assert.IsTrue(obs.IsConnected, "Connection could not be established to server");
            Assert.IsTrue(connectEvent.Task.IsCompleted && connectEvent.Task.Result, "Connected event was not fired using correct password");
        }

        [TestMethod]
        public async Task FaultyAuthentication()
        {
            var obs = new OBSWebsocket();
            var connectEvent = new TaskCompletionSource<bool>();

            obs.Connected += (sender, args) =>
            {
                connectEvent.TrySetResult(true);
            };
            obs.Disconnected += (sender, info) =>
            {
                connectEvent.TrySetResult(false);
            };
            obs.Connect(ObsHelper.WebsocketConnection, "RandomBitsAndBobs");

            await Task.WhenAny(connectEvent.Task, Task.Delay(500));

            Assert.IsFalse(obs.IsConnected, "Connection reported although password was wrong");
            Assert.IsTrue(connectEvent.Task.IsCompleted, "No events were fired");
            Assert.IsFalse(connectEvent.Task.Result, "Connected event was fired, disconnected event was expected");
        }
    }
}