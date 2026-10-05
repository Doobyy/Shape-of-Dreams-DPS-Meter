using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Ai_Mon_Despair_BossAzurak_StompGlobal : AbilityInstance
{
	[Serializable]
	public struct Pattern
	{
		public float baseAngle;

		public float arc;

		public float innerRad;

		public float outerRad;
	}

	[Serializable]
	public class Wave
	{
		public Pattern[] patterns;
	}

	public DewCollider range;

	public ArcTelegraphController arcTelegraphTemplate;

	public Wave[] waves;

	public GameObject fxStompGroundPrepare;

	public GameObject fxStompGroundCast;

	public GameObject fxTelegraph;

	public GameObject fxExplode;

	public GameObject fxChunk;

	public float chunkDistance;

	public ScalingValue hitDamage;

	public GameObject fxHit;

	public float telegraphTime;

	public float perWavePostDelay = 0.55f;

	public int waveCount;

	public int waveCountMagnitude;

	public float postDelay = 2f;

	private Channel _channel;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		_channel = info.caster.Control.StartChannel(new Channel
		{
			blockedActions = Channel.BlockedAction.Everything,
			duration = float.PositiveInfinity,
			onCancel = DestroyIfActive
		});
		int count = waveCount + UnityEngine.Random.Range(-waveCountMagnitude, waveCountMagnitude);
		int waveIndex = 0;
		for (int i = 0; i < count; i++)
		{
			info.caster.Control.Rotate(ManagerBase<CameraManager>.instance.entityCamAngle + 180f, immediately: false);
			FxPlayNetworked(fxStompGroundPrepare, info.caster);
			int num;
			for (num = UnityEngine.Random.Range(0, waves.Length); num == waveIndex; num = UnityEngine.Random.Range(0, waves.Length))
			{
			}
			waveIndex = num;
			Wave wave = waves[waveIndex];
			RpcShowTelegraph(wave, telegraphTime);
			FxPlayNetworked(fxTelegraph);
			yield return new SI.WaitForSeconds(telegraphTime);
			FxPlayNetworked(fxStompGroundCast, info.caster);
			FxStopNetworked(fxStompGroundPrepare);
			FxStopNetworked(fxTelegraph);
			List<Entity> list = DewPool.GetList(out ListReturnHandle<Entity> handle);
			Pattern[] patterns = wave.patterns;
			for (int j = 0; j < patterns.Length; j++)
			{
				Pattern pattern = patterns[j];
				range.shape = DewCollider.ColliderShape.Polygon;
				range.transform.rotation = Quaternion.Euler(0f, pattern.baseAngle, 0f) * ManagerBase<CameraManager>.instance.entityCamAngleRotation;
				range.GeneratePolygonPoints_ArcDonut(pattern.innerRad, pattern.outerRad, pattern.arc);
				range.UpdateProxyCollider();
				List<Entity> entities = range.GetEntities(out var handle2, tvDefaultAllExceptSelf);
				for (int k = 0; k < entities.Count; k++)
				{
					Entity item = entities[k];
					if (!list.Contains(item))
					{
						list.Add(item);
					}
				}
				handle2.Return();
			}
			foreach (Entity item2 in list)
			{
				DefaultDamage((item2 is Monster) ? (item2.maxHealth * UnityEngine.Random.Range(0.25f, 0.5f)) : GetValue(hitDamage)).SetDirection(Vector3.up).Dispatch(item2);
				if (!item2.Status.hasCrowdControlImmunity)
				{
					item2.Visual.KnockUp(2f, isFriendly: false);
				}
				FxPlayNewNetworked(fxHit, item2);
				CreateBasicEffect(item2, new SlowEffect
				{
					strength = 70f,
					decay = true
				}, 1f);
				CreateBasicEffect(item2, new StunEffect(), (item2 is Monster) ? 0.5f : 0.25f);
			}
			handle.Return();
			RpcPlayMainEffect(wave);
			FxPlayNewNetworked(fxExplode);
			yield return new SI.WaitForSeconds(perWavePostDelay);
		}
		yield return new SI.WaitForSeconds(postDelay);
		Destroy();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && _channel != null && _channel.isAlive)
		{
			_channel.Cancel();
			_channel = null;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_channel = null;
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		position = info.caster.agentPosition;
	}

	[ClientRpc]
	private void RpcShowTelegraph(Wave w, float duration)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_Ai_Mon_Despair_BossAzurak_StompGlobal_002FWave((NetworkWriter)(object)val, w);
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, duration);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ai_Mon_Despair_BossAzurak_StompGlobal::RpcShowTelegraph(Ai_Mon_Despair_BossAzurak_StompGlobal/Wave,System.Single)", 1261594138, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcPlayMainEffect(Wave w)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_Ai_Mon_Despair_BossAzurak_StompGlobal_002FWave((NetworkWriter)(object)val, w);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ai_Mon_Despair_BossAzurak_StompGlobal::RpcPlayMainEffect(Ai_Mon_Despair_BossAzurak_StompGlobal/Wave)", 7859616, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void PlayMainEffectLocal(Wave w)
	{
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		Pattern[] patterns = w.patterns;
		for (int i = 0; i < patterns.Length; i++)
		{
			Pattern pattern = patterns[i];
			num = Mathf.Max(num, pattern.outerRad);
		}
		Vector3 vector = ((Component)(object)this).transform.position;
		Quaternion entityCamAngleRotation = ManagerBase<CameraManager>.instance.entityCamAngleRotation;
		List<ParticleSystem> componentsInChildrenNonAlloc = fxChunk.GetComponentsInChildrenNonAlloc(out ListReturnHandle<ParticleSystem> handle);
		int num2 = Mathf.CeilToInt(num / chunkDistance) * 2;
		for (int j = -num2; j <= num2; j++)
		{
			for (int k = -num2; k <= num2; k++)
			{
				float x = (float)j * chunkDistance;
				float z = (float)k * chunkDistance;
				Vector3 vector2 = new Vector3(x, 0f, z);
				if (vector2.magnitude > num + chunkDistance / 2f)
				{
					continue;
				}
				Vector3 vector3 = vector + vector2 + UnityEngine.Random.onUnitSphere * chunkDistance * 0.25f;
				patterns = w.patterns;
				for (int i = 0; i < patterns.Length; i++)
				{
					Pattern pattern2 = patterns[i];
					float magnitude = vector2.magnitude;
					if (magnitude < pattern2.innerRad || magnitude > pattern2.outerRad || !(Mathf.Abs(Vector3.SignedAngle(Quaternion.Euler(0f, pattern2.baseAngle, 0f) * entityCamAngleRotation * Vector3.forward, vector2, Vector3.up)) <= pattern2.arc / 2f))
					{
						continue;
					}
					vector3 = Dew.GetPositionOnGround(vector3);
					foreach (ParticleSystem item in componentsInChildrenNonAlloc)
					{
						EmissionModule emission = item.emission;
						if (emission.enabled)
						{
							EmitParams val = default;
							val.position = vector3;
							val.applyShapeToPosition = true;
							EmitParams val2 = val;
							emission = item.emission;
							Burst burst = emission.GetBurst(0);
							MinMaxCurve count = burst.count;
							item.Emit(val2, (int)count.constant);
						}
					}
					break;
				}
			}
		}
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcShowTelegraph__Wave__Single(Wave w, float duration)
	{
		Pattern[] patterns = w.patterns;
		for (int i = 0; i < patterns.Length; i++)
		{
			Pattern pattern = patterns[i];
			arcTelegraphTemplate.innerRadius = pattern.innerRad;
			arcTelegraphTemplate.outerRadius = pattern.outerRad;
			arcTelegraphTemplate.arcAngle = pattern.arc;
			arcTelegraphTemplate.duration = duration;
			FxPlayNew(arcTelegraphTemplate.gameObject, position, Quaternion.Euler(0f, pattern.baseAngle, 0f));
		}
	}

	protected static void InvokeUserCode_RpcShowTelegraph__Wave__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShowTelegraph called on server.");
		}
		else
		{
			((Ai_Mon_Despair_BossAzurak_StompGlobal)(object)obj).UserCode_RpcShowTelegraph__Wave__Single(GeneratedNetworkCode._Read_Ai_Mon_Despair_BossAzurak_StompGlobal_002FWave(reader), NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	protected void UserCode_RpcPlayMainEffect__Wave(Wave w)
	{
		PlayMainEffectLocal(w);
	}

	protected static void InvokeUserCode_RpcPlayMainEffect__Wave(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayMainEffect called on server.");
		}
		else
		{
			((Ai_Mon_Despair_BossAzurak_StompGlobal)(object)obj).UserCode_RpcPlayMainEffect__Wave(GeneratedNetworkCode._Read_Ai_Mon_Despair_BossAzurak_StompGlobal_002FWave(reader));
		}
	}

	static Ai_Mon_Despair_BossAzurak_StompGlobal()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_Mon_Despair_BossAzurak_StompGlobal), "System.Void Ai_Mon_Despair_BossAzurak_StompGlobal::RpcShowTelegraph(Ai_Mon_Despair_BossAzurak_StompGlobal/Wave,System.Single)", (RemoteCallDelegate)InvokeUserCode_RpcShowTelegraph__Wave__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_Mon_Despair_BossAzurak_StompGlobal), "System.Void Ai_Mon_Despair_BossAzurak_StompGlobal::RpcPlayMainEffect(Ai_Mon_Despair_BossAzurak_StompGlobal/Wave)", (RemoteCallDelegate)InvokeUserCode_RpcPlayMainEffect__Wave);
	}
}
