using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class FlavourManager : NetworkedManagerBase<FlavourManager>, ISettingsChangedCallback
{
	private class Ad_DamageShake
	{
		public bool isStale;

		public EntityTransformModifier modifier;

		public float intensity;

		public Entity victim;

		public float duration;

		public float remaining;

		public int sign;

		public float nextTime;

		public bool hasDirection;

		public Vector3 direction;
	}

	public Vector2 shakeDamageRatioRange;

	public AnimationCurve shakeIntensity;

	public AnimationCurve shakeDuration;

	public float onStunDurationMultiplier;

	public float shakeInterval;

	public float deviateAngle;

	public GameObject hitStopDealDamage;

	public float hitTimeStopCooldownTime = 10f;

	public bool disableOnLesser;

	public Vector2 damageRatioRange;

	public AnimationCurve chanceByRatio;

	public float killingBlowDamageMultiplier;

	public float minDamageAmount;

	public float everyoneVicinityRadius;

	public bool ignoreAssists;

	public GameObject killFeedback;

	public GameObject killFeedbackLesser;

	public GameObject killFeedbackMiniBoss;

	public GameObject killFeedbackBoss;

	public FxVolume[] screenKillEffects;

	private float _lastHitStopTime = float.NegativeInfinity;

	private List<float> _originalStrengths = new List<float>();

	private const int KillFeedbackPoolSize = 3;

	private GameObject[] _killFeedbackInstances;

	private GameObject[] _killFeedbackLesserInstances;

	private int _killFeedbackCursor;

	private int _killFeedbackLesserCursor;

	private List<Ad_DamageShake> _activeShakes = new List<Ad_DamageShake>();

	private Stack<Ad_DamageShake> _shakePool = new Stack<Ad_DamageShake>();

	protected override void Awake()
	{
		base.Awake();
		FxVolume[] array = screenKillEffects;
		foreach (FxVolume fxVolume in array)
		{
			_originalStrengths.Add(fxVolume.targetStrength);
		}
	}

	private void UpdateScreenKillEffectsStrength()
	{
		if (_originalStrengths.Count > 0)
		{
			for (int i = 0; i < screenKillEffects.Length; i++)
			{
				screenKillEffects[i].targetStrength = _originalStrengths[i] * DewSave.profileMain.gameplay.killScreenEffectsStrength;
			}
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		GameManager.CallOnReady(() =>
		{
			UpdateScreenKillEffectsStrength();
			_killFeedbackInstances = CreatePersistentKillFeedbackPool(killFeedback);
			_killFeedbackLesserInstances = CreatePersistentKillFeedbackPool(killFeedbackLesser);
			NetworkedManagerBase<ClientEventManager>.instance.OnTakeDamage += new Action<EventInfoDamage>(OnClientTakeDamage);
			if (ignoreAssists)
			{
				NetworkedManagerBase<ClientEventManager>.instance.OnDeath += (Action<EventInfoKill>)((EventInfoKill info) =>
				{
					if (info.victim is Monster m && !((UnityEngine.Object)(object)info.actor == null))
					{
						Hero hero = info.actor.FindFirstOfType<Hero>();
						if (!((UnityEngine.Object)(object)hero == null) && ((NetworkBehaviour)hero).isOwned)
						{
							PlayKillFeedback(m);
						}
					}
				});
			}
			else
			{
				DewPlayer.local.hero.ClientHeroEvent_OnKillOrAssist += (Action<EventInfoKill>)((EventInfoKill info) =>
				{
					if (info.victim is Monster m)
					{
						PlayKillFeedback(m);
					}
				});
			}
		});
	}

	private void OnClientTakeDamage(EventInfoDamage info)
	{
		if (info.actor is ElementalStatusEffect)
		{
			return;
		}
		float num = (info.damage.amount + info.damage.discardedAmount) / (info.victim.maxHealth + info.victim.Status.currentShield);
		num = (num - shakeDamageRatioRange.x) / (shakeDamageRatioRange.y - shakeDamageRatioRange.x);
		float num2 = shakeIntensity.Evaluate(num);
		float num3 = shakeDuration.Evaluate(num);
		if (info.victim.TryGetData<Ad_DamageShake>(out var data))
		{
			if (data.intensity >= num2)
			{
				return;
			}
			data.isStale = true;
			data.modifier.Stop();
			info.victim.RemoveData<Ad_DamageShake>();
		}
		Ad_DamageShake ad_DamageShake = ((_shakePool.Count > 0) ? _shakePool.Pop() : new Ad_DamageShake());
		ad_DamageShake.isStale = false;
		ad_DamageShake.intensity = num2;
		ad_DamageShake.victim = info.victim;
		ad_DamageShake.duration = num3;
		ad_DamageShake.remaining = num3;
		ad_DamageShake.sign = 1;
		ad_DamageShake.nextTime = Time.time;
		ad_DamageShake.hasDirection = info.damage.direction.HasValue;
		ad_DamageShake.direction = info.damage.direction.GetValueOrDefault();
		ad_DamageShake.modifier = info.victim.Visual.GetNewTransformModifier();
		info.victim.AddData(ad_DamageShake);
		if (ad_DamageShake.remaining <= 0f)
		{
			info.victim.RemoveData<Ad_DamageShake>();
			ad_DamageShake.modifier.Stop();
			ReturnShake(ad_DamageShake);
		}
		else
		{
			_activeShakes.Add(ad_DamageShake);
			StepShake(ad_DamageShake);
		}
	}

	private void LateUpdate()
	{
		for (int num = _activeShakes.Count - 1; num >= 0; num--)
		{
			Ad_DamageShake ad_DamageShake = _activeShakes[num];
			if (!(Time.time < ad_DamageShake.nextTime))
			{
				if (ad_DamageShake.remaining <= 0f)
				{
					if ((UnityEngine.Object)(object)ad_DamageShake.victim != null)
					{
						ad_DamageShake.victim.RemoveData<Ad_DamageShake>();
						ad_DamageShake.modifier.Stop();
					}
					ReturnShake(ad_DamageShake);
					_activeShakes[num] = _activeShakes[_activeShakes.Count - 1];
					_activeShakes.RemoveAt(_activeShakes.Count - 1);
				}
				else if (ad_DamageShake.isStale || (UnityEngine.Object)(object)ad_DamageShake.victim == null)
				{
					if (!ad_DamageShake.isStale)
					{
						ad_DamageShake.modifier.Stop();
					}
					ReturnShake(ad_DamageShake);
					_activeShakes[num] = _activeShakes[_activeShakes.Count - 1];
					_activeShakes.RemoveAt(_activeShakes.Count - 1);
				}
				else
				{
					StepShake(ad_DamageShake);
				}
			}
		}
	}

	private void StepShake(Ad_DamageShake shake)
	{
		Vector3 vector = ((!shake.hasDirection) ? UnityEngine.Random.insideUnitCircle.normalized.ToXZ() : (Quaternion.Euler(0f, UnityEngine.Random.Range(0f - deviateAngle, deviateAngle), 0f) * shake.direction));
		shake.sign *= -1;
		shake.modifier.worldOffset = vector * ((float)shake.sign * (shake.remaining / shake.duration) * shake.intensity);
		shake.remaining -= ((shake.victim.Status.hasStun || shake.victim.Control.isAirborne) ? (shakeInterval / onStunDurationMultiplier) : shakeInterval);
		shake.nextTime = Time.time + shakeInterval;
	}

	private void ReturnShake(Ad_DamageShake shake)
	{
		shake.modifier = null;
		shake.victim = null;
		_shakePool.Push(shake);
	}

	private void PlayKillFeedback(Monster m)
	{
		switch (m.type)
		{
		case Monster.MonsterType.Lesser:
			PlayKillFeedbackFromPool(_killFeedbackLesserInstances, ref _killFeedbackLesserCursor, m);
			break;
		case Monster.MonsterType.Normal:
		case Monster.MonsterType.MiniBoss:
			PlayKillFeedbackFromPool(_killFeedbackInstances, ref _killFeedbackCursor, m);
			break;
		case Monster.MonsterType.Boss:
			break;
		}
	}

	private void PlayKillFeedbackFromPool(GameObject[] pool, ref int cursor, Monster m)
	{
		if (pool != null)
		{
			GameObject effect = pool[cursor];
			cursor = (cursor + 1) % pool.Length;
			FxPlay(effect, m);
		}
	}

	private GameObject[] CreatePersistentKillFeedbackPool(GameObject prefab)
	{
		if (prefab == null)
		{
			return null;
		}
		DewEffect.EnsureSourceBaked(prefab);
		GameObject[] array = new GameObject[3];
		for (int i = 0; i < 3; i++)
		{
			array[i] = UnityEngine.Object.Instantiate(prefab, ((Component)(object)this).transform);
		}
		return array;
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnEntityAdd += new Action<Entity>(OnEntityAdd);
		_lastHitStopTime = Time.unscaledTime + hitTimeStopCooldownTime;
	}

	private void OnEntityAdd(Entity obj)
	{
		Monster m = obj as Monster;
		if (m == null || (disableOnLesser && m.type == Monster.MonsterType.Lesser))
		{
			return;
		}
		m.EntityEvent_OnTakeDamage += (Action<EventInfoDamage>)((EventInfoDamage info) =>
		{
			if (!m.Status.hasMirageSkin && !(Time.unscaledTime - _lastHitStopTime < hitTimeStopCooldownTime) && !(info.damage.amount + info.damage.discardedAmount < minDamageAmount))
			{
				float num = (info.damage.amount + info.damage.discardedAmount) / m.maxHealth;
				if (m.currentHealth < 0.001f)
				{
					num *= killingBlowDamageMultiplier;
				}
				num = (num - damageRatioRange.x) / (damageRatioRange.y - damageRatioRange.x);
				if (!(num < 0f))
				{
					float num2 = chanceByRatio.Evaluate(num);
					if (!(UnityEngine.Random.value > num2) && !((UnityEngine.Object)(object)info.actor.FindFirstOfType<Hero>() == null))
					{
						foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
						{
							if (!gamePlayer.hero.IsNullInactiveDeadOrKnockedOut() && Vector2.Distance(m.position.ToXY(), gamePlayer.hero.position.ToXY()) > everyoneVicinityRadius)
							{
								return;
							}
						}
						FxPlayNewNetworked(hitStopDealDamage, m);
						_lastHitStopTime = Time.unscaledTime;
					}
				}
			}
		});
		m.EntityEvent_OnDeath += (Action<EventInfoKill>)((EventInfoKill info) =>
		{
			if (m.type == Monster.MonsterType.MiniBoss)
			{
				FxPlayNewNetworked(killFeedbackMiniBoss, m);
			}
			if (m.type == Monster.MonsterType.Boss && !(m is BossMonster { playBossKillFeedback: false }))
			{
				FxPlayNewNetworked(killFeedbackBoss, m);
			}
		});
	}

	public void OnSettingsChanged()
	{
		UpdateScreenKillEffectsStrength();
	}

	private void MirrorProcessed()
	{
	}
}
