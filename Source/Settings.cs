using UnityEngine;
using Verse;

namespace RiceRiceBaby
{
	public class RiceRiceBabySettings : ModSettings
	{
		public bool whipAchtungForce = true;
		public bool lovin = true;
		public bool profanity = true;
		public bool swearing = true;
		public bool death = true;
		public bool romancing = true;
		public float romanceLevel = 0.4f;
		public float cheatingLevel = 0.2f;
		public float riceLevel = 0.4f;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref whipAchtungForce, "whipAchtungForce", true);
			Scribe_Values.Look(ref lovin, "lovin", true);
			Scribe_Values.Look(ref profanity, "profanity", true);
			Scribe_Values.Look(ref swearing, "swearing", true);
			Scribe_Values.Look(ref death, "death", true);
			Scribe_Values.Look(ref romancing, "romancing", true);
			Scribe_Values.Look(ref romanceLevel, "romanceLevel", 0.4f);
			Scribe_Values.Look(ref cheatingLevel, "cheatingLevel", 0.2f);
			Scribe_Values.Look(ref riceLevel, "riceLevel", 0.4f);

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
			list.CheckboxLabeled("Lovin'", ref lovin);
			list.CheckboxLabeled("Profanity", ref profanity);
			list.CheckboxLabeled("Swearing", ref swearing);
			list.CheckboxLabeled("Death", ref death);

			var str = romancing ? (int)(romanceLevel * 100) + "%" : "-";
			list.CheckboxLabeled("Romance: " + str, ref romancing);
			if (romancing)
			{
				romanceLevel = list.Slider(romanceLevel, 0f, 1f);

				list.Label("Cheating: " + (int)(cheatingLevel * 100) + "%");
				cheatingLevel = list.Slider(cheatingLevel, 0f, 1f);

				list.Label("Rice: " + (int)(riceLevel * 100) + "%");
				riceLevel = list.Slider(riceLevel, 0f, 1f);
			}

			list.End();
		}
	}
}