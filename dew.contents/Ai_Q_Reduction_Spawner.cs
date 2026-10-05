using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Q_Reduction_Spawner : AbilityInstance
{
	public int shootCount = 3;

	public float shootInterval = 0.2f;

	public float sacrificeHpRatio = 0.1f;

	private int _baseShootCount;

	private float _baseSacrificeHpRatio;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseShootCount = shootCount;
		_baseSacrificeHpRatio = sacrificeHpRatio;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		shootCount = _baseShootCount;
		sacrificeHpRatio = _baseSacrificeHpRatio;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		info.caster.Control.StartChannel(new Channel
		{
			blockedActions = Channel.BlockedAction.Attack,
			duration = (float)shootCount * shootInterval
		});
		PureDamage(Mathf.Min(info.caster.Status.maxHealth * sacrificeHpRatio, info.caster.Status.currentHealth - 1f)).SetAttr(DamageAttribute.IgnoreArmor).SetAttr(DamageAttribute.IgnoreShield).SetAttr(DamageAttribute.IgnoreDamageImmunity)
			.Dispatch(info.caster);
		for (int i = 0; i < shootCount; i++)
		{
			CreateAbilityInstance(position, null, new CastInfo(info.caster, info.angle), (Ai_Q_Reduction_Projectile ai) =>
			{
				ai.SetCustomStartPosition(info.caster.Visual.GetCenterPosition() + Random.insideUnitSphere * 0.5f);
			});
			yield return new SI.WaitForSeconds(shootInterval);
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
