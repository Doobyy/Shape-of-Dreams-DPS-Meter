using Febucci.UI.Core;
using UnityEngine;

public class FontTest_JapaneseCrashRepro : MonoBehaviour
{
	private void Start()
	{
		TypewriterCore componentInChildren = GetComponentInChildren<TypewriterCore>();
		componentInChildren.ShowText("");
		componentInChildren.StartShowingText(false);
		componentInChildren.ShowText("…不吉な予感がします。");
	}
}
