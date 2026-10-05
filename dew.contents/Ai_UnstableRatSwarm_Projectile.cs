using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_UnstableRatSwarm_Projectile : StandardProjectile
{
	public float spawnRatCount;

	public float startHeight;

	public float startRadius;

	public float ratDisplaceDistance;

	public float ratDisplaceDuration;

	public DewEase ease;

	public DewCollider range;

	public ScalingValue dmgByMaxHp;

	public Knockback knockback;

	public GameObject fxTelegraph;

	public GameObject fxHit;

	public GameObject fxSpawn;

	public GameObject fxExplosion;

	private float _telegraphSpeed;

	public override bool reuseInRoom => true;

	protected override void OnPrepare()
	{
		base.OnPrepare();
		targetPosition = ((Component)(object)this).transform.position;
		Vector3 vector = targetPosition + Vector3.up * startHeight + Random.insideUnitCircle.ToXZ().normalized * startRadius;
		SetCustomStartPosition(vector);
		float num = Vector3.Distance(targetPosition, vector) / initialSpeed;
		_telegraphSpeed = 1f / num;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			FxApplySpeedMultiplierNetworked(fxTelegraph, _telegraphSpeed);
			FxPlayNewNetworked(fxTelegraph, targetPosition, Quaternion.identity);
		}
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		FxPlayNetworked(fxExplosion, targetPosition, Quaternion.identity);
		if ((Object)(object)SingletonDewNetworkBehaviour<Room>.instance == null || SingletonDewNetworkBehaviour<Room>.instance.didClearRoom)
		{
			return;
		}
		range.transform.position = targetPosition;
		range.transform.rotation = Quaternion.identity;
		List<Entity> entities = range.GetEntities(out var handle);
		for (int i = 0; i < entities.Count; i++)
		{
			Entity entity = entities[i];
			if (!entity.IsNullInactiveDeadOrKnockedOut() && entity.GetTeamRelation(DewPlayer.creep) != TeamRelation.Ally)
			{
				CreateDamage(DamageData.SourceType.Default, entity.Status.maxHealth * dmgByMaxHp).SetOriginPosition(targetPosition).Dispatch(entity);
				knockback.ApplyWithOrigin(targetPosition, entity);
				FxPlayNewNetworked(fxHit, entity);
			}
		}
		handle.Return();
		int num = Random.Range(0, 360);
		for (int j = 0; (float)j < spawnRatCount; j++)
		{
			Vector3 vector = Quaternion.AngleAxis((float)num + 360f / spawnRatCount * (float)j, Vector3.up) * Vector3.forward;
			Vector3 vector2 = targetPosition + vector * ratDisplaceDistance;
			vector2 = Dew.GetPositionOnGround(vector2);
			Mon_Despair_UnstableRat mon_Despair_UnstableRat = SpawnEntity(targetPosition, Quaternion.Euler(0f, Random.Range(0, 360), 0f), DewPlayer.creep, NetworkedManagerBase<GameManager>.instance.ambientLevel, (Mon_Despair_UnstableRat rat) =>
			{
				rat.disableLoot = true;
			});
			FxPlayNewNetworked(fxSpawn, mon_Despair_UnstableRat);
			mon_Despair_UnstableRat.Control.Rotate(vector, immediately: false, ratDisplaceDuration);
			mon_Despair_UnstableRat.Visual.KnockUp(KnockUpStrength.Big, isFriendly: false);
			mon_Despair_UnstableRat.Control.StartDaze(ratDisplaceDuration);
			mon_Despair_UnstableRat.Control.StartDisplacement(new DispByDestination
			{
				affectedByMovementSpeed = false,
				canGoOverTerrain = false,
				destination = vector2,
				ease = ease,
				duration = ratDisplaceDuration,
				isCanceledByCC = true,
				isFriendly = true
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
