//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using UnityEngine;
using TMPro;

public class Carousel : MonoBehaviour
{
    [SerializeField] string[] stringsToDisplay = new string[] { "OptionA", "OptionB", "OptionC" };

    [SerializeField] TextMeshProUGUI text;

    int index = 0;

    void Start()
    {
        text.text = stringsToDisplay[index];
    }

    public void Next()
    {
        IncrementIndex();
        text.text = stringsToDisplay[index];
    }

    public void Previous()
    {
        DecrementIndex();
        text.text = stringsToDisplay[index];
    }

    void DecrementIndex()
    {
        if (stringsToDisplay.Length == 0)
        {
            return;
        }

        if (index == 0) {
            index = stringsToDisplay.Length - 1;
        } else {
            index--;
        }
    }

    void IncrementIndex()
    {
        if (stringsToDisplay.Length == 0)
        {
            return;
        }

        if (index == stringsToDisplay.Length - 1) {
            index = 0;
        } else {
            index++;
        }
    }

    public int GetIndex()
    {
        return index;
    }
}
