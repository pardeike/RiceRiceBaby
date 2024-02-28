using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace RiceRiceBaby
{
	[HarmonyPatch(typeof(Root_Play), nameof(Root_Play.Start))]
	static class Root_Play_Start_Patch
	{
		static void Prefix()
		{
			if (RiceRiceBabyMain.Settings.romancing == false)
				return;
			Tools.ManipulateDefs();
		}
	}

	[HarmonyPatch(typeof(LovePartnerRelationUtility), nameof(LovePartnerRelationUtility.GetLovinMtbHours))]
	static class LovePartnerRelationUtility_GetLovinMtbHours_Patch
	{
		static void Postfix(Pawn pawn, Pawn partner, ref float __result)
		{
			if (RiceRiceBabyMain.Settings.romancing == false)
				return;

			if (pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness) < 0.5f)
				return;
			if (pawn.health.hediffSet.PainTotal > 0.5f)
				return;

			if (partner.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness) < 0.5f)
				return;
			if (partner.health.hediffSet.PainTotal > 0.5f)
				return;

			if (RiceRiceBabyMain.Settings.homosexuality == false && pawn.gender == partner.gender)
				return;

			if (Rand.Chance(RiceRiceBabyMain.Settings.riceLevel))
				__result = 0.1f;
		}
	}

	[HarmonyPatch(typeof(JobDriver_Lovin), nameof(JobDriver_Lovin.GenerateRandomMinTicksToNextLovin))]
	static class JobDriver_Lovin_GenerateRandomMinTicksToNextLovin_Patch
	{
		static void Postfix(ref int __result)
		{
			if (RiceRiceBabyMain.Settings.romancing == false)
				return;
			__result = (int)GenMath.LerpDoubleClamped(0f, 1f, 5000, 60, RiceRiceBabyMain.Settings.riceLevel);
		}
	}

	[HarmonyPatch(typeof(Pawn_InteractionsTracker), nameof(Pawn_InteractionsTracker.TryInteractRandomly))]
	static class GenCollection_TryInteractRandomly_Patch
	{
		static InteractionDef dummy = null;
		static readonly MethodInfo m_SelectRomanceAttempt = SymbolExtensions.GetMethodInfo(() => SelectRomanceAttempt(default, null, null, out dummy));

		static bool SelectRomanceAttempt(List<InteractionDef> defs, Pawn initiator, Pawn recipient, out InteractionDef result)
		{
			result = null;

			if (RiceRiceBabyMain.Settings.romancing == false)
				return false;

			if (RiceRiceBabyMain.Settings.homosexuality == false && initiator.gender == recipient.gender)
				return false;

			if (defs.Any(def => def == InteractionDefOf.RomanceAttempt) == false)
				return false;

			if (initiator.IsColonist == false || recipient.IsColonist == false)
				return false;

			if (LovePartnerRelationUtility.LovePartnerRelationExists(initiator, recipient))
				return false;

			var p1 = LovePartnerRelationUtility.HasAnyLovePartner(initiator);
			var p2 = LovePartnerRelationUtility.HasAnyLovePartner(recipient);
			var chance = (p1 || p2) ? RiceRiceBabyMain.Settings.cheatingLevel : RiceRiceBabyMain.Settings.romanceLevel;
			if (Rand.Chance(chance) == false)
				return false;

			var romanceAttempt = InteractionDefOf.RomanceAttempt;
			if (initiator.interactions.TryInteractWith(recipient, romanceAttempt) == false)
				return false;

			result = romanceAttempt;
			Pawn_InteractionsTracker.workingList.Clear();
			return true;
		}

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var method = AccessTools.Method(typeof(GenCollection), nameof(GenCollection.TryRandomElementByWeight));
			var m_TryRandomElementByWeight = method.MakeGenericMethod(typeof(InteractionDef));

			var list = instructions.ToList();
			var idx = list.FirstIndexOf(code => code.Calls(m_TryRandomElementByWeight));
			if (idx >= 5)
			{
				var allDefs = list[idx - 5].opcode;
				var intDef = list[idx - 1].operand;
				var actionLabel = list[idx + 1].operand;

				idx -= 5;
				var idx2 = idx;
				while (idx2 > 0 && list[idx2].opcode != OpCodes.Ldarg_0)
					idx2--;
				if (idx2 > 0)
				{
					var labels = list[idx].labels.ToArray();
					list[idx].labels.Clear();

					list.InsertRange(idx, new[]
					{
						new CodeInstruction(allDefs) { labels = labels.ToList() },
						list[idx2++], list[idx2++],
						list[idx2++], list[idx2++],
						new CodeInstruction(OpCodes.Ldloca_S, intDef),
						new CodeInstruction(OpCodes.Call, m_SelectRomanceAttempt),
						new CodeInstruction(OpCodes.Brtrue, actionLabel),
					});
				}
			}

			return list.AsEnumerable();
		}
	}

	[HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), nameof(InteractionWorker_RomanceAttempt.SuccessChance))]
	static class InteractionWorker_RomanceAttempt_SuccessChance_Patch
	{
		// https://github.com/rwpsychology/Psychology/blob/638698b6982931216a25465067b1e37034f842a3/Source/Psychology/Harmony/InteractionWorker_RomanceAttempt.cs#L194
		//
		[HarmonyPriority(-100)] // Psychology uses Priority.Last
		[HarmonyPostfix]
		static void Postfix(Pawn initiator, Pawn recipient, ref float __result)
		{
			if (RiceRiceBabyMain.Settings.romancing == false)
				return;
			if (initiator.IsColonist == false)
				return;
			if (recipient.IsColonist == false)
				return;

			if (RiceRiceBabyMain.Settings.homosexuality == false && initiator.gender == recipient.gender)
				return;

			var p1 = initiator.relations != null && LovePartnerRelationUtility.HasAnyLovePartner(initiator);
			var p2 = recipient.relations != null && LovePartnerRelationUtility.HasAnyLovePartner(recipient);

			if (p1 && p2)
				__result = RiceRiceBabyMain.Settings.cheatingLevel / 2f;
			else if (p1 || p2)
				__result = RiceRiceBabyMain.Settings.cheatingLevel;
			else
				__result = RiceRiceBabyMain.Settings.romanceLevel;
		}
	}
}