using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

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
		static Dictionary<Pawn, DateTime> lastBreath = new Dictionary<Pawn, DateTime>();

		static bool Prefix(IntVec3 cell, Map map, ThingDef moteDef)
		{
			if (RiceRiceBabyMain.Settings.riceInsteadOfHeartWhileLovin && moteDef == ThingDefOf.Mote_Heart)
			{
				if (Rand.Chance(0.25f))
				{
					if (Rand.Chance(0.5f))
					{
						var def = Rand.Chance(0.5f) ? Defs.mmmSound : Defs.achSound;
						var pawn = map.thingGrid.ThingAt<Pawn>(cell);
						if (pawn != null && pawn.gender == Gender.Male) def = Defs.mmmSound;
						if (pawn != null && pawn.gender == Gender.Female) def = Defs.achSound;
						def.PlaySound(cell, map);
					}
					_ = MoteMaker.ThrowMetaIcon(cell, map, Defs.riceMote);
					Defs.riceSound.PlaySound(cell, map);
					return false;
				}
				else
					Defs.bedSound.PlaySound(cell, map);
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
					{
						var now = DateTime.Now;
						if (lastBreath.TryGetValue(pawn, out var date) == false || now > date.AddSeconds(4.3))
						{
							def = Defs.sleepingSound;
							lastBreath[pawn] = now;
						}
					}

					def?.PlaySound(cell, map);
				}
			}

			return true;
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
		}
	}
}