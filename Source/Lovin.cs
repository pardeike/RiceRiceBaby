using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RiceRiceBaby
{
	class Couple
	{
		readonly Pawn p1;
		readonly Pawn p2;

		public Couple(Pawn p1, Pawn p2)
		{
			this.p1 = p1;
			this.p2 = p2;
		}

		public bool Contains(Pawn p)
		{
			return p == p1 || p == p2;
		}

		public override bool Equals(object obj)
		{
			var other = obj as Couple;
			if (other == null)
				return false;
			return (p1 == other.p1 && p2 == other.p2) || (p2 == other.p1 && p1 == other.p2);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return 17 * 23 + (p1.GetHashCode() + p2.GetHashCode());
			}
		}
	}

	class Lovin
	{
		static readonly Dictionary<Couple, Lovin> state = new Dictionary<Couple, Lovin>();
		static readonly Rot4[] directions = new[] { Rot4.East, Rot4.South, Rot4.West };

		public readonly bool onTop;
		public readonly Rot4[] face = new[] { Rot4.South, Rot4.South };

		// transient
		public Vector3 longSide;
		public Vector3 shortSide;
		public float longHump;
		public float sway;

		public static Lovin LovinFor(Pawn p1, Pawn p2)
		{
			var couple = new Couple(p1, p2);
			if (state.TryGetValue(couple, out var result) == false)
			{
				result = new Lovin();
				state[couple] = result;
			}
			return result;
		}

		public static void Done(Pawn pawn)
		{
			_ = state.RemoveAll(pair => pair.Key.Contains(pawn));
		}

		public Lovin()
		{
			onTop = Rand.Bool;
			if (onTop)
			{
				face[0] = Rot4.North;
				face[1] = Rot4.Random;
			}
			else
			{
				while (true)
				{
					face[0] = directions[Rand.Range(0, 3)];
					face[1] = directions[Rand.Range(0, 3)];
					if (face[0] == Rot4.East || face[1] == Rot4.West) break;
				}
			}
		}
	}
}