using UnityEngine;

public class UI_CreditsView_Particle : MonoBehaviour
{
	private ParticleSystem _ps;

	private void Awake()
	{
		_ps = GetComponent<ParticleSystem>();
	}

	private void Update()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		Rect screenSpaceRect = ((RectTransform)transform.parent).GetScreenSpaceRect();
		ShapeModule shape = _ps.shape;
		shape.scale = new Vector3(screenSpaceRect.width / 10f, 1f, screenSpaceRect.height / 10f);
	}
}
