using System;
using HarmonyLib;
using static Pheonix.PheonixRole;

namespace Pheonix
{
    [HarmonyPatch(typeof(IntroCutscene.CoBegin__d), nameof(IntroCutscene.CoBegin__d.MoveNext))]
    class IntroCutscenePatch
    {
        static void Postfix(IntroCutscene.CoBegin__d __instance)
        {
            ModPlayerControl Pheonix = Main.Logic.getRolePlayer("Pheonix");
            if (Pheonix != null)
                Pheonix.LastAbilityTime = DateTime.UtcNow;
            if (PlayerControl.LocalPlayer.isPlayerRole("Pheonix"))
            {
                __instance.__this.Title.Text = "Phoenix";
                __instance.__this.Title.Color = Main.Palette.pheonixColor;
                __instance.__this.ImpostorText.Text = "Bring balance to The [FF0000FF]Impostors[] force.";
                __instance.__this.BackgroundBar.material.color = Main.Palette.pheonixColor;
            }
        }
    }
}
