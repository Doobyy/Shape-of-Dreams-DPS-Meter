using Mirror;
using UnityEngine;

public class Se_MonsterSprint : StatusEffect
{
	public bool isDecay;

	public float duration;

	public float amount;

	public float destroyDistance;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			SetTimer(duration);
			DoSpeed(amount).decay = isDecay;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			Entity targetEnemy = victim.AI.context.targetEnemy;
			if ((Object)(object)targetEnemy != null && Vector2.Distance(victim.agentPosition.ToXY(), targetEnemy.agentPosition.ToXY()) < destroyDistance)
			{
				Destroy();
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
