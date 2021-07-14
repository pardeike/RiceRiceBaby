using HarmonyLib;
using Verse;

namespace RiceRiceBaby
{
	[HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.HealthTick))]
	static class Pawn_HealthTracker_HealthTick_Patch
	{
		static void Postfix(Pawn ___pawn)
		{
			if (RiceRiceBabyMain.Settings.death == false) return;

			if (___pawn.Dead || ___pawn.IsColonist == false)
				return;

			if (___pawn.IsHashIntervalTick(30) == false)
				return;

			if (___pawn.health.hediffSet.PainTotal < 0.5)
				return;

			Throttled.Every(5.5f, ___pawn, ThrottleType.wince, () => Defs.dyingSound.PlaySound(___pawn));
		}
	}
}
