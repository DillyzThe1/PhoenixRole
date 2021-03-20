using System;
using HarmonyLib;
using System.Net.Http;
using UnityEngine;
using Hazel;
using System.Reflection;
using Reactor.Extensions;
using System.IO;
using Reactor.Unstrip;
using System.Collections.Generic;
using static Pheonix.PheonixRole;
using UnityEngine.SceneManagement;

namespace Pheonix
{
    [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.Method_24))]
    class GameOptionsData_ToHudString
    {
        static void Postfix(ref string __result)
        {
            DestroyableSingleton<HudManager>.Instance.GameSettings.scale = 0.75f;
        }
    }
    [HarmonyPatch]
    class GameOptionsMenuManager
    {
        static float defaultBounds = 0f;
        [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
        class Start
        {
            static void Postfix(ref GameOptionsMenu __instance)
            {
                defaultBounds = __instance.GetComponentInParent<Scroller>().YBounds.max;
            }
        }

        [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
        class Update
        {
            static void Postfix(ref GameOptionsMenu __instance)
            {
                __instance.GetComponentInParent<Scroller>().YBounds.max = 13.5f;
            }
        }
        [HarmonyPatch]
        public static class HudPatch
        {
            public static void updateMeetingHUD(MeetingHud __instance)
            {
                foreach (PlayerVoteArea player in __instance.playerStates)
                {
                    if (player.NameText.Text == PlayerControl.LocalPlayer.nameText.Text && PlayerControl.LocalPlayer.isPlayerRole("Pheonix"))
                    {
                        player.NameText.Color = Main.Palette.pheonixColor;
                    }
                    else if (!PlayerControl.LocalPlayer.Data.IsImpostor)
                    {
                        player.NameText.Color = Color.white;
                    }
                }
            }
            [HarmonyPatch(typeof(HudManager), "Update")]
            public static void Postfix(HudManager __instance)
            {
                if (MeetingHud.Instance != null)
                {
                    HudPatch.updateMeetingHUD(MeetingHud.Instance);
                    HudStartPatch.reliveButton.killButtonManager.gameObject.SetActive(false);
                    HudStartPatch.dropletButton.killButtonManager.gameObject.SetActive(false);
                    HudStartPatch.flashButton.killButtonManager.gameObject.SetActive(false);
                    HudStartPatch.wrathButton.killButtonManager.gameObject.SetActive(false);
                    HudStartPatch.extinguishButton.killButtonManager.gameObject.SetActive(false);
                    PlayerTools.wrathWasActive = false;
                }
                else
                {
                    HudStartPatch.reliveButton.killButtonManager.gameObject.SetActive(true);
                    HudStartPatch.dropletButton.killButtonManager.gameObject.SetActive(true);
                    HudStartPatch.flashButton.killButtonManager.gameObject.SetActive(true);
                    HudStartPatch.wrathButton.killButtonManager.gameObject.SetActive(true);
                    HudStartPatch.extinguishButton.killButtonManager.gameObject.SetActive(true);
                    PlayerTools.wrathWasActive = true;
                }
                if (PlayerControl.AllPlayerControls.Count > 1 && PlayerControl.LocalPlayer.isPlayerRole("Pheonix"))
                {
                    PlayerControl.LocalPlayer.nameText.Color = Main.Palette.pheonixColor;
                }
                else if (!PlayerControl.LocalPlayer.Data.IsImpostor)
                {
                    PlayerControl.LocalPlayer.nameText.Color = Color.white;
                }

            }
        }
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
        public static class HudStartPatch
        {
            public static CooldownButton reliveButton;
            public static CooldownButton dropletButton;
            public static CooldownButton flashButton;
            public static CooldownButton wrathButton;
            public static CooldownButton extinguishButton;
            public static void Postfix(HudManager __instance)
            {
                #region add the immortal button
                reliveButton = new CooldownButton(delegate ()
                {
                    Immortal.onClick(reliveButton);
                }, (float)1, "Pheonix.Assets.scndchancedone.png", 100f, Vector2.zero, CooldownButton.Category.OnlyPheonix, __instance, (float)0, delegate ()
                {
                    Immortal.onDone();
                });

                reliveButton.killButtonManager.gameObject.SetActive(false);
                #endregion
                #region add the droplet button
                dropletButton = new CooldownButton(delegate ()
                {
                    Droplit.onClick(dropletButton);
                }, (float)5, "Pheonix.Assets.commsIcon.png", 100f, Vector2.zero, CooldownButton.Category.OnlyPheonix, __instance, (float)0, delegate ()
                {
                    Droplit.onDone();
                });

                dropletButton.killButtonManager.SetTarget(PlayerTools.getClosestDeadPlayer(PlayerControl.LocalPlayer));
                dropletButton.killButtonManager.gameObject.SetActive(false);
                #endregion
                #region add the reveal button
                flashButton = new CooldownButton(delegate ()
                {
                    Reveal.onClick(flashButton);
                }, (float)30f, "Pheonix.Assets.revealIcon.png", 100f, new Vector2(1.1f, 0f), CooldownButton.Category.OnlyPheonix, __instance, (float)HarmonyMain.flashDuration.GetValue(), delegate ()
                {
                    Reveal.onDone();
                });

                flashButton.killButtonManager.gameObject.SetActive(false);
                #endregion
                #region add the wrath button
                wrathButton = new CooldownButton(delegate ()
                {
                    Wrath.onClick(wrathButton);
                }, (float)15f, "Pheonix.Assets.wrathIcon.png", 100f, new Vector2(0.6f, 1.2f), CooldownButton.Category.OnlyPheonix, __instance, (float)0, delegate ()
                {
                    Wrath.onDone();
                });

                wrathButton.killButtonManager.gameObject.SetActive(false);
                #endregion
                #region add the extinguish button
                extinguishButton = new CooldownButton(delegate ()
                {
                    Water.onClick(extinguishButton);
                }, (float)HarmonyMain.extinguishCooldown.GetValue(), "Pheonix.Assets.clenseIcon.png", 100f, Vector2.zero, CooldownButton.Category.OnlyImpostor, __instance, (float)0, delegate ()
                {
                    Water.onDone();
                });

                extinguishButton.killButtonManager.gameObject.SetActive(false);
                #endregion
            }

        }
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        public static class HudUpdatePatch
        {
            public static void Postfix()
            {
                if (ShipStatus.Instance != null)
                {
                    CooldownButton.HudUpdate();
                    if (PlayerControl.LocalPlayer.Data.IsDead)
                    {
                        PlayerTools.hasDied = true;
                        HudStartPatch.extinguishButton.killButtonManager.gameObject.SetActive(false);
                        if (!PlayerTools.hasPressedRelive)
                        {
                            HudStartPatch.reliveButton.killButtonManager.gameObject.SetActive(true);
                        }
                        else
                        {
                            HudStartPatch.reliveButton.killButtonManager.gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        HudStartPatch.reliveButton.killButtonManager.gameObject.SetActive(false);
                        HudStartPatch.extinguishButton.killButtonManager.gameObject.SetActive(true);
                    }
                    if (!PlayerControl.LocalPlayer.Data.IsDead && PlayerTools.hasDied)
                    {
                        if (!PlayerTools.hasPressedDroplit)
                            HudStartPatch.dropletButton.killButtonManager.gameObject.SetActive(true);
                        else
                            HudStartPatch.dropletButton.killButtonManager.gameObject.SetActive(false);
                        if (!PlayerTools.hasPressedRelive)
                            HudStartPatch.reliveButton.killButtonManager.gameObject.SetActive(true);
                        else
                            HudStartPatch.reliveButton.killButtonManager.gameObject.SetActive(false);
                        if (!PlayerTools.hasPressedWrath)
                        {
                            HudStartPatch.wrathButton.killButtonManager.gameObject.SetActive(true);
                            PlayerTools.wrathWasActive = true;
                        }
                        else
                        {
                            HudStartPatch.wrathButton.killButtonManager.gameObject.SetActive(false);
                            PlayerTools.wrathWasActive = false;
                        }
                        if (!PlayerTools.hasPressedReveal)
                            HudStartPatch.flashButton.killButtonManager.gameObject.SetActive(true);
                        else
                            HudStartPatch.flashButton.killButtonManager.gameObject.SetActive(false);
                    }
                    else
                    {
                        HudStartPatch.dropletButton.killButtonManager.gameObject.SetActive(false);
                        HudStartPatch.flashButton.killButtonManager.gameObject.SetActive(false);
                        HudStartPatch.wrathButton.killButtonManager.gameObject.SetActive(false);
                        PlayerTools.wrathWasActive = false;
                    }
                    if (PlayerTools.waterPressed)
                    {
                        HudStartPatch.extinguishButton.killButtonManager.gameObject.SetActive(false);
                    }
                }
                else
                {
                    HudStartPatch.wrathButton.killButtonManager.gameObject.SetActive(false);
                    HudStartPatch.reliveButton.killButtonManager.gameObject.SetActive(false);
                    HudStartPatch.dropletButton.killButtonManager.gameObject.SetActive(false);
                    HudStartPatch.flashButton.killButtonManager.gameObject.SetActive(false);
                    HudStartPatch.extinguishButton.killButtonManager.gameObject.SetActive(false);
                    PlayerTools.wrathWasActive = false;
                }
            }

            public static void BobuxUpdate(CooldownButton bttn)
            {
                if (!PlayerControl.LocalPlayer.isPlayerRole("Pheonix"))
                    return;
                double mindist = 2.5;
                Wrath.wrathtarget = null;
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    Vector3 refpos = PlayerControl.LocalPlayer.GetTruePosition();
                    Vector3 playerpos = player.GetTruePosition();
                    double dist = Math.Sqrt((refpos.x - playerpos.x) * (refpos.x - playerpos.x) + (refpos.y - playerpos.y) * (refpos.y - playerpos.y));
                    if (player == PlayerControl.LocalPlayer)
                        continue;
                    if (dist <= mindist)
                    {
                        mindist = dist;
                        Wrath.wrathtarget = player;
                    }
                }
                bool someoneExists = false;
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player != Wrath.wrathtarget || player.inVent || player.Data.IsDead || PlayerControl.LocalPlayer.Data.IsDead || PlayerTools.hasPressedWrath || !PlayerTools.hasDied)
                    {
                        player.GetComponent<SpriteRenderer>().material.SetFloat("_Outline", 0f);
                        player.GetComponent<SpriteRenderer>().material.SetColor("_OutlineColor", Color.clear);
                        continue;
                    }
                    player.GetComponent<SpriteRenderer>().material.SetFloat("_Outline", 1f);
                    player.GetComponent<SpriteRenderer>().material.SetColor("_OutlineColor", Main.Palette.pheonixColor);
                    someoneExists = true;
                }
                if (someoneExists && !Wrath.wrathtarget.Data.IsDead)
                {
                    HudStartPatch.wrathButton.killButtonManager.renderer.color = new Color(1f, 1f, 1f, 1f);
                }
                else
                {
                    Wrath.wrathtarget = null;
                    HudStartPatch.wrathButton.killButtonManager.renderer.color = new Color(1f, 1f, 1f, 0.5f);
                }
            }
            public static PlayerControl dripTarget;
            public static void Deadate(CooldownButton bttn)
            {
                if (!PlayerControl.LocalPlayer.isPlayerRole("Pheonix"))
                    return;
                double mindist = 2.5;
                dripTarget = null;
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    Vector3 refpos = PlayerControl.LocalPlayer.GetTruePosition();
                    Vector3 playerpos = player.GetTruePosition();
                    double dist = Math.Sqrt((refpos.x - playerpos.x) * (refpos.x - playerpos.x) + (refpos.y - playerpos.y) * (refpos.y - playerpos.y));
                    if (player == PlayerControl.LocalPlayer)
                        continue;
                    if (dist <= mindist)
                    {
                        mindist = dist;
                        dripTarget = player;
                    }
                }
                bool someoneExists = false;
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player != dripTarget || player.inVent || !player.Data.IsDead || PlayerControl.LocalPlayer.Data.IsDead || !HudStartPatch.dropletButton.killButtonManager.isActive || !PlayerTools.hasDied || PlayerTools.hasPressedDroplit)
                        continue;
                    player.myRend.enabled = true;
                    player.HatRenderer.enabled = true;
                    someoneExists = true;
                }
                if (someoneExists && dripTarget.Data.IsDead)
                {
                    HudStartPatch.dropletButton.killButtonManager.renderer.color = new Color(1f, 1f, 1f, 1f);
                }
                else
                {
                    dripTarget = null;
                    HudStartPatch.dropletButton.killButtonManager.renderer.color = new Color(1f, 1f, 1f, 0.5f);
                }
            }
            public static void WaterUpdate(CooldownButton bttn)
            {
                if (!PlayerControl.LocalPlayer.Data.IsImpostor)
                    return;
                double mindist = PlayerControl.GameOptions.KillDistance * 2;
                Water.watertarget = null;
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    Vector3 refpos = PlayerControl.LocalPlayer.GetTruePosition();
                    Vector3 playerpos = player.GetTruePosition();
                    double dist = Math.Sqrt((refpos.x - playerpos.x) * (refpos.x - playerpos.x) + (refpos.y - playerpos.y) * (refpos.y - playerpos.y));
                    if (player == PlayerControl.LocalPlayer || player.Data.IsImpostor)
                        continue;
                    if (dist <= mindist)
                    {
                        mindist = dist;
                        Water.watertarget = player;
                    }
                }
                bool someoneExists2 = false;
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player != Water.watertarget || player.inVent || player.Data.IsDead || PlayerControl.LocalPlayer.Data.IsDead || PlayerTools.waterPressed)
                        continue;
                    someoneExists2 = true;
                }
                if (someoneExists2 && !Water.watertarget.Data.IsDead)
                {
                    HudStartPatch.extinguishButton.killButtonManager.renderer.color = new Color(1f, 1f, 1f, 1f);
                }
                else
                {
                    Water.watertarget = null;
                    HudStartPatch.extinguishButton.killButtonManager.renderer.color = new Color(1f, 1f, 1f, 0.5f);
                }
            }
        }
        public static class Immortal
        {
            public static void onClick(CooldownButton bttn)
            {
                PlayerTools.hasPressedRelive = true;
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ReviveAbility, Hazel.SendOption.None, -1);
                writer.Write(PlayerControl.LocalPlayer.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                PlayerControl.LocalPlayer.Revive();
                switch (PlayerControl.GameOptions.MapId)
                {
                    case 0:
                        PlayerControl.LocalPlayer.transform.position = new Vector3(-7.25f, -4.85f, PlayerControl.LocalPlayer.transform.position.z);
                        break;
                    case 1:
                        PlayerControl.LocalPlayer.transform.position = new Vector3(16.26f, 0.54f, PlayerControl.LocalPlayer.transform.position.z);
                        break;
                    case 2:
                        PlayerControl.LocalPlayer.transform.position = new Vector3(40.39f, -6.74f, PlayerControl.LocalPlayer.transform.position.z);
                        break;
                }
            }
            public static void onDone() //a
            { }
        }
        public static class Droplit
        {
            public static void onClick(CooldownButton bttn)
            {
                if (HudUpdatePatch.dripTarget != null)
                {
                    PlayerTools.hasPressedDroplit = true;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ReviveAbility, Hazel.SendOption.None, -1);
                    writer.Write(HudUpdatePatch.dripTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    HudUpdatePatch.dripTarget.Revive();
                    switch (PlayerControl.GameOptions.MapId)
                    {
                        case 0:
                            HudUpdatePatch.dripTarget.transform.position = new Vector3(-7.25f, -4.85f, PlayerControl.LocalPlayer.transform.position.z);
                            break;
                        case 1:
                            HudUpdatePatch.dripTarget.transform.position = new Vector3(16.26f, 0.54f, PlayerControl.LocalPlayer.transform.position.z);
                            break;
                        case 2:
                            HudUpdatePatch.dripTarget.transform.position = new Vector3(40.39f, -6.74f, PlayerControl.LocalPlayer.transform.position.z);
                            break;
                    }
                }
                else
                {
                    bttn.killButtonManager.TimerText.Color = new Color(255, 255, 255);
                    bttn.Timer = 0f;
                    bttn.isEffectActive = false;
                }
            }
            public static void onDone() //a
            { }
        }
        public static class Reveal
        {
            public static void onClick(CooldownButton bttn)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.FlashAbility, Hazel.SendOption.None, -1);
                writer.Write(PlayerControl.LocalPlayer.PlayerId);
                writer.Write(true);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
            public static void onDone() //a
            {
                PlayerTools.hasPressedReveal = true;
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.FlashAbility, Hazel.SendOption.None, -1);
                writer.Write(PlayerControl.LocalPlayer.PlayerId);
                writer.Write(false);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
        }
        public static class Wrath
        {
            public static PlayerControl wrathtarget;
            public static void onClick(CooldownButton bttn)
            {
                if (Wrath.wrathtarget != null)
                {
                    PlayerTools.wrathWasActive = false;
                    PlayerControl.LocalPlayer.Data.IsImpostor = true;
                    PlayerControl.LocalPlayer.MurderPlayer(wrathtarget);
                    PlayerControl.LocalPlayer.Data.IsImpostor = false;
                    PlayerTools.hasPressedWrath = true;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.WrathAbility, Hazel.SendOption.None, -1);
                    writer.Write(wrathtarget.PlayerId);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    if (wrathtarget.Data.IsImpostor)
                    {
                        PlayerTools.hasPressedWrath = false;
                    }
                }
                else
                {
                    bttn.killButtonManager.TimerText.Color = new Color(255, 255, 255);
                    bttn.Timer = 0f;
                    bttn.isEffectActive = false;
                }

            }
            public static void onDone() //a
            { }
        }
        public static class Water
        {
            public static PlayerControl watertarget;
            public static void onClick(CooldownButton bttn)
            {
                if (Water.watertarget != null)
                {
                    watertarget.nameText.Color = Color.white;
                    MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.FlashAbility, Hazel.SendOption.None, -1);
                    writer2.Write(watertarget.PlayerId);
                    writer2.Write(false);
                    AmongUsClient.Instance.FinishRpcImmediately(writer2);
                    PlayerTools.waterPressed = true;
                    watertarget.getModdedControl().Role = "Crewmate";
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ExtinguishAbility, Hazel.SendOption.None, -1);
                    writer.Write(watertarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);

                }
                else
                {
                    bttn.killButtonManager.TimerText.Color = new Color(255, 255, 255);
                    bttn.Timer = 0f;
                    bttn.isEffectActive = false;
                }
            }
            public static void onDone() //a
            { }
        }
    }

}
// iumpsotoer aossis sus1@1!!!!!!!!