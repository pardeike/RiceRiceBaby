using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace RiceRiceBaby
{
	[HarmonyPatch(typeof(MoteMaker))]
	[HarmonyPatch(nameof(MoteMaker.ThrowMetaIcon))]
	static class MoteMaker_ThrowMetaIcon_Patch1
	{
		static bool Lovin(Pawn pawn)
		{
			var cell = pawn.Position;
			var map = pawn.Map;

			var soundDef = Rand.Chance(0.5f) ? Defs.mmmSound : Defs.achSound;
			if (pawn.gender == Gender.Male) soundDef = Defs.mmmSound;
			if (pawn.gender == Gender.Female) soundDef = Defs.achSound;

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
							_ = bed.TakeDamage(new DamageInfo(DamageDefOf.Crush, Rand.Range(20f, 40f)));
						});
						playDefaultSound = false;
					}
				}
			}

			if (playDefaultSound)
				Defs.bedSound.PlaySound(cell, map);

			return true;
		}

		static bool Mating(Pawn pawn)
		{
			var cell = pawn.Position;
			var map = pawn.Map;

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

			return true;
		}

		static bool Prefix(IntVec3 cell, Map map, ThingDef moteDef)
		{
			if (RiceRiceBabyMain.Settings.lovin && moteDef == ThingDefOf.Mote_Heart)
			{
				var pawn = map.thingGrid.ThingAt<Pawn>(cell);
				if (pawn == null || pawn.IsColonist == false) return true;

				if (pawn.CurJobDef == JobDefOf.Lovin)
					if (Lovin(pawn) == false)
						return false;

				if (pawn.CurJobDef == JobDefOf.Mate)
					if (Mating(pawn) == false)
						return false;
			}

			return true;
		}
	}

	static class LoveAnimation
	{
		static readonly AccessTools.FieldRef<JobDriver_Lovin, int> ticksLeftRef = AccessTools.FieldRefAccess<JobDriver_Lovin, int>("ticksLeft");

		public static Lovin GetLovin(Pawn pawn)
		{
			if (RiceRiceBabyMain.Settings.lovin == false) return null;
			if (pawn.CurJobDef != JobDefOf.Lovin) return null;

			var bed = pawn.CurrentBed();
			if (bed == null) return null;
			var baseRotation = bed.Rotation;

			var idx = bed.GetCurOccupantSlotIndex(pawn);
			if (idx < 0 || idx > 1) return null;

			var partner = bed.GetCurOccupant(1 - idx);

			var driver1 = pawn.jobs.curDriver as JobDriver_Lovin;
			var driver2 = partner.jobs.curDriver as JobDriver_Lovin;
			if (driver1 == null || driver2 == null) return null;
			var ticksLeft = Math.Min(ticksLeftRef(driver1), ticksLeftRef(driver2));

			var lovin = Lovin.LovinFor(pawn, partner, baseRotation.AsInt < 2);

			lovin.longSide = baseRotation.FacingCell.ToVector3();
			var orthogonalRotation = baseRotation.Rotated(RotationDirection.Clockwise);
			lovin.shortSide = orthogonalRotation.FacingCell.ToVector3();

			var ticks = Tools.Ticker();
			var offset = pawn.GetHashCode() % 1000 - 500;

			const float maxTicksLeft = 2500f;
			var speed = GenMath.LerpDoubleClamped(0f, maxTicksLeft + 500, 3f, 0f, ticksLeft + offset);

			var hump = Mathf.Sin(ticks * speed);
			lovin.sway = (float)Math.Sin(ticks * (speed + offset / 10f)) / 50f;
			lovin.longHump = (pawn.gender == Gender.Male ? 1 / 25f : 1 / 45f) * hump;

			return lovin;
		}
	}

	[HarmonyPatch(typeof(PawnRenderer))]
	[HarmonyPatch("RenderPawnInternal")]
	[HarmonyPatch(new[] { typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool), typeof(bool) })]
	static class PawnRenderer_RenderPawnInternal_Patch
	{
		public static bool setRootLoc = true;

		public static void LovinFix(Pawn pawn, ref Vector3 rootLoc, ref float angle, ref Rot4 headFacing)
		{
			var lovin = LoveAnimation.GetLovin(pawn);
			if (lovin == null) return;

			var idx = pawn.CurrentBed().GetCurOccupantSlotIndex(pawn);
			var ticks = Tools.Ticker();

			headFacing = lovin.face[idx];
			if (lovin.onTop)
			{
				var shortHump = idx == 0 ? -0.45f : 0.45f;
				if (lovin.flipped) shortHump *= -1;
				if (setRootLoc)
				{
					rootLoc += lovin.longSide * lovin.longHump + lovin.shortSide * shortHump;
					if (idx == 0)
						rootLoc += new Vector3(0f, 0.01f, 0f);
				}
				angle += (float)Math.Sin(ticks * 10f) * 3f;
			}
			else
			{
				var shortHump = idx == 0 ? -0.2f - lovin.sway : 0.2f + lovin.sway;
				if (lovin.flipped) shortHump *= -1;
				if (setRootLoc)
					rootLoc += lovin.longSide * lovin.longHump + lovin.shortSide * shortHump;
				angle += (float)Math.Sin(ticks * 50f) * 2f;
			}
		}

		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
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

	[HarmonyPatch]
	static class FacialStuff_HarmonyPatch_PawnRenderer_Prefix_Patch
	{
		static bool Prepare()
		{
			return TargetMethod() != null;
		}

		static MethodBase TargetMethod()
		{
			return AccessTools.Method("FacialStuff.Harmony.HarmonyPatch_PawnRenderer:Prefix");
		}

		static void Prefix([HarmonyArgument(0)] PawnRenderer renderer, ref Rot4 headFacing)
		{
			var pawn = renderer.graphics.pawn;
			var lovin = LoveAnimation.GetLovin(pawn);
			if (lovin == null) return;
			var idx = pawn.CurrentBed().GetCurOccupantSlotIndex(pawn);
			headFacing = lovin.face[idx];
		}
	}

	[HarmonyPatch]
	static class FacialStuff_RecalcRootLocY_Patch
	{
		static bool Prepare()
		{
			return TargetMethod() != null;
		}

		static MethodBase TargetMethod()
		{
			var method = AccessTools.Method("FacialStuff.Harmony.HarmonyPatch_PawnRenderer:RecalcRootLocY");
			PawnRenderer_RenderPawnInternal_Patch.setRootLoc = method == null;
			return method;
		}

		static void Postfix(ref Vector3 rootLoc, Pawn pawn)
		{
			var lovin = LoveAnimation.GetLovin(pawn);
			if (lovin == null) return;

			var idx = pawn.CurrentBed().GetCurOccupantSlotIndex(pawn);

			if (lovin.onTop)
			{
				var shortHump = idx == 0 ? -0.45f : 0.45f;
				rootLoc += lovin.longSide * lovin.longHump + lovin.shortSide * shortHump;
				if (idx == 0)
					rootLoc += new Vector3(0f, 0.01f, 0f);
			}
			else
			{
				var shortHump = idx == 0 ? -0.2f - lovin.sway : 0.2f + lovin.sway;
				rootLoc += lovin.longSide * lovin.longHump + lovin.shortSide * shortHump;
			}
		}
	}

	[HarmonyPatch(typeof(MemoryThoughtHandler))]
	[HarmonyPatch(nameof(MemoryThoughtHandler.TryGainMemory))]
	[HarmonyPatch(new Type[] { typeof(Thought_Memory), typeof(Pawn) })]
	static class MemoryThoughtHandler_TryGainMemory_Patch2
	{
		static void Postfix(Thought_Memory newThought, Pawn ___pawn)
		{
			if (newThought.def == ThoughtDefOf.GotSomeLovin)
			{
				Lovin.Done(___pawn);
				Throttled.ResetEvery(___pawn, ThrottleType.breakingBed);
			}

			// https://github.com/fluffy-mods/BirdsAndBees/blob/master/Defs/ThoughtDefs/Thoughts_Memory_Social.xml
			if (newThought.def.defName == "LovinPerformance")
			{
				if (newThought.CurStageIndex == 0) // terribly lovin
				{
					Defs.mehSound.PlaySound(___pawn);
					___pawn.interactions.StartSocialFight(newThought.otherPawn);
				}

				if (newThought.CurStageIndex == 3) // amazing lovin
				{
					var bed = ___pawn.CurrentBed();
					if (bed != null && bed.GetCurOccupantSlotIndex(___pawn) == 0)
					{
						var mote = (Mote)ThingMaker.MakeThing(Defs.amazingMote, null);
						_ = GenSpawn.Spawn(mote, ___pawn.Position, ___pawn.Map, WipeMode.Vanish);
						mote.exactPosition = bed.TrueCenter();

						Defs.riceEchoSound.PlaySound(___pawn);
						Find.CameraDriver.shaker.DoShake(4f);
					}
				}
			}
		}
	}

	[HarmonyPatch(typeof(Pawn_InteractionsTracker))]
	[HarmonyPatch(nameof(Pawn_InteractionsTracker.TryInteractWith))]
	static class Pawn_InteractionsTracker_TryInteractWith_Patch
	{
		static void Postfix(InteractionDef intDef, Pawn ___pawn, bool __result)
		{
			if (__result == false || ___pawn.IsColonist == false)
				return;

			if (RiceRiceBabyMain.Settings.lovin)
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