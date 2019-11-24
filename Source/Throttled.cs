using System;
using System.Collections.Generic;
using Verse;

namespace RiceRiceBaby
{
	public enum ThrottleType
	{
		lastBreath,
		whisle,
		breakingBed,
		wince
	}

	public static class Throttled
	{
		static readonly Dictionary<Pawn, Dictionary<ThrottleType, DateTime>> timeState = new Dictionary<Pawn, Dictionary<ThrottleType, DateTime>>();
		static readonly Dictionary<Pawn, Dictionary<ThrottleType, int>> countState = new Dictionary<Pawn, Dictionary<ThrottleType, int>>();

		static Dictionary<ThrottleType, DateTime> GetDateKeys(Pawn pawn, ThrottleType type)
		{
			if (timeState.TryGetValue(pawn, out var keys) == false)
			{
				keys = new Dictionary<ThrottleType, DateTime>();
				timeState[pawn] = keys;
			}
			if (keys.ContainsKey(type) == false)
				keys[type] = DateTime.MinValue;
			return keys;
		}

		static Dictionary<ThrottleType, int> GetCountKeys(Pawn pawn, ThrottleType type)
		{
			if (countState.TryGetValue(pawn, out var keys) == false)
			{
				keys = new Dictionary<ThrottleType, int>();
				countState[pawn] = keys;
			}
			if (keys.ContainsKey(type) == false)
				keys[type] = 0;
			return keys;
		}

		public static void Every(double seconds, Pawn pawn, ThrottleType type, Action action)
		{
			var keys = GetDateKeys(pawn, type);
			var now = DateTime.Now;
			if (keys[type].AddSeconds(seconds) < now)
			{
				action();
				keys[type] = now;
			}
		}

		public static void AfterIdle(double seconds, Pawn pawn, ThrottleType type, Action action)
		{
			var keys = GetDateKeys(pawn, type);
			var now = DateTime.Now;
			if (keys[type].AddSeconds(seconds) < now)
				action();
			keys[type] = now;
		}

		public static void Every(int count, Pawn pawn, ThrottleType type, Action action)
		{
			var keys = GetCountKeys(pawn, type);
			keys[type] += 1;
			if (keys[type] >= count)
			{
				action();
				keys[type] = 0;
			}
		}

		public static int CurrentEvery(Pawn pawn, ThrottleType type)
		{
			var keys = GetCountKeys(pawn, type);
			return keys[type];
		}

		public static void ResetEvery(Pawn pawn, ThrottleType type)
		{
			var keys = GetCountKeys(pawn, type);
			keys[type] = 0;
		}
	}
}