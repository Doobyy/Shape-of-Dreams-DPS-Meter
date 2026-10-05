using Mirror;
using UnityEngine;

public class Se_D_ScarOfTheWind_NextAtkCrit : StatusEffect
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			SetTimer(4f);
			DoAttackCritical(DestroyIfActive);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && (Object)(object)firstTrigger != null && normalizedDuration.HasValue)
		{
			firstTrigger.fillAmount = normalizedDuration.Value;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)firstTrigger != null)
		{
			firstTrigger.fillAmount = 0f;
		}
	}

	private void MirrorProcessed()
	{
	}
}
