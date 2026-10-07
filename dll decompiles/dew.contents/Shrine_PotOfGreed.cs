using System.Collections;
using UnityEngine;

public class Shrine_PotOfGreed : Shrine
{
	public GameObject fxSpawnGold;

	public Vector2Int countRange = new Vector2Int(4, 8);

	public float goldMultiplier = 0.25f;

	protected override bool OnUse(Entity entity)
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		return true;
		IEnumerator Routine()
		{
			Dew.CreateActor<Ge_Shrine_PotOfGreed>();
			int count = Random.Range(countRange.x, countRange.y);
			int amount = DewMath.RandomRoundToInt(NetworkedManagerBase<GameManager>.instance.GetSpecialRewardAmount_Gold() * goldMultiplier * (float)DewPlayer.gamePlayers.Count);
			for (int i = 0; i < count; i++)
			{
				yield return new WaitForSeconds(0.3f);
				FxPlayNewNetworked(fxSpawnGold);
				NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: false, isGivenByOtherPlayer: false, amount, position);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
