using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class PlayerPrefDisplay : MonoBehaviour
{
    enum KeyType
    {
        Float,
        Int,
        String
    }

    [SerializeField]
    private string m_label = "";

    [SerializeField]
    private string m_key = "";

    [SerializeField, Tooltip("Only implemented for integers")]
    private int m_digitCount = 0;

    [SerializeField]
    private KeyType m_keyType = KeyType.Float;

    [SerializeField]
    private bool m_useDefaultIfNotFound = false;

    public void Start() {
        if ( PlayerPrefs.HasKey( m_key ) == false ) {
            if ( m_useDefaultIfNotFound ) {
                switch ( m_keyType ) {
                    case KeyType.Float:
                        PlayerPrefs.SetFloat( m_key, 0.0f );
                        break;
                    case KeyType.Int:
                        PlayerPrefs.SetInt( m_key, 0 );
                        break;
                    case KeyType.String:
                        PlayerPrefs.SetString( m_key, "" );
                        break;
                }
            } else {
                GetComponent<TextMeshProUGUI>().text 
                    = string.Format( "NO KEY {0} OF TYPE {1} IN PLAYERPREFS", m_key, m_keyType );
                return;
            }
        }

        var textMesh = GetComponent<TextMeshProUGUI>();
        textMesh.text = m_label;
        switch ( m_keyType ) {
            case KeyType.Float:
                textMesh.text += PlayerPrefs.GetFloat( m_key );
                return;
            case KeyType.Int:
                var format = m_digitCount == 0 ? "{0}" : string.Format( "{{0:D{0}}}", m_digitCount );
                textMesh.text += string.Format( format, PlayerPrefs.GetInt( m_key ) );
                return;
            case KeyType.String:
                textMesh.text += PlayerPrefs.GetString( m_key );
                return;
        }
    }
}
