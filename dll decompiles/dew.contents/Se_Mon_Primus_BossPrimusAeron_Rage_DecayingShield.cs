using System.Collections;
using Mirror;
using UnityEngine;

public class Se_Mon_Primus_BossPrimusAeron_Rage_DecayingShield : StatusEffect
{
	public float bonusHpPercentage = 50f;

	public float shieldDuration = 90f;

	private ShieldEffect _shield;

	private float _initAmount;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoStatBonus(new StatBonus
			{
				maxHealthPercentage = bonusHpPercentage
			});
			_initAmount = victim.Status.missingHealth;
			_shield = DoShield(0f);
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			for (int i = 0; i < 30; i++)
			{
				_shield.amount = Mathf.Lerp(0f, _initAmount, (float)(i + 1) / 30f);
				yield return new WaitForSeconds(1f / 30f);
			}
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && !victim.Status.hasInvulnerable && _shield.amount > 0f)
		{
			_shield.amount = Mathf.MoveTowards(_shield.amount, 0f, _initAmount / shieldDuration * dt);
		}
	}

	private void MirrorProcessed()
	{
	}
}
