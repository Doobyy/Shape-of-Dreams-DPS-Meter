using System.Buffers;
using System.Collections;
using UnityEngine;

public class FxGibs : FxInterpolatedEffectBase
{
	public float minImpulse = 4f;

	public float maxImpulse = 8f;

	public float randomImpulse = 6f;

	public float angularSpeed = 720f;

	[HideInInspector]
	public GibInfo? info;

	private int _childCount;

	private Rigidbody[] _children;

	private Vector3[] _localScales;

	private Vector3[] _localPositions;

	private Quaternion[] _localRotations;

	private Transform[] _parents;

	private void EnableAllChildren()
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			transform.GetChild(i).gameObject.SetActive(value: true);
		}
	}

	private void DisableAllChildren()
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			transform.GetChild(i).gameObject.SetActive(value: false);
		}
	}

	protected override void OnInit()
	{
		base.OnInit();
		Transform transform = base.transform;
		_childCount = 0;
		_children = ArrayPool<Rigidbody>.Shared.Rent(transform.childCount);
		_localScales = ArrayPool<Vector3>.Shared.Rent(transform.childCount);
		_localPositions = ArrayPool<Vector3>.Shared.Rent(transform.childCount);
		_localRotations = ArrayPool<Quaternion>.Shared.Rent(transform.childCount);
		_parents = ArrayPool<Transform>.Shared.Rent(transform.childCount);
		for (int i = 0; i < transform.childCount; i++)
		{
			Rigidbody component = transform.GetChild(i).GetComponent<Rigidbody>();
			if (!((Object)(object)component == null))
			{
				_children[_childCount] = component;
				Transform transform2 = ((Component)(object)component).transform;
				_localScales[_childCount] = transform2.localScale;
				_localPositions[_childCount] = transform2.localPosition;
				_localRotations[_childCount] = transform2.localRotation;
				_parents[_childCount] = transform2.parent;
				_childCount++;
				((Component)(object)component).gameObject.SetActive(value: false);
				Collider[] componentsInChildren = ((Component)(object)component).GetComponentsInChildren<Collider>(true);
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					componentsInChildren[j].enabled = false;
				}
			}
		}
	}

	private void OnDestroy()
	{
		PhysX3DSimulationDriver.Unregister(this);
		if (_children == null)
		{
			return;
		}
		Rigidbody[] children = _children;
		foreach (Rigidbody val in children)
		{
			if ((Object)(object)val != null && ((Component)(object)val).gameObject != null)
			{
				Object.Destroy(((Component)(object)val).gameObject);
			}
		}
		ArrayPool<Rigidbody>.Shared.Return(_children, false);
		ArrayPool<Vector3>.Shared.Return(_localScales, false);
		ArrayPool<Vector3>.Shared.Return(_localPositions, false);
		ArrayPool<Quaternion>.Shared.Return(_localRotations, false);
		ArrayPool<Transform>.Shared.Return(_parents, false);
		_children = null;
		_localScales = null;
		_localPositions = null;
		_localRotations = null;
		_parents = null;
	}

	public override void Play()
	{
		base.Play();
		PhysX3DSimulationDriver.Register(this);
		Vector3 vector = (info.HasValue ? info.Value.normalizedCurrentDamage : Random.insideUnitSphere.normalized);
		float magnitude = vector.magnitude;
		if (magnitude < 0.0001f)
		{
			vector = Vector3.zero;
		}
		else if (magnitude > 0.8f)
		{
			vector = vector.normalized * maxImpulse;
		}
		else
		{
			vector = ((!(magnitude < 0.25f)) ? (Mathf.Lerp(minImpulse, maxImpulse, (magnitude - 0.25f) / 0.55f) * vector.normalized) : (vector.normalized * minImpulse));
		}
		for (int i = 0; i < _childCount; i++)
		{
			Rigidbody val = _children[i];
			if (!((Object)(object)val == null))
			{
				((Component)(object)val).transform.parent = null;
				((Component)(object)val).gameObject.SetActive(value: true);
			}
		}
		StartCoroutine(DelayedForce(vector));
	}

	private IEnumerator DelayedForce(Vector3 baseForce)
	{
		yield return null;
		for (int i = 0; i < _childCount; i++)
		{
			Rigidbody val = _children[i];
			if (!((Object)(object)val == null))
			{
				val.AddForce(baseForce + Random.onUnitSphere * randomImpulse, (ForceMode)1);
				if (info.HasValue)
				{
					val.linearVelocity += info.Value.velocity + info.Value.yVelocity * Vector3.up;
				}
				val.angularVelocity = Random.onUnitSphere * angularSpeed;
			}
		}
	}

	public override void Stop()
	{
		base.Stop();
		PhysX3DSimulationDriver.Unregister(this);
		for (int i = 0; i < _childCount; i++)
		{
			Rigidbody val = _children[i];
			if (!((Object)(object)val == null))
			{
				((Component)(object)val).gameObject.SetActive(value: false);
				((Component)(object)val).transform.parent = _parents[i];
				((Component)(object)val).transform.localScale = _localScales[i];
				((Component)(object)val).transform.localPosition = _localPositions[i];
				((Component)(object)val).transform.localRotation = _localRotations[i];
				val.linearVelocity = Vector3.zero;
				val.angularVelocity = Vector3.zero;
			}
		}
	}

	protected override void ValueSetter(float value)
	{
		for (int i = 0; i < _childCount; i++)
		{
			Rigidbody val = _children[i];
			if (!((Object)(object)val == null) && ((Component)(object)val).gameObject.activeSelf)
			{
				((Component)(object)val).gameObject.SetActive(value > 0.0001f);
				if (_localScales != null)
				{
					((Component)(object)val).transform.localScale = _localScales[i] * value;
				}
			}
		}
	}
}
