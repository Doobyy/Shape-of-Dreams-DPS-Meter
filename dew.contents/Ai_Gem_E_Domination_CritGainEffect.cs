using UnityEngine;

public class Ai_Gem_E_Domination_CritGainEffect : StandardProjectile
{
	public GameObject sfxCaster;

	internal bool enableEntitySound;

	public override bool reuseInRoom => true;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		if (enableEntitySound)
		{
			FxPlayNetworked(sfxCaster, info.caster);
		}
	}

	private void MirrorProcessed()
	{
	}
}
