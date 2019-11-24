using Harmony;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RiceRiceBaby
{
	[HarmonyPatch(typeof(Root_Play))]
	[HarmonyPatch(nameof(Root_Play.Start))]
	static class Game_LoadGame_Patch
	{
		static void Prefix()
		{
			if (RiceRiceBabyMain.Settings.romancing == false) return;

			Tools.ManipulateDefs();
		}
	}

	[HarmonyPatch(typeof(Alert_NeedColonistBeds))]
	[HarmonyPatch("NeedColonistBeds")]
	static class Alert_NeedColonistBeds_NeedColonistBeds_Patch
	{
		static void ErrorOnce(string text, int key, bool ignoreStopLoggingLimit)
		{
			_ = text;
			_ = key;
			_ = ignoreStopLoggingLimit;
		}

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var from = SymbolExtensions.GetMethodInfo(() => Log.ErrorOnce("", 0, false));
			var to = SymbolExtensions.GetMethodInfo(() => ErrorOnce("", 0, false));
			return Transpilers.MethodReplacer(instructions, from, to);
		}
	}

	[HarmonyPatch(typeof(LovePartnerRelationUtility))]
	[HarmonyPatch(nameof(LovePartnerRelationUtility.GetLovinMtbHours))]
	static class LovePartnerRelationUtility_GetLovinMtbHours_Patch
	{
		static void Postfix(Pawn pawn, Pawn partner, ref float __result)
		{
			if (RiceRiceBabyMain.Settings.romancing == false) return;

			if (pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness) < 0.5f)
				return;
			if (pawn.health.hediffSet.PainTotal > 0.5f)
				return;

			if (partner.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness) < 0.5f)
				return;
			if (partner.health.hediffSet.PainTotal > 0.5f)
				return;

			var chance = GenMath.LerpDoubleClamped(0f, 1f, 0f, 1f, RiceRiceBabyMain.Settings.romanceLevel);
			if (Rand.Chance(chance))
				__result = 0.1f;
		}
	}

	[HarmonyPatch(typeof(JobDriver_Lovin))]
	[HarmonyPatch("GenerateRandomMinTicksToNextLovin")]
	static class JobDriver_Lovin_GenerateRandomMinTicksToNextLovin_Patch
	{
		static void Postfix(ref int __result)
		{
			if (RiceRiceBabyMain.Settings.romancing == false) return;
			__result = (int)GenMath.LerpDoubleClamped(0f, 1f, 2500, 60, RiceRiceBabyMain.Settings.romanceLevel);
		}
	}

	[HarmonyPatch(typeof(InteractionWorker_RomanceAttempt))]
	[HarmonyPatch(nameof(InteractionWorker_RomanceAttempt.RandomSelectionWeight))]
	static class InteractionWorker_RomanceAttempt_RandomSelectionWeight_Patch
	{
		static bool Prefix(Pawn initiator, Pawn recipient, ref float __result)
		{
			if (RiceRiceBabyMain.Settings.romancing == false) return true;

			if (LovePartnerRelationUtility.LovePartnerRelationExists(initiator, recipient))
				__result = 0f;
			else
				__result = GenMath.LerpDoubleClamped(0f, 1f, 0f, 0.9f, RiceRiceBabyMain.Settings.romanceLevel);

			return false;
		}
	}

	[HarmonyPatch(typeof(InteractionWorker_RomanceAttempt))]
	[HarmonyPatch(nameof(InteractionWorker_RomanceAttempt.SuccessChance))]
	static class InteractionWorker_RomanceAttempt_SuccessChance_Patch
	{
		static readonly HashSet<PawnRelationDef> partnerRelations = new HashSet<PawnRelationDef>()
		{
			PawnRelationDefOf.Fiance,
			PawnRelationDefOf.Spouse,
			PawnRelationDefOf.Lover
		};

		static bool HasPartner(this Pawn pawn)
		{
			return pawn.relations
				.DirectRelations.Any(relation => relation.otherPawn.Dead == false && partnerRelations.Contains(relation.def));
		}

		static bool Prefix(Pawn initiator, Pawn recipient, ref float __result)
		{
			if (RiceRiceBabyMain.Settings.romancing == false) return true;

			var p1 = initiator.HasPartner();
			var p2 = recipient.HasPartner();

			if (p1 && p2)
				__result = GenMath.LerpDoubleClamped(0f, 1f, 0f, 0.2f, RiceRiceBabyMain.Settings.romanceLevel);
			else if (p1 || p2)
				__result = GenMath.LerpDoubleClamped(0f, 1f, 0f, 0.4f, RiceRiceBabyMain.Settings.romanceLevel);
			else
				__result = RiceRiceBabyMain.Settings.romanceLevel;

			return false;
		}
	}
}
