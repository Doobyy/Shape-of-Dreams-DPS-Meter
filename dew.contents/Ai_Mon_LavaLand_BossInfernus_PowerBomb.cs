using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_LavaLand_BossInfernus_PowerBomb : InstantDamageInstance
{
	public float spawnStartDelay;

	public int spawnCount;

	public float spawnInterval;

	public float postDelay;

	public DewAnimationClip loopClip;

	public DewAnimationClip endClip;

	private int _effectiveSpawnCount;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			info.caster.Control.IncrementBlockCounters(Channel.BlockedAction.Everything);
			CreateBasicEffect(info.caster, new UnstoppableEffect(), float.PositiveInfinity).DestroyOnDestroy(this);
			_effectiveSpawnCount = spawnCount + Mathf.RoundToInt(NetworkedManagerBase<GameManager>.instance.GetSpecialSkillChanceMultiplier() * 3f);
		}
		base.OnCreate();
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		Quaternion value = Quaternion.LookRotation(entity.agentPosition - info.caster.agentPosition).Flattened();
		CreateAbilityInstance(entity.position, value, new CastInfo(info.caster, entity), (Ai_Mon_LavaLand_BossInfernus_WallStunKnockback b) =>
		{
			b.knockbackMaxDist *= 2f;
			b.knockbackSpeed *= 1.5f;
			b.wallStunDmg *= 2f;
		});
	}

	protected override void OnAfterDelay()
	{
		base.OnAfterDelay();
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			info.caster.Animation.PlayAbilityAnimation(loopClip);
			yield return new WaitForSeconds(spawnStartDelay);
			Vector3 center = SingletonBehaviour<Room_BossArena>.instance.center;
			float maxRange = SingletonBehaviour<Room_BossArena>.instance.radius;
			RoomSection section = info.caster.section;
			if (section == null)
			{
				section = SingletonDewNetworkBehaviour<Room>.instance.GetFinalSection();
			}
			if (section == null)
			{
				DestroyIfActive();
			}
			else
			{
				int maxTryChance = 8;
				List<Vector3> positions = new List<Vector3>();
				for (int i = 0; i < _effectiveSpawnCount; i++)
				{
					Vector3 point = section.GetAnyRandomNode();
					for (int j = 0; j < maxTryChance; j++)
					{
						Vector3 anyRandomNode = section.GetAnyRandomNode();
						if (!((anyRandomNode - center).sqrMagnitude >= maxRange * maxRange))
						{
							bool flag = true;
							foreach (Vector3 item in positions)
							{
								if ((item - anyRandomNode).sqrMagnitude <= 6.25f)
								{
									flag = false;
									break;
								}
							}
							if (flag)
							{
								point = anyRandomNode;
								positions.Add(anyRandomNode);
								break;
							}
						}
					}
					CreateAbilityInstance<Ai_Mon_LavaLand_BossInfernus_PowerBomb_FlamePillar>(point, null, new CastInfo(info.caster, point));
					yield return new WaitForSeconds(spawnInterval);
				}
				positions.Clear();
				yield return new WaitForSeconds(postDelay * 0.5f);
				info.caster.Animation.PlayAbilityAnimation(endClip);
				info.caster.Control.StartDaze(postDelay * 0.5f);
				DestroyIfActive();
			}
		}
	}

	protected override void OnDestroyActor()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !info.caster.IsNullOrInactive())
		{
			info.caster.Control.DecrementBlockCounters(Channel.BlockedAction.Everything);
		}
	}

	private void MirrorProcessed()
	{
	}
}
