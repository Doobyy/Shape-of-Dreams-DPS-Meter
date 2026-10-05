using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossPolaris_Monster_Scream : AbilityInstance
{
	public GameObject fxHit;

	public float slowAmount = 100f;

	public float slowDuration = 3f;

	public bool isSlowDecay = true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			if (!allEntity.IsNullInactiveDeadOrKnockedOut() && !allEntity.Status.hasUncollidable && !allEntity.Status.hasUntargetable && !((Object)(object)allEntity == (Object)(object)info.caster))
			{
				CreateStatusEffect<Se_Mon_Special_BossPolaris_Monster_Scream_SkillLocked>(allEntity, new CastInfo(info.caster));
				CreateBasicEffect(allEntity, new SlowEffect
				{
					decay = isSlowDecay,
					strength = slowAmount
				}, slowDuration);
				FxPlayNewNetworked(fxHit, allEntity);
			}
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
