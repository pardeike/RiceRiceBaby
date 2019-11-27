using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
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

			if (Rand.Chance(RiceRiceBabyMain.Settings.riceLevel))
				__result = 0.1f;

			Log.Warning($"GetLovinMtbHours {__result} " + pawn.Name.ToStringShort + " " + partner.Name.ToStringShort);
		}
	}

	[HarmonyPatch(typeof(JobDriver_Lovin))]
	[HarmonyPatch("GenerateRandomMinTicksToNextLovin")]
	static class JobDriver_Lovin_GenerateRandomMinTicksToNextLovin_Patch
	{
		static void Postfix(ref int __result)
		{
			if (RiceRiceBabyMain.Settings.romancing == false) return;
			__result = (int)GenMath.LerpDoubleClamped(0f, 1f, 5000, 60, RiceRiceBabyMain.Settings.riceLevel);
		}
	}

	[HarmonyPatch(typeof(Pawn_InteractionsTracker))]
	[HarmonyPatch("TryInteractRandomly")]
	static class GenCollection_TryRandomElementByWeight_Patch
	{
		static bool TryRandomElementByWeight(IEnumerable<InteractionDef> source, Func<InteractionDef, float> weightSelector, out InteractionDef result, Pawn initiator, Pawn recipient)
		{
			if (RiceRiceBabyMain.Settings.romancing)
				if (initiator.CapableColonist() && recipient.CapableColonist())
				{
					var p1 = initiator.relations != null && LovePartnerRelationUtility.HasAnyLovePartner(initiator);
					var p2 = recipient.relations != null && LovePartnerRelationUtility.HasAnyLovePartner(recipient);
					var chance = (p1 || p2) ? RiceRiceBabyMain.Settings.cheatingLevel : RiceRiceBabyMain.Settings.romanceLevel;
					if (Rand.Chance(chance))
					{
						result = source.First(def => def.defName == "RomanceAttempt");
						return true;
					}
				}
			return source.TryRandomElementByWeight((InteractionDef def) => weightSelector(def), out result);
		}

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var method = AccessTools.Method(typeof(GenCollection), "TryRandomElementByWeight");
			var m_TryRandomElementByWeight = method.MakeGenericMethod(typeof(InteractionDef));
			var f_pawn = AccessTools.Field(typeof(Pawn_InteractionsTracker), "pawn");
			InteractionDef dummy;

			var list = instructions.ToList();
			var idx = list.FirstIndexOf(code => code.opcode == OpCodes.Call && code.operand == m_TryRandomElementByWeight);
			list.Insert(idx++, new CodeInstruction(OpCodes.Ldarg_0));
			list.Insert(idx++, new CodeInstruction(OpCodes.Ldfld, f_pawn));
			list.Insert(idx++, new CodeInstruction(OpCodes.Ldloc, 3));
			list[idx].operand = SymbolExtensions.GetMethodInfo(() => TryRandomElementByWeight(default, (def) => 0f, out dummy, null, null));

			return list.AsEnumerable();
		}
	}

	[HarmonyPatch(typeof(InteractionWorker_RomanceAttempt))]
	[HarmonyPatch(nameof(InteractionWorker_RomanceAttempt.SuccessChance))]
	static class InteractionWorker_RomanceAttempt_SuccessChance_Patch
	{
		// https://github.com/rwpsychology/Psychology/blob/638698b6982931216a25465067b1e37034f842a3/Source/Psychology/Harmony/InteractionWorker_RomanceAttempt.cs#L194
		//
		[HarmonyPriority(-100)] // Psychology uses Priority.Last
		[HarmonyPostfix]
		static void Postfix(Pawn initiator, Pawn recipient, ref float __result)
		{
			if (RiceRiceBabyMain.Settings.romancing == false) return;
			if (initiator.CapableColonist() == false) return;
			if (recipient.CapableColonist() == false) return;

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