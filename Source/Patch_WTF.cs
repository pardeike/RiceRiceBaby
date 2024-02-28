using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RiceRiceBaby
{
	[HarmonyPatch(typeof(MemoryThoughtHandler), nameof(MemoryThoughtHandler.TryGainMemory))]
	[HarmonyPatch(new Type[] { typeof(Thought_Memory), typeof(Pawn) })]
	static class MemoryThoughtHandler_TryGainMemory_Patch1
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

						var motesToRemove = cell.GetThingList(map)
							.OfType<MoteBubble>()
							.Where(mote => mote.link1.Linked && mote.link1.Target.HasThing && mote.link1.Target == pawn)
							.Where(mote => mote.def != Defs.swearMote)
							.ToArray();
						for (var i = 0; i < motesToRemove.Length; i++)
							motesToRemove[i].Destroy(DestroyMode.Vanish);

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

	[HarmonyPatch(typeof(Pawn_InteractionsTracker))]
	[HarmonyPatch(nameof(Pawn_InteractionsTracker.TryInteractWith))]
	static class Pawn_InteractionsTracker_TryInteractWith_Patch1
	{
		static void Postfix(InteractionDef intDef, Pawn ___pawn, bool __result)
		{
			if (__result == false || ___pawn.IsColonist == false)
				return;

			if (RiceRiceBabyMain.Settings.swearing)
				if ("Insult,Slight,Breakup".Contains(intDef.defName))
					Defs.mehSound.PlaySound(___pawn.Position, ___pawn.Map);
		}
	}
}