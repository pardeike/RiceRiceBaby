using HarmonyLib;
using System.Reflection;
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
				// TODO: need to use VideoPlayer in Unity 2019
				//
				/*var w = 320f;
				var h = 234f;
				var dx = (Screen.width - w) / 2f;
				var dy = (Screen.height - h) / 2f;
				var rect = new Rect(dx, dy, w, h);
				_ = pawn.Map.GetComponent<MoviePlayer>().Play("Whip", rect, false);*/

				Defs.whipSound.PlaySound(pawn);
			}
		}
	}
}