using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_GhostBlade_MeleeAtk : AbilityInstance
{
	public float atkInterval;

	public float damageDelay;

	public int atkCount;

	public DewCollider range;

	public AbilityTargetValidator hittable;

	public ScalingValue dmgFactor;

	public GameObject firstAtkEffect;

	public GameObject secondAtkEffect;

	public GameObject hitEffect;

	private GameObject _atkEffect;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		_atkEffect = firstAtkEffect;
		if (damageDelay > 0f)
		{
			yield return new SI.WaitForSeconds(damageDelay);
		}
		int cnt = 0;
		while (cnt < atkCount)
		{
			cnt++;
			if (cnt % 2 == 0)
			{
				_atkEffect = secondAtkEffect;
			}
			FxPlayNewNetworked(_atkEffect, info.caster);
			List<Entity> entities = range.GetEntities(out var handle, hittable, info.caster);
			for (int i = 0; i < entities.Count; i++)
			{
				Entity entity = entities[i];
				CreateDamage(DamageData.SourceType.Default, dmgFactor).SetElemental(ElementalType.Dark).Dispatch(entity);
				FxPlayNetworked(hitEffect, entity);
			}
			handle.Return();
			yield return new SI.WaitForSeconds(atkInterval);
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
