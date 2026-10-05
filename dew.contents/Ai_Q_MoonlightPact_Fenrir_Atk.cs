using UnityEngine;

public class Ai_Q_MoonlightPact_Fenrir_Atk : InstantDamageInstance
{
	public float summonerAttackEffect = 0.5f;

	private ScalingValue _baseDmgFactor;

	private float _baseStartEffectScaleX = 1f;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseDmgFactor = dmgFactor;
		if (startEffectNoStop != null)
		{
			_baseStartEffectScaleX = startEffectNoStop.transform.localScale.x;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		dmgFactor = _baseDmgFactor;
	}

	protected override void OnCreate()
	{
		if (startEffectNoStop != null)
		{
			bool flag = DewAnimationClip.GetEntryIndex(info.animSelectValue, 2) == 1;
			startEffectNoStop.transform.localScale = startEffectNoStop.transform.localScale.WithX(flag ? (0f - _baseStartEffectScaleX) : _baseStartEffectScaleX);
		}
		base.OnCreate();
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		if (!(summonerAttackEffect < 0.001f))
		{
			Hero hero = FindFirstOfType<Hero>();
			if (!((Object)(object)hero == null))
			{
				TriggerAttackEffects(hero, entity, summonerAttackEffect, AttackEffectType.Others);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
