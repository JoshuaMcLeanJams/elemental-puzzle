using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Pauser : MonoBehaviour
{
    static public Pauser instance = null;

    [SerializeField]
    private string m_pauseInput = "Pause";

    [SerializeField]
    private Menu m_pauseMenu = null;

    [SerializeField]
    private AudioMixer m_mixer = null;

    [SerializeField]
    private string m_masterVolumeString = "Master Volume";

    [SerializeField]
    private Image m_dimmer = null;

    private float m_prevTimeScale = 1.0f;
    private float m_prevVolume = 0.0f;
    private bool m_paused = false;

    public void Pause() {
        if ( m_paused || ElementManager.instance.IsTransitioning ) return;

        m_paused = true;
        m_prevTimeScale = Time.timeScale;

        if ( m_pauseMenu != null ) 
            m_pauseMenu.ShowTop();
        m_dimmer.gameObject.SetActive( true);

        Time.timeScale = 0.0f;
        if ( m_mixer != null )
            m_mixer.SetFloat( m_masterVolumeString, -80.0f );
    }

    public void Unpause() {
        if ( m_paused == false || ElementManager.instance.IsTransitioning ) return;

        if ( m_pauseMenu != null ) {
            m_pauseMenu.Back();
            if ( m_pauseMenu.IsVisible ) return;
        }

        m_paused = false;
        m_dimmer.gameObject.SetActive( false);

        Time.timeScale = m_prevTimeScale;
        if ( m_mixer != null )
            m_mixer.SetFloat( m_masterVolumeString, m_prevVolume );
    }

    private void Awake() {
        if( instance != null ) {
            Debug.LogErrorFormat( "Duplicate Pauser in {0}. Destroying.", name );
            Destroy( this );
            return;
        }
        instance = this;
    }

    private void Update() {
        if ( Input.GetButtonDown( m_pauseInput ) == false ) return;
        if ( m_paused ) Unpause();
        else Pause();
    }
}
