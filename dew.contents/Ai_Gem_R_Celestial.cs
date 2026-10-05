using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Gem_R_Celestial : AbilityInstance
{
	public ScalingValue starDamage;

	public float delay;

	public float procCoefficient = 0.5f;

	public GameObject hitEffect;

	public DewCollider range;

	public GameObject explodeEffect;

	public bool followTarget;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		position = info.target.Visual.GetBasePosition();
		if (((NetworkBehaviour)this).isServer)
		{
			yield return new SI.WaitForSeconds(delay);
			Damage(starDamage, procCoefficient).SetOriginPosition(position).SetElemental(ElementalType.Light).Dispatch(info.target, chain);
			FxPlayNetworked(explodeEffect);
			FxPlayNewNetworked(hitEffect, info.target);
			Destroy();
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if ((Object)(object)info.target != null && followTarget)
		{
			position = info.target.Visual.GetBasePosition();
		}
	}

	private void MirrorProcessed()
	{
	}
}
