using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.Common.DataCollection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OBSWebsocketDotNet.Tests.Helper
{
    internal class ObsHelper
    {
        private readonly TestContext testContext;
        private readonly string obsPath;
        private readonly string obsExec;

        /// <summary>
        /// Websocket password as set in global.ini supplied by the test
        /// </summary>
        public const string WebsocketPassword = "LjbT8ySzicc6WLMi";

        public const string WebsocketConnection = "ws://127.0.0.1:4444";

        private const string defaultSceneCollection = "Default";
        private const string defaultProfile = "Default";

        private Process obs;
        private OBSWebsocket websocket;

        public ObsHelper(TestContext testContext)
        {
            this.testContext = testContext;
            obsPath = testContext.Properties["obsPath"] as string;

            if (!Directory.Exists(obsPath))
            {
                throw new ArgumentException(
                    "The obs path pointed to in the config does not exist. Are the runsettings configured properly?");
            }

            obsExec = Path.Combine(obsPath, @"obs64.exe");

            if (!File.Exists(obsExec))
            {
                throw new ArgumentException(
                    @"The obs executable was not found in the given path");
            }

            var websocketPlugin = Path.Combine(obsPath, @"..\..\obs-plugins\64bit\obs-websocket.dll");

            if (!File.Exists(websocketPlugin))
            {
                throw new ArgumentException(@"Obs was found but the websocket plugin seems to be missing");
            }

            ResetConfigFiles();
        }

        public void ResetConfigFiles()
        {
            // Prepare the obs settings
            var basePath = Path.Combine(obsPath, @"..\..");
            var configPath = Path.Combine(basePath, @"config\obs-studio");

            using (var globalMasterConfig = Assembly.GetExecutingAssembly()
                       .GetManifestResourceStream("OBSWebsocketDotNet.Tests.Helper.ObsConfiguration.global.ini"))
            {
                if (globalMasterConfig != null)
                {
                    var globalConfig = Path.Combine(configPath, "global.ini");
                    if (File.Exists(globalConfig))
                    {
                        File.Delete(globalConfig);
                    }

                    using (var file = File.Create(globalConfig))
                    {
                        globalMasterConfig.CopyTo(file);
                    }
                }
                else
                {
                    throw new ArgumentException("obs config files are missing");
                }
            }

            // Prepare all the scene configs for the tests
            var sceneMasterConfigs = Assembly.GetExecutingAssembly().GetManifestResourceNames()
                .Where(x => x.Contains(".ObsConfiguration.scenes."));
            var scenesConfigDir = Path.Combine(configPath, @"basic\scenes");

            if (Directory.Exists(scenesConfigDir))
                Directory.Delete(scenesConfigDir, true);

            Directory.CreateDirectory(scenesConfigDir);

            foreach (var config in sceneMasterConfigs)
            {
                var configName = String.Join('.', config.Split('.').TakeLast(2));
                using (var sceneMasterStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(config))
                {
                    using (var file = File.Create(Path.Combine(scenesConfigDir, configName)))
                    {
                        sceneMasterStream.CopyTo(file);
                    }
                }
            }

            // Prepare all profiles for the tests
            var profileMasterConfigs = Assembly.GetExecutingAssembly().GetManifestResourceNames()
                .Where(x => x.Contains(".ObsConfiguration.profiles."));
            var profileConfigDir = Path.Combine(configPath, @"basic\profiles");

            if (Directory.Exists(profileConfigDir))
                Directory.Delete(profileConfigDir, true);

            Directory.CreateDirectory(profileConfigDir);

            foreach (var config in profileMasterConfigs)
            {
                var profileName = config.Split('.').TakeLast(2).First();

                Directory.CreateDirectory(Path.Combine(profileConfigDir, profileName));

                using (var profileMasterStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(config))
                {
                    using (var file = File.Create(Path.Combine(profileConfigDir, profileName, "basic.ini")))
                    {
                        profileMasterStream.CopyTo(file);
                    }
                }
            }
        }

        public bool LaunchObs(string collection = defaultSceneCollection, string profile = defaultProfile)
        {
            if (obs == null)
            {
                var info = new ProcessStartInfo(obsExec, $"-p --collection \"{collection}\" --profile \"{profile}\"");
                info.WorkingDirectory = obsPath;

                obs = Process.Start(info);

                if (obs?.HasExited == true)
                    obs = null;
            }
            
            return obs != null;
        }

        public async Task<bool> WaitForWebsocket(int retry = 5, int delay = 500)
        {
            while (retry-- > 0)
            {
                var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
                var tcpConnInfoArray = ipGlobalProperties.GetActiveTcpListeners();

                if (tcpConnInfoArray.Any(x => x.Port == 4444))
                    return true;

                await Task.Delay(delay);
            }

            return false;
        }

        public bool ResetObs()
        {
            TerminateObs();
            Task.WaitAll(Task.Delay(500));
            ResetConfigFiles();
            return LaunchObs();
        }

        public void TerminateObs()
        {
            if (obs != null)
            {
                if (websocket != null)
                {
                    websocket.Disconnect();
                }

                obs.Kill();
                obs = null;
            }
        }

        public async Task<OBSWebsocket> GetConnection()
        {
            if (websocket == null)
            {
                if (obs != null)
                {
                    await WaitForWebsocket();

                    websocket = new OBSWebsocket();
                    websocket.Connect(WebsocketConnection, WebsocketPassword);
                }
            }

            return websocket;
        }
    }
}