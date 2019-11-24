using Harmony;
using System.Reflection;
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
}