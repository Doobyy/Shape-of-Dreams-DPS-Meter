using Mirror;
using UnityEngine;

public class Se_D_HeartOfThePack_Bond : StatusEffect
{
	public GameObject fxActivate;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		SetTimer(10f);
		DoAttackEmpower((EventInfoAttackEffect effect, int i) =>
		{
			if (!effect.chain.DidReact(this) && isActive && effect.type != AttackEffectType.Others)
			{
				CreateAbilityInstance(effect.victim.agentPosition, null, new CastInfo(info.caster), (Ai_D_HeartOfThePack_Explosion ai) =>
				{
					ai.strengthMultiplier = effect.strength;
					ai.chain = effect.chain.New(this);
				});
				FxPlayNetworked(fxActivate, victim);
				Destroy();
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
