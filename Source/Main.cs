using Brrainz;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace RiceRiceBaby
{
	class RiceRiceBabyMain : Mod
	{
		public static RiceRiceBabySettings Settings;
		public static string rootDir;

		public RiceRiceBabyMain(ModContentPack content) : base(content)
		{
			rootDir = content.RootDir.Replace("\\", "/");
			Settings = GetSettings<RiceRiceBabySettings>();

			var harmony = new Harmony("net.pardeike.rimworld.mod.ricericebaby");
			harmony.PatchAll();

			CrossPromotion.Install(76561197973010050);
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
}