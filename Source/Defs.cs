﻿using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RiceRiceBaby
{
	[StaticConstructorOnStartup]
	class Defs
	{
		public static FleckDef riceMote = DefDatabase<FleckDef>.GetNamed("Fleck_Rice");
		public static FleckDef wantRiceMote = DefDatabase<FleckDef>.GetNamed("Fleck_WantRice");

		public static ThingDef swearMote = ThingDef.Named("Mote_Swear");
		public static ThingDef amazingMote = ThingDef.Named("Mote_Amazing");
		public static ThingDef gasMote = ThingDef.Named("Mote_Gas");

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
		public static SoundDef riceEchoSound = SoundDef.Named("RiceEcho");

		public static readonly TraitDef[] snoringTraits = new TraitDef[]
		{
			TraitDefOf.AnnoyingVoice,
			TraitDefOf.CreepyBreathing,
			TraitDefOf.Psychopath,
		};

		public static readonly Dictionary<ThoughtDef, SwearThought> triggeringDefs;

		static Defs()
		{
			triggeringDefs = new Dictionary<ThoughtDef, SwearThought>();
			static void saveAdd(ThoughtDef def, SwearThought thought) { if (def != null && thought.icon != null) triggeringDefs[def] = thought; }

			saveAdd(ThoughtDefOf.Insulted, new SwearThought(fuMote, 1.0f));

			saveAdd(ThoughtDefOf.Insulted, new SwearThought(fuMote, 1.0f));
			saveAdd(ThoughtDefOf.SleepDisturbed, new SwearThought(fuMote, 1.0f));
			saveAdd(ThoughtDefOf.KnowColonistExecuted, new SwearThought(fuMote, 1.0f));
			saveAdd(ThoughtDefOf.CheatedOnMe, new SwearThought(fuMote, 0.5f));
			saveAdd(ThoughtDefOf.RebuffedMyRomanceAttempt, new SwearThought(fuMote, 0.5f));
			saveAdd(ThoughtDefOf.FailedRomanceAttemptOnMe, new SwearThought(fuMote, 0.25f));
			saveAdd(ThoughtDefOf.DivorcedMe, new SwearThought(fuMote, 1.0f));
			saveAdd(ThoughtDefOf.ForcedMeToTakeLuciferium, new SwearThought(fuMote, 1.0f));

			saveAdd(ThoughtDefOf.SleptOutside, new SwearThought(fthisMote, 0.5f));
			saveAdd(ThoughtDefOf.SleptOnGround, new SwearThought(fthisMote, 0.25f));
			saveAdd(ThoughtDefOf.SleptInRoomWithSlave, new SwearThought(fthisMote, 0.25f));
			saveAdd(ThoughtDefOf.AteWithoutTable, new SwearThought(fthisMote, 0.25f));
			saveAdd(ThoughtDefOf.ObservedTerror, new SwearThought(fthisMote, 1.0f));
			saveAdd(ThoughtDefOf.HadAngeringFight, new SwearThought(fthisMote, 0.5f));
			saveAdd(ThoughtDefOf.BrokeUpWithMe, new SwearThought(fthisMote, 0.75f));
			saveAdd(ThoughtDefOf.RejectedMyProposal, new SwearThought(fthisMote, 0.75f));
			saveAdd(ThoughtDefOf.SoldMyBondedAnimalMood, new SwearThought(fthisMote, 0.75f));
			saveAdd(ThoughtDefOf.WasEnslaved, new SwearThought(fthisMote, 0.75f));
			saveAdd(ThoughtDefOf.TerribleSpeech, new SwearThought(fthisMote, 0.25f));

			saveAdd(ThoughtDefOf.AteRawFood, new SwearThought(wtfMote, 0.5f));
			saveAdd(ThoughtDefOf.AteRottenFood, new SwearThought(wtfMote, 0.5f));
			saveAdd(ThoughtDefOf.AteFoodInappropriateForTitle, new SwearThought(wtfMote, 1.0f));
			saveAdd(ThoughtDefOf.AteHumanlikeMeatDirect, new SwearThought(wtfMote, 1.0f));
			saveAdd(ThoughtDefOf.AteHumanlikeMeatAsIngredient, new SwearThought(wtfMote, 0.5f));
			saveAdd(ThoughtDefOf.MyOrganHarvested, new SwearThought(wtfMote, 1.0f));
			saveAdd(ThoughtDefOf.KnowColonistDied, new SwearThought(wtfMote, 1.0f));
			saveAdd(ThoughtDefOf.KnowGuestOrganHarvested, new SwearThought(wtfMote, 1.0f));
			saveAdd(ThoughtDefOf.KnowColonistOrganHarvested, new SwearThought(wtfMote, 1.0f));
			saveAdd(ThoughtDefOf.KnowGuestExecuted, new SwearThought(wtfMote, 1.0f));
			saveAdd(ThoughtDefOf.BotchedMySurgery, new SwearThought(wtfMote, 0.25f));
			saveAdd(ThoughtDefOf.IRejectedTheirProposal, new SwearThought(wtfMote, 0.25f));
			saveAdd(ThoughtDefOf.AttendedWedding, new SwearThought(wtfMote, 0.05f));
			saveAdd(ThoughtDefOf.KilledMyFriend, new SwearThought(wtfMote, 0.9f));
			saveAdd(ThoughtDefOf.ObservedGibbetCage, new SwearThought(wtfMote, 0.5f));
			saveAdd(ThoughtDefOf.ConnectedTreeDied, new SwearThought(wtfMote, 1.0f));

			saveAdd(ThoughtDefOf.ForcedMeToTakeDrugs, new SwearThought(wtfMote, 0.75f));
		}
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
			if (traits == null) return false;
			return Defs.snoringTraits.Any(t => traits.HasTrait(t));
		}
	}
}
