using UnityEngine;

[CreateAssetMenu(fileName = "DeclineTexts", menuName = "Dialogue/Decline Texts")]
public class DeclineTextsSO : ScriptableObject
{
    [TextArea(2, 4)]
    [SerializeField] private string[] texts;

    private int _last = -1;

    public string GetRandom()
    {
        if (texts == null || texts.Length == 0) return string.Empty;
        if (texts.Length == 1) return texts[0];

        int index;
        do { index = Random.Range(0, texts.Length); } while (index == _last);

        _last = index;
        return texts[index];
    }
}