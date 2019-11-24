using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RiceRiceBaby
{
	class RiceRiceBabyMain : Mod
	{
		public static RiceRiceBabySettings Settings;

		public RiceRiceBabyMain(ModContentPack content) : base(content)
		{
			Settings = GetSettings<RiceRiceBabySettings>();

			var harmony = HarmonyInstance.Create("net.pardeike.rimworld.mod.ricericebaby");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			Settings.DoWindowContents(inRect);
		}

		public override string SettingsCategory()
		{
			return "Rice Rice Baby";
		}
	}

	// whip it real good
	//
	[HarmonyPatch]
	static class AchtungMod_ForcedWork_AddForcedJob_Patch
	{
		static bool Prepare()
		{
			return TargetMethod() != null;
		}

		static MethodBase TargetMethod()
		{
			return AccessTools.Method("AchtungMod.ForcedWork:AddForcedJob");
		}

		static void Postfix(Pawn pawn)
		{
			if (RiceRiceBabyMain.Settings.whipAchtungForce)
				Defs.whipSound.PlaySound(pawn);
		}
	}

	// rice rice baby!
	//
	[HarmonyPatch(typeof(MoteMaker))]
	[HarmonyPatch(nameof(MoteMaker.ThrowMetaIcon))]
	static class MoteMaker_ThrowMetaIcon_Patch
	{
		static bool Prefix(IntVec3 cell, Map map, ThingDef moteDef)
		{
			if (RiceRiceBabyMain.Settings.riceInsteadOfHeartWhileLovin && moteDef == ThingDefOf.Mote_Heart)
			{
				var pawn = map.thingGrid.ThingAt<Pawn>(cell);
				if (pawn == null || pawn.IsColonist == false) return true;

				var soundDef = Rand.Chance(0.5f) ? Defs.mmmSound : Defs.achSound;
				if (pawn.gender == Gender.Male) soundDef = Defs.mmmSound;
				if (pawn.gender == Gender.Female) soundDef = Defs.achSound;

				if (pawn.CurJobDef == JobDefOf.Lovin)
				{
					var playDefaultSound = true;

					if (Rand.Chance(0.25f))
					{
						if (Rand.Chance(0.5f))
							soundDef.PlaySound(cell, map);

						_ = MoteMaker.ThrowMetaIcon(cell, map, Defs.riceMote);
						Defs.riceSound.PlaySound(cell, map);
						return false;
					}
					else
					{
						if (pawn.gender == Gender.Male)
						{
							var bed = map.thingGrid.ThingAt<Building_Bed>(cell);
							if (bed != null && Rand.Chance(0.2f))
							{
								Defs.damageSound.PlaySound(cell, map);
								Throttled.Every(3, pawn, ThrottleType.breakingBed, () =>
								{
									Defs.breakSound.PlaySound(cell, map);
									_ = bed.TakeDamage(new DamageInfo(DamageDefOf.Crush, Rand.Range(40f, 80f)));
								});
								playDefaultSound = false;
							}
						}
					}

					if (playDefaultSound)
						Defs.bedSound.PlaySound(cell, map);
				}
				else // JobDefOf.Mate
				{
					if (pawn.gender == Gender.Male)
					{
						var stop = false;
						Throttled.AfterIdle(10, pawn, ThrottleType.lastBreath, () =>
						{
							Defs.whisleSound.PlaySound(cell, map);
							_ = MoteMaker.ThrowMetaIcon(cell, map, Defs.wantRiceMote);
							stop = true;
						});
						if (stop)
							return false;
					}
					else
					{
						if (Rand.Chance(0.1f))
						{
							Defs.achSound.PlaySound(cell, map);
							_ = MoteMaker.ThrowMetaIcon(cell, map, Defs.wantRiceMote);
							return false;
						}
					}
				}
			}

			if (RiceRiceBabyMain.Settings.snoring && moteDef == ThingDefOf.Mote_SleepZ)
			{
				var pawn = map.thingGrid.ThingAt<Pawn>(cell);
				if (pawn.IsColonist && Rand.Chance(0.4f))
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

			return true;
		}
	}

	[HarmonyPatch(typeof(PawnRenderer))]
	[HarmonyPatch("RenderPawnInternal")]
	[HarmonyPatch(new[] { typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool) })]
	static class PawnRenderer_RenderPawnInternal_Patch
	{
		static readonly AccessTools.FieldRef<JobDriver_Lovin, int> ticksLeftRef = AccessTools.FieldRefAccess<JobDriver_Lovin, int>("ticksLeft");
		static readonly AccessTools.FieldRef<JobDriver_Lovin, TargetIndex> PartnerIndRef = AccessTools.FieldRefAccess<JobDriver_Lovin, TargetIndex>("PartnerInd");

		static void LovinFix(Pawn pawn, ref Vector3 rootLoc, ref float angle, ref Rot4 headFacing)
		{
			if (RiceRiceBabyMain.Settings.riceInsteadOfHeartWhileLovin == false) return;
			if (pawn.CurJobDef != JobDefOf.Lovin) return;

			var bed = pawn.CurrentBed();
			if (bed == null) return;

			var idx = bed.GetCurOccupantSlotIndex(pawn);
			if (idx < 0 || idx > 1) return;

			var partner = bed.GetCurOccupant(1 - idx);

			var driver1 = pawn.jobs.curDriver as JobDriver_Lovin;
			var driver2 = partner.jobs.curDriver as JobDriver_Lovin;
			if (driver1 == null || driver2 == null) return;
			var ticksLeft = Math.Min(ticksLeftRef(driver1), ticksLeftRef(driver2));

			var baseRotation = bed.Rotation;
			var longSide = baseRotation.FacingCell.ToVector3();
			var orthogonalRotation = baseRotation.Rotated(RotationDirection.Clockwise);
			var shortSide = orthogonalRotation.FacingCell.ToVector3();

			var ticks = GenTicks.TicksGame / 600f;
			const float maxTicksLeft = 2500f;
			var speed = GenMath.LerpDoubleClamped(0f, maxTicksLeft + 64f, 4f, 0f, ticksLeft + (pawn.GetHashCode() % 600));

			var hump = (float)Math.Sin(ticks * speed);
			var sway = (float)Math.Sin(ticks * 400f) / 100f;
			var longHump = (pawn.gender == Gender.Male ? 1 / 25f : 1 / 45f) * hump;

			var lovin = Lovin.LovinFor(pawn, partner);
			headFacing = lovin.face[idx];
			if (lovin.onTop)
			{
				var shortHump = idx == 0 ? -0.45f : 0.45f;
				rootLoc += longSide * longHump + shortSide * shortHump;
				if (idx == 0)
					rootLoc += new Vector3(0f, 0.01f, 0f);
				angle += (float)Math.Sin(ticks * 10f) * 2f;
			}
			else
			{
				var shortHump = idx == 0 ? -0.2f - sway : 0.2f + sway;
				rootLoc += longSide * longHump + shortSide * shortHump;
				angle += (float)Math.Sin(ticks * 50f) * 2f;
			}
		}

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var dummy1 = Vector3.zero; var dummy2 = 0f; var dummy3 = Rot4.Invalid;
			yield return new CodeInstruction(OpCodes.Ldarg_0);
			yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), "pawn"));
			yield return new CodeInstruction(OpCodes.Ldarga_S, 1);
			yield return new CodeInstruction(OpCodes.Ldarga_S, 2);
			yield return new CodeInstruction(OpCodes.Ldarga_S, 5);
			yield return new CodeInstruction(OpCodes.Call, SymbolExtensions.GetMethodInfo(() => LovinFix(null, ref dummy1, ref dummy2, ref dummy3)));
			foreach (var instruction in instructions)
				yield return instruction;
		}
	}

	// hey romance baby!

	[HarmonyPatch(typeof(Root_Play))]
	[HarmonyPatch(nameof(Root_Play.Start))]
	static class Game_LoadGame_Patch
	{
		static void Prefix()
		{
			if (RiceRiceBabyMain.Settings.romancing == false) return;

			DefTools.ManipulateDefs();
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

	// fu & wtf's
	//
	[HarmonyPatch(typeof(MemoryThoughtHandler))]
	[HarmonyPatch(nameof(MemoryThoughtHandler.TryGainMemory))]
	[HarmonyPatch(new Type[] { typeof(Thought_Memory), typeof(Pawn) })]
	static class MemoryThoughtHandler_TryGainMemory_Patch
	{
		static MoteBubble CreateSwearBubble(Pawn pawn, Thought_Memory thought)
		{
			if (RiceRiceBabyMain.Settings.swearing)
				if (Defs.triggeringDefs.TryGetValue(thought.def, out var swearThought) && Rand.Chance(swearThought.chance))
				{
					if (pawn != null && pawn.Spawned)
					{
						var cell = pawn.Position;
						var map = pawn.Map;

						var motestToRemove = cell.GetThingList(map)
							.OfType<MoteBubble>()
							.Where(mote => mote.link1.Linked && mote.link1.Target.HasThing && mote.link1.Target == pawn)
							.Where(mote => mote.def != Defs.swearMote)
							.ToArray();
						for (var i = 0; i < motestToRemove.Length; i++)
							motestToRemove[i].Destroy(DestroyMode.Vanish);

						var bubble = (MoteBubble)ThingMaker.MakeThing(Defs.swearMote, null);
						bubble.SetupMoteBubble(swearThought.icon, null);
						bubble.Attach(pawn);
						_ = GenSpawn.Spawn(bubble, cell, map, WipeMode.Vanish);

						Defs.sighSound.PlaySound(cell, map);

						return bubble;
					}
				}

			return null;
		}

		static MoteBubble MakeMoodThoughtWithSwearingBubble(Pawn pawn, Thought_Memory thought)
		{
			var bubble = CreateSwearBubble(pawn, thought);
			if (bubble != null)
				return bubble;
			return MoteMaker.MakeMoodThoughtBubble(pawn, thought);
		}

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var from = SymbolExtensions.GetMethodInfo(() => MoteMaker.MakeMoodThoughtBubble(default, default));
			var to = SymbolExtensions.GetMethodInfo(() => MakeMoodThoughtWithSwearingBubble(default, default));
			return Transpilers.MethodReplacer(instructions, from, to);
		}

		static void Postfix(Thought_Memory newThought, Pawn ___pawn)
		{
			if (newThought.def.showBubble == false)
				_ = CreateSwearBubble(___pawn, newThought);

			if (newThought.def == ThoughtDefOf.GotSomeLovin)
			{
				Lovin.Done(___pawn);
				Throttled.ResetEvery(___pawn, ThrottleType.breakingBed);
			}
		}
	}

	[HarmonyPatch(typeof(Pawn_InteractionsTracker))]
	[HarmonyPatch(nameof(Pawn_InteractionsTracker.TryInteractWith))]
	static class MoteMaker_MakeInteractionBubble_Patch
	{
		static void Postfix(InteractionDef intDef, Pawn ___pawn, bool __result)
		{
			if (__result == false || ___pawn.IsColonist == false)
				return;

			if (RiceRiceBabyMain.Settings.swearing)
				if ("Insult,Slight,Breakup".Contains(intDef.defName))
					Defs.mehSound.PlaySound(___pawn.Position, ___pawn.Map);

			if (RiceRiceBabyMain.Settings.riceInsteadOfHeartWhileLovin)
				if ("RomanceAttempt".Contains(intDef.defName))
				{
					var soundDef = Rand.Chance(0.5f) ? Defs.mmmSound : Defs.achSound;
					if (___pawn.gender == Gender.Male) soundDef = Defs.mmmSound;
					if (___pawn.gender == Gender.Female) soundDef = Defs.achSound;
					soundDef.PlaySound(___pawn.Position, ___pawn.Map);
				}
		}
	}
}