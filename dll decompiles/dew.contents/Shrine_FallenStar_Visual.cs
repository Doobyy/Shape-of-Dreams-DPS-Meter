using UnityEngine;

public class Shrine_FallenStar_Visual : LogicBehaviour, IEffectComponent
{
	private static readonly int MotionTimeParam = Animator.StringToHash("MotionTime");

	public Animator animator;

	public string stateName = "FallenShrine";

	public RotateTransform[] rotators;

	public Renderer[] starRenderers;

	public Material floatingMaterial;

	public Material staticMaterial;

	public AnimationCurve motionCurve;

	public float duration = 1f;

	private bool _assembled;

	private bool _dimmed;

	private int _stateHash;

	private bool _driving;

	private float _driveElapsed;

	private float _driveFrom;

	private float _driveTo;

	public bool isPlaying => _assembled;

	public bool isLooping => true;

	private void Awake()
	{
		_stateHash = Animator.StringToHash(stateName);
		if ((Object)(object)animator == null)
		{
			animator = GetComponentInChildren<Animator>();
		}
	}

	public void Play()
	{
		SetAssembled(assembled: true, instant: false);
	}

	public void Stop()
	{
		SetAssembled(assembled: false, instant: false);
	}

	public void Snap(bool assembled)
	{
		SetAssembled(assembled, instant: true);
	}

	private void SetAssembled(bool assembled, bool instant)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (!instant && _assembled == assembled)
		{
			return;
		}
		_assembled = assembled;
		((Behaviour)(object)animator).enabled = true;
		float driveFrom = (assembled ? 1f : 0f);
		AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
		if (currentAnimatorStateInfo.shortNameHash == _stateHash)
		{
			driveFrom = Mathf.Clamp01(currentAnimatorStateInfo.normalizedTime);
		}
		_driveTo = (assembled ? 0f : 1f);
		if (!assembled)
		{
			SetMaterial(assembled: false);
			ApplyRotators();
		}
		if (instant)
		{
			_driving = false;
			ApplyClipTime(_driveTo);
			if (assembled)
			{
				EnterAssembledLook(fireEvent: false);
			}
		}
		else
		{
			_driveFrom = driveFrom;
			_driveElapsed = 0f;
			_driving = true;
		}
	}

	public override void FrameUpdate()
	{
		if (!_driving)
		{
			return;
		}
		_driveElapsed += Time.deltaTime;
		float num = ((duration > 0.0001f) ? Mathf.Clamp01(_driveElapsed / duration) : 1f);
		float t = motionCurve.Evaluate(num);
		ApplyClipTime(Mathf.Lerp(_driveFrom, _driveTo, t));
		if (num >= 1f)
		{
			_driving = false;
			if (_assembled)
			{
				EnterAssembledLook(fireEvent: true);
			}
		}
	}

	private void ApplyClipTime(float normalizedClipTime)
	{
		animator.SetFloat(MotionTimeParam, normalizedClipTime);
		animator.Update(0f);
	}

	private void EnterAssembledLook(bool fireEvent)
	{
		((Behaviour)(object)animator).enabled = false;
		SetMaterial(assembled: true);
		ApplyRotators();
	}

	private void ApplyRotators()
	{
		bool flag = _assembled && !_dimmed && !((Behaviour)(object)animator).enabled;
		RotateTransform[] array = rotators;
		foreach (RotateTransform rotateTransform in array)
		{
			if (rotateTransform != null)
			{
				rotateTransform.enabled = flag;
			}
		}
	}

	private void SetMaterial(bool assembled)
	{
		Material material = (assembled ? floatingMaterial : staticMaterial);
		if (material == null)
		{
			return;
		}
		Renderer[] array = starRenderers;
		foreach (Renderer renderer in array)
		{
			if (renderer != null)
			{
				renderer.sharedMaterial = material;
			}
		}
	}
}
