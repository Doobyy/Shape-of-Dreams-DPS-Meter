using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_QR_Innocence_Spawner : AbilityInstance
{
	public DewCollider range;

	public ScalingValue countRaw;

	[NonSerialized]
	public int spawnCountOffset;

	public int clampedCount => Mathf.Min(Mathf.RoundToInt(GetValue(countRaw)), 8);

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		spawnCountOffset = 0;
		ClientActorEvent_OnDestroyed = null;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		Hero_Bismuth hero = FindFirstAncestorOfType<Hero_Bismuth>();
		int num = clampedCount + spawnCountOffset;
		int b;
		if (DewPlayer.gamePlayers.Count >= 3)
		{
			b = 10;
		}
		else
		{
			b = ((DewPlayer.gamePlayers.Count < 2) ? 30 : 20);
		}
		int coopClampedCount = Mathf.Min(num, b);
		coopClampedCount = Mathf.RoundToInt((float)coopClampedCount * (1f - ManagerBase<GraphicsManager>.instance.perfPressureStrength));
		float strengthMultiplier = (float)num / (float)coopClampedCount;
		List<Entity> targets = Hero_Bismuth.GetTargetEntities(out var handle, info.caster, canBeNeutral: true, range.radius);
		Vector3 targetPoint = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, info.caster.owner.cursorWorldPos);
		try
		{
			for (int i = 0; i < coopClampedCount; i++)
			{
				if (targets.Count > 0)
				{
					Entity entity = targets[i % targets.Count];
					if (!entity.IsNullInactiveDeadOrKnockedOut())
					{
						targetPoint = entity.agentPosition;
					}
				}
				if ((UnityEngine.Object)(object)hero != null)
				{
					hero.book.RpcBookCast(Quaternion.LookRotation(targetPoint - hero.book.bookTransform.position) * Quaternion.Euler(-40f, 0f, 0f));
					hero.SpendAttack();
				}
				Vector3 positionOnGround = Dew.GetPositionOnGround(targetPoint + UnityEngine.Random.insideUnitSphere * 2f);
				CreateAbilityInstance(info.caster.agentPosition, null, new CastInfo(info.caster, positionOnGround), (Ai_QR_Innocence_Projectile ai) =>
				{
					ai.strengthMultiplier = strengthMultiplier;
				});
				yield return new SI.WaitForSeconds(0.4f / (float)coopClampedCount);
			}
		}
		finally
		{
			handle.Return();
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
