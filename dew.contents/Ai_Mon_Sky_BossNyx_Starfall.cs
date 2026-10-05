using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Sky_BossNyx_Starfall : AbilityInstance
{
	public int starfallWavaCount;

	public int starfallCount;

	public float starfallDelay;

	public float starfallInterval;

	private int _pristineStarfallWavaCount;

	private int _pristineStarfallCount;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_pristineStarfallWavaCount = starfallWavaCount;
		_pristineStarfallCount = starfallCount;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		starfallWavaCount = _pristineStarfallWavaCount;
		starfallCount = _pristineStarfallCount;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
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
		for (int i = 0; i < starfallWavaCount; i++)
		{
			for (int j = 0; j < starfallCount; j++)
			{
				Vector3 vector;
				if (j < DewPlayer.gamePlayers.Count && !DewPlayer.gamePlayers[j].hero.isKnockedOut)
				{
					vector = DewPlayer.gamePlayers[j].hero.GetAIPosition(info.caster) + Random.insideUnitSphere.Flattened() * 4.5f;
				}
				else
				{
					vector = ((j < starfallCount - DewPlayer.gamePlayers.Count || DewPlayer.gamePlayers[j - starfallCount + DewPlayer.gamePlayers.Count].hero.isKnockedOut) ? (section.GetAnyRandomNode() + Random.insideUnitSphere.Flattened() * 1.5f) : (AbilityTrigger.PredictPoint_Simple(info.caster, Random.Range(0.7f, 1f), DewPlayer.gamePlayers[j - starfallCount + DewPlayer.gamePlayers.Count].hero, starfallDelay) + Random.insideUnitSphere.Flattened() * 4.5f));
				}
				vector = Dew.GetPositionOnGround(vector);
				CreateAbilityInstance<Ai_Mon_Sky_BossNyx_Starfall_Instance>(vector, null, new CastInfo(info.caster, vector));
				yield return new SI.WaitForSeconds(starfallInterval);
			}
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
