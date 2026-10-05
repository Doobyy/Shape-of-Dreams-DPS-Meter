using System;
using System.Collections.Generic;
using UnityEngine;

public struct ReactionChain : IEquatable<ReactionChain>
{
	internal List<Actor> _actors;

	private ReactionChain(ReactionChain original)
	{
		if (original._actors == null)
		{
			_actors = null;
			return;
		}
		_actors = new List<Actor>(original._actors.Count + 1);
		_actors.AddRange(original._actors);
	}

	private void Add(Actor actor)
	{
		if (_actors == null)
		{
			_actors = new List<Actor>(4);
		}
		_actors.Add(actor);
	}

	public ReactionChain New(Actor actor)
	{
		ReactionChain result = new ReactionChain(this);
		result.Add(actor);
		return result;
	}

	public bool DidReact(Actor actor, bool checkOnlyType = false)
	{
		if (_actors == null)
		{
			return false;
		}
		if (checkOnlyType)
		{
			foreach (Actor actor2 in _actors)
			{
				if (((object)actor2).GetType() == ((object)actor).GetType())
				{
					return true;
				}
			}
			return false;
		}
		return _actors.Contains(actor);
	}

	public bool Equals(ReactionChain other)
	{
		if (_actors != null && other._actors != null)
		{
			if (_actors.Count != other._actors.Count)
			{
				return false;
			}
			for (int i = 0; i < _actors.Count; i++)
			{
				if ((UnityEngine.Object)(object)_actors[i] != (UnityEngine.Object)(object)other._actors[i])
				{
					return false;
				}
			}
			return true;
		}
		return _actors == null == (other._actors == null);
	}

	public override bool Equals(object obj)
	{
		if (obj is ReactionChain other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (_actors == null || _actors.Count == 0)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < _actors.Count; i++)
		{
			num = HashCode.Combine<int, Actor>(num, _actors[i]);
		}
		return num;
	}
}
