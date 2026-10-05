using Mirror;
using UnityEngine;

public class St_C_Sneeze : SkillTrigger
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer && !((Object)(object)PlayTutorialManager.instance != null))
		{
			configs[0].postDelay = 0f;
		}
	}

	private void MirrorProcessed()
	{
	}
}
