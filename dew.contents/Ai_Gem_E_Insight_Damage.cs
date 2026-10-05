using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Gem_E_Insight_Damage : AbilityInstance
{
	public ScalingValue damage;

	public float procCoefficient = 0.75f;

	public GameObject hitEffect;

	internal float _delay;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_delay = 0f;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		yield return new SI.WaitForSeconds(_delay);
		if ((Object)(object)info.caster == null || (Object)(object)info.target == null)
		{
			Destroy();
			yield break;
		}
		Damage(damage, procCoefficient).SetElemental(ElementalType.Light).SetOriginPosition(info.caster.position).Dispatch(info.target, chain);
		if (!info.target.IsNullOrInactive())
		{
			FxPlayNewNetworked(hitEffect, info.target);
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
