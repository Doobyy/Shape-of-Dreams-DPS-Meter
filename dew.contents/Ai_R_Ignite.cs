using Mirror;
using UnityEngine;

public class Ai_R_Ignite : AbilityInstance
{
	public DewCollider range;

	public ScalingValue dmgFactor;

	public float explodeDmgPercentage;

	public GameObject igniteEffect;

	public GameObject explodeEffect;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		CreateAbilityInstance<Ai_R_Ignite_FakeProjectile>(info.caster.agentPosition, null, new CastInfo(info.caster, info.target));
		DestroyOnDeath(info.caster);
		Entity target = info.target;
		if (target.Status.fireStack > 0)
		{
			range.transform.position = target.agentPosition;
			FxPlayNetworked(explodeEffect, target.agentPosition, null);
			ListReturnHandle<Entity> handle;
			foreach (Entity entity in range.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
			{
				FxPlayNewNetworked(igniteEffect, entity);
				Damage(dmgFactor).ApplyRawMultiplier(1f + explodeDmgPercentage).SetElemental(ElementalType.Fire).SetAttr(DamageAttribute.IsCrit)
					.SetOriginPosition(target.agentPosition)
					.Dispatch(entity);
			}
			handle.Return();
			Destroy();
		}
		else
		{
			FxPlayNetworked(igniteEffect, target);
			Damage(dmgFactor).SetElemental(ElementalType.Fire).SetOriginPosition(info.caster.agentPosition).Dispatch(target);
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
