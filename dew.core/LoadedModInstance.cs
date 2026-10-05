using System.Collections.Generic;
using UnityEngine;

public class LoadedModInstance
{
	public ModItem mod;

	public GameObject container;

	public List<string> registeredCommands = new List<string>();

	public bool isAlteringGameplay;
}
