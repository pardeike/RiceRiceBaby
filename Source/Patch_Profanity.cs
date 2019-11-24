using Harmony;
using RimWorld;
using Verse;

namespace RiceRiceBaby
{
	[HarmonyPatch(typeof(MoteMaker))]
	[HarmonyPatch(nameof(MoteMaker.ThrowMetaIcon))]
	static class MoteMaker_ThrowMetaIcon_Patch2
	{
		static void Postfix(IntVec3 cell, Map map, ThingDef moteDef)
		{
			if (RiceRiceBabyMain.Settings.profanity == false || moteDef != ThingDefOf.Mote_SleepZ) return;
			
			var pawn = map.thingGrid.ThingAt<Pawn>(cell);
			if (pawn.IsColonist == false) return;

			if (Rand.Chance(0.4f))
			{
				SoundDef def = null;
				if (pawn.CanSnore() && Rand.Chance(0.5f))
				{
					def = Rand.Chance(0.5f) ? Defs.snoreMaleSound : Defs.snoreFemaleSound;
					if (pawn != null && pawn.gender == Gender.Male) def = Defs.snoreMaleSound;
					if (pawn != null && pawn.gender == Gender.Female) def = Defs.snoreFemaleSound;
				}
				else
					Throttled.Every(4.3, pawn, ThrottleType.lastBreath, () => def = Defs.sleepingSound);

				def?.PlaySound(cell, map);
			}
		}
	}

	[HarmonyPatch(typeof(PawnRenderer))]
	[HarmonyPatch(nameof(PawnRenderer.RendererTick))]
	static class PawnRenderer_RendererTick_Patch
	{
		static void Postfix(Pawn ___pawn)
		{
			if (RiceRiceBabyMain.Settings.profanity == false) return;

			if (___pawn.Downed == false || ___pawn.IsColonist == false)
				return;

			// TODO
		}
	}
}