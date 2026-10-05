using System.Collections;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Ai_Mon_Sky_LittleBaam_Atk : AbilityInstance
{
	public ScalingValue damage;

	public GameObject explodeEffect;

	public GameObject hitEffect;

	public DewCollider range;

	public bool stopOnCasterDeath;

	public int explodeCount;

	public float explodeInterval;

	public float maxAngle;

	public float angleDeviation;

	public AnimationCurve stepDistByDistance;

	public DewAudioSource explodeAudio;

	public AnimationCurve explodeAudioPitch;

	private int _explodeIndex;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_explodeIndex = 0;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		if (stopOnCasterDeath)
		{
			DestroyOnDeath(info.caster);
		}
		Vector3 knownPos = info.target.agentPosition;
		float angle = CastInfo.GetAngle(info.target.agentPosition - info.caster.position);
		Vector3 pos = position;
		for (int i = 0; i < explodeCount; i++)
		{
			float target = CastInfo.GetAngle(knownPos - pos) + angleDeviation * (Random.value * 2f - 1f);
			angle = Mathf.MoveTowardsAngle(angle, target, maxAngle);
			pos += Quaternion.Euler(0f, angle, 0f) * Vector3.forward * stepDistByDistance.Evaluate(Vector2.Distance(knownPos.ToXY(), pos.ToXY()));
			pos = Dew.GetPositionOnGround(pos);
			RpcExplode(pos);
			if ((Object)(object)info.target != null && info.target.isActive && !info.target.Status.isUndetectableByNonAllies)
			{
				knownPos = info.target.agentPosition;
			}
			yield return new SI.WaitForSeconds(explodeInterval);
		}
		Destroy();
	}

	[ClientRpc]
	private void RpcExplode(Vector3 pos)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteVector3((NetworkWriter)(object)val, pos);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ai_Mon_Sky_LittleBaam_Atk::RpcExplode(UnityEngine.Vector3)", 1700155998, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcExplode__Vector3(Vector3 pos)
	{
		((Component)(object)this).transform.position = pos;
		if (explodeAudio != null)
		{
			explodeAudio.pitchMultiplier = explodeAudioPitch.Evaluate((float)_explodeIndex / (float)explodeCount);
		}
		_explodeIndex++;
		FxPlayNew(explodeEffect);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		ListReturnHandle<Entity> handle;
		foreach (Entity entity in range.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
		{
			Damage(damage).SetElemental(ElementalType.Light).SetOriginPosition(pos).Dispatch(entity);
			FxPlayNewNetworked(hitEffect, entity);
		}
		handle.Return();
	}

	protected static void InvokeUserCode_RpcExplode__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcExplode called on server.");
		}
		else
		{
			((Ai_Mon_Sky_LittleBaam_Atk)(object)obj).UserCode_RpcExplode__Vector3(NetworkReaderExtensions.ReadVector3(reader));
		}
	}

	static Ai_Mon_Sky_LittleBaam_Atk()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_Mon_Sky_LittleBaam_Atk), "System.Void Ai_Mon_Sky_LittleBaam_Atk::RpcExplode(UnityEngine.Vector3)", (RemoteCallDelegate)InvokeUserCode_RpcExplode__Vector3);
	}
}
