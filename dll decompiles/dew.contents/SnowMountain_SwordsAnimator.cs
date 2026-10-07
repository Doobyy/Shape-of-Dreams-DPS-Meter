using UnityEngine;

public class SnowMountain_SwordsAnimator : MonoBehaviour
{
	private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

	public float amplitude;

	public float frequency;

	public float timeOffset;

	private Transform[] _swordsList;

	private Material[] _swordsMeshList;

	private float _startYPos;

	private Vector3[] _initPositions;

	private void Start()
	{
		int childCount = transform.childCount;
		_swordsList = new Transform[childCount];
		_swordsMeshList = new Material[childCount];
		_initPositions = new Vector3[childCount];
		for (int i = 0; i < childCount; i++)
		{
			Transform child = transform.GetChild(i);
			_swordsMeshList[i] = child.GetComponent<MeshRenderer>().material;
			_swordsList[i] = child;
			_initPositions[i] = child.position;
		}
	}

	private void Update()
	{
		float time = Time.time;
		for (int i = 0; i < _swordsList.Length; i++)
		{
			Transform obj = _swordsList[i];
			Vector3 position = _initPositions[i];
			position.y += amplitude * Mathf.Sin(frequency * time + timeOffset * (float)i);
			obj.position = position;
			Material obj2 = _swordsMeshList[i];
			float num = 0.1f;
			num += Mathf.Abs(Mathf.Sin((time + timeOffset * (float)i) / 2f) * 0.05f);
			obj2.SetColor(value: new Color(9f, 93f, 191f) * num, nameID: EmissionColor);
		}
	}
}
