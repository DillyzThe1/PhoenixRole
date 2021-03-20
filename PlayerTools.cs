using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Pheonix.PheonixRole;

namespace Pheonix
{
    [HarmonyPatch]
    public static class PlayerTools
    {
        public static PlayerControl closestPlayer = null;
        public static List<PlayerControl> getCrewMates()
        {
            List<PlayerControl> CrewmateIds = new List<PlayerControl>();
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                bool isInfected = false;
                if (player.Data.IsImpostor)
                {
                    isInfected = true;
                    break;
                }
                if (!isInfected)
                {
                    CrewmateIds.Add(player);
                }
            }
            return CrewmateIds;
        }
        public static PlayerControl getPlayerById(byte id)
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId == id)
                {
                    return player;
                }
            }
            return null;
        }
        public static PlayerControl getClosestPlayer(PlayerControl refplayer)
        {
            var mindist = 2.5;
            PlayerControl closestplayer = null;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.IsDead)
                    continue;
                if (player == refplayer)
                    continue;
                var dist = getDistBetweenPlayers(player, refplayer);
                if (dist >= mindist)
                    continue;
                mindist = dist;
                closestplayer = player;
            }
            return closestplayer;
        }
        public static bool ghostInRange = false;
        public static bool hasDone = false;
        public static bool hasDied = false;
        public static bool hasPressedRelive = false;
        public static bool hasPressedDroplit = false;
        public static bool hasPressedReveal = false;
        public static bool hasPressedWrath = false;
        public static bool wrathWasActive = false;
        public static bool waterPressed = false;
        public static PlayerControl getClosestDeadPlayer(PlayerControl refplayer)
        {
            double mindist = 1.5;
            PlayerControl closestplayer = null;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (!player.Data.IsDead) continue;
                if (player != refplayer)
                {
                    double dist = getDistBetweenPlayers(player, refplayer);
                    if (dist < mindist)
                    {
                        mindist = dist;
                        closestplayer = player;
                        ghostInRange = true;
                    }
                    else
                    {
                        ghostInRange = false;
                    }
                }
            }
            return closestplayer;
        }
        public static PlayerControl getPlayerFromId(byte id)
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                if (player.PlayerId == id)
                    return player;
            return null;
        }
        public static double getDistBetweenPlayers(PlayerControl player, PlayerControl refplayer)
        {
            var refpos = refplayer.GetTruePosition();
            var playerpos = player.GetTruePosition();

            return Math.Sqrt((refpos[0] - playerpos[0]) * (refpos[0] - playerpos[0]) + (refpos[1] - playerpos[1]) * (refpos[1] - playerpos[1]));
        }
    }
}