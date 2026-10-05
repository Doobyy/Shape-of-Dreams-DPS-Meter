public abstract class BasicEffect
{
	public abstract BasicEffectMask mask { get; }

	public Entity victim { get; internal set; }

	public StatusEffect parent { get; internal set; }

	public bool isAlive { get; internal set; }

	internal BasicEffect()
	{
	}
}
