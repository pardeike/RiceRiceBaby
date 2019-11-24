using UnityEngine;
using Verse;

namespace RiceRiceBaby
{
	public class RiceRiceBabySettings : ModSettings
	{
		public bool whipAchtungForce = true;
		public bool riceInsteadOfHeartWhileLovin = true;
		public bool snoring = true;
		public bool swearing = true;
		public bool profanity = true;
		public bool romancing = true;
		public float romanceLevel = 0.4f;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref whipAchtungForce, "whipAchtungForce", true);
			Scribe_Values.Look(ref riceInsteadOfHeartWhileLovin, "riceInsteadOfHeartWhileLovin", true);
			Scribe_Values.Look(ref snoring, "snoring", true);
			Scribe_Values.Look(ref swearing, "swearing", true);
			Scribe_Values.Look(ref profanity, "profanity", true);
			Scribe_Values.Look(ref romancing, "romancing", true);
			Scribe_Values.Look(ref romanceLevel, "romanceLevel", 0.4f);

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
			list.CheckboxLabeled("Snoring", ref snoring);
			list.CheckboxLabeled("Swearing", ref swearing);
			list.CheckboxLabeled("Profanity", ref profanity);

			var str = romancing ? (int)(romanceLevel * 100) + "%" : "-";
			list.CheckboxLabeled("Romance level: " + str, ref romancing);
			romanceLevel = list.Slider(romanceLevel, 0f, 1f);

			list.End();
		}
	}
}