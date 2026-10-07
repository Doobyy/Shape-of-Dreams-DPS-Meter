using Mirror;
using UnityEngine;

public class Se_Gem_E_Obsidian_EmpowerAttack : StatusEffect
{
	public float duration = 5f;

	public ScalingValue damage;

	public ScalingValue reducedCooldown;

	public GameObject fxHit;

	private bool _didReduce;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_didReduce = false;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		SetTimer(duration);
		ShowOnScreenTimer("Gem_E_Obsidian");
		DoAttackEmpower((EventInfoAttackEffect effect, int i) =>
		{
			if (!effect.chain.DidReact(this))
			{
				if (!_didReduce)
				{
					_didReduce = true;
					if ((Object)(object)firstTrigger != null)
					{
						ApplyCooldownReduction(firstTrigger, GetValue(reducedCooldown));
					}
				}
				Damage(damage).SetElemental(ElementalType.Dark).ApplyStrength(effect.strength).Dispatch(effect.victim, effect.chain.New(this));
				FxPlayNewNetworked(fxHit, effect.victim);
				if (!gem.IsNullOrInactive())
				{
					gem.NotifyUse();
				}
			}
		}, 1, DestroyIfActive);
	}

	private void MirrorProcessed()
	{
	}
}
