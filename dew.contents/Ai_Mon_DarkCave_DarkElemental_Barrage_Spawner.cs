using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_DarkCave_DarkElemental_Barrage_Spawner : AbilityInstance
{
	public DewCollider targetRange;

	public int shootCount;

	public float maxDeviation;

	public Vector2 landTime;

	public float interval;

	public float minRangeFromSelf = 2.5f;

	private int _baseShootCount;

	private float _baseMaxDeviation;

	private float _baseInterval;

	private bool _cachedBase;

	protected override void Awake()
	{
		base.Awake();
		if (!_cachedBase)
		{
			_baseShootCount = shootCount;
			_baseMaxDeviation = maxDeviation;
			_baseInterval = interval;
			_cachedBase = true;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		shootCount = _baseShootCount;
		maxDeviation = _baseMaxDeviation;
		interval = _baseInterval;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		targetRange.transform.position = info.target.position;
		Entity[] ents = targetRange.GetEntities(out var handle, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
		{
			includeUncollidable = true,
			sortComparer = CollisionCheckSettings.Random
		}).ToArray();
		handle.Return();
		if (ents.Length == 0)
		{
			Destroy();
			yield break;
		}
		Vector3[] lastKnownPositions = new Vector3[ents.Length];
		for (int i = 0; i < shootCount; i++)
		{
			int num = Random.Range(0, ents.Length);
			Entity entity = ents[num];
			if ((Object)(object)entity != null)
			{
				lastKnownPositions[num] = entity.position;
			}
			Vector3 pos = lastKnownPositions[num] + Random.onUnitSphere.Flattened().normalized * (Random.value * maxDeviation);
			Vector3 vector = (pos - info.caster.position).Flattened();
			if (vector.sqrMagnitude < minRangeFromSelf * minRangeFromSelf)
			{
				vector = vector.normalized * minRangeFromSelf;
				pos = info.caster.position + vector;
			}
			pos = Dew.GetPositionOnGround(pos);
			CreateAbilityInstance(info.caster.position, Quaternion.identity, new CastInfo(info.caster, pos), (Ai_Mon_DarkCave_DarkElemental_Barrage_Arrow a) =>
			{
				a.initialSpeed = Vector3.Distance(info.caster.position, pos) / Random.Range(landTime.x, landTime.y);
				a.targetSpeed = a.initialSpeed;
				a.acceleration = 0f;
			});
			yield return new SI.WaitForSeconds(interval);
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
