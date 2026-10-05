using TMPro;
using UnityEngine;

[ExecuteAlways]
public class DewLocalizedText : MonoBehaviour, ILangaugeChangedCallback
{
	public string key;

	private TextMeshProUGUI _text;

	private void Awake()
	{
		_text = GetComponent<TextMeshProUGUI>();
	}

	private void Start()
	{
		UpdateText();
	}

	public void UpdateText()
	{
		if (!(Object)(object)_text)
		{
			_text = GetComponent<TextMeshProUGUI>();
		}
		((TMP_Text)_text).text = DewLocalization.GetUIValue(key);
	}

	public void OnLanguageChanged()
	{
		UpdateText();
	}
}
