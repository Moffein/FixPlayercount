using BepInEx;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using R2API.Utils;
using RoR2;
using System;
using System.Runtime.CompilerServices;
using TPDespair.ZetArtifacts;
using UnityEngine;

namespace FixPlayercount
{
    [BepInDependency("com.bepis.r2api")]
    [BepInDependency("dev.wildbook.multitudes", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.TPDespair.ZetArtifacts", BepInDependency.DependencyFlags.SoftDependency)]

    [BepInPlugin("com.Moffein.FixPlayercount", "Fix Playercount", "1.2.2")]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class FixPlayercount : BaseUnityPlugin
    {
        private static bool MultitudesLoaded = false;
        private static bool ZetArtifactsLoaded = false;

        public static bool UpdateOnStageStart = false;

        public static int stageMaxPlayers = 0;

        public void Awake()
        {
            UpdateOnStageStart = Config.Bind("Settings", "Update on Stage Start", false, "Only update playerount on stage start. This will make it so that the game will ignore player disconnects and will use the maximum connected players during the stage to determine difficulty/item drop calculations.").Value;

            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("dev.wildbook.multitudes"))
            {
                MultitudesLoaded = true;
            }
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.TPDespair.ZetArtifacts"))
            {
                ZetArtifactsLoaded = true;
            }

            if (UpdateOnStageStart)
            {
                On.RoR2.Stage.Start += (orig, self) =>
                {
                    stageMaxPlayers = 0;
                    orig(self);
                };
            }

            //Based on https://github.com/wildbook/R2Mods/blob/master/Multitudes/Multitudes.cs
            var getParticipatingPlayerCount = new Hook(typeof(Run).GetMethodCached("get_participatingPlayerCount"),
                typeof(FixPlayercount).GetMethodCached(nameof(GetParticipatingPlayerCountHook)));
        }

        private static int GetParticipatingPlayerCountHook(Run self)
        {
            return GetConnectedPlayers();
        }

        private static int GetConnectedPlayers()
        {
            int players = 0;
            foreach (PlayerCharacterMasterController pc in PlayerCharacterMasterController.instances)
            {
                if (pc.isConnected)
                {
                    players++;
                }
            }
            if (MultitudesLoaded)
            {
                players = ApplyMultitudes(players);
            }
            if (ZetArtifactsLoaded)
            {
                players = ApplyZetMultitudesArtifact(players);
            }

            if (UpdateOnStageStart)
            {
                if (players > FixPlayercount.stageMaxPlayers)
                {
                    FixPlayercount.stageMaxPlayers = players;
                }
                else
                {
                    players = FixPlayercount.stageMaxPlayers;
                }
            }

            return players;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static int ApplyMultitudes(int origPlayerCount)
        {
            return origPlayerCount * Multitudes.Multitudes.Multiplier;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static int ApplyZetMultitudesArtifact(int origPlayerCount)
        {
            if (TPDespair.ZetArtifacts.ZetMultifact.Enabled && RunArtifactManager.instance.IsArtifactEnabled(ZetArtifactsContent.Artifacts.ZetMultifact))
            {
                return origPlayerCount * Math.Max(2, ZetArtifactsPlugin.MultifactMultiplier.Value); //GetMultiplier is private so I copypasted the code.
            }
            else
            {
                return origPlayerCount;
            }
        }
    }
}
