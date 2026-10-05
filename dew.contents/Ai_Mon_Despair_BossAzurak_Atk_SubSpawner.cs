using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Despair_BossAzurak_Atk_SubSpawner : AbilityInstance
{
	public int count = 8;

	public float interval = 0.35f;

	public float maxAngleDev = 20f;

	public float gap = 3f;

	[NonSerialized]
	public float startAngle;

	[NonSerialized]
	public bool playSounds;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		Vector3 currentPos = position;
		float currentAngle = startAngle;
		for (int i = 0; i < count; i++)
		{
			yield return new SI.WaitForSeconds(interval);
			Hero closestAliveHero = Dew.GetClosestAliveHero(currentPos, fallbackToDead: false, info.caster);
			if ((UnityEngine.Object)(object)closestAliveHero != null)
			{
				float angle = CastInfo.GetAngle(closestAliveHero.GetAIAgentPosition(info.caster).ToXY() - currentPos.ToXY());
				currentAngle = Mathf.MoveTowardsAngle(currentAngle, angle, maxAngleDev);
			}
			currentPos += Quaternion.Euler(0f, currentAngle, 0f) * Vector3.forward * gap;
			currentPos = Dew.GetPositionOnGround(currentPos);
			CreateAbilityInstance(currentPos, null, new CastInfo(info.caster), (Ai_Mon_Despair_BossAzurak_Atk_SubDamage ai) =>
			{
				ai.NetworkplaySounds = playSounds;
			});
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
