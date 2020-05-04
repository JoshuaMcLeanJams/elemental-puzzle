using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Dropdown))]
public class MusicDropdown : MonoBehaviour
{
    private void Start() {
        var musicList = Utility.GameData.MusicList;
        if ( musicList == null ) return;

        var dropdown = GetComponent<Dropdown>();
        dropdown.AddOptions( new List<string>(musicList) );

        var curMusic = PlayerPrefs.GetString( "music", "" );
        if ( curMusic != "" ) {
            for ( int i = 0; i < musicList.Length; ++i ) {
                if ( curMusic == musicList[i] ) {
                    dropdown.value = i;
                    break;
                }
            }
        }

        dropdown.onValueChanged.AddListener( delegate ( int a_index ) {
            PlayerPrefs.SetString( "music", musicList[a_index] );
        } );
    }
}
