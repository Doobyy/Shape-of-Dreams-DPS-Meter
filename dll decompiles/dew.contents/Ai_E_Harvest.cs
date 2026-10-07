using Mirror;

public class Ai_E_Harvest : AbilityInstance
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		Actor[] array = parentActor.children.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] is Se_E_Harvest_OnVictim se_E_Harvest_OnVictim)
			{
				se_E_Harvest_OnVictim.Explode(this);
			}
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
