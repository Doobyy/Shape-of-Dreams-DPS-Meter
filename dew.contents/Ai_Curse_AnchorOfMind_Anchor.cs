using System.Collections;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Ai_Curse_AnchorOfMind_Anchor : AbilityInstance
{
	public float radius = 6f;

	public DewBeamRenderer normalBeam;

	public DewBeamRenderer explodeBeam;

	public GameObject fxExplode;

	public float[] damageHealthRatio;

	private Vector3 _cv;

	protected override void OnCreate()
	{
		base.OnCreate();
		UpdateBeamPoints();
		normalBeam.enabled = true;
		explodeBeam.enabled = false;
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster, includeKnockOuts: true);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		normalBeam.enabled = false;
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (((NetworkBehaviour)this).isServer)
		{
			Vector3 vector = Vector3.SmoothDamp(((Component)(object)this).transform.position, info.caster.agentPosition, ref _cv, 1.4f);
			vector.y = info.caster.agentPosition.y;
			((Component)(object)this).transform.position = vector;
		}
		UpdateBeamPoints();
	}

	private void UpdateBeamPoints()
	{
		normalBeam.SetPoints(((Component)(object)this).transform.position + Vector3.up * 1.55f, info.caster.Visual.GetCenterPosition());
		explodeBeam.SetPoints(((Component)(object)this).transform.position + Vector3.up * 1.55f, info.caster.Visual.GetCenterPosition());
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			if (ManagerBase<CameraManager>.instance.isPlayingCutscene || ManagerBase<TransitionManager>.instance.state == TransitionManager.StateType.Loading || NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition || info.caster.Status.HasStatusEffect<Se_Shrine_Despair_Teleport>())
			{
				Destroy();
			}
			else if (Vector2.Distance(info.caster.agentPosition.ToXY(), position.ToXY()) > radius)
			{
				Explode();
			}
		}
	}

	private void Explode()
	{
		RpcExplodeBeam();
		FxPlayNetworked(fxExplode, info.caster);
		PureDamage(GetValue(damageHealthRatio) * info.caster.currentHealth).SetAttr(DamageAttribute.IgnoreDamageImmunity).Dispatch(info.caster);
		CreateBasicEffect(info.caster, new SlowEffect
		{
			decay = true,
			strength = 100f
		}, 2.5f);
		Destroy();
	}

	[ClientRpc]
	private void RpcExplodeBeam()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ai_Curse_AnchorOfMind_Anchor::RpcExplodeBeam()", -158064382, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcExplodeBeam()
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			if (explodeBeam != null)
			{
				explodeBeam.enabled = true;
			}
			yield return new WaitForSeconds(0.1f);
			if (explodeBeam != null)
			{
				explodeBeam.enabled = false;
			}
		}
	}

	protected static void InvokeUserCode_RpcExplodeBeam(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcExplodeBeam called on server.");
		}
		else
		{
			((Ai_Curse_AnchorOfMind_Anchor)(object)obj).UserCode_RpcExplodeBeam();
		}
	}

	static Ai_Curse_AnchorOfMind_Anchor()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_Curse_AnchorOfMind_Anchor), "System.Void Ai_Curse_AnchorOfMind_Anchor::RpcExplodeBeam()", (RemoteCallDelegate)InvokeUserCode_RpcExplodeBeam);
	}
}
