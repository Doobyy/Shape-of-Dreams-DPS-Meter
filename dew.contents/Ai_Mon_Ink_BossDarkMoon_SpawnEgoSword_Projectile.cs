using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossDarkMoon_SpawnEgoSword_Projectile : StandardProjectile
{
	public DewCollider range;

	public ScalingValue dmgFactor;

	public Knockback Knockback;

	public GameObject fxHit;

	public GameObject fxComplete;

	private Quaternion _rotation;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_rotation = Quaternion.LookRotation(info.point - ((Component)(object)this).transform.position);
		}
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		Quaternion value = Quaternion.Euler(0f, rotation.eulerAngles.y, 0f);
		FxPlayNetworked(fxComplete, info.point, value);
		range.transform.position = info.point + _rotation * Vector3.forward * range.transform.localPosition.z;
		List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < entities.Count; i++)
		{
			Entity entity = entities[i];
			CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(info.point).Dispatch(entity);
			Knockback.ApplyWithDirection(((Component)(object)this).transform.forward, entity);
			FxPlayNewNetworked(fxHit, entity);
		}
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}
}
