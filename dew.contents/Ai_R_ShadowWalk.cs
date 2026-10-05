using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_R_ShadowWalk : AbilityInstance
{
	public Dash dash;

	public DewCollider range;

	public int spawnCount;

	public float initDelay;

	public float spawnInterval;

	public ScalingValue empowerChance;

	public int addedSpawnCountByEmpowered;

	public GameObject fxEmpoweredStart;

	private bool _isEmpowered;

	private float _baseSpawnInterval;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseSpawnInterval = spawnInterval;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		spawnInterval = _baseSpawnInterval;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		CreateStatusEffect<Se_R_ShadowWalk_Disappear>(info.caster);
		int decidedSpawnCount = spawnCount;
		_isEmpowered = Random.value * 100f < GetValue(empowerChance);
		if (_isEmpowered)
		{
			FxPlayNetworked(fxEmpoweredStart, info.caster.position, null);
			decidedSpawnCount += addedSpawnCountByEmpowered;
			spawnInterval = _baseSpawnInterval * 0.6f;
		}
		dash.ApplyByDirection(info.caster, info.forward);
		yield return new SI.WaitForSeconds(initDelay);
		for (int i = 0; i < decidedSpawnCount; i++)
		{
			range.transform.position = info.caster.position;
			List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
			{
				sortComparer = CollisionCheckSettings.Random
			});
			if (entities.Count > 0)
			{
				Entity target = entities[Random.Range(0, entities.Count)];
				CreateAbilityInstance(info.caster.position, Quaternion.identity, new CastInfo(info.caster, target), (Ai_R_ShadowWalk_Projectile ai) =>
				{
					ai.isEmpoweredByCrit = _isEmpowered;
				});
			}
			handle.Return();
			yield return new SI.WaitForSeconds(spawnInterval);
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
