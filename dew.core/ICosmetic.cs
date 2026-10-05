public interface ICosmetic
{
	string name { get; }

	bool generatedFromServer { get; }

	string[] dlcIds { get; }
}
