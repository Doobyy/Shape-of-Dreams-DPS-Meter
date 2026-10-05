using UnityEngine;

public class Ai_Q_EtherealInfluence_Projectile_Forward : Ai_Q_EtherealInfluence_Projectile_Base
{
	protected override void OnComplete()
	{
		base.OnComplete();
		CreateAbilityInstance<Ai_Q_EtherealInfluence_Projectile_Backward>(position, Quaternion.identity, new CastInfo(info.caster, info.caster));
	}

	private void MirrorProcessed()
	{
	}
}
