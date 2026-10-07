public struct ResourceLoadSettings
{
	public VariantDef varDef;

	public bool loadLight;

	public static ResourceLoadSettings Light => new ResourceLoadSettings
	{
		loadLight = true
	};
}
