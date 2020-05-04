using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class GameDataSlider : MonoBehaviour
{
    [SerializeField]
    private string m_dataKey = "";

    private Slider m_slider = null;

    private void Awake() {
        m_slider = GetComponent<Slider>();
    }

    private void Start() {
        var gameData = Utility.GameData;
        if ( gameData == null ) return;

        m_slider.minValue = gameData.GetInt( m_dataKey + " min" );
        m_slider.maxValue = gameData.GetInt( m_dataKey + " max" );

        m_slider.value = PlayerPrefs.GetInt( m_dataKey, 0 );
        PlayerPrefs.SetInt( m_dataKey,
            Mathf.FloorToInt( Mathf.Clamp( m_slider.value, m_slider.minValue, m_slider.maxValue ) ) );

        m_slider.onValueChanged.AddListener( delegate ( float a_value ) {
            PlayerPrefs.SetInt( m_dataKey, Mathf.FloorToInt( a_value ) );
        } );
    }
}
