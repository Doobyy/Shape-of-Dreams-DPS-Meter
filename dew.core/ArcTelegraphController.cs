using System;
using System.Collections;
using UnityEngine;

public class ArcTelegraphController : MonoBehaviour, IEffectComponent, IEffectWithSpeed
{
	private static readonly int OuterRadius = Shader.PropertyToID("_OuterRadius");

	private static readonly int InnerRadius = Shader.PropertyToID("_InnerRadius");

	private static readonly int ArcAngle = Shader.PropertyToID("_ArcAngle");

	private static readonly int OutlineWidth = Shader.PropertyToID("_OutlineWidth");

	private static readonly int ColorId = Shader.PropertyToID("_Color");

	private static readonly int MainTexST = Shader.PropertyToID("_MainTex_ST");

	private static readonly int DistortTexST = Shader.PropertyToID("_DistortTex_ST");

	public float duration = 1f;

	public float arcAngle = 360f;

	public float outerRadius;

	public float innerRadius = 3f;

	public MeshRenderer whiteBorder;

	public MeshRenderer fillBorder;

	public MeshRenderer fillArea;

	public MeshRenderer maxBorder;

	private float _value;

	private MeshRenderer[] _renderers;

	private MaterialPropertyBlock[] _propertyBlocks;

	private Vector4[] _mainTexStDefaults;

	private Vector4[] _distortTexStDefaults;

	private Transform _whiteBorderT;

	private Transform _fillBorderT;

	private Transform _fillAreaT;

	private Transform _maxBorderT;

	private float _origDuration = -1f;

	public float value
	{
		get
		{
			return _value;
		}
		set
		{
			if (_value != value)
			{
				_value = value;
				UpdateProperties();
			}
		}
	}

	public bool isPlaying => _value > 0f;

	private void Awake()
	{
		_renderers = new MeshRenderer[4] { whiteBorder, fillBorder, fillArea, maxBorder };
		_whiteBorderT = whiteBorder.transform;
		_fillBorderT = fillBorder.transform;
		_fillAreaT = fillArea.transform;
		_maxBorderT = maxBorder.transform;
		_propertyBlocks = new MaterialPropertyBlock[_renderers.Length];
		_mainTexStDefaults = new Vector4[_renderers.Length];
		_distortTexStDefaults = new Vector4[_renderers.Length];
		for (int i = 0; i < _renderers.Length; i++)
		{
			_propertyBlocks[i] = new MaterialPropertyBlock();
			Material material = ((_renderers[i] != null) ? _renderers[i].sharedMaterial : null);
			_mainTexStDefaults[i] = ((material != null && material.HasProperty(MainTexST)) ? material.GetVector(MainTexST) : new Vector4(1f, 1f, 0f, 0f));
			_distortTexStDefaults[i] = ((material != null && material.HasProperty(DistortTexST)) ? material.GetVector(DistortTexST) : new Vector4(1f, 1f, 0f, 0f));
		}
		UpdateProperties();
	}

	private void OnValidate()
	{
		outerRadius = Mathf.Max(innerRadius, outerRadius, 0f);
		innerRadius = Mathf.Clamp(innerRadius, 0f, outerRadius);
		arcAngle = Mathf.Clamp(arcAngle, 0f, 360f);
	}

	private void UpdateProperties()
	{
		whiteBorder.gameObject.SetActive(isPlaying);
		fillBorder.gameObject.SetActive(isPlaying);
		fillArea.gameObject.SetActive(isPlaying);
		maxBorder.gameObject.SetActive(isPlaying);
		if (isPlaying)
		{
			float num = value;
			float t = EasingFunction.EaseOutQuad(0f, 1f, num);
			SetScaleAll(outerRadius * 2f);
			SetPropertyMax(OuterRadius, 0.5f);
			SetPropertyMax(InnerRadius, innerRadius / outerRadius * 0.5f);
			SetPropertyFill(OuterRadius, Mathf.Lerp(0.5f * innerRadius / outerRadius, 0.5f, t));
			SetPropertyFill(InnerRadius, innerRadius / outerRadius * 0.5f);
			float x = _whiteBorderT.lossyScale.x;
			SetPropertyAll(ArcAngle, arcAngle);
			SetPropertyAll(OutlineWidth, 0.5f / x);
			SetTextureScaleAll(Vector2.one * (x * 0.11f));
			SetColor(3, new Color(0.75f, 0f, 0f, 0.3f + num * 0.15f));
			SetColor(0, new Color(1f, 1f, 1f, num * 0.5f - 0.15f));
			SetColor(1, new Color(1f, 0f, 0f, 0.25f + num * 0.25f));
			SetColor(2, new Color(1f, 0f, 0f, 0.15f + num * 0.35f));
			ApplyPropertyBlocks();
		}
	}

	private void SetPropertyAll(int id, float val)
	{
		for (int i = 0; i < _propertyBlocks.Length; i++)
		{
			_propertyBlocks[i].SetFloat(id, val);
		}
	}

	private void SetTextureScaleAll(Vector2 scale)
	{
		for (int i = 0; i < _propertyBlocks.Length; i++)
		{
			_propertyBlocks[i].SetVector(MainTexST, new Vector4(scale.x, scale.y, _mainTexStDefaults[i].z, _mainTexStDefaults[i].w));
			_propertyBlocks[i].SetVector(DistortTexST, new Vector4(scale.x, scale.y, _distortTexStDefaults[i].z, _distortTexStDefaults[i].w));
		}
	}

