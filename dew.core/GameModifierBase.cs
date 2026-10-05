public class GameModifierBase : GameEffect, IExcludeFromPool
{
	public bool excludeFromPool;

	bool IExcludeFromPool.excludeFromPool => excludeFromPool;

	public virtual void OnStartServerLobby()
	{
	}

	public virtual void OnStartClientLobby()
	{
	}

	private void MirrorProcessed()
	{
	}
}
