using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public void ChangeScene( string a_name ) {
        SceneManager.LoadScene( a_name, LoadSceneMode.Single );
    }
}
