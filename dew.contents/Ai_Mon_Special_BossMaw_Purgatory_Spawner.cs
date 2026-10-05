using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossMaw_Purgatory_Spawner : AbilityInstance
{
	public int spawnCount;

	public float spawnInterval;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			int count = spawnCount + Mathf.CeilToInt((float)Dew.GetAliveHeroCount() * 0.5f) + Mathf.RoundToInt(NetworkedManagerBase<GameManager>.instance.GetSpecialSkillChanceMultiplier() * 2f);
			Vector3 center = SingletonBehaviour<Room_BossArena>.instance.center;
			Vector2 range = new Vector2
			{
				x = SingletonBehaviour<Room_BossArena>.instance.radius * 0.25f,
				y = SingletonBehaviour<Room_BossArena>.instance.radius * 0.8f
			};
			for (int i = 0; i < count; i++)
			{
				float y = 360f / (float)count * (float)i;
				Vector3 vector = center + Quaternion.Euler(0f, y, 0f) * Vector3.forward * range.RandomRange();
				vector = Dew.GetPositionOnGround(vector);
				CreateAbilityInstance<Ai_Mon_Special_BossMaw_Purgatory_Trap>(vector, null, new CastInfo(info.caster, vector));
				yield return new SI.WaitForSeconds(spawnInterval);
			}
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
