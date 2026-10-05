using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Rendering;

public class FeedbackManager : ManagerBase<FeedbackManager>
{
	[Serializable]
	public class EffectPool
	{
		public GameObject effect;

		public int numOfSources = 2;

		private GameObject[] _effects;

		private int _cursor;

		public void Init(Transform parent)
		{
			_effects = new GameObject[numOfSources];
			for (int i = 0; i < numOfSources; i++)
			{
				_effects[i] = UnityEngine.Object.Instantiate(effect, parent);
				_effects[i].name = effect.name + " " + i;
			}
		}

		public void Play()
		{
			DewEffect.EnsureSourceBaked(_effects[_cursor]);
			DewEffect.Play(_effects[_cursor]);
			_cursor++;
			if (_cursor == numOfSources)
			{
				_cursor = 0;
			}
		}
	}

	public Transform genericFeedbacksParent;

	public Volume slowTimeBySpecialMenuVolume;

	public float slowTimeBySpecialMenuWeightSpeed = 10f;

	public GameObject fxGainStardustOnHero;

	public EffectPool dealDmg;

	public EffectPool dealDmgCrit;

	public EffectPool dealDmgDot;

	public EffectPool dealDmgDotCrit;

	public EffectPool takeDmg;

	public EffectPool takeDmgDot;

	public EffectPool healNormal;

	public EffectPool healCrit;

	private Dictionary<string, GameObject> _feedbackEffects = new Dictionary<string, GameObject>();

	protected override void Awake()
	{
		base.Awake();
		dealDmg.Init(transform);
		dealDmgCrit.Init(transform);
		dealDmgDot.Init(transform);
		dealDmgDotCrit.Init(transform);
		takeDmg.Init(transform);
		takeDmgDot.Init(transform);
		healNormal.Init(transform);
		healCrit.Init(transform);
		for (int i = 0; i < genericFeedbacksParent.childCount; i++)
		{
			Transform child = genericFeedbacksParent.GetChild(i);
			_feedbackEffects.Add(child.name, child.gameObject);
		}
	}

	private void Start()
	{
		NetworkedManagerBase<ClientEventManager>.instance.OnTakeDamage += (Action<EventInfoDamage>)((EventInfoDamage info) =>
		{
			if (!((UnityEngine.Object)(object)ManagerBase<CameraManager>.instance.focusedEntity == null))
			{
				if ((UnityEngine.Object)(object)info.victim == (UnityEngine.Object)(object)ManagerBase<CameraManager>.instance.focusedEntity)
				{
					if (info.damage.HasAttr(DamageAttribute.DamageOverTime))
					{
						takeDmgDot.Play();
					}
					else
					{
						takeDmg.Play();
					}
				}
				else if ((UnityEngine.Object)(object)info.actor != null && ((UnityEngine.Object)(object)info.actor == (UnityEngine.Object)(object)ManagerBase<CameraManager>.instance.focusedEntity || info.actor.IsDescendantOf(ManagerBase<CameraManager>.instance.focusedEntity)))
				{
					if (info.damage.HasAttr(DamageAttribute.DamageOverTime))
					{
						if (info.damage.HasAttr(DamageAttribute.IsCrit))
						{
							dealDmgDotCrit.Play();
						}
						else
						{
							dealDmgDot.Play();
						}
					}
					else if (info.damage.HasAttr(DamageAttribute.IsCrit))
					{
						dealDmgCrit.Play();
					}
					else
					{
						dealDmg.Play();
					}
				}
			}
		});
		NetworkedManagerBase<ClientEventManager>.instance.OnTakeHeal += (Action<EventInfoHeal>)((EventInfoHeal info) =>
		{
			if (!((UnityEngine.Object)(object)ManagerBase<CameraManager>.instance.focusedEntity == null) && !((UnityEngine.Object)(object)ManagerBase<CameraManager>.instance.focusedEntity.owner == null) && (UnityEngine.Object)(object)info.actor != null && ((UnityEngine.Object)(object)info.actor == (UnityEngine.Object)(object)ManagerBase<CameraManager>.instance.focusedEntity || info.actor.IsDescendantOf(ManagerBase<CameraManager>.instance.focusedEntity)))
			{
				if (info.isCrit)
				{
					healCrit.Play();
				}
				else
				{
					healNormal.Play();
				}
			}
		});
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnLocalHeroAdd += (Action<Hero>)((Hero h) =>
		{
			h.ClientHeroEvent_OnKnockedOut += (Action<EventInfoKill>)((EventInfoKill _) =>
			{
				PlayFeedbackEffect("UI_Game_LocalHeroKnockedOut");
			});
		});
		DewPlayer.onGamePlayerAdded += new Action<DewPlayer>(OnGamePlayerAdded);
		foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
		{
			OnGamePlayerAdded(gamePlayer);
		}
	}

	private void OnGamePlayerAdded(DewPlayer h)
	{
		h.ClientEvent_OnEarnStardust += (Action<int>)((int _) =>
		{
			DewEffect.PlayNew(fxGainStardustOnHero, h.hero, null);
			if (((NetworkBehaviour)h).isLocalPlayer)
			{
				PlayFeedbackEffect("UI_Game_LocalPlayerGainStardust");
			}
		});
	}

	private void OnDestroy()
	{
		DewPlayer.onGamePlayerAdded -= new Action<DewPlayer>(OnGamePlayerAdded);
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		slowTimeBySpecialMenuVolume.weight = Mathf.MoveTowards(slowTimeBySpecialMenuVolume.weight, NetworkedManagerBase<TimescaleManager>.instance.shouldTimeBeSlowedBySpecialMenu ? 1 : 0, Time.unscaledDeltaTime * slowTimeBySpecialMenuWeightSpeed);
	}

	public void PlayFeedbackEffect(string feedbackName)
	{
		if (_feedbackEffects.TryGetValue(feedbackName, out var value))
		{
			DewEffect.EnsureSourceBaked(value);
			DewEffect.Play(value);
		}
		else
		{
			Debug.LogWarning("Feedback of name '" + feedbackName + "' not found");
		}
	}

	public void StopFeedbackEffect(string feedbackName)
	{
		if (_feedbackEffects.TryGetValue(feedbackName, out var value))
		{
			DewEffect.Stop(value);
		}
		else
		{
			Debug.LogWarning("Feedback of name '" + feedbackName + "' not found");
		}
	}
}
