using System;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Se_R_AnnihilationStance : StatusEffect
{
	public ScalingValue adPercentage;

	public ScalingValue atkSpdPercentage;

	public float duration;

	public DewAnimationClip animLeft;

	public DewAnimationClip animRight;

	public GameObject fxUse;

	[NonSerialized]
	public bool disableShockwave;

	private bool _isLeft;

	private Action<Actor> _cachedClientActorEventOnCreate;

	private Action<EventInfoAttackFired> _cachedOnAttackFired;

	private Action<EventInfoAttackHit> _cachedEntityEventOnAttackHit;

	private ScalingValue _baseAdPercentage;

	private ScalingValue _baseAtkSpdPercentage;

	private float _baseDuration;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseAdPercentage = adPercentage;
		_baseAtkSpdPercentage = atkSpdPercentage;
		_baseDuration = duration;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		adPercentage = _baseAdPercentage;
		atkSpdPercentage = _baseAtkSpdPercentage;
		duration = _baseDuration;
		_isLeft = false;
		disableShockwave = false;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		victim.ClientActorEvent_OnCreate += new Action<Actor>(ClientActorEventOnCreate);
		if (((NetworkBehaviour)this).isServer)
		{
			UpdateAnimOverride();
			DoStatBonus(new StatBonus
			{
				attackDamagePercentage = GetValue(adPercentage),
				attackRangePercentage = 50f
			});
			victim.Status.finalStatsProcessors.Add(FinalStatProcessor, 11);
			victim.Status.CalculateStats();
			SetTimer(duration);
			ShowOnScreenTimer();
			((Hero)victim).Skill.Movement.SetCharge(0, 999);
			ResetCooldown(victim.Ability.originalAttackAbility);
			if (!disableShockwave && victim.Ability.originalAttackAbility is At_Atk_HuskSword at_Atk_HuskSword)
			{
				at_Atk_HuskSword.ignoreRangeCheck = true;
			}
			victim.EntityEvent_OnAttackFired += new Action<EventInfoAttackFired>(EntityEventOnAttackFired);
			victim.EntityEvent_OnAttackHit += new Action<EventInfoAttackHit>(EntityEventOnAttackHit);
		}
	}

	private void FinalStatProcessor(ref FinalStats data)
	{
		data.attackSpeedMultiplier += GetValue(atkSpdPercentage) * 0.01f;
	}

	private void ClientActorEventOnCreate(Actor obj)
	{
		float hue;
		if (obj is Ai_Atk_HuskSword || obj is Ai_Atk_HuskSword_Crit)
		{
			hue = 0f;
			MeleeAttackInstance meleeAttackInstance = (MeleeAttackInstance)obj;
			meleeAttackInstance.range.transform.localScale = Vector3.Scale(meleeAttackInstance.range.transform.localScale, new Vector3(2f, 1f, 1f));
			Process(meleeAttackInstance.startEffect);
			Process(meleeAttackInstance.startEffectNoStop);
			Process(meleeAttackInstance.endEffect);
		}
		void Process(GameObject gobj)
		{
			if (!(gobj == null))
			{
				DewEffect.ChangeColorRecursively(gobj, hue, 1f, 0f);
				ListReturnHandle<DewAudioSource> handle;
				foreach (DewAudioSource item in gobj.GetComponentsInChildrenNonAlloc(out handle))
				{
					item.volumeMultiplier *= 0.4f;
				}
				handle.Return();
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if ((UnityEngine.Object)(object)victim != null)
		{
			victim.ClientActorEvent_OnCreate -= new Action<Actor>(ClientActorEventOnCreate);
		}
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			At_Atk_HuskSword at_Atk_HuskSword = (At_Atk_HuskSword)victim.Ability.originalAttackAbility;
			victim.EntityEvent_OnAttackFired -= _cachedOnAttackFired;
			victim.EntityEvent_OnAttackHit -= _cachedEntityEventOnAttackHit;
			at_Atk_HuskSword.cooldownTimeMultiplier = null;
			at_Atk_HuskSword.ignoreRangeCheck = false;
			at_Atk_HuskSword.overrideEndAnim = null;
			victim.Status.finalStatsProcessors.Remove(FinalStatProcessor);
			victim.Status.CalculateStats();
		}
	}

	private void UpdateAnimOverride()
	{
		if (victim.Ability.originalAttackAbility is At_Atk_HuskSword at_Atk_HuskSword)
		{
			at_Atk_HuskSword.overrideEndAnim = (_isLeft ? animLeft : animRight);
		}
	}

	private void EntityEventOnAttackHit(EventInfoAttackHit obj)
	{
		if (!(obj.strength < 0.999f) && !((UnityEngine.Object)(object)obj.attacker != (UnityEngine.Object)(object)victim))
		{
			obj.victim.IsNullInactiveDeadOrKnockedOut();
		}
	}

	private void EntityEventOnAttackFired(EventInfoAttackFired obj)
	{
		float angle = obj.info.angle;
		if (angle == 0f && (UnityEngine.Object)(object)obj.info.target != null)
		{
			angle = CastInfo.GetAngle(obj.info.target.position - victim.position);
		}
		if (!disableShockwave)
		{
			CreateAbilityInstance(victim.position, null, new CastInfo(victim, angle), (Ai_R_AnnihilationStance_Projectile ai) =>
			{
				ai.NetworkisLeft = _isLeft;
			});
		}
		RpcPlayUse(_isLeft, angle);
		_isLeft = !_isLeft;
		UpdateAnimOverride();
	}

	[ClientRpc]
	private void RpcPlayUse(bool isLeft, float angle)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, isLeft);
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, angle);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Se_R_AnnihilationStance::RpcPlayUse(System.Boolean,System.Single)", -570074237, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcPlayUse__Boolean__Single(bool isLeft, float angle)
	{
		Vector3 localScale = fxUse.transform.localScale;
		if (isLeft)
		{
			fxUse.transform.localScale = localScale.WithX(0f - localScale.x);
		}
		FxPlayNew(fxUse, victim, victim.agentPosition, Quaternion.Euler(0f, angle, 0f));
		fxUse.transform.localScale = localScale;
	}

	protected static void InvokeUserCode_RpcPlayUse__Boolean__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayUse called on server.");
		}
		else
		{
			((Se_R_AnnihilationStance)(object)obj).UserCode_RpcPlayUse__Boolean__Single(NetworkReaderExtensions.ReadBool(reader), NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	static Se_R_AnnihilationStance()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Se_R_AnnihilationStance), "System.Void Se_R_AnnihilationStance::RpcPlayUse(System.Boolean,System.Single)", (RemoteCallDelegate)InvokeUserCode_RpcPlayUse__Boolean__Single);
	}
}
