using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpdateWidthAccordingToTextLength : MonoBehaviour
{
	public TMP_Text text;

	public DewNumberLabel numberLabel;

	public LayoutElement layoutElement;

	public int InitialSize;

	public bool stickWithMaxLength;

	public float normalizedWidthChangeEpsilon = 0.5f;

	private float _lastWidth = float.NegativeInfinity;

	private float _maxWidth;

	private string _lastTmpText;

	private float _tmpWidth;

	private void Update()
	{
		float num = MeasureWidth();
		if (stickWithMaxLength)
		{
			if (num > _maxWidth)
			{
				_maxWidth = num;
			}
			num = _maxWidth;
		}
		float num2 = normalizedWidthChangeEpsilon * CharacterWidth();
		if (num != _lastWidth && Mathf.Abs(num - _lastWidth) >= num2)
		{
			layoutElement.preferredWidth = num + (float)InitialSize;
			_lastWidth = num;
		}
	}

	private float MeasureWidth()
	{
		if ((Object)(object)numberLabel != null)
		{
			return numberLabel.PreferredWidth;
		}
		if ((Object)(object)this.text != null)
		{
			string text = this.text.text;
			if (text != _lastTmpText)
			{
				_lastTmpText = text;
				_tmpWidth = this.text.preferredWidth;
			}
			return _tmpWidth;
		}
		return 0f;
	}

	private float CharacterWidth()
	{
		if ((Object)(object)numberLabel != null)
		{
			return numberLabel.CharacterWidth;
		}
		if ((Object)(object)text != null)
		{
			return text.fontSize * 0.5f;
		}
		return 0f;
	}
}
