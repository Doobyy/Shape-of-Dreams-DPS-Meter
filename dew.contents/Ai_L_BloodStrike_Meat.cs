using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_L_BloodStrike_Meat : AbilityInstance
{
	public float initDelay = 0.5f;

	public float consumeDistance = 3f;

	public float damageAmp = 0.01f;

	public ScalingValue healAmount;

	public float lifeTime = 10f;

	public GameObject fxConsume;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			yield return new SI.WaitForSeconds(lifeTime);
			Destroy();
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && !(Time.time - creationTime < initDelay) && !info.caster.IsNullInactiveDeadOrKnockedOut() && Vector2.Distance(position.ToXY(), info.caster.position.ToXY()) < consumeDistance)
		{
			FxPlayNetworked(fxConsume, info.caster);
			Heal(healAmount).Dispatch(info.caster);
			if ((Object)(object)firstTrigger != null && firstTrigger is St_L_ButchersStrike st_L_ButchersStrike)
			{
				st_L_ButchersStrike.NetworkcurrentDamageAmp = st_L_ButchersStrike.currentDamageAmp + damageAmp;
			}
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
