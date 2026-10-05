using UnityEngine;

public class EntityFootsteps : MonoBehaviour
{
	private EntitySound _sound;

	private void Start()
	{
		_sound = GetComponentInParent<EntitySound>();
	}

	public void DoFootstep(AnimationEvent e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		AnimatorClipInfo animatorClipInfo = e.animatorClipInfo;
		if (!(animatorClipInfo.weight < 0.3f) && !((Object)(object)_sound == null))
		{
			_sound.DoFootstep();
		}
	}
}
