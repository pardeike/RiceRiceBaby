using HarmonyLib;
using RimWorld;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace RiceRiceBaby
{
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
				VideoPlayer_OnGUI.started = Time.realtimeSinceStartup;
				Defs.whipSound.PlaySound(pawn);
			}
		}
	}

	[StaticConstructorOnStartup]
	[HarmonyPatch(typeof(UIRoot_Play), nameof(UIRoot_Play.UIRootOnGUI))]
	static class VideoPlayer_OnGUI
	{
		public static Texture2D[] frames = Enumerable.Range(1, 25).Select(i =>
		{
			var texture = new Texture2D(320, 240, TextureFormat.ARGB32, false);
			var path = Path.Combine(RiceRiceBabyMain.rootDir, "Textures", "Whip", $"{i:D2}.png");
			_ = texture.LoadImage(File.ReadAllBytes(path));
			return texture;
		}).ToArray();

		const float movieTime = 1.3f;
		const float width = 640;
		const float height = 480;
		public static float started = 0;

		static void Postfix()
		{
			if (started == 0) return;

			var delta = Time.realtimeSinceStartup - started;
			var i = (int)(Mathf.Min(1, delta / movieTime) * frames.Length);
			if (i == frames.Length)
			{
				started = 0;
				return;
			}

			Color backgroundColor = GUI.backgroundColor;
			GUI.backgroundColor = Color.white;
			var x = (UI.screenWidth - width) / 2;
			var y = (UI.screenHeight - height) / 2;
			var rect = new Rect(new Vector2(x, y), new Vector2(width, height));
			GUI.DrawTexture(rect, frames[i], ScaleMode.ScaleToFit);
			GUI.backgroundColor = backgroundColor;
		}
	}
}
