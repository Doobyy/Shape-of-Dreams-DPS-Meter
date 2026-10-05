using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Primus_BossPrimusAeron_Rage_DashAttack : InstantDamageInstance
{
	public Dash dash;

	public ScalingValue secondHitDamage;

	private List<Entity> _hitEntities = new List<Entity>();

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			dash.ApplyByDirection(info.caster, info.forward);
			ResetCooldown(info.caster.Ability.attackAbility);
			CreateBasicEffect(info.caster, new UnstoppableEffect(), 0.9f);
		}
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		_hitEntities.Add(entity);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_hitEntities.Clear();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && _hitEntities.Count > 0 && !info.caster.IsNullInactiveDeadOrKnockedOut())
		{
			for (int i = 0; i < _hitEntities.Count; i++)
			{
				Entity e = _hitEntities[i];
				((MonoBehaviour)(object)this).StartCoroutine(Routine());
				IEnumerator Routine()
				{
					yield return new WaitForSeconds(0.05f);
					e.Visual.KnockUp(1.5f, isFriendly: false);
					e.Control.StartDisplacement(new DispByDestination
					{
						destination = info.caster.agentPosition + (info.caster.position - e.agentPosition).normalized * Random.Range(-1f, -0.5f),
						isFriendly = false,
						rotateForward = false,
						duration = 0.6f,
						ease = DewEase.EaseOutQuart,
						canGoOverTerrain = true,
						isCanceledByCC = false
					});
					yield return new WaitForSeconds(0.1f);
					Damage(secondHitDamage).SetDirection(info.forward).Dispatch(e);
				}
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
