using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_DarkCave_BossSeeker_YellowDiagram_Spawner : AbilityInstance
{
	public float startDelay;

	public int spawnCount = 5;

	public int spawnAngle = 72;

	public GameObject fxCast;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		FxPlayNetworked(fxCast, info.point, null);
		yield return new SI.WaitForSeconds(startDelay);
		int num = Random.Range(0, 360);
		int i;
		for (i = 0; i < spawnCount; i++)
		{
			int num2 = num + spawnAngle * i;
			CreateAbilityInstance(info.point, Quaternion.Euler(0f, num2, 0f), new CastInfo(info.caster, num2), (Ai_Mon_DarkCave_BossSeeker_YellowDiagram_MainInstance b) =>
			{
				if (i == 0)
				{
					b.isFirstInstance = true;
				}
			});
		}
		yield return new SI.WaitForSeconds(1f);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
