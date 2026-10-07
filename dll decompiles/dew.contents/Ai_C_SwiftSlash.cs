using Mirror;
using UnityEngine;

public class Ai_C_SwiftSlash : DashAttackInstance
{
	public float cooldownReductionRatio;

	public Transform knifeTransform;

	private bool _didHit;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_didHit = false;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (knifeTransform != null)
		{
			knifeTransform.rotation = info.caster.rotation;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			CreateBasicEffect(info.caster, new UnstoppableEffect(), dash.duration);
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (knifeTransform != null)
		{
			knifeTransform.rotation = info.caster.rotation;
		}
	}

	protected override void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
		base.OnBeforeDispatchDamage(ref dmg, target);
		if (!((Object)(object)firstTrigger == null) && !_didHit)
		{
			_didHit = true;
			ApplyCooldownReductionByRatio(firstTrigger, cooldownReductionRatio);
		}
	}

	private void MirrorProcessed()
	{
	}
}
