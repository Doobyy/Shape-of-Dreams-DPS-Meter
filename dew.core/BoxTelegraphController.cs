using System;
using System.Collections;
using UnityEngine;

public class BoxTelegraphController : MonoBehaviour, IEffectComponent, IEffectWithSpeed
{
	private static readonly EaseFunction OuterSizeEase = EasingFunction.GetEasingFunction(DewEase.EaseOutExpo);

	private static readonly EaseFunction InnerSizeEase = EasingFunction.GetEasingFunction(DewEase.EaseOutQuad);

	private static readonly int Alpha = Shader.PropertyToID("_Alpha");

	public float duration = 1f;

	public BoxTelegraphAnimType animateX;

	public DewEase animateXEase;

	public BoxTelegraphAnimType animateY;

	public DewEase animateYEase;

	public float width = 4f;

	public float height = 20f;

	public SpriteRenderer[] whites;

	public SpriteRenderer[] reds;

	public SpriteRenderer[] outers;

	public SpriteRenderer[] inners;

	private MaterialPropertyBlock[] _whitePropertyBlocks;

	private MaterialPropertyBlock[] _redPropertyBlocks;

	private float _value;

	private Transform _tf;

	private bool _appliedIsPlaying;

	private bool _hasAppliedIsPlaying;

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

	public bool isPlaying => value > 0.0001f;

