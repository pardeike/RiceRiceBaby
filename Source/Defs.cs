using Harmony;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RiceRiceBaby
{
	class DefTools
	{
		public static void ManipulateDefs()
		{
			void MakeEasy(ThoughtDef def)
			{
				def.durationDays = 1;
				def.stackLimit = 2;
				def.stackLimitForSameOtherPawn = 2;
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
	}

	[StaticConstructorOnStartup]
	class Defs
	{
		public static ThingDef riceMote = ThingDef.Named("Mote_Rice");
		public static ThingDef wantRiceMote = ThingDef.Named("Mote_WantRice");
		public static ThingDef swearMote = ThingDef.Named("Mote_Swear");

		public static Texture2D fuMote = ContentFinder<Texture2D>.Get("Mote-FU", true);
		public static Texture2D fthisMote = ContentFinder<Texture2D>.Get("Mote-F-this", true);
		public static Texture2D wtfMote = ContentFinder<Texture2D>.Get("Mote-WTF", true);

		public static SoundDef whipSound = SoundDef.Named("WhipCrack");
		public static SoundDef whisleSound = SoundDef.Named("Whisle");
		public static SoundDef riceSound = SoundDef.Named("Rice");
		public static SoundDef sleepingSound = SoundDef.Named("Sleeping");
		public static SoundDef mmmSound = SoundDef.Named("Mmm");
		public static SoundDef achSound = SoundDef.Named("Ach");
		public static SoundDef snoreMaleSound = SoundDef.Named("SnoreMale");
		public static SoundDef snoreFemaleSound = SoundDef.Named("SnoreFemale");
		public static SoundDef bedSound = SoundDef.Named("Bed");
		public static SoundDef damageSound = SoundDef.Named("Damage");
		public static SoundDef breakSound = SoundDef.Named("Break");
		public static SoundDef dyingSound = SoundDef.Named("Dying");
		public static SoundDef belchSound = SoundDef.Named("Belch");
		public static SoundDef fartSound = SoundDef.Named("Fart");
		public static SoundDef sighSound = SoundDef.Named("Sigh");
		public static SoundDef mehSound = SoundDef.Named("Meh");

		public static readonly TraitDef[] snoringTraits = new TraitDef[]
		{
			TraitDefOf.AnnoyingVoice,
			TraitDefOf.CreepyBreathing,
			TraitDefOf.Psychopath,
		};

		public static readonly Dictionary<ThoughtDef, SwearThought> triggeringDefs = new Dictionary<ThoughtDef, SwearThought>()
		{
			{ ThoughtDefOf.Insulted, new SwearThought(fuMote, 1.0f) },
			{ ThoughtDefOf.SleepDisturbed, new SwearThought(fuMote, 1.0f) },
			{ ThoughtDefOf.KnowColonistExecuted, new SwearThought(fuMote, 1.0f) },
			{ ThoughtDefOf.CheatedOnMe, new SwearThought(fuMote, 0.5f) },
			{ ThoughtDefOf.RebuffedMyRomanceAttempt, new SwearThought(fuMote, 0.5f) },
			{ ThoughtDefOf.FailedRomanceAttemptOnMe, new SwearThought(fuMote, 0.25f) },
			{ ThoughtDefOf.DivorcedMe, new SwearThought(fuMote, 1.0f) },
			{ ThoughtDefOf.ForcedMeToTakeLuciferium, new SwearThought(fuMote, 1.0f) },

			{ ThoughtDefOf.SleptOutside, new SwearThought(fthisMote, 0.5f) },
			{ ThoughtDefOf.SleptOnGround, new SwearThought(fthisMote, 0.25f) },
			{ ThoughtDefOf.AteWithoutTable, new SwearThought(fthisMote, 0.25f) },
			{ ThoughtDefOf.ObservedLayingCorpse, new SwearThought(fthisMote, 0.25f) },
			{ ThoughtDefOf.HadAngeringFight, new SwearThought(fthisMote, 0.5f) },
			{ ThoughtDefOf.BrokeUpWithMe, new SwearThought(fthisMote, 0.75f) },
			{ ThoughtDefOf.RejectedMyProposal, new SwearThought(fthisMote, 0.75f) },

			{ ThoughtDefOf.AteAwfulMeal, new SwearThought(wtfMote, 0.5f) },
			{ ThoughtDefOf.AteHumanlikeMeatDirect, new SwearThought(wtfMote, 1.0f) },
			{ ThoughtDefOf.AteHumanlikeMeatAsIngredient, new SwearThought(wtfMote, 0.5f) },
			{ ThoughtDefOf.MyOrganHarvested, new SwearThought(wtfMote, 1.0f) },
			{ ThoughtDefOf.KnowGuestOrganHarvested, new SwearThought(wtfMote, 1.0f) },
			{ ThoughtDefOf.KnowColonistOrganHarvested, new SwearThought(wtfMote, 1.0f) },
			{ ThoughtDefOf.KnowGuestExecuted, new SwearThought(wtfMote, 1.0f) },
			{ ThoughtDefOf.BotchedMySurgery, new SwearThought(wtfMote, 0.25f) },
			{ ThoughtDefOf.IRejectedTheirProposal, new SwearThought(wtfMote, 0.25f) },
			{ ThoughtDefOf.AttendedWedding, new SwearThought(wtfMote, 0.05f) },
			{ ThoughtDefOf.KilledMyFriend, new SwearThought(wtfMote, 0.9f) },
			{ ThoughtDefOf.ForcedMeToTakeDrugs, new SwearThought(wtfMote, 0.75f) },
		};
	}

	class SwearThought
	{
		public Texture2D icon;
		public float chance;

		public SwearThought(Texture2D icon, float chance)
		{
			this.icon = icon;
			this.chance = chance;
		}
	}

	static class Extensions
	{
		public static void PlaySound(this SoundDef def, Pawn pawn)
		{
			var info = SoundInfo.InMap(new TargetInfo(pawn.Position, pawn.Map));
			def.PlayOneShot(info);
		}

		public static void PlaySound(this SoundDef def, IntVec3 cell, Map map)
		{
			var info = SoundInfo.InMap(new TargetInfo(cell, map));
			def.PlayOneShot(info);
		}

		public static bool CanSnore(this Pawn pawn)
		{
			var traits = pawn.story.traits;
			foreach (var traitDef in Defs.snoringTraits)
				if (traits.HasTrait(traitDef))
					return true;
			return false;
		}
	}
}