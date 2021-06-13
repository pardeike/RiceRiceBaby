using RimWorld;
using System;
using Verse;

namespace RiceRiceBaby
{
	static class Tools
	{
		public static void ManipulateDefs()
		{
			static void MakeEasy(ThoughtDef def)
			{
				def.durationDays = 1;
				def.stackLimit = 8;
				def.stackLimitForSameOtherPawn = 4;
				def.stages[0].baseOpinionOffset = -1;
				def.stages[0].baseMoodEffect = -1;
			}

			MakeEasy(DefDatabase<ThoughtDef>.GetNamed("SoldMyLovedOne"));
			MakeEasy(DefDatabase<ThoughtDef>.GetNamed("RebuffedMyRomanceAttempt"));
			MakeEasy(DefDatabase<ThoughtDef>.GetNamed("RebuffedMyRomanceAttemptMood"));
			MakeEasy(DefDatabase<ThoughtDef>.GetNamed("FailedRomanceAttemptOnMe"));
			MakeEasy(DefDatabase<ThoughtDef>.GetNamed("FailedRomanceAttemptOnMeLowOpinionMood"));
			MakeEasy(DefDatabase<ThoughtDef>.GetNamed("BrokeUpWithMe"));
			MakeEasy(DefDatabase<ThoughtDef>.GetNamed("BrokeUpWithMeMood"));
			MakeEasy(DefDatabase<ThoughtDef>.GetNamed("CheatedOnMe"));
			MakeEasy(DefDatabase<ThoughtDef>.GetNamed("CheatedOnMeMood"));
			MakeEasy(DefDatabase<ThoughtDef>.GetNamed("DivorcedMe"));
			MakeEasy(DefDatabase<ThoughtDef>.GetNamed("DivorcedMeMood"));
			MakeEasy(DefDatabase<ThoughtDef>.GetNamed("RejectedMyProposal"));
			MakeEasy(DefDatabase<ThoughtDef>.GetNamed("RejectedMyProposalMood"));
			MakeEasy(DefDatabase<ThoughtDef>.GetNamed("IRejectedTheirProposal"));
			MakeEasy(DefDatabase<ThoughtDef>.GetNamed("KilledMyLover"));
			MakeEasy(DefDatabase<ThoughtDef>.GetNamed("KilledMyFiance"));
			MakeEasy(DefDatabase<ThoughtDef>.GetNamed("KilledMySpouse"));
		}

		static long freezeTicks = 0;
		static long subTicks = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
		static readonly long startTicks = DateTime.Now.Ticks;
		const float slowness = 2000f;

		public static float Ticker()
		{
			var nowTicks = (DateTime.Now.Ticks - startTicks) / TimeSpan.TicksPerMillisecond;
			var paused = Find.TickManager.Paused;

			if (freezeTicks == 0 && paused)
				freezeTicks = nowTicks;

			if (freezeTicks > 0 && paused == false)
			{
				subTicks = nowTicks - freezeTicks;
				freezeTicks = 0;
			}

			if (paused)
				return freezeTicks / slowness;
			return (nowTicks - subTicks) / slowness;
		}
	}
}
