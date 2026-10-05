using UnityEngine;

public class FxGibs_Kinematic : FxInterpolatedEffectBase
{
	public float impulse = 7f;

	public float randomImpulse = 2.5f;

	public float upwardBias = 4f;

	public float gravity = 18f;

	public float angularSpeed = 180f;

	public bool useGroundPlane = true;

	public float groundYOffset;

	[Range(0f, 1f)]
	public float bounciness = 0.35f;

	[Range(0f, 1f)]
	public float groundFriction = 0.5f;

	public GameObject hideOnBurst;

	public bool shouldShowAgain;

	private Transform[] _pieces;

	private Renderer[] _renderers;

	private Vector3[] _velocities;

	private Vector3[] _axes;

	private float[] _spins;

	private Vector3[] _localPositions;

	private Quaternion[] _localRotations;

	private Vector3[] _localScales;

	private bool _burst;

	private float _groundY;

	protected override void OnInit()
	{
		base.OnInit();
		isLoop = false;
		Renderer[] array = (_renderers = GetComponentsInChildren<Renderer>(includeInactive: true));
		_pieces = new Transform[array.Length];
		_velocities = new Vector3[array.Length];
		_axes = new Vector3[array.Length];
		_spins = new float[array.Length];
		_localPositions = new Vector3[array.Length];
		_localRotations = new Quaternion[array.Length];
		_localScales = new Vector3[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			Transform transform = array[i].transform;
			_pieces[i] = transform;
			_localPositions[i] = transform.localPosition;
			_localRotations[i] = transform.localRotation;
			_localScales[i] = transform.localScale;
			transform.gameObject.SetActive(value: false);
		}
	}

	public override void Play()
	{
		base.Play();
		_burst = false;
	}

	private void DoBurst()
	{
		_burst = true;
		if (hideOnBurst != null)
		{
			hideOnBurst.SetActive(value: false);
		}
		Vector3 position = base.transform.position;
		for (int i = 0; i < _pieces.Length; i++)
		{
			Transform transform = _pieces[i];
			if (!(transform == null))
			{
				transform.localPosition = _localPositions[i];
				transform.localRotation = _localRotations[i];
				transform.localScale = _localScales[i];
				transform.gameObject.SetActive(value: true);
				Vector3 vector = _renderers[i].bounds.center - position;
				vector = ((vector.sqrMagnitude < 0.0001f) ? Random.onUnitSphere : vector.normalized);
				_velocities[i] = vector * impulse + Vector3.up * upwardBias + Random.insideUnitSphere * randomImpulse;
				_axes[i] = Random.onUnitSphere;
				_spins[i] = angularSpeed * Random.Range(0.7f, 1.3f);
			}
		}
		_groundY = base.transform.position.y + groundYOffset;
	}

	public override void Stop()
	{
		base.Stop();
		if (shouldShowAgain && hideOnBurst != null)
		{
			hideOnBurst.SetActive(value: true);
		}
		_burst = false;
		HidePieces();
	}

	protected override void Update()
	{
		base.Update();
		if (!_burst && currentValue > 0.0001f)
		{
			DoBurst();
		}
		if (!_burst)
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		for (int i = 0; i < _pieces.Length; i++)
		{
			Transform transform = _pieces[i];
			if (transform == null || !transform.gameObject.activeSelf)
			{
				continue;
			}
			_velocities[i] += Vector3.down * (gravity * deltaTime);
			transform.position += _velocities[i] * deltaTime;
			transform.Rotate(_axes[i], _spins[i] * deltaTime, Space.World);
			if (!useGroundPlane)
			{
				continue;
			}
			Bounds bounds = _renderers[i].bounds;
			float num = _groundY - bounds.min.y;
			if (!(num <= 0f) && !(_velocities[i].y > 0f))
			{
				transform.position += Vector3.up * num;
				Vector3 vector = _velocities[i];
				vector.y = (0f - vector.y) * bounciness;
				if (vector.y < 0.4f)
				{
					vector.y = 0f;
				}
				vector.x *= 1f - groundFriction;
				vector.z *= 1f - groundFriction;
				_velocities[i] = vector;
				_spins[i] *= 1f - groundFriction;
			}
		}
		if (!isEmitting && currentValue <= 0.0001f)
		{
			FinishBurst();
		}
	}

	protected override void ValueSetter(float value)
	{
		if (_pieces == null)
		{
			return;
		}
		for (int i = 0; i < _pieces.Length; i++)
		{
			Transform transform = _pieces[i];
			if (!(transform == null) && transform.gameObject.activeSelf)
			{
				transform.localScale = _localScales[i] * value;
			}
		}
	}

	private void FinishBurst()
	{
		_burst = false;
		HidePieces();
	}

	private void HidePieces()
	{
		if (_pieces == null)
		{
			return;
		}
		for (int i = 0; i < _pieces.Length; i++)
		{
			Transform transform = _pieces[i];
			if (!(transform == null))
			{
				transform.gameObject.SetActive(value: false);
				transform.localPosition = _localPositions[i];
				transform.localRotation = _localRotations[i];
				transform.localScale = _localScales[i];
			}
		}
	}
}
