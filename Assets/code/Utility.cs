using UnityEngine;

[System.Serializable]
public class GameData
{
    [SerializeField]
    private StringInt[] dictionary = null;

    [SerializeField]
    private string[] musicList = null;

    [SerializeField]
    private SpeedData[] speedList = null;

    [SerializeField]
    private string versionString = "";

    public string[] MusicList {  get { return musicList; } }
    public string VersionString {  get { return versionString; } }

    public int GetInt(string a_key) {
        foreach ( var stringInt in dictionary )
            if ( stringInt.key == a_key ) return stringInt.value;

        Debug.LogErrorFormat( "No int for key {0} in game data", a_key );
        return 0;
    }

    public SpeedData GetSpeedData(int a_index) {
        if ( a_index < 0 || a_index >= speedList.Length ) return null;
        return speedList[a_index];
    }
}

[System.Serializable]
public class SpeedData
{
    public string name;
    public float dropSec;
    public float pointMult;
}

[System.Serializable]
public class StringInt
{
    public string key;
    public int value;
}


static class Utility
{
    static GameData m_gameData = null;

    static public GameData GameData {
        get {
            if ( m_gameData == null ) LoadGameData();
            return m_gameData;
        }
    }

    static private void LoadGameData() {
        var gameDataFile = Resources.Load<TextAsset>( "game" );
        if( gameDataFile == null ) {
            Debug.LogError( "Could not load game data file (game.json)" );
            return;
        }

        m_gameData = JsonUtility.FromJson<GameData>( gameDataFile.text );
    }
}
