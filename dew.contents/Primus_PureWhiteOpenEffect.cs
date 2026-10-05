using UnityEngine;

public class Primus_PureWhiteOpenEffect : MonoBehaviour
{
	public GameObject burnObject;

	public AnimationCurve xRotationCurve;

	private float _startTime;

	private void OnEnable()
	{
		if ((Object)(object)DewPlayer.local == null || (Object)(object)DewPlayer.local.hero == null)
		{
			return;
		}
		string text = ((object)DewPlayer.local.hero).GetType().Name;
		_startTime = Time.time;
		foreach (Transform item in base.transform)
		{
			if (item.name.StartsWith("Hero_"))
			{
				if (item.name == text)
				{
					item.gameObject.SetActive(value: true);
				}
				else
				{
					item.gameObject.SetActive(value: false);
				}
			}
		}
		DewEffect.Play(gameObject);
	}

	private void LateUpdate()
	{
		if (!((Object)(object)DewPlayer.local == null) && !((Object)(object)DewPlayer.local.hero == null))
		{
			Transform boneTransform = DewPlayer.local.hero.Animation.animator.GetBoneTransform((HumanBodyBones)9);
			Transform transform = ((Component)(object)DewPlayer.local.hero).transform;
			Quaternion rotation = boneTransform.rotation;
			Quaternion quaternion = Quaternion.AngleAxis(0f - xRotationCurve.Evaluate(Time.time - _startTime), transform.right);
			boneTransform.rotation = quaternion * rotation;
		}
	}
}
