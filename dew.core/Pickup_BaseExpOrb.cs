using System;

public class Pickup_BaseExpOrb : PickupInstance
{
	[NonSerialized]
	public float amount;

	public override bool reuseInRoom => true;

	protected override void OnPickup(Hero hero)
	{
		base.OnPickup(hero);
		GrantExp(amount);
	}

	public static void GrantExp(float amount)
	{
		int num = DewMath.RandomRoundToInt(amount / (float)DewPlayer.gamePlayers.Count);
		foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
		{
			gamePlayer.hero.ReceiveExperience(num);
		}
	}

	private void MirrorProcessed()
	{
	}
}
