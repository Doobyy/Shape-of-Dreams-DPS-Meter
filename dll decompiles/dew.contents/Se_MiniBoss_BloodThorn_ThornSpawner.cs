using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_MiniBoss_BloodThorn_ThornSpawner : StatusEffect
{
	public float maxSlow = 25f;

	public float delay = 1.5f;

	public int minSpawnCount = 3;

	public Vector2 randomSpawnRange = new Vector2(3f, 10f);

	public float postDelay = 1f;

	public float pointDistance;

	private SlowEffect _slow;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_slow = null;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(victim);
		victim.Control.StartChannel(new Channel
		{
			blockedActions = (Channel.BlockedAction.Ability | Channel.BlockedAction.Attack),
			duration = delay
		});
		_slow = DoSlow(0f);
		yield return new SI.WaitForSeconds(delay);
		victim.Control.StartDaze(postDelay);
		List<Entity> list = DewPool.GetList(out ListReturnHandle<Entity> handle);
		foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
		{
			if (!allHero.IsNullInactiveDeadOrKnockedOut())
			{
				list.Add(allHero);
			}
		}
		for (int i = 0; i < Mathf.Max(minSpawnCount, list.Count); i++)
		{
			if (i >= list.Count)
			{
				Vector3 end = victim.agentPosition + Random.insideUnitCircle.ToXZ() * Random.Range(randomSpawnRange.x, randomSpawnRange.y);
				end = Dew.GetValidAgentDestination_Closest(victim.agentPosition, end);
				Vector3 vector = end - info.caster.agentPosition;
				Vector3 vector2 = victim.agentPosition + vector.normalized * pointDistance;
				vector2 = Dew.GetPositionOnGround(vector2);
				CreateAbilityInstance(vector2, null, new CastInfo(victim, end), (Ai_MiniBoss_BloodThorn_MainThorn b) =>
				{
					b.isTargetMode = false;
				});
			}
			else
			{
				Entity entity = list[i];
				Vector3 normalized = (entity.agentPosition - victim.agentPosition).normalized;
				Vector3 vector3 = victim.agentPosition + normalized * pointDistance;
				vector3 = Dew.GetPositionOnGround(vector3);
				CreateAbilityInstance<Ai_MiniBoss_BloodThorn_MainThorn>(vector3, null, new CastInfo(info.caster, entity));
			}
		}
		handle.Return();
		Destroy();
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (_slow != null)
		{
			_slow.strength = Mathf.Lerp(0f, maxSlow, dt);
		}
	}

	private void MirrorProcessed()
	{
	}
}
