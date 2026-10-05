using System.Collections;
using UnityEngine;

public class Shrine_PyranasLove : Shrine
{
	protected override bool OnUse(Entity entity)
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		return true;
		IEnumerator Routine()
		{
			Vector3 pivot = GetRandomSpawnPosition(entity.position);
			yield return new WaitForSeconds(1.3f);
			if (!entity.IsNullInactiveDeadOrKnockedOut())
			{
				Vector3 goodRewardPosition = Dew.GetGoodRewardPosition(pivot);
				NetworkedManagerBase<LootManager>.instance.SelectSkillAndLevel(Rarity.Legendary, out var _, out var level);
				Dew.CreateSkillTrigger<St_L_PyranasFireball>(goodRewardPosition, level, entity.owner);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
