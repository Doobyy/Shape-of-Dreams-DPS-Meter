using UnityEngine;

public class Ai_Mon_LavaLand_BossInfernus_Swipe : InstantDamageInstance
{
	public override bool reuseInRoom => true;

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		Quaternion value = Quaternion.LookRotation(entity.agentPosition - info.caster.agentPosition).Flattened();
		CreateAbilityInstance<Ai_Mon_LavaLand_BossInfernus_WallStunKnockback>(entity.position, value, new CastInfo(info.caster, entity));
	}

	private void MirrorProcessed()
	{
	}
}
