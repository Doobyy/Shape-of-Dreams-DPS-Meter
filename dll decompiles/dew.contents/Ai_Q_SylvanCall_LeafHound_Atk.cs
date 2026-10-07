using UnityEngine;

public class Ai_Q_SylvanCall_LeafHound_Atk : InstantDamageInstance
{
	public float summonerAttackEffect = 0.35f;

	private float _baseSummonerAttackEffect;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseSummonerAttackEffect = summonerAttackEffect;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		summonerAttackEffect = _baseSummonerAttackEffect;
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