	public void Play()
	{
		StopAllCoroutines();
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			value = 0f;
			UpdateProperties();
			yield return null;
			for (float t = 0f; t < duration; t += Time.deltaTime)
			{
				value = t / duration;
				yield return null;
			}
			value = 1f;
			Stop();
		}
	}

	public void Stop()
	{
		StopAllCoroutines();
		value = 0f;
	}

	private void Awake()
	{
		_tf = transform;
		_whitePropertyBlocks = new MaterialPropertyBlock[whites.Length];
		for (int i = 0; i < whites.Length; i++)
		{
			SpriteRenderer spriteRenderer = whites[i];
			if (!(spriteRenderer == null))
			{
				spriteRenderer.gameObject.SetActive(value: true);
				_whitePropertyBlocks[i] = new MaterialPropertyBlock();
			}
		}
		_redPropertyBlocks = new MaterialPropertyBlock[reds.Length];
		for (int j = 0; j < reds.Length; j++)
		{
			SpriteRenderer spriteRenderer2 = reds[j];
			if (!(spriteRenderer2 == null))
			{
				spriteRenderer2.gameObject.SetActive(value: true);
				_redPropertyBlocks[j] = new MaterialPropertyBlock();
			}
		}
		UpdateProperties();
	}

	private void UpdateProperties()
	{
		bool flag = isPlaying;
		SpriteRenderer[] array;
		if (!_hasAppliedIsPlaying || _appliedIsPlaying != flag)
		{
			_hasAppliedIsPlaying = true;
			_appliedIsPlaying = flag;
			array = whites;
			foreach (SpriteRenderer spriteRenderer in array)
			{
				if (!(spriteRenderer == null))
				{
					spriteRenderer.gameObject.SetActive(flag);
				}
			}
			array = reds;
			foreach (SpriteRenderer spriteRenderer2 in array)
			{
				if (!(spriteRenderer2 == null))
				{
					spriteRenderer2.gameObject.SetActive(flag);
				}
			}
		}
		if (!flag)
		{
			return;
		}
		Vector2 vector = new Vector2(width, height);
		vector /= _tf.localScale.x;
		float num = OuterSizeEase(0f, 1f, Mathf.Clamp01(value * 1.5f));
		Vector2 size = vector;
		switch (animateX)
		{
		case BoxTelegraphAnimType.Center:
			size.x *= num;
			break;
		case BoxTelegraphAnimType.FromStart:
		{
			array = outers;
			for (int i = 0; i < array.Length; i++)
			{
				Transform transform2 = array[i].transform;
				transform2.localPosition = new Vector3((0f - width) * 0.5f * (1f - num) / transform2.lossyScale.x * transform2.localScale.x, 0.1f, 0f);
			}
			size.x *= num;
			break;
		}
		case BoxTelegraphAnimType.FromEnd:
		{
			array = outers;
			for (int i = 0; i < array.Length; i++)
			{
				Transform transform = array[i].transform;
				transform.localPosition = new Vector3(width * 0.5f * (1f - num) / transform.lossyScale.x * transform.localScale.x, 0.1f, 0f);
			}
			size.x *= num;
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		case BoxTelegraphAnimType.None:
			break;
		}
		switch (animateY)
		{
		case BoxTelegraphAnimType.Center:
			size.y *= num;
			break;
		case BoxTelegraphAnimType.FromStart:
		{
			array = outers;
			for (int i = 0; i < array.Length; i++)
			{
				Transform transform4 = array[i].transform;
				transform4.localPosition = new Vector3(0f, 0.1f, (0f - height) * 0.5f * (1f - num) / transform4.lossyScale.z * transform4.localScale.x);
			}
			size.y *= num;
			break;
		}
		case BoxTelegraphAnimType.FromEnd:
		{
			array = outers;
			for (int i = 0; i < array.Length; i++)
			{
				Transform transform3 = array[i].transform;
				transform3.localPosition = new Vector3(0f, 0.1f, height * 0.5f * (1f - num) / transform3.lossyScale.z * transform3.localScale.x);
			}
			size.y *= num;
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		case BoxTelegraphAnimType.None:
			break;
		}
		array = outers;
		foreach (SpriteRenderer spriteRenderer3 in array)
		{
			if (!(spriteRenderer3 == null))
			{
				spriteRenderer3.size = size;
			}
		}
		float num2 = 0.3f;
		float num3 = InnerSizeEase(0f, 1f, Mathf.Clamp01(value * (1f + num2) - num2));
		Vector2 size2 = vector;
		switch (animateX)
		{
		case BoxTelegraphAnimType.Center:
			size2.x *= num3;
			break;
		case BoxTelegraphAnimType.FromStart:
		{
			array = inners;
			for (int i = 0; i < array.Length; i++)
			{
				Transform transform6 = array[i].transform;
				transform6.localPosition = new Vector3((0f - width) * 0.5f * (1f - num3) / transform6.lossyScale.x * transform6.localScale.x, 0.1f, 0f);
			}
			size2.x *= num3;
			break;
		}
		case BoxTelegraphAnimType.FromEnd:
		{
			array = inners;
			for (int i = 0; i < array.Length; i++)
			{
				Transform transform5 = array[i].transform;
				transform5.localPosition = new Vector3(width * 0.5f * (1f - num3) / transform5.lossyScale.x * transform5.localScale.x, 0.1f, 0f);
			}
			size2.x *= num3;
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		case BoxTelegraphAnimType.None:
			break;
		}
		switch (animateY)
		{
		case BoxTelegraphAnimType.Center:
			size2.y *= num3;
			break;
		case BoxTelegraphAnimType.FromStart:
		{
			array = inners;
			for (int i = 0; i < array.Length; i++)
			{
				Transform transform8 = array[i].transform;
				transform8.localPosition = new Vector3(0f, 0.1f, (0f - height) * 0.5f * (1f - num3) / transform8.lossyScale.z * transform8.localScale.x);
			}
			size2.y *= num3;
			break;
		}
		case BoxTelegraphAnimType.FromEnd:
		{
			array = inners;
			for (int i = 0; i < array.Length; i++)
			{
				Transform transform7 = array[i].transform;
				transform7.localPosition = new Vector3(0f, 0.1f, height * 0.5f * (1f - num3) / transform7.lossyScale.z * transform7.localScale.x);
			}
			size2.y *= num3;
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		case BoxTelegraphAnimType.None:
			break;
		}
		array = inners;
		foreach (SpriteRenderer spriteRenderer4 in array)
		{
			if (!(spriteRenderer4 == null))
			{
				spriteRenderer4.size = size2;
			}
		}
		float num4 = 0.125f + 0.15f * Mathf.Clamp01(value * 3f) + value * 0.2f;
		for (int j = 0; j < reds.Length; j++)
		{
			SpriteRenderer spriteRenderer5 = reds[j];
			if (!(spriteRenderer5 == null))
			{
				ref MaterialPropertyBlock reference = ref _redPropertyBlocks[j];
				MaterialPropertyBlock materialPropertyBlock = reference ?? (reference = new MaterialPropertyBlock());
				materialPropertyBlock.SetFloat(Alpha, num4);
				spriteRenderer5.SetPropertyBlock(materialPropertyBlock);
			}
		}
		float num5 = value * 0.35f;
		for (int k = 0; k < whites.Length; k++)
		{
			SpriteRenderer spriteRenderer6 = whites[k];
			if (!(spriteRenderer6 == null))
			{
				ref MaterialPropertyBlock reference = ref _whitePropertyBlocks[k];
				MaterialPropertyBlock materialPropertyBlock2 = reference ?? (reference = new MaterialPropertyBlock());
				materialPropertyBlock2.SetFloat(Alpha, num5);
				spriteRenderer6.SetPropertyBlock(materialPropertyBlock2);
			}
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.white;
		float num = 1f / transform.localScale.x;
		Vector3 position = new Vector3((0f - width) * 0.5f, 0f, height * 0.5f) * num;
		Vector3 position2 = new Vector3(width * 0.5f, 0f, height * 0.5f) * num;
		Vector3 position3 = new Vector3(width * 0.5f, 0f, (0f - height) * 0.5f) * num;
		Vector3 position4 = new Vector3((0f - width) * 0.5f, 0f, (0f - height) * 0.5f) * num;
		position = transform.TransformPoint(position);
		position2 = transform.TransformPoint(position2);
		position3 = transform.TransformPoint(position3);
		position4 = transform.TransformPoint(position4);
		Gizmos.DrawLine(position, position2);
		Gizmos.DrawLine(position2, position3);
		Gizmos.DrawLine(position3, position4);
		Gizmos.DrawLine(position4, position);
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
