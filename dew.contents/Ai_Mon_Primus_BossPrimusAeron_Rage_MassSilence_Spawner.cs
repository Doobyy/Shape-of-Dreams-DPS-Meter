using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Primus_BossPrimusAeron_Rage_MassSilence_Spawner : AbilityInstance
{
	public int count;

	public float interval;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			for (int i = 0; i < count; i++)
			{
				Vector3 randomPathablePosition = SingletonBehaviour<Room_BossArena>.instance.GetRandomPathablePosition();
				CreateAbilityInstance<Ai_Mon_Primus_BossPrimusAeron_Rage_MassSilence_Instance>(randomPathablePosition, null, new CastInfo(info.caster));
				yield return new SI.WaitForSeconds(Random.Range(0.5f, 1.5f) * interval);
			}
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
