using System;
using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Se_D_BeautifulThreat : StackedStatusEffect
{
	public ScalingValue hasteAmount;

	public float hitCooldown = 0.05f;

	public int addedStackPerCast = 2;

	public GameObject fxFeather;

	public Transform featherParent;

	public AnimationCurve addedAngleByTime;

	private float _baseAngle;

	private float _lastShootTime;

	private List<GameObject> _feathers = new List<GameObject>();

	private List<Vector3> _cvs = new List<Vector3>();

	private Dictionary<Entity, float> _hitTimes = new Dictionary<Entity, float>();

	private HasteEffect _haste;

	protected override void OnCreate()
	{
		base.OnCreate();
		_baseAngle = Mathf.Repeat((float)(NetworkTime.time + (double)((float)((NetworkBehaviour)this).netId * 0.3794f)) * 360f, 360f);
		if (((NetworkBehaviour)this).isServer)
		{
			_haste = DoHaste(0f);
			victim.EntityEvent_OnAttackEffectTriggered += new Action<EventInfoAttackEffect>(EntityEventOnAttackEffectTriggered);
			victim.ActorEvent_OnDoHeal += new Action<EventInfoHeal>(ActorEventOnDoHeal);
			((Hero)victim).ClientHeroEvent_OnSkillUse += new Action<EventInfoSkillUse>(ClientHeroEventOnSkillUse);
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
			At_Atk_AurenaFeather attackAbility = Dew.CreateAbilityTrigger<At_Atk_AurenaFeather>();
			AbilityTrigger attackAbility2 = victim.Ability.attackAbility;
			victim.Ability.SetAttackAbility(attackAbility);
			attackAbility2.Destroy();
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		for (int i = 0; i < _feathers.Count; i++)
		{
			Vector3 currentVelocity = _cvs[i];
			_feathers[i].transform.localPosition = Vector3.SmoothDamp(_feathers[i].transform.localPosition, GetDesiredLocalPos(i), ref currentVelocity, 0.2f);
			_cvs[i] = currentVelocity;
		}
		float num = (float)NetworkTime.time - _lastShootTime;
		float y = addedAngleByTime.Evaluate(num) + _baseAngle + num * 50f;
		featherParent.rotation = Quaternion.Euler(0f, y, 0f);
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && victim.IsNullInactiveDeadOrKnockedOut() && stack > 0)
		{
			SetStack(0);
		}
	}

	private Vector3 GetDesiredLocalPos(int index)
	{
		return Quaternion.Euler(0f, 360f / (float)stack * (float)index, 0f) * Vector3.forward * 1.25f;
	}

	private void ClientHeroEventOnSkillUse(EventInfoSkillUse obj)
	{
		if (stack < maxStack)
		{
			AddStack(addedStackPerCast);
			RpcNotifyShoot(featherParent.rotation.eulerAngles.y);
		}
	}

	private void ClientEventOnRoomLoaded(EventInfoLoadRoom obj)
	{
		_hitTimes.Clear();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		foreach (GameObject feather in _feathers)
		{
			if (feather != null)
			{
				UnityEngine.Object.Destroy(feather);
			}
		}
		_feathers.Clear();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)victim != null)
			{
				victim.EntityEvent_OnAttackEffectTriggered -= new Action<EventInfoAttackEffect>(EntityEventOnAttackEffectTriggered);
				victim.ActorEvent_OnDoHeal -= new Action<EventInfoHeal>(ActorEventOnDoHeal);
				((Hero)victim).ClientHeroEvent_OnSkillUse -= new Action<EventInfoSkillUse>(ClientHeroEventOnSkillUse);
				At_Atk_AurenaSwipe attackAbility = Dew.CreateAbilityTrigger<At_Atk_AurenaSwipe>();
				AbilityTrigger attackAbility2 = victim.Ability.attackAbility;
				victim.Ability.SetAttackAbility(attackAbility);
				attackAbility2.Destroy();
			}
			if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
			{
				NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
			}
			if ((UnityEngine.Object)(object)firstTrigger != null)
			{
				firstTrigger.fillAmount = 0f;
			}
		}
	}

	private void EntityEventOnAttackEffectTriggered(EventInfoAttackEffect obj)
	{
		if (obj.chain.DidReact(this))
		{
			return;
		}
		Dew.CallDelayed(() =>
		{
			if (stack > 0)
			{
				CheckTarget(obj.victim, obj.chain);
			}
		});
	}

	private void ActorEventOnDoHeal(EventInfoHeal obj)
	{
		if (stack > 0 && !obj.chain.DidReact(this))
		{
			CheckTarget(obj.target, obj.chain);
		}
	}

	private void CheckTarget(Entity target, ReactionChain eventChain)
	{
		if (target.IsNullInactiveDeadOrKnockedOut() || (_hitTimes.TryGetValue(target, out var value) && Time.time - value < hitCooldown) || (!info.caster.CheckEnemyOrNeutral(target) && target.normalizedHealth > 0.999f))
		{
			return;
		}
		_hitTimes[target] = Time.time;
		CreateAbilityInstance(victim.position, null, new CastInfo(victim, target), (Ai_D_BeautifulThreat_Feather ai) =>
		{
			ai.chain = eventChain.New(this);
			if (_feathers.Count > 0)
			{
				ai.SetCustomStartPosition(_feathers[0].transform.position);
			}
			else
			{
				ai.SetCustomStartPosition(info.caster.Visual.GetCenterPosition());
			}
		});
		RemoveStack();
		RpcNotifyShoot(featherParent.rotation.eulerAngles.y);
	}

	protected override void OnStackChange(int oldStack, int newStack)
	{
		base.OnStackChange(oldStack, newStack);
		while (_feathers.Count < newStack)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(fxFeather, featherParent);
			gameObject.transform.localPosition = GetDesiredLocalPos(_feathers.Count);
			FxPlay(gameObject);
			_feathers.Add(gameObject);
			_cvs.Add(default);
		}
		while (_feathers.Count > newStack)
		{
			UnityEngine.Object.Destroy(_feathers[0]);
			_feathers.RemoveAt(0);
			_cvs.RemoveAt(0);
		}
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)firstTrigger != null)
			{
				firstTrigger.fillAmount = (float)newStack / (float)maxStack;
			}
			if (_haste != null)
			{
				_haste.strength = ((newStack > 0) ? GetValue(hasteAmount) : 0f);
			}
		}
	}

	[ClientRpc]
	private void RpcNotifyShoot(float newBaseAngle)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, newBaseAngle);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Se_D_BeautifulThreat::RpcNotifyShoot(System.Single)", 96927282, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcNotifyShoot__Single(float newBaseAngle)
	{
		_baseAngle = newBaseAngle;
		_lastShootTime = Time.time;
	}

	protected static void InvokeUserCode_RpcNotifyShoot__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcNotifyShoot called on server.");
		}
		else
		{
			((Se_D_BeautifulThreat)(object)obj).UserCode_RpcNotifyShoot__Single(NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	static Se_D_BeautifulThreat()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Se_D_BeautifulThreat), "System.Void Se_D_BeautifulThreat::RpcNotifyShoot(System.Single)", (RemoteCallDelegate)InvokeUserCode_RpcNotifyShoot__Single);
	}
}
