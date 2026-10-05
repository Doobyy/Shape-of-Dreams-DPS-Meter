using UnityEngine;

public class Ai_Mon_Forest_BossDemon_MainSkill_Stomp_Tree : InstantDamageInstance
{
	public Knockback knockback;

	public override bool reuseInRoom => true;

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		knockback.ApplyWithOrigin(((Component)(object)this).transform.position, entity);
	}

	private void MirrorProcessed()
	{
	}
}
