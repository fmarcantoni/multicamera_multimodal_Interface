using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ModeSwitch : MonoBehaviour
{
    public GameObject ManualPanel, VocalPanel, AutoPanel;
    public TMP_Dropdown modeDropdown;
    // Start is called before the first frame update
    void Start()
    {
        int modeIndex = modeDropdown.value;
        ManualPanel.SetActive(true);
        VocalPanel.SetActive(false);
        AutoPanel.SetActive(false);
        modeDropdown.onValueChanged.AddListener(OnModeChanged);
        OnModeChanged(modeIndex);
    }
    // Update is called once per frame
    public void OnModeChanged(int modeIndex)
    {
        Debug.Log("Mode Selected " + modeIndex);
        ManualPanel.SetActive(modeIndex == 0);
        VocalPanel.SetActive(modeIndex == 1);
        AutoPanel.SetActive(modeIndex == 2);
    }
}