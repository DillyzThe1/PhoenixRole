using HarmonyLib;

namespace Pheonix
{
	[HarmonyPatch(typeof(ExileController), "Begin")]
	public static class ExilePatch
	{
		public static void Postfix([HarmonyArgument(0)] GameData.PlayerInfo exiled, CNNGMDOPELD __instance)
		{
			PlayerControl wowww = null;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
				if (!PlayerControl.GameOptions.ConfirmImpostor)
					continue;
                if (player.PlayerId != exiled.PlayerId)
                    continue;
				if (!player.isPlayerRole("Pheonix"))
					continue;
                wowww = player;
            }
			if (wowww != null)
			{
				__instance.EOFFAJKKDMI = exiled.PlayerName + " was Our Pheonix.";
			}
		}
	}
}