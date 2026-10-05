using System.Collections;
using Mirror;

public class Ai_Mon_Special_BossObliviax_DashAtk_Spawner : AbilityInstance
{
	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		for (int i = 0; i < 3; i++)
		{
			Hero closestAliveHero = Dew.GetClosestAliveHero(info.caster.agentPosition, fallbackToDead: true, info.caster);
			if (closestAliveHero.IsNullInactiveDeadOrKnockedOut())
			{
				break;
			}
			float speed = 1f + (float)i * 0.3f;
			Ai_Mon_Special_BossObliviax_DashAtk ins = CreateAbilityInstance(info.caster.agentPosition, null, new CastInfo(info.caster, AbilityTrigger.PredictAngle_Simple(info.caster, NetworkedManagerBase<GameManager>.instance.GetPredictionStrength(), closestAliveHero, info.caster.agentPosition, speed)), (Ai_Mon_Special_BossObliviax_DashAtk d) =>
			{
				d.NetworktelegraphDuration = d.telegraphDuration / speed;
			});
			while (!ins.IsNullOrInactive())
			{
				yield return null;
			}
			if (info.caster.normalizedHealth > 1f - (float)(i + 1) * 0.3333f)
			{
				break;
			}
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
