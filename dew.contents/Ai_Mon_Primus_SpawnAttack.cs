using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Primus_SpawnAttack : AbilityInstance
{
	public float spawnDuration;

	public DewCollider range;

	public ScalingValue dmgFactor;

	public Knockback knockback;

	public GameObject Fxland;

	public GameObject Fxtelegraph;

	private Vector3 _pristineRangeLocalPos;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_pristineRangeLocalPos = range.transform.localPosition;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			range.transform.position = Dew.GetPositionOnGround(range.transform.position);
			FxPlayNetworked(Fxtelegraph);
			yield return new SI.WaitForSeconds(spawnDuration);
			FxPlayNetworked(Fxland);
			List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
			for (int i = 0; i < entities.Count; i++)
			{
				Entity entity = entities[i];
				entity.Visual.KnockUp(KnockUpStrength.Big, isFriendly: false);
				knockback.ApplyWithOrigin(info.caster.agentPosition, entity);
				CreateDamage(DamageData.SourceType.Default, dmgFactor).Dispatch(entity);
			}
			handle.Return();
			Destroy();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		range.transform.localPosition = _pristineRangeLocalPos;
	}

	private void MirrorProcessed()
	{
	}
}
