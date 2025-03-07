﻿using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ASIO.NET;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;


namespace LowLatencyTromboneChamp
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;
        public AudioClip currentClip;
        
        private void Awake()
        {
            Instance = this;
            LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();
        }
        

        #region logging
        internal static void LogDebug(string message) => Instance.Log(message, LogLevel.Debug);
        internal static void LogInfo(string message) => Instance.Log(message, LogLevel.Info);
        internal static void LogWarning(string message) => Instance.Log(message, LogLevel.Warning);
        internal static void LogError(string message) => Instance.Log(message, LogLevel.Error);
        private void Log(string message, LogLevel logLevel) => Logger.Log(logLevel, message);
        #endregion
        
        public Asio deviceOut;
        public bool isAsioInitted = false;
        public bool areSamplesloaded = false;
        public SoundStream currentStream = null;
        public int loadedClipCount = 0;

        //Game broke ASIO, so we hack it til it works again
        public void LoadDefaultTromboneSoundsHack()
        {
            
            var path = Path.Combine(Paths.BepInExRootPath, "plugins/LowLatencyAudioClips/");
            
            
            var clips = new List<string>()
            {
                "t2_tromboneC1.wav",
                "t2_tromboneD1.wav",
                "t2_tromboneE1.wav",
                "t2_tromboneF1.wav",
                "t2_tromboneG1.wav",
                "t2_tromboneA1.wav",
                "t2_tromboneB1.wav",
                "t2_tromboneC2.wav",
                "t2_tromboneD2.wav",
                "t2_tromboneE2.wav",
                "t2_tromboneF2.wav",
                "t2_tromboneG2.wav",
                "t2_tromboneA2.wav",
                "t2_tromboneB2.wav",
                "t2_tromboneC3.wav"
            };

            foreach (var sound in clips)
            {
                var filePath = Path.Combine(path, sound);
                
                byte[] fileBytes = File.ReadAllBytes (filePath);

                var clip = WavUtility.ToAudioClip(fileBytes);
                
                loadedClipCount += 1;
                var channels = clip.channels;
                var sampleRate = clip.frequency;
                var data = new float[clip.samples * channels];
                clip.GetData(data, 0);
                deviceOut.Load(data, (uint)sampleRate, (uint)channels);
                areSamplesloaded = true;
                
            }
            
        }

        
        public void InitTromboneClips(AudioClip[] clips)
        {
            //Unloads soundset
            /*
            while (loadedClipCount > 0)
            {
                deviceOut.UnLoad(loadedClipCount);
                loadedClipCount -= 1;
            }

            //Load trombone soundset
            foreach (var clip in clips)
            {
                loadedClipCount += 1;
                Logger.LogInfo(clip.name);
                var channels = clip.channels;
                var sampleRate = clip.frequency;
                var data = new float[clip.samples * channels];
                clip.GetData(data, 0);
                deviceOut.Load(data, (uint)sampleRate, (uint)channels);
                areSamplesloaded = true;
            }*/
        }
        
        public void PlaySound(int id)
        {
            Logger.LogWarning("Playing Asio Note!!");
            if (currentStream != null) currentStream.Stop();
            currentStream = deviceOut.Play(
                soundID: id+1,
                outChannels: new uint[] { 0, 1 },
                looping: true);
            
        }

        public bool hide = false;
        private void OnGUI()
        {
            if (hide) return;
            if (!isAsioInitted)
            {
                GUILayout.Label("Select Your Asio Device");
                foreach (var device in Asio.GetDeviceNames())
                {
                    if (GUILayout.Button(device))
                    {
                        deviceOut = Asio.CreatePlayer(device, 48000);
                        isAsioInitted = true;
                        LoadDefaultTromboneSoundsHack();
                    }
                }
            }
            else
            {
                if (GUILayout.Button("Show Asio Panel"))
                {
                    deviceOut.ShowControlPanel();
                }
                if (GUILayout.Button("Hide"))
                {
                    hide = true;
                }
            }

        }
    }
}
