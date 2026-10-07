using UnityEngine;

public class Ai_Mon_LavaLand_BossInfernus_Atk : InstantDamageInstance
{
	private Vector3 _startEffectPristineScale;

	private bool _didCacheStartEffectScale;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		if (startEffect != null)
		{
			_startEffectPristineScale = startEffect.transform.localScale;
			_didCacheStartEffectScale = true;
		}
	}

	protected override void OnCreate()
	{
		if (startEffect != null && DewAnimationClip.GetEntryIndex(info.animSelectValue, 2) == 1)
		{
			Vector3 localScale = startEffect.transform.localScale;
			localScale.x *= -1f;
			startEffect.transform.localScale = localScale;
		}
		base.OnCreate();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (_didCacheStartEffectScale && startEffect != null)
		{
			startEffect.transform.localScale = _startEffectPristineScale;
		}
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		Quaternion value = info.rotation;
		CreateAbilityInstance<Ai_Mon_LavaLand_BossInfernus_WallStunKnockback>(entity.position, value, new CastInfo(info.caster, entity));
	}

	private void MirrorProcessed()
	{
	}
}
