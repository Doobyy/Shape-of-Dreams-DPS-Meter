using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class DewPersistentGameObserver
{
	private List<Action<DewGameResult>> _gameResultCallbacks;

	internal List<Action> _unregisterCallbacks;

	protected List<Coroutine> _coroutines = new List<Coroutine>();

	public string name => GetType().Name;

	public virtual void OnStartLocalClient()
	{
	}

	public virtual void OnStopLocalClient()
	{
		StopCoroutines();
		UnregisterListeners();
		_gameResultCallbacks = null;
	}

	private void StopCoroutines()
	{
		if (ManagerBase<AchievementManager>.instance != null)
		{
			foreach (Coroutine coroutine in _coroutines)
			{
				ManagerBase<AchievementManager>.instance.StopCoroutine(coroutine);
			}
		}
		_coroutines.Clear();
	}

	private void UnregisterListeners()
	{
		if (_unregisterCallbacks == null)
		{
			return;
		}
		foreach (Action unregisterCallback in _unregisterCallbacks)
		{
			try
			{
				unregisterCallback();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		_unregisterCallbacks = null;
	}

	public void LoadState(Dictionary<string, string> storage)
	{
		if (storage == null)
		{
			return;
		}
		FieldInfo[] fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.GetCustomAttributes(typeof(AchPersistentVarAttribute), inherit: false).Length == 0)
			{
				continue;
			}
			try
			{
				if (storage.TryGetValue(fieldInfo.Name, out var value))
				{
					object value2 = JsonUtilityEx.FromJson(value, fieldInfo.FieldType);
					fieldInfo.SetValue(this, value2);
				}
			}
			catch (Exception exception)
			{
				Debug.LogError("Exception occured while loading persistent var: " + fieldInfo.FieldType.Name + " " + GetType().Name + "::" + fieldInfo.Name);
				Debug.LogException(exception);
			}
		}
	}

	public void SaveState(Dictionary<string, string> storage)
	{
		try
		{
			FieldInfo[] fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo fieldInfo in fields)
			{
				if (fieldInfo.GetCustomAttributes(typeof(AchPersistentVarAttribute), inherit: false).Length != 0)
				{
					try
					{
						object value = fieldInfo.GetValue(this);
						storage[fieldInfo.Name] = JsonUtilityEx.ToJson(value);
					}
					catch (Exception exception)
					{
						Debug.LogError("Exception occured while saving achievement persistent var: " + fieldInfo.FieldType.Name + " " + GetType().Name + "::" + fieldInfo.Name);
						Debug.LogException(exception);
					}
				}
			}
		}
		catch (Exception exception2)
		{
			Debug.LogError("Exception occured while saving persistent observer status: " + GetType().Name);
			Debug.LogException(exception2);
		}
	}

	public void FeedGameResult(DewGameResult obj)
	{
		if (_gameResultCallbacks == null)
		{
			return;
		}
		foreach (Action<DewGameResult> gameResultCallback in _gameResultCallbacks)
		{
			try
			{
				gameResultCallback(obj);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	protected void AchStartCoroutine(IEnumerator routine)
	{
		Coroutine item = ManagerBase<AchievementManager>.instance.StartCoroutine(routine);
		_coroutines.Add(item);
	}

	protected void AchStopAllCoroutines()
	{
		foreach (Coroutine coroutine in _coroutines)
		{
			ManagerBase<AchievementManager>.instance.StopCoroutine(coroutine);
		}
		_coroutines.Clear();
	}

	protected void AchSetInterval(Action func, float interval)
	{
		Coroutine item = ManagerBase<AchievementManager>.instance.StartCoroutine(Routine());
		_coroutines.Add(item);
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(interval * UnityEngine.Random.value);
			while (true)
			{
				if ((UnityEngine.Object)(object)DewPlayer.local != null)
				{
					try
					{
						func();
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
					}
				}
				yield return new WaitForSeconds(interval);
			}
		}
	}

	protected void AchOnKillLastHit(Action<EventInfoKill> func)
	{
		if (_unregisterCallbacks == null)
		{
			_unregisterCallbacks = new List<Action>();
		}
		NetworkedManagerBase<ClientEventManager>.instance.OnDeath += new Action<EventInfoKill>(KillListener);
		_unregisterCallbacks.Add(() =>
		{
			if ((UnityEngine.Object)(object)NetworkedManagerBase<ClientEventManager>.instance != null)
			{
				NetworkedManagerBase<ClientEventManager>.instance.OnDeath -= new Action<EventInfoKill>(KillListener);
			}
		});
		void KillListener(EventInfoKill obj)
		{
			try
			{
				if (!((UnityEngine.Object)(object)obj.actor == null) && !((UnityEngine.Object)(object)DewPlayer.local == null) && !((UnityEngine.Object)(object)DewPlayer.local.hero == null) && obj.actor.IsDescendantOf(DewPlayer.local.hero))
				{
					func(obj);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	protected void AchOnKillOrAssist(Action<EventInfoKill> func)
	{
		if (_unregisterCallbacks == null)
		{
			_unregisterCallbacks = new List<Action>();
		}
		Hero hero = DewPlayer.local.hero;
		hero.ClientHeroEvent_OnKillOrAssist += new Action<EventInfoKill>(KillListener);
		_unregisterCallbacks.Add(() =>
		{
			hero.ClientHeroEvent_OnKillOrAssist -= new Action<EventInfoKill>(KillListener);
		});
		void KillListener(EventInfoKill obj)
		{
			try
			{
				func(obj);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	protected void AchOnDealDamage(Action<EventInfoDamage> func)
	{
		if (_unregisterCallbacks == null)
		{
			_unregisterCallbacks = new List<Action>();
		}
		NetworkedManagerBase<ClientEventManager>.instance.OnTakeDamage += new Action<EventInfoDamage>(DamageListener);
		_unregisterCallbacks.Add(() =>
		{
			if ((UnityEngine.Object)(object)NetworkedManagerBase<ClientEventManager>.instance != null)
			{
				NetworkedManagerBase<ClientEventManager>.instance.OnTakeDamage -= new Action<EventInfoDamage>(DamageListener);
			}
		});
		void DamageListener(EventInfoDamage obj)
		{
			try
			{
				if (!((UnityEngine.Object)(object)obj.actor == null) && !((UnityEngine.Object)(object)DewPlayer.local == null) && !((UnityEngine.Object)(object)DewPlayer.local.hero == null) && obj.actor.IsDescendantOf(DewPlayer.local.hero))
				{
					func(obj);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	protected void AchOnDoHeal(Action<EventInfoHeal> func)
	{
		if (_unregisterCallbacks == null)
		{
			_unregisterCallbacks = new List<Action>();
		}
		NetworkedManagerBase<ClientEventManager>.instance.OnTakeHeal += new Action<EventInfoHeal>(OnTakeHeal);
		_unregisterCallbacks.Add(() =>
		{
			if ((UnityEngine.Object)(object)NetworkedManagerBase<ClientEventManager>.instance != null)
			{
				NetworkedManagerBase<ClientEventManager>.instance.OnTakeHeal -= new Action<EventInfoHeal>(OnTakeHeal);
			}
		});
		void OnTakeHeal(EventInfoHeal obj)
		{
			try
			{
				if (!((UnityEngine.Object)(object)obj.actor == null) && !((UnityEngine.Object)(object)DewPlayer.local == null) && !((UnityEngine.Object)(object)DewPlayer.local.hero == null) && (!((UnityEngine.Object)(object)obj.actor != (UnityEngine.Object)(object)DewPlayer.local.hero) || obj.actor.IsDescendantOf(DewPlayer.local.hero)))
				{
					func(obj);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	protected void AchOnTakeDamage(Action<EventInfoDamage> func)
	{
		if (_unregisterCallbacks == null)
		{
			_unregisterCallbacks = new List<Action>();
		}
		NetworkedManagerBase<ClientEventManager>.instance.OnTakeDamage += new Action<EventInfoDamage>(DamageListener);
		_unregisterCallbacks.Add(() =>
		{
			if ((UnityEngine.Object)(object)NetworkedManagerBase<ClientEventManager>.instance != null)
			{
				NetworkedManagerBase<ClientEventManager>.instance.OnTakeDamage -= new Action<EventInfoDamage>(DamageListener);
			}
		});
		void DamageListener(EventInfoDamage obj)
		{
			try
			{
				if (!((UnityEngine.Object)(object)obj.victim == null) && !((UnityEngine.Object)(object)DewPlayer.local == null) && !((UnityEngine.Object)(object)DewPlayer.local.hero == null) && !((UnityEngine.Object)(object)obj.victim != (UnityEngine.Object)(object)DewPlayer.local.hero))
				{
					func(obj);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	protected void AchOnGameConcluded(Action<DewGameResult> func)
	{
		if (_gameResultCallbacks == null)
		{
			_gameResultCallbacks = new List<Action<DewGameResult>>();
		}
		_gameResultCallbacks.Add(func);
	}
}
