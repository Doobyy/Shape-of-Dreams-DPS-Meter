using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossDarkMoon_Blade_Instance : AbilityInstance
{
	public DewCollider range;

	public ScalingValue dmgFactor;

	public Knockback Knockback;

	public float postDelay;

	public GameObject fxHit;

	[Space(15f)]
	public float rageSpawnChance;

	public float ragePostDelay;

	internal bool _isRage;

	private float _basePostDelay;

	protected override void Awake()
	{
		base.Awake();
		_basePostDelay = postDelay;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		postDelay = _basePostDelay;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		if (_isRage)
		{
			postDelay = ragePostDelay;
		}
		range.transform.position = ((Component)(object)info.caster).transform.position;
		range.transform.rotation = ((Component)(object)info.caster).transform.rotation;
		List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < entities.Count; i++)
		{
			Entity entity = entities[i];
			CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(info.caster.position).Dispatch(entity);
			Knockback.ApplyWithOrigin(info.caster.position, entity);
			FxPlayNewNetworked(fxHit, entity);
		}
		handle.Return();
		info.caster.Control.StartDaze(postDelay);
		if (_isRage && Random.value <= rageSpawnChance)
		{
			Entity entity2 = info.target;
			if (entity2.IsNullInactiveDeadOrKnockedOut())
			{
				entity2 = Dew.GetClosestAliveHero(info.caster.position, fallbackToDead: true, info.caster);
			}
			Vector3 aIAgentPosition = entity2.GetAIAgentPosition(info.caster);
			Vector3 vector = Quaternion.AngleAxis(Random.Range(-45, 45), Vector3.up) * ((Component)(object)info.caster).transform.forward;
			Vector3 vector2 = aIAgentPosition + vector * 3f;
			vector2 = Dew.GetPositionOnGround(vector2);
			SpawnEntity<Mon_Ink_BossDarkMoonHallucination>(vector2, Quaternion.LookRotation(aIAgentPosition - vector2), DewPlayer.creep, info.caster.level);
		}
		yield return new SI.WaitForSeconds(postDelay);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
