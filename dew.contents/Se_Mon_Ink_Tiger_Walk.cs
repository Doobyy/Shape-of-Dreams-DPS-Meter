using UnityEngine;

public class Se_Mon_Ink_Tiger_Walk : StatusEffect
{
	public AnimationClip walkAnimation;

	public float walkAnimationSpeed;

	public AnimationClip runAnimation;

	public float runAnimationSpeed;

	public float timeout;

	public float slowAmount;

	public Vector2 distanceRange;

	public bool endOnDamage;

	public bool castPounceAtEnd;

	protected override void OnCreate()
	{
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
	}

	protected override void OnDestroyActor()
	{
	}

	protected override void ActiveLogicUpdate(float dt)
	{
	}

	private void MirrorProcessed()
	{
	}
}
