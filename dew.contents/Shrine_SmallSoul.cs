public class Shrine_SmallSoul : Shrine
{
	protected override bool OnUse(Entity entity)
	{
		if (entity.Status.TryGetStatusEffect<Se_Shrine_SmallSoul>(out var effect))
		{
			effect.AddStack();
		}
		else
		{
			entity.CreateStatusEffect<Se_Shrine_SmallSoul>(entity, new CastInfo(entity));
		}
		return true;
	}

	private void MirrorProcessed()
	{
	}
}
