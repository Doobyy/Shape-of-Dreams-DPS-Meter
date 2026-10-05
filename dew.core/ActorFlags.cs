using System;
using System.Collections.Generic;

internal struct ActorFlags
{
	private List<Type> _flags;

	public ActorFlags ShallowCopy()
	{
		return new ActorFlags
		{
			_flags = _flags
		};
	}

	public ActorFlags DeepCopy()
	{
		if (_flags == null)
		{
			return default;
		}
		return new ActorFlags
		{
			_flags = new List<Type>(_flags)
		};
	}

	public void Add(Actor actor)
	{
		Add(((object)actor).GetType());
	}

	public void Add(Type actorType)
	{
		if (_flags == null)
		{
			_flags = new List<Type>(4);
		}
		_flags.Add(actorType);
	}

	public bool Contains(Actor actor)
	{
		return Contains(((object)actor).GetType());
	}

	public bool Contains(Type actorType)
	{
		if (_flags == null)
		{
			return false;
		}
		return _flags.Contains(actorType);
	}
}
