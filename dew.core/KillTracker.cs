using System;
using System.Collections.Generic;
using UnityEngine;

public class KillTracker
{
	private static List<Entity> _expired = new List<Entity>(16);

	private float _gracePeriod;

	private Dictionary<Entity, float> _trackingDict = new Dictionary<Entity, float>();

	private Action<EventInfoKill> _callback;

	private Action<EventInfoKill> _cachedReportKill;

	private Actor _target;

	private bool _isStopped;

	private string _trackerInfo;

	internal KillTracker(Actor target, float gracePeriod, Action<EventInfoKill> callback)
	{
		_gracePeriod = gracePeriod;
		_target = target;
		_callback = callback;
		_target.ActorEvent_OnDealDamage += new Action<EventInfoDamage>(TrackDamagedVictims);
		_target.LockDestroy();
		_trackerInfo = $"{target.GetActorReadableName()}, {gracePeriod}s";
	}

	public void Stop()
	{
		if (_isStopped)
		{
			return;
		}
		foreach (KeyValuePair<Entity, float> item in _trackingDict)
		{
			item.Key.EntityEvent_OnDeath -= _cachedReportKill;
		}
		_isStopped = true;
		_trackingDict = null;
		if ((UnityEngine.Object)(object)_target != null)
		{
			_target.ActorEvent_OnDealDamage -= new Action<EventInfoDamage>(TrackDamagedVictims);
			_target.UnlockDestroy();
		}
	}

	private void TrackDamagedVictims(EventInfoDamage obj)
	{
		if (!_isStopped && !obj.victim.Status.isDead)
		{
			if (_trackingDict.ContainsKey(obj.victim))
			{
				_trackingDict[obj.victim] = Time.time;
			}
			else
			{
				_trackingDict.Add(obj.victim, Time.time);
				obj.victim.EntityEvent_OnDeath += new Action<EventInfoKill>(ReportKill);
			}
			CullExpiredEntities();
		}
	}

	private void ReportKill(EventInfoKill obj)
	{
		if (!_isStopped && _trackingDict.TryGetValue(obj.victim, out var value) && !(Time.time - value > _gracePeriod))
		{
			_trackingDict.Remove(obj.victim);
			_callback?.Invoke(obj);
		}
	}

	private void CullExpiredEntities()
	{
		foreach (KeyValuePair<Entity, float> item in _trackingDict)
		{
			if ((UnityEngine.Object)(object)item.Key == null || Time.time - item.Value > _gracePeriod)
			{
				_expired.Add(item.Key);
			}
		}
		if (_expired.Count <= 0)
		{
			return;
		}
		foreach (Entity item2 in _expired)
		{
			_trackingDict.Remove(item2);
			if ((UnityEngine.Object)(object)item2 != null)
			{
				item2.EntityEvent_OnDeath -= _cachedReportKill;
			}
		}
		_expired.Clear();
	}
}
