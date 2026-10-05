using UnityEngine;

public class Ai_R_FrozenFists_Attack : MeleeAttackInstance
{
	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (info.animSelectValue < 0.5f)
		{
			Vector3 localScale = startEffectNoStop.transform.localScale;
			localScale.x *= -1f;
			startEffectNoStop.transform.localScale = localScale;
		}
	}

	private void MirrorProcessed()
	{
	}
}
