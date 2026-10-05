using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class TriggerConfig
{
	public enum StatusEffectVictimType
	{
		Caster,
		Target
	}

	public Sprite triggerIcon;

	public bool isActive = true;

	[SerializeField]
	[JsonProperty]
	internal float _manaCost;

	[SerializeField]
	[JsonProperty]
	internal int _maxCharges = 1;

	[SerializeField]
	[JsonProperty]
	public int startCharges = -1;

	[SerializeField]
	[JsonProperty]
	internal int _addedCharges = 1;

	[SerializeField]
	[JsonProperty]
	internal float _cooldownTime = 4f;

	[SerializeField]
	[JsonProperty]
	internal float _minimumDelay;

	public AssetRef<AbilityInstance> spawnedInstanceRef;

	public bool lockCooldownUntilKilled;

	public bool lockCastUntilKilled;

	public bool setFillAmount = true;

	public bool destroyExistingEffect;

	public StatusEffect appliedStatusEffectRef;

	[FormerlySerializedAs("animation")]
	public DewAnimationClip startAnim;

	public DewAnimationClip endAnim;

	public DewAudioClip castVoice;

	public GameObject effectOnCast;

	public StatusEffectVictimType victim;

	public TriggerChannelData channel;

	public float postDelay;

	public AbilitySelfValidator selfValidator;

	public bool ignoreBlock;

	public bool ignoreAbilityLock;

	public bool faceForward = true;

	public bool overrideRotation;

	public float overrideRotationDuration = 0.75f;

	public bool canReceiveCooldownReduction = true;

	public bool postponeBasicCommand;

	public bool moveTowardsCastDirection;

	public bool canConsumeCastBonus = true;

	public bool alwaysCastImmediately;

	public bool castByMoveDirectionByDefault;

	public bool castByMoveDirectionGamepad;

	public bool ignoreAimDirectionGamepad;

	public bool unstoppableWhileCasting;

	public AbilityTargetValidator targetValidator;

	public CastMethodData castMethod;

	public AbilityTrigger.PredictionSettings predictionSettings = AbilityTrigger.PredictionSettings.Default;

	internal AbilityTrigger _parent;

	public float manaCost
	{
		get
		{
			return _manaCost;
		}
		set
		{
			_manaCost = value;
			_parent._isConfigDirty = true;
		}
	}

	public int maxCharges
	{
		get
		{
			return _maxCharges;
		}
		set
		{
			_maxCharges = value;
			_parent._isConfigDirty = true;
		}
	}

	public int addedCharges
	{
		get
		{
			return _addedCharges;
		}
		set
		{
			_addedCharges = value;
			_parent._isConfigDirty = true;
		}
	}

	public float cooldownTime
	{
		get
		{
			return _cooldownTime;
		}
		set
		{
			_cooldownTime = value;
			_parent._isConfigDirty = true;
		}
	}

	public float minimumDelay
	{
		get
		{
			return _minimumDelay;
		}
		set
		{
			_minimumDelay = value;
			_parent._isConfigDirty = true;
		}
	}

	public AbilityInstance spawnedInstance
	{
		get
		{
			Type type = spawnedInstanceRef.type;
			if (!(type == null))
			{
				return (AbilityInstance)(object)DewResources.GetByType(type, new ResourceLoadSettings
				{
					varDef = default(VariantDef).Add(DewResources.vQualityAdjusted)
				});
			}
			return null;
		}
		set
		{
			spawnedInstanceRef = value;
		}
	}

	public StatusEffect appliedStatusEffect
	{
		get
		{
			return appliedStatusEffectRef;
		}
		set
		{
			appliedStatusEffectRef = value;
		}
	}

	public bool isSpawnedInstanceStatusEffect => typeof(StatusEffect).IsAssignableFrom(spawnedInstanceRef.type);

	public float effectiveRange => castMethod.GetEffectiveRange(_parent);

	public bool IsStatusEffectNonTargeted()
	{
		if (isSpawnedInstanceStatusEffect)
		{
			return castMethod.type != CastMethodType.Target;
		}
		return false;
	}

	public bool CheckRange(CastInfo info)
	{
		switch (castMethod.type)
		{
		case CastMethodType.None:
		case CastMethodType.Cone:
		case CastMethodType.Arrow:
			return true;
		case CastMethodType.Target:
		{
			Entity target = info.target;
			if ((UnityEngine.Object)(object)target == null)
			{
				return true;
			}
			Entity caster = info.caster;
			return Vector2.Distance(caster.agentPosition.ToXY(), target.GetAIAgentPosition(caster).ToXY()) - target.Control.outerRadius < castMethod.targetData.range;
		}
		case CastMethodType.Point:
			if (castMethod._isClamping)
			{
				return true;
			}
			return Vector2.Distance(info.caster.agentPosition.ToXY(), info.point.ToXY()) < castMethod.pointData.range;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public bool CheckRange(Entity caster, Entity target)
	{
		return Vector2.Distance(caster.agentPosition.ToXY(), target.GetAIAgentPosition(caster).ToXY()) - target.Control.outerRadius <= castMethod.GetEffectiveRange(_parent);
	}

	public Vector3 GetMoveToCastDestination(CastInfo info)
	{
		return castMethod.type switch
		{
			CastMethodType.Target => info.target.position, 
			CastMethodType.Point => info.point, 
			_ => default, 
		};
	}

	public float GetMoveToCastRequiredDistance(CastInfo info)
	{
		switch (castMethod.type)
		{
		case CastMethodType.Target:
			return castMethod.GetEffectiveRange(_parent) + info.target.Control.outerRadius;
		case CastMethodType.Point:
			if (castMethod._isClamping)
			{
				return float.PositiveInfinity;
			}
			return castMethod.GetEffectiveRange(_parent);
		default:
			return float.PositiveInfinity;
		}
	}

	public TriggerConfig Clone()
	{
		TriggerConfig triggerConfig = (TriggerConfig)MemberwiseClone();
		if (castMethod != null)
		{
			triggerConfig.castMethod = new CastMethodData(castMethod);
		}
		if (channel != null)
		{
			triggerConfig.channel = new TriggerChannelData
			{
				duration = channel.duration,
				blockedActions = channel.blockedActions
			};
		}
		if (selfValidator != null)
		{
			triggerConfig.selfValidator = new AbilitySelfValidator
			{
				isMovementAbility = selfValidator.isMovementAbility,
				allowWhileDashing = selfValidator.allowWhileDashing,
				allowWhileDodging = selfValidator.allowWhileDodging,
				allowWhileDisabled = selfValidator.allowWhileDisabled
			};
		}
		if (targetValidator != null)
		{
			triggerConfig.targetValidator = new AbilityTargetValidator
			{
				targets = targetValidator.targets
			};
		}
		return triggerConfig;
	}
}
