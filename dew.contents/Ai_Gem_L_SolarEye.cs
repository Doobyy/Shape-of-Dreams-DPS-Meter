using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Gem_L_SolarEye : AbilityInstance
{
	public float interval;

	public ScalingValue dmgFactor;

	public float procCoefficient;

	public GameObject fxHit;

	public GameObject fxInstance;

	[HideInInspector]
	public int totalStack;

	[HideInInspector]
	public int maxTickCount;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		ClientActorEvent_OnDestroyed = null;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		Entity e = info.target;
		FxPlayNetworked(fxInstance, e);
		int stackPerTick = totalStack / maxTickCount;
		int exceededStack = totalStack % maxTickCount;
		int count = 0;
		while (count < maxTickCount && !e.IsNullInactiveDeadOrKnockedOut())
		{
			int overrideElementalStacks = stackPerTick;
			if (exceededStack > 0)
			{
				overrideElementalStacks = stackPerTick + 1;
				exceededStack--;
			}
			FxPlayNetworked(fxHit, e);
			Damage(dmgFactor, procCoefficient).SetElemental(ElementalType.Fire).SetOverrideElementalStacks(overrideElementalStacks).Dispatch(e);
			count++;
			if (count == maxTickCount - 3)
			{
				FxStopNetworked(fxInstance);
			}
			yield return new SI.WaitForSeconds(interval);
		}
		FxStopNetworked(fxInstance);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
