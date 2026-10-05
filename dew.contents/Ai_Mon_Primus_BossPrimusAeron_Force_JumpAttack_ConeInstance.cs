using UnityEngine;

public class Ai_Mon_Primus_BossPrimusAeron_Force_JumpAttack_ConeInstance : InstantDamageInstance
{
	private float _originalAngle;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		_originalAngle = rotation.eulerAngles.y;
		base.OnCreate();
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		float value = Mathf.Clamp01((Time.time - creationTime) / damageDelay);
		value = EasingFunction.EaseOutQuad(0f, 1f, value);
		rotation = Quaternion.Euler(0f, _originalAngle + 90f * value, 0f);
	}

	private void MirrorProcessed()
	{
	}
}
