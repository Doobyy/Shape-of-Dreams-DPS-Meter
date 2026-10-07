using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Despair_BossAzurak_DoubleStomp_Instance : AbilityInstance
{
	public float radius;

	public float knockUpStrength;

	public float expirationTime;

	public ScalingValue dmgFactor;

	public Knockback knockback;

	public GameObject fxTelegraph;

	public GameObject fxMain;

	public GameObject fxHit;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			FxPlayNetworked(fxTelegraph, position, null);
			yield return new SI.WaitForSeconds(expirationTime);
			Destroy();
		}
	}

	[Server]
	public void DealDamageRoutine()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Ai_Mon_Despair_BossAzurak_DoubleStomp_Instance::DealDamageRoutine()' called when server was not active");
			return;
		}
		FxStopNetworked(fxTelegraph);
		FxPlayNetworked(fxMain, info.point, null);
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, info.point, radius, tvDefaultHarmfulEffectTargets))
		{
			CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(info.point).Dispatch(item);
			item.Visual.KnockUp(knockUpStrength, isFriendly: false);
			knockback.ApplyWithOrigin(info.point, item);
			FxPlayNewNetworked(fxHit, item);
		}
		handle.Return();
		DestroyIfActive();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxTelegraph);
		}
	}

	private void MirrorProcessed()
	{
	}
}
