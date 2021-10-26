using BepInEx;
using MonoMod.Cil;
using R2API.Utils;
using RoR2;
using System;
using UnityEngine;

namespace FixPlayercount
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Moffein.FixPlayercrount", "Fix Playercount", "1.0.0")]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class FixPlayercount : BaseUnityPlugin
    {
        public void Awake()
        {
            //This is called first, handles interactables on the stage.
            IL.RoR2.SceneDirector.Start += (il) =>
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                     x => x.MatchCallvirt<RoR2.Run>("get_participatingPlayerCount")
                    );
                c.Index++;
                c.EmitDelegate<Func<int, int>>((playerCount) =>
                {
                    return GetConnectedPlayers();
                });
            };

            IL.RoR2.Run.RecalculateDifficultyCoefficentInternal += (il) =>
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                     x => x.MatchCall<RoR2.Run>("get_participatingPlayerCount")
                    );
                c.Index++;
                c.EmitDelegate<Func<int, int>>((playerCount) =>
                {
                    return GetConnectedPlayers();
                });
            };
            
            IL.RoR2.CombatDirector.DirectorMoneyWave.Update += (il) =>
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                     x => x.MatchCallvirt<RoR2.Run>("get_participatingPlayerCount")
                    );
                c.Index++;
                c.EmitDelegate<Func<int, int>>((playerCount) =>
                {
                    return GetConnectedPlayers();
                });
            };
            
            IL.RoR2.BossGroup.DropRewards += (il) =>
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                     x => x.MatchCallvirt<RoR2.Run>("get_participatingPlayerCount")
                    );
                c.Index++;
                c.EmitDelegate<Func<int, int>>((playerCount) =>
                {
                    return GetConnectedPlayers();
                });
            };

            IL.RoR2.ArenaMissionController.EndRound += (il) =>
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                     x => x.MatchCallvirt<RoR2.Run>("get_participatingPlayerCount")
                    );
                c.Index++;
                c.EmitDelegate<Func<int, int>>((playerCount) =>
                {
                    return GetConnectedPlayers();
                });
            };
        }

        //Would be more elegant if I could figure out how to hook get_participatingPlayerCount and run this instead.
        private int GetConnectedPlayers()
        {
            int players = 0;
            foreach (PlayerCharacterMasterController pc in PlayerCharacterMasterController.instances)
            {
                if (pc.isConnected)
                {
                    players++;
                }
            }
            return players;
        }
    }
}
