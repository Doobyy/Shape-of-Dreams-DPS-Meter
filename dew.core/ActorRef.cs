using System;
using System.Runtime.CompilerServices;

public struct ActorRef<T>(T actor) : IEquatable<ActorRef<T>> where T : Actor
{
	private T _actor = actor;

	private uint _spawnNetId = actor?.persistentNetId ?? 0;

	public T Get()
	{
		if (_actor == null || _actor.persistentNetId != _spawnNetId)
		{
			return null;
		}
		return _actor;
	}

	public static implicit operator ActorRef<T>(T actor)
	{
		return new ActorRef<T>(actor);
	}

	public static implicit operator T(ActorRef<T> r)
	{
		return r.Get();
	}

	public bool Equals(ActorRef<T> other)
	{
		if (_actor == other._actor)
		{
			return _spawnNetId == other._spawnNetId;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is ActorRef<T> other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((_actor != null) ? RuntimeHelpers.GetHashCode(_actor) : 0) ^ (int)_spawnNetId;
	}
}
