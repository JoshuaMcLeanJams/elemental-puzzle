using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ElementManager : MonoBehaviour
{
    enum GameState
    {
        StartingLevel,
        InLevel,
        EndingLevel
    }

    static public ElementManager instance = null;

    [Header("Game Systems")]

    [SerializeField]
    private List<Element> m_elementList = new List<Element>();

    [Header( "Design Settings" )]

    [SerializeField]
    private int m_gemLevel = 3;

    [SerializeField]
    private float m_fastSpeedMultiplier = 2.0f;

    [SerializeField]
    private float m_comboResetTimeSec = 0.3f;

    [Header( "UI - Game Info" )]

    [SerializeField]
    private TextMeshProUGUI m_levelLabelTextMesh = null; 

    [SerializeField]
    private TextMeshProUGUI m_scoreLabelTextMesh = null; 

    [SerializeField]
    private TextMeshProUGUI m_scoreToAddTextMesh = null;

    [SerializeField]
    private TextMeshProUGUI m_timeElapsedTextMesh = null;

    [Header( "UI - Overlay" )]

    [SerializeField]
    private TextMeshProUGUI m_gameOverTextMesh = null; 

    [SerializeField]
    private TextMeshProUGUI m_levelCompleteTextMesh = null;

    [Header( "UI - Overlay Settings" )]

    [SerializeField]
    private float m_scoreToAddStaySec = 1.0f;

    [SerializeField]
    private Image m_dimmer = null; 

    [SerializeField]
    private float m_gameOverTitleDelaySec = 2.0f;

    [SerializeField]
    private float m_levelEndDelaySec = 2.0f;

    [SerializeField]
    private float m_levelStartDelaySec = 2.0f;

    [Header("Visual")]

    [SerializeField]
    private int m_tileSize = 1;

    private List<Element> m_allowedElementList = new List<Element>();
    private List<Element> m_grabBag = new List<Element>();
    private int m_elementDrawCount = 0;

    private float m_timeSinceTransition = 0.0f;

    public float ComboResetTimeSec {  get { return m_comboResetTimeSec; } }
    public int ElementCount {  get { return m_elementList.Count; } }
    public float FastSpeedMultiplier {  get { return m_fastSpeedMultiplier; } }
    public int GemLevel {  get { return m_gemLevel; } }
    public int Level {
        get { return m_level; }
        set {
            m_level = value;
            var levelMax = Utility.GameData.GetInt( "level max" );
            if( m_level > levelMax && SpeedLevel < SpeedLevelMax ) {
                ++SpeedLevel;
                m_level = 0;
            }
            m_levelLabelTextMesh.text = "Level " + m_level + " " + SpeedData.name;

            // remember the highest level we got to
            PlayerPrefs.SetInt( "level", m_level );
            PlayerPrefs.SetInt( "speed", SpeedLevel );
        }
    }

    public string[] AllowedElementNameArr {
        set {
            foreach ( var element in m_elementList ) {
                element.Allowed = false;
            }

            if ( value == null ) {
                foreach( var element in m_elementList) {
                    element.Allowed = true;
                    m_allowedElementList.Add( element );
                }
                return;
            }

            foreach( var elementName in value ) {
                var element = GetElement( elementName );
                element.Allowed = true;
                m_allowedElementList.Add( element );
            }
        }
    }
    public bool IsTransitioning { get { return m_state != GameState.InLevel; } }
    public Element RandomElement {
        get {
            if ( m_allowedElementList.Count == 0 ) return null;

            Element element = null;

            var totalDrawCount = Utility.GameData.GetInt("random piece count" ) + m_allowedElementList.Count;

            if ( ( m_grabBag.Count == 0 && m_elementDrawCount == 0 )
                || ( m_elementDrawCount >= totalDrawCount ) ) {

                m_elementDrawCount = 0;
                m_grabBag.AddRange( m_allowedElementList );
            }

            if ( m_grabBag.Count == 0 ) {
                var id = Random.Range( 0, m_allowedElementList.Count );
                element = m_allowedElementList[id];
            } else {
                var id = Random.Range( 0, m_grabBag.Count );
                element = m_grabBag[id];
                m_grabBag.Remove( m_grabBag[id] );
            }

            ++m_elementDrawCount;
            return element;
        }
    }
    public float SecPerGravityStep { get { return SpeedData.dropSec; } }
    public int TileSize {  get { return m_tileSize; } }

    private int SpeedLevel {
        get { return PlayerPrefs.GetInt( "speed", 0 ); }
        set { PlayerPrefs.SetInt( "speed", value ); }
    }
    private int SpeedLevelMax { get { return Utility.GameData.GetInt( "speed max" ); } }
    private SpeedData SpeedData { get { return Utility.GameData.GetSpeedData( SpeedLevel ); } }

    private int m_level = 0;
    private int m_score = 0;
    private int m_scoreToAdd = 0;
    private float m_timeSinceLastScore = 0.0f;
    GameState m_state = GameState.StartingLevel;

    public void AddScore(int a_score) {
        m_timeSinceLastScore = 0.0f;
        m_scoreToAdd += Mathf.FloorToInt( a_score * SpeedData.pointMult );
        m_scoreToAddTextMesh.gameObject.SetActive( true );
    }

    public Vector2 AlignToGrid( Vector2 a_pos ) {
        var x = Mathf.Round( a_pos.x / m_tileSize ) * m_tileSize;
        var y = Mathf.Round( a_pos.y / m_tileSize ) * m_tileSize;
        return new Vector2( x, y );
    }

    public Vector2Int AlignToGrid( Vector2Int a_pos ) {
        var x = Mathf.RoundToInt( a_pos.x / m_tileSize ) * m_tileSize;
        var y = Mathf.RoundToInt( a_pos.y / m_tileSize ) * m_tileSize;
        return new Vector2Int( x, y );
    }

    public void EndGame() {
        m_dimmer.gameObject.SetActive( true );

        m_gameOverTextMesh.text = "GAME OVER";

        m_gameOverTextMesh.text += string.Format( "\n<size=-10>Score: {0}</size>", m_score );
        if ( m_score > PlayerPrefs.GetInt( "high score", 0 ) ) {
            m_gameOverTextMesh.text += "\n<size=-10>New high score!</size>";
            PlayerPrefs.SetInt( "high score", m_score );
        }

        m_gameOverTextMesh.gameObject.SetActive( true );

        Destroy( ElementGrid.instance.gameObject );

        StartCoroutine( ReturnToTitleDelayed() );
    }

    public void EndLevel( float a_levelTimeSec, bool a_allClear ) {
        if ( m_state != GameState.InLevel ) return;

        ClearBlocks();

        m_state = GameState.EndingLevel;
        m_timeSinceTransition = 0.0f;

        var levelMaxTime = Utility.GameData.GetInt( "time max per gem" ) * ElementGrid.instance.GemCountInitial;
        var bonusTimeSecTotal = levelMaxTime - Mathf.FloorToInt( a_levelTimeSec );
        var timeBonusScore = bonusTimeSecTotal < 0 ? 0 
            : bonusTimeSecTotal * Utility.GameData.GetInt( "time bonus per second" );

        Debug.LogFormat( "Cleared in {0}/{1} seconds, bonus {2} - {3}", 
            a_levelTimeSec, levelMaxTime, timeBonusScore, a_allClear ? "ALL CLEAR" : "NOT ALL CLEAR" );

        var allClearBonus = Utility.GameData.GetInt( "all clear bonus" );
        var allClearMsg = a_allClear ? string.Format( "ALL CLEAR (+{0})", allClearBonus ) : "";
        m_levelCompleteTextMesh.text = string.Format( "LEVEL COMPLETE\n\n<size=-10>Time Bonus: {0} = {1}\n{2}</size>",
            ElementGrid.instance.ElapsedTimeString, timeBonusScore, allClearMsg );

        AddScore( timeBonusScore );
        if ( a_allClear ) AddScore( allClearBonus );
    }

    public Element GetElement( int a_id ) {
        return m_elementList[a_id];
    }

    public Element GetElement(string a_name ) {
        foreach ( var element in m_elementList )
            if ( element.name == a_name ) return element;

        Debug.LogErrorFormat( "Unknown element name '{0}'", a_name );
        return null;
    }

    public int GetElementId(Element a_element) {
        for ( int i = 0; i < m_elementList.Count; ++i )
            if ( GetElement( i ) == a_element ) return i;
        return -1;
    }

    public bool IsAllowed( Element a_element ) {
        return m_allowedElementList.Contains( a_element );
    }

    public void RemoveElement(Element a_element) {
        //Debug.LogFormat( "Remove element {0} ", a_element );

        if ( m_allowedElementList.Contains( a_element ) == false ) {
            //Debug.Log( "Not found" );
            return;
        }
        m_allowedElementList.Remove( a_element );
        a_element.Allowed = false;
    }

    public void RestartLevel() {
        Pauser.instance.Unpause();
        SceneManager.LoadScene( "main" );
    }

    public void ReturnToTitle() {
        Pauser.instance.Unpause();
        SceneManager.LoadScene( "title" );
    }

    private void Awake() {
        if( instance != null ) {
            Debug.LogErrorFormat( "[Element Manager] Duplicate in {0}", name );
            Destroy( this );
            return;
        }
        instance = this;
    }

    private void Start() {
        m_gameOverTextMesh.gameObject.SetActive( false );
        m_gameOverTextMesh.gameObject.SetActive( false );
        m_levelCompleteTextMesh.gameObject.SetActive( false );

        AddScore( 0 );
        m_scoreToAddTextMesh.gameObject.SetActive( false );

        Level = PlayerPrefs.GetInt( "level", 0 );
        StartLevel();
    }

    private void Update() {
        m_timeSinceLastScore += Time.deltaTime;
        if ( m_timeSinceLastScore > m_scoreToAddStaySec ) {
            m_score += m_scoreToAdd;
            PlayerPrefs.SetInt( "score", m_score );
            m_scoreToAdd = 0;
            m_scoreToAddTextMesh.gameObject.SetActive( false );
        }
        m_scoreToAddTextMesh.text = "+" + m_scoreToAdd;

        if ( ElementGrid.instance != null ) {
            m_timeElapsedTextMesh.text = ElementGrid.instance.ElapsedTimeString;
            m_scoreLabelTextMesh.text = string.Format( "{0:D8} (x{1})",
                m_score, ElementGrid.instance.ComboMult );
        }

        if ( m_state == GameState.InLevel ) return;
        Time.timeScale = 0.0f;
        m_levelCompleteTextMesh.gameObject.SetActive( true );
        m_dimmer.gameObject.SetActive( true );

        m_timeSinceTransition += Time.unscaledDeltaTime;
        if ( m_state == GameState.StartingLevel ) {
            m_levelCompleteTextMesh.text = string.Format( "LEVEL {0} / {1}", m_level, SpeedData.name );

            if ( m_timeSinceTransition >= m_levelStartDelaySec ) {
                m_state = GameState.InLevel;
                m_dimmer.gameObject.SetActive( false );
                m_levelCompleteTextMesh.gameObject.SetActive( false );
                Time.timeScale = 1.0f;
            }
        } else if ( m_state == GameState.EndingLevel ) {
            if ( m_timeSinceTransition >= m_levelEndDelaySec ) {
                ++Level;
                StartLevel();
            }
        }
    }

    private void ClearBlocks() {
        foreach( var elementBlock in FindObjectsOfType<ElementBlock>()) {
            Destroy( elementBlock.gameObject );
        }
    }

    private IEnumerator ReturnToTitleDelayed() {
        yield return new WaitForSeconds( m_gameOverTitleDelaySec );
        Pauser.instance.Unpause();
        SceneManager.LoadScene( "high-score" );
        //ReturnToTitle();
    }

    private void StartLevel() {
        m_allowedElementList.Clear();
        ElementGrid.instance.InitializeLevel();

        m_state = GameState.StartingLevel;
        m_timeSinceTransition = 0.0f;
    }
}
