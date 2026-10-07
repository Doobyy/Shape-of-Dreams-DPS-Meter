using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Sky_BossNyx_Teleport : AbilityInstance
{
	public float delay;

	public float awayDis;

	public float teleportDuration;

	public GameObject teleportEffect;

	private bool _renderersDisabled;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_renderersDisabled = false;
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && _renderersDisabled)
		{
			_renderersDisabled = false;
			if ((Object)(object)info.caster != null)
			{
				info.caster.Visual.EnableRenderers();
			}
		}
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			info.caster.Visual.DisableRenderers();
			_renderersDisabled = true;
			Hero hero = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true);
			Vector3 aIAgentPosition = hero.GetAIAgentPosition(info.caster);
			Vector3 normalized = (aIAgentPosition - info.caster.agentPosition).normalized;
			Vector3 vector = aIAgentPosition + normalized * awayDis;
			vector = Dew.GetPositionOnGround(vector);
			vector = Dew.GetValidAgentDestination_Closest(info.caster.agentPosition, vector);
			Teleport(info.caster, vector);
			info.caster.Control.RotateTowards(hero, immediately: true, 0.5f);
			info.caster.AI.Aggro(hero);
			yield return new SI.WaitForSeconds(teleportDuration);
			FxPlayNetworked(teleportEffect, info.caster);
			info.caster.Visual.EnableRenderers();
			_renderersDisabled = false;
			yield return new SI.WaitForSeconds(delay);
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
