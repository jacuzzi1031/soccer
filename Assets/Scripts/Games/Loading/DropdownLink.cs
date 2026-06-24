using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class DropdownLink : MonoBehaviour
{
    public TMP_Dropdown targetDropdown;

    public void UpdateOptions(int index)
    {
        targetDropdown.ClearOptions();

        if (index == 1)
        {
            targetDropdown.AddOptions(new List<string> { "1" });
        }
        else
        {
            targetDropdown.AddOptions(new List<string> { "2", "1" });
        }

        targetDropdown.RefreshShownValue();
    }
}