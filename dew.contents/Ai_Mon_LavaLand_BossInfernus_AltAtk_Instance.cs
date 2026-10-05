using UnityEngine;

public class Ai_Mon_LavaLand_BossInfernus_AltAtk_Instance : InstantDamageInstance
{
	public float addKnockbackDistance;

	public override bool reuseInRoom => true;

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		Quaternion value = info.rotation;
		CreateAbilityInstance(entity.position, value, new CastInfo(info.caster, entity), (Ai_Mon_LavaLand_BossInfernus_WallStunKnockback b) =>
		{
			b.additionalKnockbackDist = addKnockbackDistance;
			b.useCasterOrigin = true;
		});
	}

	private void MirrorProcessed()
	{
	}
}