	private void SetPropertyFill(int id, float val)
	{
		_propertyBlocks[0].SetFloat(id, val);
		_propertyBlocks[1].SetFloat(id, val);
		_propertyBlocks[2].SetFloat(id, val);
	}

	private void SetPropertyMax(int id, float val)
	{
		_propertyBlocks[3].SetFloat(id, val);
	}

	private void SetColor(int index, Color color)
	{
		_propertyBlocks[index].SetColor(ColorId, color);
	}

	private void ApplyPropertyBlocks()
	{
		for (int i = 0; i < _renderers.Length; i++)
		{
			MeshRenderer meshRenderer = _renderers[i];
			if (!(meshRenderer == null))
			{
				meshRenderer.SetPropertyBlock(_propertyBlocks[i]);
			}
		}
	}

	private void SetScaleAll(float val)
	{
		Vector3 localScale = new Vector3(val, val, val);
		_whiteBorderT.localScale = localScale;
		_fillBorderT.localScale = localScale;
		_fillAreaT.localScale = localScale;
		_maxBorderT.localScale = localScale;
	}

	public void Play()
	{
		StopAllCoroutines();
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			value = 0.001f;
			yield return null;
			for (float t = 0f; t < duration; t += Time.deltaTime)
			{
				value = t / duration;
				yield return null;
			}
			value = 1f;
			yield return null;
			value = 0f;
			Stop();
		}
	}

	public void Stop()
	{
		StopAllCoroutines();
		value = 0f;
	}

	private void OnDrawGizmos()
	{
		_ = transform.position;
		int num = Mathf.CeilToInt(36f * (Mathf.Min(arcAngle, 360f) / 360f));
		Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
		if (arcAngle >= 359f)
		{
			for (int i = 0; i < 36; i++)
			{
				float f = (float)i / 36f * 2f * (float)Math.PI;
				float f2 = (float)(i + 1) / 36f * 2f * (float)Math.PI;
				Vector3 vector = Vector3.zero + new Vector3(Mathf.Sin(f) * outerRadius, 0f, Mathf.Cos(f) * outerRadius);
				Vector3 to = Vector3.zero + new Vector3(Mathf.Sin(f2) * outerRadius, 0f, Mathf.Cos(f2) * outerRadius);
				Vector3 vector2 = Vector3.zero + new Vector3(Mathf.Sin(f) * innerRadius, 0f, Mathf.Cos(f) * innerRadius);
				Vector3 to2 = Vector3.zero + new Vector3(Mathf.Sin(f2) * innerRadius, 0f, Mathf.Cos(f2) * innerRadius);
				Gizmos.DrawLine(vector, to);
				Gizmos.DrawLine(vector2, to2);
			}
		}
		else
		{
			float num2 = (0f - arcAngle) * 0.5f * ((float)Math.PI / 180f);
			float num3 = arcAngle * ((float)Math.PI / 180f) / (float)num;
			for (int j = 0; j <= num; j++)
			{
				float f3 = num2 + (float)j * num3;
				Vector3 to3 = Vector3.zero + new Vector3(Mathf.Sin(f3) * outerRadius, 0f, Mathf.Cos(f3) * outerRadius);
				Vector3 to4 = Vector3.zero + new Vector3(Mathf.Sin(f3) * innerRadius, 0f, Mathf.Cos(f3) * innerRadius);
				if (j > 0)
				{
					float f4 = num2 + (float)(j - 1) * num3;
					Vector3 vector3 = Vector3.zero + new Vector3(Mathf.Sin(f4) * outerRadius, 0f, Mathf.Cos(f4) * outerRadius);
					Vector3 vector4 = Vector3.zero + new Vector3(Mathf.Sin(f4) * innerRadius, 0f, Mathf.Cos(f4) * innerRadius);
					Gizmos.DrawLine(vector3, to3);
					Gizmos.DrawLine(vector4, to4);
				}
			}
			float f5 = num2;
			float f6 = num2 + arcAngle * ((float)Math.PI / 180f);
			Vector3 vector5 = Vector3.zero + new Vector3(Mathf.Sin(f5) * outerRadius, 0f, Mathf.Cos(f5) * outerRadius);
			Vector3 to5 = Vector3.zero + new Vector3(Mathf.Sin(f5) * innerRadius, 0f, Mathf.Cos(f5) * innerRadius);
			Vector3 vector6 = Vector3.zero + new Vector3(Mathf.Sin(f6) * outerRadius, 0f, Mathf.Cos(f6) * outerRadius);
			Vector3 to6 = Vector3.zero + new Vector3(Mathf.Sin(f6) * innerRadius, 0f, Mathf.Cos(f6) * innerRadius);
			Gizmos.DrawLine(vector5, to5);
			Gizmos.DrawLine(vector6, to6);
		}
		Gizmos.matrix = Matrix4x4.identity;
	}

	public void ApplySpeedMultiplier(float speed)
	{
		if (_origDuration < 0f)
		{
			_origDuration = duration;
		}
		duration = _origDuration / speed;
	}
}
