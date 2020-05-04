using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    [SerializeField]
    List<AudioClip> m_musicList = new List<AudioClip>();

    private AudioSource m_musicSource = null;

    private void Awake() {
        m_musicSource = GetComponent<AudioSource>();
    }

    private void Start() {
        if ( m_musicList.Count == 0 ) {
            Debug.LogError( "No music! Games need music. Please add music. :D" );
            return;
        }

        var musicName = PlayerPrefs.GetString( "music", "" );
        foreach( var music in m_musicList) {
            if( music.name == musicName ) {
                PlayMusic( music );
                return;
            }
        }

        PlayMusic( m_musicList[0] );
    }

    private void PlayMusic( AudioClip a_clip ) {
        m_musicSource.time = 0.0f;
        m_musicSource.clip = a_clip;
        m_musicSource.Play();
    }
}
