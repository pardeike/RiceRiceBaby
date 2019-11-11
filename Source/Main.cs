using Harmony;
using RimWorld;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.Sound;

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
			{
				var info = SoundInfo.InMap(new TargetInfo(pawn.Position, pawn.Map));
				SoundDef.Named("WhipCrack").PlayOneShot(info);
			}
		}
	}

	[HarmonyPatch(typeof(MoteMaker))]
	[HarmonyPatch(nameof(MoteMaker.ThrowMetaIcon))]
	static class MoteMaker_ThrowMetaIcon_Patch
	{
		static bool Prefix(IntVec3 cell, Map map, ThingDef moteDef)
		{
			if (RiceRiceBabyMain.Settings.riceInsteadOfHeartWhileLovin && moteDef == ThingDefOf.Mote_Heart)
			{
				var riceDef = ThingDef.Named("Mote_Rice");
				_ = MoteMaker.ThrowMetaIcon(cell, map, riceDef);
				return false;
			}
			return true;
		}
	}
}