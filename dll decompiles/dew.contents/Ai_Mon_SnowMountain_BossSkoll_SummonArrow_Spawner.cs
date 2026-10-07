using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_SnowMountain_BossSkoll_SummonArrow_Spawner : AbilityInstance
{
	public int spawnCount;

	public float interval;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		float delay = DewResources.GetByType<Ai_Mon_SnowMountain_BossSkoll_SummonArrow_Instance>(default(ResourceLoadSettings)).delay;
		RoomSection section = info.caster.section;
		if (section == null)
		{
			section = SingletonDewNetworkBehaviour<Room>.instance.GetFinalSection();
		}
		if (section == null)
		{
			Destroy();
			yield break;
		}
		for (int j = 0; j < spawnCount; j++)
		{
			Vector3 vector = ((j >= DewPlayer.gamePlayers.Count || DewPlayer.gamePlayers[j].hero.isKnockedOut) ? (section.GetAnyRandomNode() + Random.insideUnitSphere.Flattened() * 1.5f) : AbilityTrigger.PredictPoint_Simple(info.caster, Random.Range(0.7f, 1f), DewPlayer.gamePlayers[j].hero, delay));
			vector = Dew.GetPositionOnGround(vector);
			CreateAbilityInstance<Ai_Mon_SnowMountain_BossSkoll_SummonArrow_Instance>(vector, Quaternion.Euler(0f, Random.Range(0, 360), 0f), new CastInfo(info.caster, vector));
			yield return new SI.WaitForSeconds(interval);
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
