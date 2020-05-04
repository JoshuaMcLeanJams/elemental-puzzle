using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class VersionDisplay : MonoBehaviour
{
    private void Start() {
        GetComponent<TextMeshProUGUI>().text = Utility.GameData.VersionString;
    }
}
