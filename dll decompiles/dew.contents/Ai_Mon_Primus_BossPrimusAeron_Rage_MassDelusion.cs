using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Primus_BossPrimusAeron_Rage_MassDelusion : AbilityInstance
{
	public GameObject fxHit;

	public Knockback knockback;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			Entity e = allEntity;
			if (!e.IsNullInactiveDeadOrKnockedOut() && !e.Status.hasDamageImmunity && !e.Status.hasUncollidable && info.caster.CheckEnemyOrNeutral(e))
			{
				FxPlayNewNetworked(fxHit, e);
				knockback.ApplyWithOrigin(info.caster.agentPosition, e);
				((MonoBehaviour)(object)this).StartCoroutine(Routine());
			}
			IEnumerator Routine()
			{
				yield return new WaitForSeconds(0.2f);
				if (!e.IsNullInactiveDeadOrKnockedOut())
				{
					if (e.Status.TryGetStatusEffect<Se_MirageSkin_Delusion_Delusional>(out var effect))
					{
						effect.Destroy();
					}
					CreateStatusEffect(e, (Se_MirageSkin_Delusion_Delusional s) =>
					{
						s.duration = "16";
					}).SetStack(99);
				}
			}
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
