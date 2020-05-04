using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefsManager : MonoBehaviour
{
    public void ResetPlayerData() {
        PlayerPrefs.DeleteAll();
    }
}
