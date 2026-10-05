using System.Collections;
using UnityEngine;

public class Ai_Mon_Special_BossPolaris_Monster_JumpStomp_LargerExplosion : InstantDamageInstance
{
	public float stunDuration = 1f;

	public int starfallCount = 20;

	public float targetedChance = 0.2f;

	protected override void OnAfterDelay()
	{
		base.OnAfterDelay();
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			int alivePlayers = 0;
			foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
			{
				if (!gamePlayer.hero.IsNullInactiveDeadOrKnockedOut())
				{
					alivePlayers++;
				}
			}
			LockDestroy();
			for (int i = 0; i < starfallCount; i++)
			{
				_ = Vector3.zero;
				Vector3 end;
				if (Random.value < targetedChance * (float)alivePlayers)
				{
					Vector3 aIAgentPosition = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true).GetAIAgentPosition(info.caster);
					end = aIAgentPosition + (Random.onUnitSphere * 5.5f).Flattened();
					end = Dew.GetValidAgentDestination_LinearSweep(aIAgentPosition, end);
				}
				else
				{
					end = SingletonBehaviour<Room_BossArena>.instance.GetRandomPathablePosition();
				}
				CreateAbilityInstance<Ai_Mon_Special_BossPolaris_Monster_JumpStomp_StarfallInstance>(end, null, new CastInfo(info.caster));
				yield return new WaitForSeconds(0.05f);
			}
			UnlockDestroy();
		}
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		CreateBasicEffect(entity, new StunEffect(), stunDuration);
	}

	private void MirrorProcessed()
	{
	}
}
