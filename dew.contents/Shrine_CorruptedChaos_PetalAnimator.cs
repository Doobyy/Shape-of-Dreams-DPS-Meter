using UnityEngine;

public class Shrine_CorruptedChaos_PetalAnimator : LogicBehaviour
{
	public float sin0Time;

	public float sin0Offset;

	public float sin0Mag;

	public float sin1Time;

	public float sin1Offset;

	public float sin1Mag;

	private Vector3[] _originalLocalScales;

	private void Start()
	{
		_originalLocalScales = new Vector3[transform.childCount];
		for (int i = 0; i < transform.childCount; i++)
		{
			_originalLocalScales[i] = transform.GetChild(i).localScale;
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		for (int i = 0; i < _originalLocalScales.Length; i++)
		{
			transform.GetChild(i).localScale = _originalLocalScales[i] * (1f + Mathf.Sin(Time.time * sin0Time + (float)i * sin0Offset) * sin0Mag + Mathf.Sin(Time.time * sin1Time + (float)i * sin1Offset) * sin1Mag);
		}
	}
}
