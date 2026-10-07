using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Primus_BossPrimusAeron_Adapt_Starfall : AbilityInstance
{
	public float interval = 0.15f;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnCondition(() => ((Mon_Primus_BossPrimusAeron)info.caster).phase != Mon_Primus_BossPrimusAeron.PhaseType.Adapt);
			int count = 4 + DewPlayer.gamePlayers.Count * 3;
			for (int i = 0; i < count; i++)
			{
				Hero hero = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true);
				CreateAbilityInstance<Ai_Mon_Primus_BossPrimusAeron_Adapt_Starfall_Instance>(hero.GetAIAgentPosition(info.caster) + Random.insideUnitCircle.ToXZ() * 6.5f, null, new CastInfo(info.caster, hero));
				yield return new SI.WaitForSeconds(interval);
			}
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
