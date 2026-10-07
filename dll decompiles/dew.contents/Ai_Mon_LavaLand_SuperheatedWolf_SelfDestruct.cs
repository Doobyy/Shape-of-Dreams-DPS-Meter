using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_LavaLand_SuperheatedWolf_SelfDestruct : AbilityInstance
{
	public GameObject fxTelegraph;

	public GameObject fxHit;

	public GameObject fxExplode;

	public GameObject fxDeathOnCaster;

	public DewAnimationClip animStart;

	public DewCollider range;

	public Knockback knockback;

	public float explodeDelay = 3.5f;

	public ScalingValue dmgFactor;

	protected override IEnumerator OnCreateSequenced()
	{
		yield return base.OnCreateSequenced();
		if (((NetworkBehaviour)this).isServer)
		{
			CreateBasicEffect(info.caster, new UnstoppableEffect(), float.PositiveInfinity);
			DestroyOnDeath(info.caster);
			FxPlayNetworked(fxTelegraph, info.caster);
			yield return new SI.WaitForSeconds(explodeDelay - 0.2f);
			info.caster.Control.StartDaze(10f);
			yield return new SI.WaitForSeconds(0.2f);
			info.caster.Visual.DisableRenderers();
			FxPlayNetworked(fxExplode);
			List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
			for (int i = 0; i < entities.Count; i++)
			{
				Entity entity = entities[i];
				CreateDamage(DamageData.SourceType.Default, dmgFactor).SetElemental(ElementalType.Fire).Dispatch(entity);
				knockback.ApplyWithOrigin(info.caster.position, entity);
				FxPlayNewNetworked(fxHit, entity);
			}
			handle.Return();
			FxPlayNetworked(fxDeathOnCaster, info.caster);
			info.caster.Kill();
			if (info.caster is Mon_LavaLand_SuperheatedWolf mon_LavaLand_SuperheatedWolf)
			{
				mon_LavaLand_SuperheatedWolf.didSelfDestruct = true;
			}
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		((Component)(object)this).transform.position = Dew.GetPositionOnGround(info.caster.position);
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
