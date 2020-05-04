using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class MixerGroupSlider : MonoBehaviour
{
    [SerializeField]
    private AudioMixer m_mixer = null;

    [SerializeField]
    private string m_groupName = "";

    [SerializeField]
    private float m_minVolume = -10.0f;

    private Slider m_slider = null;
    private string m_valueName;

    private void Awake() {
        m_slider = GetComponent<Slider>();
    }

    private void Start() {
        m_valueName = string.Format( "{0} Volume", m_groupName );
        m_mixer.GetFloat( m_valueName, out float volume );
        var percent = Mathf.Abs( ( m_minVolume - volume ) / m_minVolume );
        m_slider.value = Mathf.Floor( percent * 100.0f );

        var audioSource = GetComponent<AudioSource>();
        if ( audioSource != null ) {
            m_slider.onValueChanged.AddListener( delegate ( float a_value ) {
                audioSource.Play();
            } );
        }
    }

    private void Update() {
        var percent = m_slider.value / 100.0f;
        var volume = percent == 0.0f ? -80.0f : m_minVolume - m_minVolume * percent;

        m_mixer.SetFloat( m_valueName, volume );
    }
}
