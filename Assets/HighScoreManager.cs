using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class HighScoreContainer
{
    public HighScoreData[] data = null;
}

[System.Serializable]
public class HighScoreData
{
    public string name = "no one";
    public int score = 0;
    public int level = 0;
    public int speed = 0;
}

public class HighScoreManager : MonoBehaviour
{
    const string PLAYER_PREFS_KEY = "high score data";

    [SerializeField]
    private HighScoreEntry m_entryPrefab = null;

    [SerializeField]
    private int m_entryCountMax = 5;

    [SerializeField]
    private GameObject m_table = null;

    List<HighScoreData> m_highScoreDataList = new List<HighScoreData>();

    private void Start() {
        if ( LoadScores() == false ) {
            m_highScoreDataList = new List<HighScoreData>();
        }

        AddScore();
        SaveScores();

        DisplayScores();
    }

    private void AddScore() {
        var score = PlayerPrefs.GetInt( "score" );
        if ( score <= 0 ) return;

        var level = PlayerPrefs.GetInt( "level" );
        var speed = PlayerPrefs.GetInt( "speed" );

        var data = new HighScoreData() {
            level = level,
            name = "Player",
            score = score,
            speed = speed
        };
        m_highScoreDataList.Add( data );

        m_highScoreDataList = m_highScoreDataList.OrderByDescending(scoreData => scoreData.score )
            .Take( m_entryCountMax ).ToList();
    }
    
    private void DisplayScores() {
        for( var i = 0; i < m_highScoreDataList.Count; ++i ) {
            var entry = Instantiate( m_entryPrefab );
            entry.transform.SetParent( m_table.transform, false );
            entry.Set( i + 1, m_highScoreDataList[i] );
        }
    }

    private bool LoadScores() {
        if ( PlayerPrefs.HasKey( PLAYER_PREFS_KEY ) == false ) return false;

        var jsonHighScoreData = PlayerPrefs.GetString( PLAYER_PREFS_KEY );
        var highScoreContainer = JsonUtility.FromJson<HighScoreContainer>( jsonHighScoreData );
        if ( highScoreContainer == null ) {
            Debug.LogError( "Failed to parse high score list" );
            return false;
        }

        var highScoreArr = highScoreContainer.data;
        if ( highScoreArr == null ) {
            Debug.LogError( "High score list doesn't exist" );
            return false;
        }

        m_highScoreDataList = new List<HighScoreData>( highScoreArr );
        return true;
    }

    private void SaveScores() {
        var highScoreContainer = new HighScoreContainer() {
            data = m_highScoreDataList.ToArray()
        };
        var highScoreDataJson = JsonUtility.ToJson( highScoreContainer );
        PlayerPrefs.SetString( PLAYER_PREFS_KEY, highScoreDataJson );
    }
}
