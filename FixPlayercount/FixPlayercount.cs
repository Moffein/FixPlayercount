using BepInEx;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using R2API.Utils;
using RoR2;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace FixPlayercount
{
    [BepInDependency("com.bepis.r2api")]
    [BepInDependency("dev.wildbook.multitudes", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin("com.Moffein.FixPlayercount", "Fix Playercount", "1.1.0")]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class FixPlayercount : BaseUnityPlugin
    {
        private static bool MultitudesLoaded = false;
        public void Awake()
        {
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("dev.wildbook.multitudes"))
            {
                MultitudesLoaded = true;
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
            return players;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static int ApplyMultitudes(int origPlayerCount)
        {
            return origPlayerCount * Multitudes.Multitudes.Multiplier;
        }
    }
}
