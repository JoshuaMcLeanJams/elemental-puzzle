using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HighScoreEntry : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI m_rankTextMesh = null;

    [SerializeField]
    private TextMeshProUGUI m_nameTextMesh = null;

    [SerializeField]
    private TextMeshProUGUI m_scoreTextMesh = null;

    [SerializeField]
    private TextMeshProUGUI m_levelTextMesh = null;

    public void Set( int a_rank, HighScoreData a_data ) {
        m_rankTextMesh.text = "" + a_rank;
        m_nameTextMesh.text = "" + a_data.name;
        m_scoreTextMesh.text = string.Format( "{0:D8}", a_data.score );
        m_levelTextMesh.text = "Lvl " + a_data.level;
    }
}
