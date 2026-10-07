using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_R_DarkGrenade_ShardSpawner : StandardProjectile
{
	public ScalingValue empowerChance;

	public int defaultShardCount = 7;

	public int addedEmpoweredShardCount = 4;

	public GameObject fxEmpoweredCast;

	public GameObject fxEmpoweredExplosion;

	public GameObject fxNormalExplosion;

	public GameObject fxExplosionProjectile;

	[NonSerialized]
	public HashSet<Entity> hitEntities;

	private bool _isEmpowered;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		hitEntities = null;
		_isEmpowered = false;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hitEntities = new HashSet<Entity>();
			_isEmpowered = UnityEngine.Random.value * 100f < GetValue(empowerChance);
			if (_isEmpowered)
			{
				FxPlayNetworked(fxEmpoweredCast, info.caster);
			}
		}
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		Explode();
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		Explode();
	}

	private void Explode()
	{
		int num = defaultShardCount;
		if (_isEmpowered)
		{
			num += addedEmpoweredShardCount;
		}
		if (_isEmpowered)
		{
			FxPlayNetworked(fxEmpoweredExplosion, ((Component)(object)this).transform.position, Quaternion.identity);
		}
		FxPlayNetworked(fxNormalExplosion, ((Component)(object)this).transform.position, Quaternion.identity);
		float num2 = UnityEngine.Random.Range(0f, 360f);
		for (int i = 0; i < num; i++)
		{
			float num3 = Mathf.Lerp(-180f, 180f, (float)i / (float)num);
			float angle = info.angle + num2 + num3;
			Quaternion value = Quaternion.AngleAxis(angle, Vector3.up);
			FxPlayNewNetworked(fxExplosionProjectile, info.point + Vector3.up * 0.2f, value);
			CreateAbilityInstance(position, null, new CastInfo(info.caster, angle), (Ai_R_DarkGrenade_Shard b) =>
			{
				b.hitEntities = hitEntities;
				b.spawnPos = position;
				b.isEmpowered = _isEmpowered;
			});
		}
		DestroyIfActive();
	}

	private void MirrorProcessed()
	{
	}
}
