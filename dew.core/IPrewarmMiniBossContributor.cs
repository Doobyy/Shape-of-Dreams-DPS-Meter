using System.Collections.Generic;

public interface IPrewarmMiniBossContributor
{
	void ContributeMiniBossPrewarm(Dictionary<Monster, int> counts, int instanceCount);
}
