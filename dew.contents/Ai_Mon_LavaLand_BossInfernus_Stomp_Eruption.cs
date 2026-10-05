using UnityEngine;

public class Ai_Mon_LavaLand_BossInfernus_Stomp_Eruption : InstantDamageInstance
{
	private class Ad_EruptionHit
	{
		public float lastHitTime;
	}

	public float hitCooldownTime = 1.5f;

	public override bool reuseInRoom => true;

	public override bool reuseInRoomSkipPrewarmCap => true;

	protected override bool OnValidateTarget(Entity entity)
	{
		if (!entity.TryGetData<Ad_EruptionHit>(out var data))
		{
			data = new Ad_EruptionHit
			{
				lastHitTime = Time.time
			};
			entity.AddData(data);
			return true;
		}
		if (Time.time - data.lastHitTime > hitCooldownTime)
		{
			data.lastHitTime = Time.time;
			return true;
		}
		return false;
	}

	private void MirrorProcessed()
	{
	}
}
