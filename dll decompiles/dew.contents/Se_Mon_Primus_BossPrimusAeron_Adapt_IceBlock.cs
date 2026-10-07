using System.Collections;
using Mirror;

public class Se_Mon_Primus_BossPrimusAeron_Adapt_IceBlock : StatusEffect
{
	public float hpRatio = 0.1f;

	public float duration;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DoShield(victim.maxHealth * hpRatio, (EventInfoDamageNegatedByShield obj) =>
		{
			if (obj.shield.amount < 0.001f)
			{
				DestroyIfActive();
			}
		});
		SetTimer(duration);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !victim.IsNullInactiveDeadOrKnockedOut())
		{
			CreateAbilityInstance<Ai_Mon_Primus_BossPrimusAeron_Adapt_IceBlock_Damage>(victim.agentPosition, null, new CastInfo(victim));
		}
	}

	private void MirrorProcessed()
	{
	}
}
