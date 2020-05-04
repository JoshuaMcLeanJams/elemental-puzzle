using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Element : MonoBehaviour
{
    [SerializeField]
    private Element m_opposite = null;

    [SerializeField]
    private Color m_color = Color.white;

    [SerializeField]
    private Sprite m_elementSprite = null;

    [SerializeField]
    private Sprite m_gemSprite = null;

    [SerializeField]
    private Image m_uiImage = null;

    [SerializeField]
    private TextMeshProUGUI m_countTextMesh = null;

    public bool Allowed {
        get { return m_allowed; }
        set { m_allowed = value; m_uiImage.gameObject.SetActive( m_allowed ); }
    }
    public Color Color {  get { return m_color; } }
    public Element Opposite {  get { return m_opposite; } }

    private bool m_allowed = false;

    public Sprite GetSprite( int a_level ) {
        if ( a_level >= ElementManager.instance.GemLevel ) return m_gemSprite;
        return m_elementSprite;
    }

    public int Count { get; private set; }

    public void IncrementCount() {
        ++Count;
        if ( m_countTextMesh != null ) {
            m_countTextMesh.gameObject.SetActive( true );
            m_countTextMesh.text = "" + Count;
        }
    }

    public void ResetCount() {
        Count = 0;
        if ( m_countTextMesh != null )
            m_countTextMesh.gameObject.SetActive( false );
    }
}
