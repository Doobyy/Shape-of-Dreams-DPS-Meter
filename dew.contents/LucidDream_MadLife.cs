using UnityEngine;

public class LucidDream_MadLife : LucidDream
{
	public float predictionBase = 0.8f;

	public float predictionRandom = 0.2f;

	protected override void OnCreate()
	{
		base.OnCreate();
		NetworkedManagerBase<GameManager>.instance.predictionStrengthOverride = () => predictionBase + Random.value * predictionRandom;
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		NetworkedManagerBase<GameManager>.instance.predictionStrengthOverride = null;
	}

	private void MirrorProcessed()
	{
	}
}
