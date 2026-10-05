using UnityEngine;

public class Gem_E_Insensitivity : Gem
{
	protected override void OnCastComplete(EventInfoCast info)
	{
		base.OnCastComplete(info);
		if (owner.isInCombat)
		{
			float acc = 1f;
			if (owner.Status.TryGetStatusEffect<Se_Gem_E_Insensitivity_BombSpawner>(out var effect))
			{
				acc = Mathf.Max(effect.accumulatedDistance, acc);
				effect.Destroy();
			}
			CreateStatusEffectWithSource(info.instance, owner, new CastInfo(owner), (Se_Gem_E_Insensitivity_BombSpawner se) =>
			{
				se.accumulatedDistance = acc;
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
