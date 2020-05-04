using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class SliderLabel : MonoBehaviour
{
    [SerializeField]
    private Slider m_targetSlider = null;

    [SerializeField]
    private bool m_isPercent = false;

    protected string Text {  set { m_textMeshPro.text = value; } }
    private TextMeshProUGUI m_textMeshPro = null;

    private void Awake() {
        m_textMeshPro = GetComponent<TextMeshProUGUI>();
        m_targetSlider.onValueChanged.AddListener( OnValueChanged );
    }

    virtual protected void OnValueChanged( float a_value ) {
        Text = string.Format( m_isPercent ? "{0}%" : "{0}", a_value );
    }
}
