using UnityEngine;

public class St_QR_ValiantHeart : SkillTrigger
{
	protected override Vector3 GetInstanceSpawnPosition(int configIndex, CastInfo info)
	{
		if (info.caster is Hero_Bismuth hero_Bismuth)
		{
			if (hero_Bismuth.book.targetPosClamped.HasValue)
			{
				return hero_Bismuth.book.targetPosClamped.Value.WithY(hero_Bismuth.Visual.GetMuzzlePosition().y);
			}
			return hero_Bismuth.Visual.GetMuzzlePosition();
		}
		return base.GetInstanceSpawnPosition(configIndex, info);
	}

	private void MirrorProcessed()
	{
	}
}
