using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_L_Blizzard : AbilityInstance
{
	public ScalingValue waveCount = "3.1 0.5x";

	public float waveInterval = 0.7f;

	public int maxConcurrentCount = 10;

	protected override IEnumerator OnCreateSequenced()
	{
		if ((Object)(object)parentActor != null && (Object)(object)parentActor.parentActor != null)
		{
			List<Ai_L_Blizzard> list = DewPool.GetList(out ListReturnHandle<Ai_L_Blizzard> handle);
			foreach (Actor child in parentActor.parentActor.children)
			{
				if (!(child is St_L_Blizzard))
				{
					continue;
				}
				foreach (Actor child2 in child.children)
				{
					if (child2 is Ai_L_Blizzard item && !list.Contains(item))
					{
						list.Add(item);
					}
				}
			}
			list.Sort((Ai_L_Blizzard a, Ai_L_Blizzard b) => a.creationTime.CompareTo(b.creationTime));
			while (list.Count >= maxConcurrentCount)
			{
				if (!list[0].IsNullOrInactive())
				{
					list[0].Destroy();
				}
				list.RemoveAt(0);
			}
			handle.Return();
		}
		position = Dew.GetPositionOnGround(info.point);
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		int wc = Mathf.RoundToInt(GetValue(waveCount));
		for (int i = 0; i < wc; i++)
		{
			if ((Object)(object)parentActor == null || !(parentActor is SkillTrigger skillTrigger) || skillTrigger.owner.IsNullOrInactive())
			{
				Destroy();
				yield break;
			}
			CreateAbilityInstance<Ai_L_Blizzard_Instance>(position, null, new CastInfo(info.caster));
			if (i != wc - 1)
			{
				yield return new SI.WaitForSeconds(waveInterval);
			}
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
