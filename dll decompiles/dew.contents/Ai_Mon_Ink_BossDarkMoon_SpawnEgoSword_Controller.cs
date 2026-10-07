using System.Collections;
using System.Linq;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossDarkMoon_SpawnEgoSword_Controller : AbilityInstance
{
	public float spawnTime;

	public float interval;

	private Ai_Mon_Ink_BossDarkMoon_SpawnEgoSword[] _swords;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer && ((Mon_Ink_BossDarkMoon)info.caster)._isRage)
		{
			DestroyOnDeath(info.caster);
			_swords = ((Mon_Ink_BossDarkMoon)info.caster)._swords.OfType<Ai_Mon_Ink_BossDarkMoon_SpawnEgoSword>().ToArray();
			for (int i = 0; i < _swords.Length; i++)
			{
				_swords[i]._spawnTime = Time.time + spawnTime + interval * (float)i;
				_swords[i].info = new CastInfo(info.caster, info.target);
			}
			Destroy();
		}
		yield break;
	}

	private void MirrorProcessed()
	{
	}
}
