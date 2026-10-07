using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossErebos_AtkSpawner : AbilityInstance
{
	public int atkCount;

	public float atkInterval;

	public float curveRadius;

	public float arcAngle;

	private List<Vector3> spawnPoints;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			spawnPoints = new List<Vector3>(atkCount);
			Vector3 normalized = (info.point - info.caster.agentPosition).normalized;
			Vector3 vector = info.point - normalized * curveRadius;
			float num = arcAngle / 2f;
			float num2 = (0f - arcAngle) / (float)atkCount;
			Vector3 vector2 = info.point - vector;
			for (int i = 0; i < atkCount; i++)
			{
				Vector3 vector3 = Quaternion.AngleAxis(num + (float)i * num2, Vector3.up) * vector2;
				Vector3 item = vector + vector3;
				spawnPoints.Add(item);
			}
			for (int j = 0; j < spawnPoints.Count; j++)
			{
				Vector3 point = spawnPoints[j];
				CreateAbilityInstance<Ai_Mon_Special_BossErebos_AtkInstance>(point, null, new CastInfo(info.caster, point));
				yield return new SI.WaitForSeconds(atkInterval);
			}
			spawnPoints.Clear();
		}
	}

	private void MirrorProcessed()
	{
	}
}
