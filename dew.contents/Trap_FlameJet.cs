using Mirror;
using UnityEngine;

public class Trap_FlameJet : Room_Trap_Toggleable
{
	public GameObject fxHit;

	public float startGracePeriod;

	public float dmgMaxHpRatio;

	public float dmgProcCoefficient;

	public float dmgInterval;

	public float monsterDmgMultiplier;

	public DewCollider range;

	private float _lastDamageTick;

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || !isOn || Time.time - _lastDamageTick < dmgInterval || Time.time - startTime < startGracePeriod)
		{
			return;
		}
		_lastDamageTick = Time.time;
		ListReturnHandle<Entity> handle;
		foreach (Entity entity in range.GetEntities(out handle))
		{
			float num = dmgMaxHpRatio * entity.maxHealth;
			if (entity is Monster)
			{
				num *= monsterDmgMultiplier;
			}
			DefaultDamage(num, dmgProcCoefficient).SetElemental(ElementalType.Fire).SetOriginPosition(((Component)(object)this).transform.position).Dispatch(entity);
			FxPlayNewNetworked(fxHit, entity);
		}
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}
}
