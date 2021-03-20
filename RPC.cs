using System;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using static Pheonix.PheonixRole;
using HarmonyLib;
using UnityEngine;
using Reactor.Extensions;
using System.Reflection;
using System.IO;
using Reactor.Unstrip;

namespace Pheonix
{
    enum RPC
    {
        PlayAnimation = 0,
        CompleteTask = 1,
        SyncSettings = 2,
        SetInfected = 3,
        Exiled = 4,
        CheckName = 5,
        SetName = 6,
        CheckColor = 7,
        SetColor = 8,
        SetHat = 9,
        SetSkin = 10,
        ReportDeadBody = 11,
        MurderPlayer = 12,
        SendChat = 13,
        StartMeeting = 14,
        SetScanner = 15,
        SendChatNote = 16,
        SetPet = 17,
        SetStartCounter = 18,
        EnterVent = 19,
        ExitVent = 20,
        SnapTo = 21,
        Close = 22,
        VotingComplete = 23,
        CastVote = 24,
        ClearVote = 25,
        AddVote = 26,
        CloseDoorsOfType = 27,
        RepairSystem = 28,
        SetTasks = 29,
        UpdateGameData = 30,
    }
    enum CustomRPC
    {
        SetPheonix = 53,
        SetLocalPlayers = 54,
        ResetVariiables = 55,
        ReviveAbility = 56,
        FlashAbility = 57,
        WrathAbility = 58,
        ExtinguishAbility = 59,
        RotateAbility = 60,
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    class HandleRpcPatch
    {
        static void Postfix(byte HKHMBLJFLMC, MessageReader ALMCIJKELCP)
        {
            byte packetId = HKHMBLJFLMC;
            MessageReader reader = ALMCIJKELCP;
            switch (packetId)
            {
                case (byte)CustomRPC.SetPheonix:
                    byte bobux = ALMCIJKELCP.ReadByte();
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        if (player.PlayerId == bobux)
                            player.getModdedControl().Role = "Pheonix";
                    break;
                case (byte)CustomRPC.SetLocalPlayers:
                    localPlayers.Clear();
                    localPlayer = PlayerControl.LocalPlayer;
                    var localPlayerBytes = ALMCIJKELCP.ReadBytesAndSize();
                    foreach (byte id in localPlayerBytes)
                        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                            if (player.PlayerId == id)
                                localPlayers.Add(player);
                    break;
                case (byte)CustomRPC.ResetVariiables:
                    Main.Config.SetConfigSettings();
                    Main.Logic.AllModPlayerControl.Clear();
                    killedPlayers.Clear();
                    #region reset pheonix stuff
                    PlayerTools.hasDied = false;
                    PlayerTools.hasDone = false;
                    PlayerTools.hasPressedDroplit = false;
                    PlayerTools.hasPressedRelive = false;
                    PlayerTools.hasPressedReveal = false;
                    PlayerTools.hasPressedWrath = false;
                    PlayerTools.waterPressed = false;
                    #endregion
                    List<PlayerControl> crewmates = PlayerControl.AllPlayerControls.ToArray().ToList();
                    foreach (PlayerControl plr in crewmates)
                        Main.Logic.AllModPlayerControl.Add(new ModPlayerControl { PlayerControl = plr, Role = "Impostor", UsedAbility = false, LastAbilityTime = null, Immortal = false });
                    crewmates.RemoveAll(x => x.Data.IsImpostor);
                    foreach (PlayerControl plr in crewmates)
                        plr.getModdedControl().Role = "Crewmate";
                    break;
                case (byte)CustomRPC.ReviveAbility:
                    byte revivedId = ALMCIJKELCP.ReadByte();
                    PlayerControl revive = PlayerTools.getPlayerFromId(revivedId);
                    revive.Revive();
                    switch (PlayerControl.GameOptions.MapId)
                    {
                        case 0:
                            revive.transform.position = new Vector3(-7.25f, -4.85f, revive.transform.position.z);
                            break;
                        case 1:
                            revive.transform.position = new Vector3(16.26f, 0.54f, revive.transform.position.z);
                            break;
                        case 2:
                            revive.transform.position = new Vector3(40.39f, -6.74f, revive.transform.position.z);
                            break;
                    }
                    break;
                case (byte)CustomRPC.FlashAbility:
                    byte pheonixid = ALMCIJKELCP.ReadByte();
                    bool activated = ALMCIJKELCP.ReadBoolean();
                    PlayerControl pheonix = PlayerTools.getPlayerById(pheonixid);
                    if (activated)
                        pheonix.nameText.Color = Main.Palette.pheonixColor;
                    else
                        pheonix.nameText.Color = Color.white;
                    break;
                case (byte)CustomRPC.WrathAbility:
                    byte targetid = ALMCIJKELCP.ReadByte();
                    byte killerid = ALMCIJKELCP.ReadByte();
                    PlayerControl target = PlayerTools.getPlayerFromId(targetid);
                    PlayerControl killer = PlayerTools.getPlayerFromId(killerid);
                    killer.Data.IsImpostor = true;
                    killer.MurderPlayer(target);
                    killer.Data.IsImpostor = false;
                    break;
                case (byte)CustomRPC.ExtinguishAbility:
                    byte targetid2 = ALMCIJKELCP.ReadByte();
                    PlayerControl target2 = PlayerTools.getPlayerById(targetid2);
                    //target2.getModdedControl().Role = "Crewmate";
                    target2.getModdedControl().Role = "Crewmate";
                    if (PlayerControl.LocalPlayer == target2)
                        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        {
                            player.GetComponent<SpriteRenderer>().material.SetFloat("_Outline", 0f);
                            player.GetComponent<SpriteRenderer>().material.SetColor("_OutlineColor", Color.clear);
                        }
                    break;
            }
        }
    }
}
