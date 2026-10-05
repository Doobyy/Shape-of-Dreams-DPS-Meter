using System.Collections;
using Mirror;
using UnityEngine;

public class Se_Mon_Despair_ParalyticFly_Atk_Instance : StatusEffect
{
	public float slowAmount = 40f;

	public float slowDuration = 2f;

	public bool isDecay;

	public ScalingValue tickDmgFactor;

	public float tickDuration;

	private float _time;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_time = 0f;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DoSlow(slowAmount).decay = isDecay;
			SetTimer(slowDuration);
		}
		yield break;
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && !victim.IsNullInactiveDeadOrKnockedOut() && !(Time.time - _time <= tickDuration))
		{
			CreateDamage(DamageData.SourceType.Default, tickDmgFactor).SetOriginPosition(info.caster.agentPosition).SetAttr(DamageAttribute.DamageOverTime).Dispatch(victim);
			_time = Time.time;
		}
	}

	private void MirrorProcessed()
	{
	}
}
