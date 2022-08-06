﻿using Newtonsoft.Json.Linq;
using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestClient
{
    public partial class AdvancedWindow : Form
    {
#pragma warning disable IDE1006 // Naming Styles
        // Source to test on
        private const string SOURCE_NAME = "BarRaider";
        private readonly Random random = new Random();

        protected OBSWebsocket obs;

        public void SetOBS(OBSWebsocket obs)
        {
            this.obs = obs;
        }

        public AdvancedWindow()
        {
            InitializeComponent();
        }

        private void AdvancedWindow_Load(object sender, EventArgs e)
        {

        }

        private void btnEvents_Click(object sender, EventArgs e)
        {
            if (obs == null)
            {
                LogMessage("Error: OBS is null!");
                return;
            }

            obs.RecordStateChanged += OBS_onRecordStateChanged;
            obs.SceneTransitionStarted += OBS_onSceneTransitionStarted;
            obs.SceneTransitionEnded += OBS_onSceneTransitionEnded;
            obs.SceneTransitionVideoEnded += OBS_onSceneTransitionVideoEnded;
            obs.SourceFilterCreated += OBS_onSourceFilterCreated;
            obs.SourceFilterRemoved += OBS_onSourceFilterRemoved;
            obs.SourceFilterEnableStateChanged += OBS_onSourceFilterEnableStateChanged;
            obs.SceneItemListReindexed += OBS_onSceneItemListIndexingChanged;
            obs.SourceFilterListReindexed += OBS_onSourceFilterListReindexed;
            obs.SceneItemLockStateChanged += OBS_onSceneItemLockStateChanged;
            obs.SceneItemEnableStateChanged += OBS_onSceneItemEnableStateChanged;
            obs.InputVolumeChanged += OBS_onInputVolumeChanged;
        }
        private void OBS_onInputVolumeChanged(OBSWebsocket sender, InputVolume volume)
        {
            LogMessage($"[SourceVolumeChanged] Source: {volume.InputName} Volume: {volume.InputVolumeMul} VolumeDB: {volume.InputVolumeDb}");
        }

        private void OBS_onSceneItemEnableStateChanged(OBSWebsocket sender, string sceneName, int sceneItemId, bool sceneItemEnabled)
        {
            LogMessage($"[SceneItemEnableStateChanged] Scene: {sceneName} ItemId: {sceneItemId} Enabled?: {sceneItemEnabled}");
        }

        private void OBS_onSceneItemLockStateChanged(OBSWebsocket sender, string sceneName, int scenItemId, bool sceneItemLocked)
        {
            LogMessage($"[SceneItemLockStateChanged] Scene: {sceneName} ItemId: {scenItemId} IsLocked: {sceneItemLocked}");
        }

        private void OBS_onSourceFilterListReindexed(OBSWebsocket sender, string sourceName, List<FilterReorderItem> filters)
        {
            LogMessage($"[SourceFilterListReindexed] Source: {sourceName}");
            foreach(var filter in filters)
            {
                LogMessage($"\t{filter.Name}");
            }
        }

        private void OBS_onSceneItemListIndexingChanged(OBSWebsocket sender, string sceneName, List<JObject> sceneItems)
        {
            LogMessage($"[SceneItemListReindexed] Scene: {sceneName}{ Environment.NewLine}\tSceneItems: {sceneItems}");
        }

        private void OBS_onSourceFilterEnableStateChanged(OBSWebsocket sender, string sourceName, string filterName, bool filterEnabled)
        {
            LogMessage($"[SourceFilterEnableStateChanged] Source: {sourceName} Filter: {filterName} Enabled: {filterEnabled}");
        }

        private void OBS_onSourceFilterRemoved(OBSWebsocket sender, string sourceName, string filterName)
        {
            LogMessage($"[SourceFilterRemoved] Source: {sourceName} Filter: {filterName}");
        }

        private void OBS_onSourceFilterCreated(OBSWebsocket sender, string sourceName, string filterName, string filterKind, int filterIndex, JObject filterSettings, JObject defaultFilterSettings)
        {
            LogMessage($"[SourceFilterCreated] Source: {sourceName} Filter: {filterName} FilterKind: {filterKind} FilterIndex: {filterIndex}{Environment.NewLine}\tSettings: {filterSettings}{Environment.NewLine}\tDefaultSettings: {defaultFilterSettings}");
        }

        private void OBS_onSceneTransitionVideoEnded(OBSWebsocket sender, string transitionName)
        {
            LogMessage($"[SceneTransitionVideoEnded] Name: {transitionName}");
        }

        private void OBS_onSceneTransitionEnded(OBSWebsocket sender, string transitionName)
        {
            LogMessage($"[SceneTransitionEnded] Name: {transitionName}");
        }

        private void OBS_onSceneTransitionStarted(OBSWebsocket sender, string transitionName)
        {
            LogMessage($"[SceneTransitionStarted] Name: {transitionName}");
        }

        private void OBS_onRecordStateChanged(OBSWebsocket sender, bool outputActive, string outputState)
        {
            LogMessage($"[RecordStateChanged] State: {outputState}");
        }

        private void LogMessage(string message)
        {
            if (InvokeRequired)
            {
                this.Invoke(new MethodInvoker(() =>
                {
                    tbLog.AppendText($"{Environment.NewLine}[{DateTime.Now:HH:mm:ss}]{message}");
                }));
            }
            else
            {
                tbLog.AppendText($"{Environment.NewLine}[{DateTime.Now:HH:mm:ss}]{message}");
            }
        }

        private void btnProjector_Click(object sender, EventArgs e)
        {
            /* This needs to be refactored for v5.0.0 if possible
            const string SCENE_NAME = "Webcam Full";
            obs.OpenProjector();
            MessageBox.Show("Press Ok to continue");
            obs.OpenProjector("preview", 0);
            MessageBox.Show("Press Ok to continue");
            // Should not do anything as sceneName only works in "Source" and "Scene"
            obs.OpenProjector("preview", 0, null, SOURCE_NAME);
            MessageBox.Show("Press Ok to continue");
            obs.OpenProjector("source", 0, null, SOURCE_NAME);
            MessageBox.Show("Press Ok to continue");
            obs.OpenProjector("scene", 0, null, SCENE_NAME);
            */
        }

        private void btnRename_Click(object sender, EventArgs e)
        {
            var active = obs.GetSourceActive(SOURCE_NAME).VideoActive;
            LogMessage($"GetSourceActive for {SOURCE_NAME}: {active}. Renaming source");
            obs.SetInputName(SOURCE_NAME, SOURCE_NAME + random.Next(100));
        }

        private void btnSourceFilters_Click(object sender, EventArgs e)
        {
            try
            {
                LogMessage("GetSourceFilters:");
                var filters = obs.GetSourceFilterList(SOURCE_NAME);

                foreach (var filter in filters)
                {
                    LogFilter(filter);
                }

                var firstFilter = filters.FirstOrDefault();
                if (firstFilter == null)
                {
                    LogMessage("ERROR: No filters found");
                    return;
                }

                LogMessage("GetSourceFilterInfo:");
                LogFilter(obs.GetSourceFilter(SOURCE_NAME, firstFilter.Name));
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR: {ex}");
            }
        }

        private void LogFilter(FilterSettings filter)
        {
            LogMessage($"Filter: {filter.Name} Type: {filter.Kind} Enabled: {filter.IsEnabled}{Environment.NewLine}Settings: {filter.Settings}");
        }

        private void btnCreateScene_Click(object sender, EventArgs e)
        {
            string newScene = SOURCE_NAME + random.Next(100);
            
            obs.CreateScene(newScene); 
            var createdScene = obs.GetSceneList().Scenes.FirstOrDefault(s => s.Name == newScene);
            if (createdScene == null)
            {
                LogMessage($"ERROR: Scene was not created!");
                return;
            }
            LogMessage($"Created scene: {createdScene.Name}");
        }

        private void btnOutputs_Click(object sender, EventArgs e)
        {
            // TODO refactor for v5.0.0 if possible
            /*LogMessage("Testing ListOutputs:");
            var outputs = obs.ListOutputs();
            foreach (var output in outputs)
            {
                LogOutput(output);
            }

            LogMessage("Testing GetOutputInfo:");
            var firstOutput = outputs.Skip(1).FirstOrDefault();
            if (firstOutput == null)
            {
                LogMessage($"ERROR: No outputs retrieved!");
                return;
            }*/

            // TODO: Reuse when properly works on Windows
            /* Output information does not work properly on OBS Websocket Window

            string outputName = firstOutput.Name;
            var retrievedOutput = obs.GetOutputInfo(outputName);
            LogOutput(retrievedOutput);

            LogMessage("Testing StartOutput:");
            obs.StartOutput(outputName);
            retrievedOutput = obs.GetOutputInfo(outputName);
            LogOutput(retrievedOutput);

            LogMessage("Testing StopOutput:");
            obs.StopOutput(outputName);
            retrievedOutput = obs.GetOutputInfo(outputName);
            LogOutput(retrievedOutput);
            */
        }

        private void LogOutput(OBSOutputInfo output)
        {
            if (output == null)
            {
                LogMessage("ERROR: Output is null!");
                return;
            }
            LogMessage($"Output: {output.Name} Type: {output.Type} Width: {output.Width} Height: {output.Height} Active: {output.IsActive} Reconnecting: {output.IsReconnecting} Congestion: {output.Congestion} TotalFrames: {output.TotalFrames} DroppedFrames: {output.DroppedFrames} TotalBytes: {output.TotalBytes}");
            LogMessage($"\tFlags: {output.Flags.RawValue} Audio: {output.Flags.IsAudio} Video: {output.Flags.IsVideo} Encoded: {output.Flags.IsEncoded} MultiTrack: {output.Flags.IsMultiTrack} Service: {output.Flags.IsService}");
            LogMessage($"\tSettings: {output.Settings}");
        }

        private void btnTransition_Click(object sender, EventArgs e)
        {
            LogMessage($"Getting Transitions");
            var transitions = obs.GetSceneTransitionList();

            LogMessage($"Found {transitions.Transitions.Count} transitions. Active: {transitions.CurrentTransition}");
            var enteringTransition = obs.GetCurrentSceneTransition();
            foreach (var transition in transitions.Transitions)
            {
                obs.SetCurrentSceneTransition(transition.Name);
                var activeTransition = obs.GetCurrentSceneTransition();
                var info = activeTransition.Settings;
                info ??= new JObject();
                LogMessage($"Transition: {transition.Name} has {info.Count} settings");
            }
            obs.SetCurrentSceneTransition(enteringTransition.Name);
        }

        private void btnTracks_Click(object sender, EventArgs e)
        {
            try
            {
                LogMessage($"Getting tracks for source {SOURCE_NAME}:");
                var tracks = obs.GetInputAudioTracks(SOURCE_NAME);

                if (tracks == null)
                {
                    LogMessage("ERROR: No tracks returned");
                    return;
                }
                LogMessage($"Active Tracks: 1 {tracks.IsTrack1Active}, 2 {tracks.IsTrack2Active}, 3 {tracks.IsTrack3Active}, 4 {tracks.IsTrack4Active}, 5 {tracks.IsTrack5Active}, 6 {tracks.IsTrack6Active}");

                bool trackToggle = !tracks.IsTrack3Active;
                LogMessage($"Setting Track 3 to {trackToggle}");

                // TODO: Get track settings structure to set track values appropriately
                //obs.SetInputAudioTracks(SOURCE_NAME, 3, trackToggle);
                tracks = obs.GetInputAudioTracks(SOURCE_NAME);
                LogMessage($"Active Tracks: 1 {tracks.IsTrack1Active}, 2 {tracks.IsTrack2Active}, 3 {tracks.IsTrack3Active}, 4 {tracks.IsTrack4Active}, 5 {tracks.IsTrack5Active}, 6 {tracks.IsTrack6Active}");
                LogMessage($"Value is {tracks.IsTrack3Active} expected {trackToggle}");
                
                if (tracks.IsTrack3Active != trackToggle)
                {
                    LogMessage("ERROR: FAILED!");
                    return;
                }

                trackToggle = !tracks.IsTrack3Active;
                LogMessage($"Setting Track 3 back to to {trackToggle}");

                // TODO: Get track settings structure to set track values appropriately
                //obs.SetInputAudioTracks(SOURCE_NAME, 3, trackToggle);
                tracks = obs.GetInputAudioTracks(SOURCE_NAME);
                LogMessage($"Active Tracks: 1 {tracks.IsTrack1Active}, 2 {tracks.IsTrack2Active}, 3 {tracks.IsTrack3Active}, 4 {tracks.IsTrack4Active}, 5 {tracks.IsTrack5Active}, 6 {tracks.IsTrack6Active}");
                LogMessage($"Value is {tracks.IsTrack3Active} expected {trackToggle}");

                if (tracks.IsTrack3Active != trackToggle)
                {
                    LogMessage("ERROR: FAILED!");
                    return;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR: {ex}");
            }
        }

        private void btnToggleVidCapDvc_Click(object sender, EventArgs e)
        {
            var scene = obs.GetCurrentProgramScene();
            var sourceItems = obs.GetSceneItemList(scene.Name);
            var vidCapItems = sourceItems.Where(x => x.SourceKind.Equals("dshow_input"));
            var itemListSettings = new List<InputSettings>();
            foreach (var vidCapItem in vidCapItems)
            {
                var enabled = obs.GetSceneItemEnabled(scene.Name, vidCapItem.ItemId);
                obs.SetSceneItemEnabled(scene.Name, vidCapItem.ItemId, enabled ? false : true);
                LogMessage($"{vidCapItem.SourceName} active button toggled.");
            }
        }

        private void btn_GetInputList_Click(object sender, EventArgs e)
        {
            LogMessage("Getting OBS Input List...");
            var inputList = obs.GetInputList();
            foreach (var input in inputList)
            {
                LogMessage($"{input.Name} {input.Kind} {input.UnversionedKind}");
            }
            LogMessage("Input List Complete...");
        }

        private void btn_GetGroupList_Click(object sender, EventArgs e)
        {
            LogMessage("Getting Group Item List...");
            var groupItems = obs.GetGroupItemList(obs.GetCurrentProgramScene().Name);
            foreach(var groupItem in groupItems)
            {
                LogMessage(groupItem.ToString());
            }
        }

        private void btn_GetMonitorList_Click(object sender, EventArgs e)
        {
            LogMessage("Getting Monitor List...");
            var monitorList = obs.GetMonitorList();
            foreach(var monitor in monitorList)
            {
                LogMessage($"{monitor.Index} {monitor.Name} {monitor.Width}x{monitor.Height} {monitor.PositionX},{monitor.PositionY}");
            }

        }
#pragma warning restore IDE1006 // Naming Styles
    }
}
