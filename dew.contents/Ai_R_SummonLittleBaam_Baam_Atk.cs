using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_R_SummonLittleBaam_Baam_Atk : AbilityInstance
{
	public DewBeamRenderer beamRenderer;

	public ScalingValue dmgFactor;

	public float procCoefficient;

	public GameObject fxHit;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if ((Object)(object)info.target == null)
		{
			if (((NetworkBehaviour)this).isServer)
			{
				Destroy();
			}
			yield break;
		}
		beamRenderer.SetPoints(info.caster.Visual.GetWeaponPosition(), info.target.Visual.GetCenterPosition());
		beamRenderer.enabled = true;
		if (((NetworkBehaviour)this).isServer)
		{
			CreateDamage(DamageData.SourceType.Default, dmgFactor, procCoefficient).SetActor(info.caster).SetOriginPosition(info.caster.agentPosition).SetElemental(ElementalType.Light)
				.DoAttackEffect(AttackEffectType.BasicAttackMain)
				.Dispatch(info.target);
			FxPlayNewNetworked(fxHit, info.target);
			yield return new SI.WaitForSeconds(0.1f);
			Destroy();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (beamRenderer != null)
		{
			beamRenderer.enabled = false;
		}
	}

	private void MirrorProcessed()
	{
	}
}
