using UnityEngine;
using Verse;

namespace RiceRiceBaby
{
	public class RiceRiceBabySettings : ModSettings
	{
		public bool whipAchtungForce = true;
		public bool riceInsteadOfHeartWhileLovin = true;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref whipAchtungForce, "whipAchtungForce", true);
			Scribe_Values.Look(ref riceInsteadOfHeartWhileLovin, "riceInsteadOfHeartWhileLovin", true);

			if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
			{
			}
		}

		public void DoWindowContents(Rect inRect)
		{
			var list = new Listing_Standard { ColumnWidth = (inRect.width - 34f) / 2f };
			list.Begin(inRect);

			list.Gap(16f);

			list.CheckboxLabeled("Whip Force", ref whipAchtungForce);
			list.CheckboxLabeled("Rice instead of hearts", ref riceInsteadOfHeartWhileLovin);

			list.End();
		}
	}
}