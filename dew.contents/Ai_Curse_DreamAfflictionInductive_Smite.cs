using Mirror;
using UnityEngine;

public class Ai_Curse_DreamAfflictionInductive_Smite : InstantDamageInstance, IOtherPlayersTonedDownDisable
{
	public float otherPlayerMultiplier = 0.5f;

	public override bool reuseInRoom => true;

	protected override bool ShouldUseDefaultAbilityTargetValidator()
	{
		return false;
	}

	protected override void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
		base.OnBeforeDispatchDamage(ref dmg, target);
		if ((Object)(object)target != (Object)(object)info.caster)
		{
			dmg.ApplyRawMultiplier(otherPlayerMultiplier);
		}
		if (ManagerBase<CameraManager>.instance.isPlayingCutscene)
		{
			dmg.ApplyRawMultiplier(0f);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && Se_Curse_IntermittentExplosion.ShouldBeDestroyed())
		{
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
