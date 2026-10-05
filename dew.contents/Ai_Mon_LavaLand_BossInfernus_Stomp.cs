using System;
using System.Collections;
using System.Linq;
using Mirror;
using UnityEngine;

public class Ai_Mon_LavaLand_BossInfernus_Stomp : InstantDamageInstance
{
	[Serializable]
	public struct Pattern
	{
		public float[] eruptAngles;

		public bool useLocalRotation;
	}

	public Pattern[] patterns;

	public float startDistance;

	public int maxSpawnCount = 7;

	public float stepDistance = 1.5f;

	public GameObject fxPerWave;

	public float waveInterval = 0.125f;

	public float stunDuration = 0.75f;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		yield return base.OnCreateSequenced();
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		int num = UnityEngine.Random.Range(0, patterns.Length);
		float[] eruptAngles = patterns[num].eruptAngles;
		bool[] isFinished = new bool[eruptAngles.Length];
		float startAngle = (patterns[num].useLocalRotation ? info.caster.rotation.eulerAngles.y : 0f);
		Vector3 center = Dew.GetPositionOnGround(info.caster.agentPosition);
		for (int wave = 0; wave < maxSpawnCount; wave++)
		{
			for (int i = 0; i < eruptAngles.Length; i++)
			{
				if (!isFinished[i])
				{
					float num2 = startDistance + stepDistance * (float)wave;
					Vector3 vector = center + Quaternion.Euler(0f, startAngle + eruptAngles[i], 0f) * Vector3.forward * num2;
					vector = Dew.GetPositionOnGround(vector);
					if (Mathf.Abs(vector.y - center.y) > 2f)
					{
						isFinished[i] = true;
					}
					else
					{
						CreateAbilityInstance<Ai_Mon_LavaLand_BossInfernus_Stomp_Eruption>(vector, Quaternion.LookRotation(vector - info.caster.agentPosition), new CastInfo(info.caster));
					}
				}
			}
			if (isFinished.All((bool b) => b))
			{
				break;
			}
			FxPlayNewNetworked(fxPerWave);
			yield return new SI.WaitForSeconds(waveInterval);
		}
		Destroy();
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		CreateBasicEffect(entity, new StunEffect(), stunDuration, "infernus_stompstun", DuplicateEffectBehavior.UsePrevious);
	}

	private void MirrorProcessed()
	{
	}
}
