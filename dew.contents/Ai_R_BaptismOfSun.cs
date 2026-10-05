using System;
using System.Collections;
using Mirror;

public class Ai_R_BaptismOfSun : InstantDamageInstance
{
	private int _hitCountNonBoss;

	private int _hitCountBoss;

	[NonSerialized]
	public bool skipBuff;

	[NonSerialized]
	private ScalingValue _baseDmgFactor;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseDmgFactor = dmgFactor;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_hitCountNonBoss = 0;
		_hitCountBoss = 0;
		skipBuff = false;
		dmgFactor = _baseDmgFactor;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		yield return base.OnCreateSequenced();
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		if (skipBuff)
		{
			if (isActive)
			{
				Destroy();
			}
			yield break;
		}
		Se_R_BaptismOfSun_Buff actor = CreateStatusEffect(info.caster, new CastInfo(info.caster), (Se_R_BaptismOfSun_Buff b) =>
		{
			b.hitCountNonBoss = _hitCountNonBoss;
			b.hitCountBoss = _hitCountBoss;
		});
		DestroyOnDestroy(actor);
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			ResetCooldown(info.caster.Ability.attackAbility);
		}
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		if (entity.IsAnyBoss())
		{
			_hitCountBoss++;
		}
		else
		{
			_hitCountNonBoss++;
		}
		if (entity is Monster)
		{
			entity.Control.Stop();
			entity.AI.Aggro(info.caster);
		}
	}

	private void MirrorProcessed()
	{
	}
}
