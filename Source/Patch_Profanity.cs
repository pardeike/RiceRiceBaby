using HarmonyLib;
using RimWorld;
using System;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RiceRiceBaby
{
	[HarmonyPatch(typeof(FleckMaker), nameof(FleckMaker.ThrowMetaIcon))]
	static class MoteMaker_ThrowMetaIcon_Patch2
	{
		static void Postfix(IntVec3 cell, Map map, FleckDef fleckDef)
		{
			if (RiceRiceBabyMain.Settings.profanity == false || fleckDef != FleckDefOf.SleepZ) return;

			if (map?.thingGrid == null) return;
			var pawn = map.thingGrid.ThingAt<Pawn>(cell);
			if (pawn == null) return;
			if (pawn.IsColonist == false) return;

			if (Rand.Chance(0.4f))
			{
				SoundDef def = null;
				if (Rand.Chance(0.5f) && pawn.CanSnore())
				{
					def = Rand.Chance(0.5f) ? Defs.snoreMaleSound : Defs.snoreFemaleSound;
					if (pawn.gender == Gender.Male) def = Defs.snoreMaleSound;
					if (pawn.gender == Gender.Female) def = Defs.snoreFemaleSound;
				}
				else
					Throttled.Every(4.3, pawn, ThrottleType.lastBreath, () => def = Defs.sleepingSound);

				def?.PlaySound(cell, map);
			}
		}
	}

	[HarmonyPatch(typeof(Toils_Ingest), nameof(Toils_Ingest.ChewIngestible))]
	static class Toils_Ingest_ChewIngestible_Patch
	{
		static void ThrowMote(Vector3 loc, Map map, float size, float angle)
		{
			var mote = (MoteThrown)ThingMaker.MakeThing(Defs.gasMote);
			mote.Scale = size;
			mote.rotationRate = Rand.Range(-5f, 5f);
			mote.exactPosition = loc;
			mote.SetVelocity(angle + Rand.Range(-15, 15), Rand.Range(0.5f, 0.7f));
			_ = GenSpawn.Spawn(mote, loc.ToIntVec3(), map);
		}

		static void Postfix(Pawn chewer, float durationMultiplier, TargetIndex ingestibleInd, Toil __result)
		{
			if (RiceRiceBabyMain.Settings.profanity == false) return;

			var toil = __result;
			if (toil == null || chewer == null)
				return;

			SoundDef noiseDef = null;
			Action effect = () => { };
			var time = -1f;
			if (Rand.Chance(0.25f))
			{
				if (Rand.Bool)
				{
					noiseDef = Defs.fartSound;
					effect = () =>
					{
						var rotation = chewer.Rotation.Opposite;
						ThrowMote(chewer.TrueCenter() + new Vector3(0f, 0f, -0.25f), chewer.Map, 1.5f, rotation.AsAngle);
					};
				}
				else
				{
					noiseDef = Defs.belchSound;
					effect = () =>
					{
						var rotation = chewer.Rotation;
						ThrowMote(chewer.TrueCenter() + new Vector3(0f, 0f, 0.25f), chewer.Map, 0.75f, rotation.AsAngle);
					};
				}
				time = Rand.Range(0.15f, 0.85f);
			}

			var originalTickAction = toil.tickAction;
			toil.tickAction = delegate ()
			{
				originalTickAction();

				if (chewer == toil.actor && time >= 0f)
				{
					var thing = chewer.CurJob.GetTarget(ingestibleInd).Thing;
					if (thing != null)
					{
						var progress = 1f - toil.actor.jobs.curDriver.ticksLeftThisToil / Mathf.Round(thing.def.ingestible.baseIngestTicks * durationMultiplier);
						if (progress >= time)
						{
							effect();
							noiseDef?.PlaySound(toil.actor);
							time = -1f;
						}
					}
				}
			};
		}
	}
}
