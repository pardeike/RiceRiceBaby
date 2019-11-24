using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RiceRiceBaby
{
	public class Movie
	{
		public readonly MovieTexture texture;
		public Rect position = Rect.zero;
		public Action doneCallback;

		private bool started = false;

		public Movie(string name, Rect position, bool loop)
		{
			texture = MoviePlayer.GetTexture(name);
			texture.loop = loop;
			this.position = position;
		}

		~Movie()
		{
			texture.Stop();
		}

		public bool IsLooping => texture.loop;
		public bool IsPlaying => texture.isPlaying;
		public bool IsReady => texture.isReadyToPlay;

		public void Update()
		{
			if (IsPlaying == false)
			{
				if (started)
					doneCallback();
				else if (IsReady)
				{
					texture.Play();
					started = true;
				}
			}

			if (IsReady)
				GUI.DrawTexture(position, texture);
		}
	}

	public class MoviePlayer : MapComponent
	{
		readonly List<Movie> movies = new List<Movie>();
		static readonly Dictionary<string, MovieTexture> cachedTextures = new Dictionary<string, MovieTexture>();

		public MoviePlayer(Map map) : base(map)
		{
			// precaching
			_ = GetTexture("Whip");
		}

		public static MovieTexture GetTexture(string name)
		{
			if (cachedTextures.TryGetValue(name, out var texture) == false)
			{
				var loader = new WWW("file:" + "///" + RiceRiceBabyMain.rootDir + "/Movies/" + name + ".ogv");
				texture = WWWAudioExtensions.GetMovieTexture(loader);
			}
			return texture;
		}

		public Movie Play(string name, Rect initialRect, bool loop)
		{
			var movie = new Movie(name, initialRect, loop);
			if (movie.IsLooping == false)
				movie.doneCallback = () => { _ = movies.Remove(movie); };
			movies.Add(movie);
			return movie;
		}

		public void Remove(Movie movie)
		{
			_ = movies.Remove(movie);
		}

		public override void MapComponentOnGUI()
		{
			base.MapComponentOnGUI();
			foreach (var movie in movies.ToArray())
				movie.Update();
		}
	}
}