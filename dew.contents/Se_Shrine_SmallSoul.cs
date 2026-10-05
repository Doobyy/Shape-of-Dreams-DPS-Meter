using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

[SaveActor(true)]
public class Se_Shrine_SmallSoul : StackedStatusEffect
{
	public GameObject fxSoul;

	public GameObject fxSoulFlash;

	public float initGap = 1f;

	public float perSoulGap = 0.7f;

	public float initAttackDelay = 0.5f;

	public float perSoulDelay = 0.3f;

	private List<GameObject> _souls = new List<GameObject>();

	private List<Vector3> _desiredPositions = new List<Vector3>();

	private List<Vector3> _cvs = new List<Vector3>();

	private Vector3 _spawnPosition;

	public void SetSpawnPosition(Vector3 value)
	{
		_spawnPosition = value;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		OnStackChange(0, stack);
		if (((NetworkBehaviour)this).isServer)
		{
			if (NetworkedManagerBase<ZoneManager>.instance.currentZone.name != "Zone_Ink")
			{
				Destroy();
				return;
			}
			DestroyOnDeath(victim, includeKnockOuts: true);
			victim.EntityEvent_OnAttackHit += new Action<EventInfoAttackHit>(EntityEventOnAttackHit);
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded += new Action<EventInfoLoadZone>(ClientEventOnZoneLoaded);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		foreach (GameObject soul in _souls)
		{
			if (!(soul == null))
			{
				UnityEngine.Object.Destroy(soul);
			}
		}
		_souls.Clear();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)victim != null)
			{
				victim.EntityEvent_OnAttackHit -= new Action<EventInfoAttackHit>(EntityEventOnAttackHit);
			}
			if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
			{
				NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded -= new Action<EventInfoLoadZone>(ClientEventOnZoneLoaded);
			}
		}
	}

	private void ClientEventOnZoneLoaded(EventInfoLoadZone obj)
	{
		if (obj.to != "Zone_Ink")
		{
			Destroy();
		}
	}

	private void EntityEventOnAttackHit(EventInfoAttackHit obj)
	{
		int attackCount;
		if (!obj.victim.IsNullInactiveDeadOrKnockedOut())
		{
			attackCount = stack;
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(initAttackDelay);
			for (int i = 0; i < attackCount; i++)
			{
				if (this.IsNullOrInactive())
				{
					break;
				}
				CreateAbilityInstance<Ai_Shrine_SmallSoul_Attack>(position, null, new CastInfo(victim, obj.victim));
				RpcPlaySoulFlash(i);
				yield return new WaitForSeconds(perSoulDelay);
			}
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		for (int i = 0; i < _desiredPositions.Count; i++)
		{
			_desiredPositions[i] = victim.Visual.GetCenterPosition() - ((Component)(object)victim).transform.forward * (initGap + perSoulGap * (float)i);
		}
		for (int j = 0; j < _souls.Count; j++)
		{
			if (j >= _cvs.Count)
			{
				Debug.LogError("Se_Shrine_SmallSoul; ActiveFrameUpdate(); _cvs.Count is smaller than _souls.Count.");
				break;
			}
			Vector3 currentVelocity = _cvs[j];
			_souls[j].transform.position = Vector3.SmoothDamp(_souls[j].transform.position, _desiredPositions[j] + UnityEngine.Random.insideUnitSphere * 0.2f, ref currentVelocity, 0.2f + 0.1f * (float)j);
			_cvs[j] = currentVelocity;
		}
	}

	protected override void OnStackChange(int oldStack, int newStack)
	{
		base.OnStackChange(oldStack, newStack);
		while (_souls.Count > newStack)
		{
			FxStop(_souls[0]);
			UnityEngine.Object.Destroy(_souls[0]);
			_souls.RemoveAt(0);
			_desiredPositions.RemoveAt(0);
			_cvs.RemoveAt(0);
		}
		while (_souls.Count < newStack)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(fxSoul, ((Component)(object)this).transform);
			gameObject.transform.position = _spawnPosition;
			FxPlay(gameObject);
			_souls.Add(gameObject);
			_desiredPositions.Add(victim.Visual.GetCenterPosition() + UnityEngine.Random.insideUnitSphere.Flattened());
			_cvs.Add(default);
		}
	}

	[ClientRpc]
	private void RpcPlaySoulFlash(int index)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, index);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Se_Shrine_SmallSoul::RpcPlaySoulFlash(System.Int32)", 274584093, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcPlaySoulFlash__Int32(int index)
	{
		if (index >= 0 && index < _souls.Count)
		{
			FxPlayNew(fxSoulFlash, _souls[index].transform.position, null);
		}
	}

	protected static void InvokeUserCode_RpcPlaySoulFlash__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlaySoulFlash called on server.");
		}
		else
		{
			((Se_Shrine_SmallSoul)(object)obj).UserCode_RpcPlaySoulFlash__Int32(NetworkReaderExtensions.ReadInt(reader));
		}
	}

	static Se_Shrine_SmallSoul()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Se_Shrine_SmallSoul), "System.Void Se_Shrine_SmallSoul::RpcPlaySoulFlash(System.Int32)", (RemoteCallDelegate)InvokeUserCode_RpcPlaySoulFlash__Int32);
	}
}
