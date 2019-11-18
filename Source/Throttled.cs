using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RiceRiceBaby
{
	public enum ThrottleType
	{
		lastBreath,
		whisle
	}

	public static class Throttled
	{
		static readonly Dictionary<Pawn, Dictionary<ThrottleType, DateTime>> state = new Dictionary<Pawn, Dictionary<ThrottleType, DateTime>>();

		private static Dictionary<ThrottleType, DateTime> GetKeys(Pawn pawn, ThrottleType type)
		{
			if (state.TryGetValue(pawn, out var keys) == false)
			{
				keys = new Dictionary<ThrottleType, DateTime>();
				state[pawn] = keys;
			}
			if (keys.ContainsKey(type) == false)
				keys[type] = DateTime.MinValue;
			return keys;
		}

		public static void Every(double seconds, Pawn pawn, ThrottleType type, Action action)
		{
			var keys = GetKeys(pawn, type);
			var now = DateTime.Now;
			if (keys[type].AddSeconds(seconds) < now)
			{
				action();
				keys[type] = now;
			}
		}

		public static void AfterIdle(double seconds, Pawn pawn, ThrottleType type, Action action)
		{
			var keys = GetKeys(pawn, type);
			var now = DateTime.Now;
			if (keys[type].AddSeconds(seconds) < now)
				action();
			keys[type] = now;
		}
	}
}
