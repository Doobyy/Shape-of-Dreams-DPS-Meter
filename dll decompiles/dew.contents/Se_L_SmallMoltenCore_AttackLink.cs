using System;
using Mirror;

public class Se_L_SmallMoltenCore_AttackLink : StatusEffect
{
	[NonSerialized]
	public Sum_L_SmallMoltenCore_Dragon dragon;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		DoAttackEmpower((EventInfoAttackEffect effect, int i) =>
		{
			if (!dragon.IsNullOrInactive() && effect.type != AttackEffectType.Others && !effect.victim.IsNullInactiveDeadOrKnockedOut() && !effect.chain.DidReact(this))
			{
				dragon.CreateAbilityInstance(dragon.Visual.GetMuzzlePosition(), null, new CastInfo(dragon, effect.victim), (Ai_L_SmallMoltenCore_Dragon_Atk ai) =>
				{
					ai.empowered = dragon.IsEmpowered;
					if (dragon.IsEmpowered)
					{
						ai.initialSpeed *= 4f;
					}
					ai.chain = effect.chain.New(this);
				});
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
