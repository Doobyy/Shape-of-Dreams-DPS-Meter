using UnityEngine;

public class Ai_Gem_R_Scorched_Meteor : InstantDamageInstance
{
	public Transform fallTransform;

	public float fallSpeed;

	private bool _fallTransformCaptured;

	private Vector3 _fallTransformLocalPosition;

	private Quaternion _fallTransformLocalRotation;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		if (!_fallTransformCaptured)
		{
			_fallTransformLocalPosition = fallTransform.localPosition;
			_fallTransformLocalRotation = fallTransform.localRotation;
			_fallTransformCaptured = true;
		}
		else
		{
			fallTransform.localPosition = _fallTransformLocalPosition;
			fallTransform.localRotation = _fallTransformLocalRotation;
		}
		mainEffect.transform.rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
		base.OnCreate();
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		fallTransform.position += fallTransform.forward * (fallSpeed * Time.deltaTime);
	}

	private void MirrorProcessed()
	{
	}
}
