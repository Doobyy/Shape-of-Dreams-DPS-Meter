using System.Collections.Generic;

public interface IPrewarmMonsterContributor
{
	void ContributeMonsterPrewarm(Dictionary<Monster, int> counts, int instanceCount);
}
