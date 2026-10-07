using System;
using Mirror;

public class Mon_Sky_BigBaam_Regular : Mon_Sky_BigBaam_Base, ISpawnableAsMiniBoss
{
	public void OnBeforeSpawnAsMiniBoss()
	{
	}

	public void OnCreateAsMiniBoss()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		ISpawnableAsMiniBoss.GiveGenericMiniBossBonus(this);
		Ability.GetAbility<At_Mon_Sky_BigBaam_BeamAtk>().spawnAdditionalProjectile = true;
		ActorEvent_OnAbilityInstanceBeforePrepare += (Action<EventInfoAbilityInstance>)((EventInfoAbilityInstance instance) =>
		{
			if (instance.instance is Ai_Mon_Sky_BigBaam_BeamAtk { dealtDamageProcessor: var dataProcessorGroup })
			{
				dataProcessorGroup.Add(delegate(ref DamageData data, Actor actor, Entity target)
				{
					data.ApplyRawMultiplier(0.6f);
				});
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
