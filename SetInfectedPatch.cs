using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using UnhollowerBaseLib;
using static Pheonix.PheonixRole;

namespace Pheonix
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetInfected))]
    class SetInfectedPatch
    {
        public static void Postfix(Il2CppReferenceArray<GameData.PlayerInfo> JPGEIBIBJPJ)
        {
            Main.Config.SetConfigSettings();
            Main.Logic.AllModPlayerControl.Clear();
            #region reset pheonix stuff
            PlayerTools.hasDied = false;
            PlayerTools.hasDone = false;
            PlayerTools.hasPressedDroplit = false;
            PlayerTools.hasPressedRelive = false;
            PlayerTools.hasPressedReveal = false;
            PlayerTools.hasPressedWrath = false;
            PlayerTools.waterPressed = false;
            #endregion
            killedPlayers.Clear();
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ResetVariiables, Hazel.SendOption.None, -1);
            //     writer.Write(writer.ToString());a
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            List<PlayerControl> crewmates = PlayerControl.AllPlayerControls.ToArray().ToList();
            foreach (PlayerControl plr in crewmates)
                Main.Logic.AllModPlayerControl.Add(new ModPlayerControl { PlayerControl = plr, Role = "Impostor", UsedAbility = false, LastAbilityTime = null, Immortal = false });
            crewmates.RemoveAll(x => x.Data.IsImpostor);
            foreach (PlayerControl plr in crewmates)
                plr.getModdedControl().Role = "Crewmate";
            foreach (PlayerControl plr in PlayerControl.AllPlayerControls)
            {
                if (!plr.Data.IsImpostor)
                    plr.getModdedControl().Role = "Crewmate";
                else
                    plr.getModdedControl().Role = "Impostor";
            }
            MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetPheonix, Hazel.SendOption.None, -1);
            var pheonixRandom = rng.Next(0, crewmates.Count);
            crewmates[pheonixRandom].getModdedControl().Role = "Pheonix";
            byte PheonixId = crewmates[pheonixRandom].PlayerId;
            crewmates.RemoveAt(pheonixRandom);
            writer2.Write(PheonixId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);

            localPlayers.Clear();
            localPlayer = PlayerControl.LocalPlayer;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.IsImpostor)
                    continue;
                if (player.isPlayerRole("Pheonix"))
                    continue;
                else
                    localPlayers.Add(player);
            }
            var localPlayerBytes = new List<byte>();
            foreach (PlayerControl player in localPlayers)
            {
                localPlayerBytes.Add(player.PlayerId);
            }
            writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetLocalPlayers, Hazel.SendOption.None, -1);
            writer.WriteBytesAndSize(localPlayerBytes.ToArray());
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
    }
}
