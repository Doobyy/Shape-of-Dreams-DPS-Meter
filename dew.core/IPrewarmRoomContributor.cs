using System.Collections.Generic;

public interface IPrewarmRoomContributor
{
	void ContributeMonsterPrewarm(Dictionary<Monster, int> counts);
}
