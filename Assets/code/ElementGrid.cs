using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
class Coord
{
    public int x = 0;
    public int y = 0;
    public Vector2Int Vector2Int { get { return new Vector2Int( x, y ); } }
}

[System.Serializable]
class TileData
{
    public string elementName = "";
    public int level = 0;
    public Coord pos = null;
}

[System.Serializable]
class LevelData
{
    public Coord size = null;
    public TileData[] gemList = null;
    public string[] allowedElementList = null;
}

public class ElementGrid : MonoBehaviour
{
    static public ElementGrid instance = null;

    [Header( "Debug" )]

    [SerializeField]
    private bool m_sequentialBlocks = false;

    [SerializeField]
    private bool m_gemsOnFloorOnly = false;

    [Header("Design")]

    [SerializeField]
    private int m_gemCountBase = 0;

    [SerializeField]
    private int m_gemIncPerLevel = 1;

    [SerializeField]
    private int m_minClearRowsAtTop = 5;

    [Header( "Grid" )]

    [SerializeField]
    private Vector2Int m_gridSizeDefault = new Vector2Int( 5, 10 );

    private Vector2Int m_gridSize = Vector2Int.one;

    [SerializeField]
    private Sprite m_emptyTileSprite = null;

    [Header( "Block/Gem" )]

    [SerializeField]
    private ElementBlock m_blockPrefab = null;

    [SerializeField]
    private float m_lockedColorMult = 1.0f;

    [SerializeField]
    private Color m_gemDarkTint = Color.gray;

    [SerializeField]
    private float m_gemLightTimeSec = 0.2f;

    [SerializeField]
    private float m_gemDarkTimeSec = 0.5f;

    [Header( "UI" )]

    [SerializeField]
    private Image m_nextElementImage = null;

    [SerializeField]
    private TextMeshProUGUI m_gemCountTextMesh = null;

    private int m_blockGenCount = 0;
    private int m_comboMult = 1;
    private bool m_gemLight = false;
    private Element[,] m_gridElement = null;
    private int[,] m_gridLevel = null;
    private bool[,] m_gridClearFlag = null;
    private float m_levelTimeElapsedSec = 0.0f;
    private Element m_nextElement = null;
    private SpriteRenderer[,] m_rendererGrid = null;
    private float m_timeSinceLastCombo = 0.0f;
    private float m_timeSinceLastColorChange = 0.0f;

    public int ComboMult {  get { return m_comboMult; } }
    public int GemCountInitial { get; private set; }
    public string ElapsedTimeString {
        get {
            var timeMin = Mathf.FloorToInt( m_levelTimeElapsedSec ) / 60;
            var timeSec = Mathf.FloorToInt( m_levelTimeElapsedSec ) - timeMin * 60;
            return string.Format( "{0:D2}:{1:D2}", timeMin, timeSec );
        }
    }

    private Vector2Int BlockSpawnPoint {
        get {
            var x = Mathf.FloorToInt( CenterWorldPoint.x );
            var y = ( m_gridSize.y - 1 ) * ElementManager.instance.TileSize;
            return new Vector2Int( x, y );
        }
    }
    private Vector2 CenterWorldPoint { get { return (Vector2)m_gridSize * ElementManager.instance.TileSize * 0.5f; } }
    private Element NextElement {
        set {
            m_nextElement = value;
            m_nextElementImage.sprite = m_nextElement.GetSprite( 1 );
        }
    }

    public ElementBlock GenerateBlock() {
        return GenerateBlock( BlockSpawnPoint, m_nextElement );
    }

    private ElementBlock GenerateBlock( Vector2Int a_gridPosition, Element a_element = null ) {
        if ( a_gridPosition == BlockSpawnPoint && IsOpen( BlockSpawnPoint ) == false ) {
            ElementManager.instance.EndGame();
            return null;
        }

        var elementBlock = Instantiate( m_blockPrefab );
        elementBlock.transform.position = (Vector2)a_gridPosition;

        var collider = elementBlock.GetComponent<BoxCollider2D>();
        collider.size = Vector2.one * ElementManager.instance.TileSize;
        collider.offset = collider.size * 0.5f;

        UpdateAllowedElements();
        if ( a_element != null && a_element.Allowed == false )
            a_element = null;

        ++m_blockGenCount;
        if ( a_element == null ) {
            if ( m_sequentialBlocks ) {
                var elementId = m_blockGenCount % ElementManager.instance.ElementCount;
                elementBlock.Element = ElementManager.instance.GetElement( elementId );
            } else elementBlock.RandomizeElement();
        } else elementBlock.Element = a_element;

        NextElement = ElementManager.instance.RandomElement;

        return elementBlock;
    }

    public Vector2Int GetFloorPos( Vector2Int a_worldPos ) {
        var gridPos = WorldToGridPos( a_worldPos );

        for( var y = gridPos.y - 1; y >= 0; --y ) {
            if ( m_gridElement[gridPos.x, y] != null )
                return new Vector2Int( gridPos.x, y + 1 );
        }

        return new Vector2Int( gridPos.x, 0 );
    }

    public void InitializeLevel() {
        m_nextElement = null;

        var filename = string.Format( "level{0}", ElementManager.instance.Level );
        var levelDataFile = Resources.Load<TextAsset>( filename );
        if ( levelDataFile == null ) {
            Debug.LogFormat( "Generating random level {0}", ElementManager.instance.Level );
            InitializeGrid();
            GenerateRandomLevel();
        } else {
            Debug.LogFormat( "Loading level {0} from data", ElementManager.instance.Level );

            var jsonStr = levelDataFile.text;
            var levelData = JsonUtility.FromJson<LevelData>( jsonStr );

            if ( levelData.size == null || levelData.size.x == 0 || levelData.size.y == 0 ) 
                InitializeGrid();
            else
                InitializeGrid( levelData.size.Vector2Int);
            Debug.LogFormat( "Grid size: {0}", m_gridSize );

            foreach( var gemData in levelData.gemList) {
                var element = ElementManager.instance.GetElement( gemData.elementName );

                Debug.LogFormat( "Add {0} lvl {1} at {2}", 
                    element, gemData.level, gemData.pos.Vector2Int );

                var pos = gemData.pos.Vector2Int;
                pos.x = Mathf.Clamp( pos.x, 0, m_gridSize.x - 1 );
                pos.y = Mathf.Clamp( pos.y, 0, m_gridSize.y - 1 );
                SetElement( pos, element, gemData.level );
            }

            ElementManager.instance.AllowedElementNameArr = levelData.allowedElementList;
            if ( levelData.allowedElementList == null ) {
                Debug.Log( "All elements allowed." );
            }else {
                /*
                var msg = "Allowed elements: ";
                foreach ( var elementName in levelData.allowedElementList )
                    msg += elementName + " ";
                Debug.Log( msg );
                */
            }

            Debug.LogFormat( "Loaded {0} gems from level {1} data", 
                levelData.gemList.Length, ElementManager.instance.Level );
        }

        GemCountInitial = 0;
        for ( int x = 0; x < m_gridSize.x; ++x ) {
            for ( int y = 0; y < m_gridSize.y; ++y ) {
                if( m_gridLevel[x, y] > 1 ) 
                    ++GemCountInitial;
            }
        }

        GenerateBlock();
        m_levelTimeElapsedSec = 0.0f;
    }

    private void GenerateGemRandom( int a_maxHeight ) {
        var loopCount = 0;
        while ( true ) {
            ++loopCount;
            if ( loopCount > 10000 ) return;

            var x = Random.Range( 0, m_gridSize.x );
            var y = m_gemsOnFloorOnly ? 0 : Random.Range( 0, a_maxHeight );
            var element = ElementManager.instance.RandomElement;
            if ( GenerateGem( x, y, element ) ) return;
        }
    }

    private bool GenerateGem( int a_x, int a_y, Element a_element = null ) {
        if ( IsOpen( new Vector2Int( a_x, a_y ) ) == false ) return false;

        SetElement( new Vector2Int( a_x, a_y ), a_element, ElementManager.instance.GemLevel );
        return true;
    }

    private void GenerateRandomLevel() {
        ElementManager.instance.AllowedElementNameArr = null;

        var gemCountMax = m_gridSize.x * ( m_gridSize.y - m_minClearRowsAtTop );
        var gemCount = Mathf.Min( gemCountMax, ElementManager.instance.Level * m_gemIncPerLevel + m_gemCountBase );
        if ( gemCount <= 0 ) gemCount = 1;

        var perRow = m_gridSize.x / 2;
        var maxHeight = Mathf.Min( gemCount / perRow, m_gridSize.y - m_minClearRowsAtTop );

        Debug.LogFormat( "Random level: {0} gems, max height {1}", gemCount, maxHeight );

        for ( int i = 0; i < gemCount; ++i )
            GenerateGemRandom( maxHeight );
    }

    public bool IsOpen( Vector2Int a_worldPos ) {
        var gridCoordinate = WorldToGridPos( a_worldPos );
        if ( ( gridCoordinate.y < 0 ) || ( gridCoordinate.x < 0 || gridCoordinate.x >= m_gridSize.x ) ) {
            return false;
        }
        return m_gridElement[gridCoordinate.x, gridCoordinate.y] == null;
    }

    private bool CheckClear( Vector2Int a_gridPosition) {
        var level = m_gridLevel[a_gridPosition.x, a_gridPosition.y];
        var element = m_gridElement[a_gridPosition.x, a_gridPosition.y];
        if ( element == null ) return false;

        var didClear = false;
        var clearList = new List<Vector2Int>();

        {
            var y = a_gridPosition.y;
            var right = m_gridSize.x - 1;
            for ( int x = a_gridPosition.x + 1; x <= right; ++x ) {
                if ( m_gridElement[x, y] != element.Opposite )
                    break;
                clearList.Add( new Vector2Int( x, y ) );
            }

            var left = 0;
            for ( int x = a_gridPosition.x - 1; x >= left; --x ) {
                if ( m_gridElement[x, y] != element.Opposite )
                    break;
                clearList.Add( new Vector2Int( x, y ) );
            }
        }

        if ( clearList.Count >= level ) {
            if( FinalizeClear( a_gridPosition, clearList, level ) ) {
                didClear = true;
            }
        }
        clearList.Clear();

        {
            var x = a_gridPosition.x;

            var top = m_gridSize.y - 1;
            for ( int y = a_gridPosition.y + 1; y <= top; ++y ) {
                if ( m_gridElement[x, y] != element.Opposite )
                    break;
                clearList.Add( new Vector2Int( x, y ) );
            }

            var bottom = 0;
            for ( int y = a_gridPosition.y - 1; y >= bottom; --y ) {
                if ( m_gridElement[x, y] != element.Opposite )
                    break;
                clearList.Add( new Vector2Int( x, y ) );
            }
        }

        if ( clearList.Count >= level ) {
            if( FinalizeClear( a_gridPosition, clearList, level ) ) {
                didClear = true;
            }
        }

        return didClear;
    }

    private bool CheckPowerUp(Vector2Int a_gridPosition ) {
        var element = m_gridElement[a_gridPosition.x, a_gridPosition.y];
        if ( element == null ) return false;

        if ( m_gridLevel[a_gridPosition.x, a_gridPosition.y] > 1 ) return false;

        {
            var y = a_gridPosition.y;
            var right = m_gridSize.x - 1;
            for ( int x = a_gridPosition.x + 1; x <= right; ++x ) {
                if ( x == a_gridPosition.x ) continue;
                if ( m_gridElement[x, y] != element ) break;
                if ( m_gridLevel[x, y] > 1 ) return true;
            }

            var left = 0;
            for ( int x = a_gridPosition.x - 1; x >= left; --x ) {
                if ( x == a_gridPosition.x ) continue;
                if ( m_gridElement[x, y] != element ) break;
                if ( m_gridLevel[x, y] > 1 ) return true;
            }
        }

        {
            var x = a_gridPosition.x;

            var top = m_gridSize.y - 1;
            for ( int y = a_gridPosition.y + 1; y <= top; ++y ) {
                if ( y == a_gridPosition.y ) continue;
                if ( m_gridElement[x, y] != element ) break;
                if ( m_gridLevel[x, y] > 1 ) return true;
            }

            var bottom = 0;
            for ( int y = a_gridPosition.y - 1; y >= bottom; --y ) {
                if ( y == a_gridPosition.y ) continue;
                if ( m_gridElement[x, y] != element ) break;
                if ( m_gridLevel[x, y] > 1 ) return true;
            }
        }

        return false;
    }

    private bool FinalizeClear( Vector2Int a_ourPos, List<Vector2Int> a_clearPosList, int a_level ) {
        foreach ( var clearPos in a_clearPosList ) {
            if ( m_gridLevel[clearPos.x, clearPos.y] > a_level ) return false;
        }

        if ( a_level > 1 ) {
            var hasLevelOne = false;
            foreach ( var clearPos in a_clearPosList ) {
                if ( m_gridLevel[clearPos.x, clearPos.y] == 1 ) hasLevelOne = true;
            }
            if ( hasLevelOne == false ) return false;
        }

        foreach ( var clearPos in a_clearPosList ) {

            // don't clear matching levels unless we're level one (i.e. gems don't clear gems)
            if ( a_level > 1 && m_gridLevel[clearPos.x, clearPos.y] >= a_level ) continue;
            m_gridClearFlag[clearPos.x, clearPos.y] = true;
        }

        // clear us
        m_gridClearFlag[a_ourPos.x, a_ourPos.y] = true;

        return true;
    }

    public void Lock( Element a_element, Vector2Int a_worldPos ) {
        var newElementGridCoordinate = WorldToGridPos( a_worldPos );

        if ( IsOpen( newElementGridCoordinate ) == false ) {
            Debug.LogErrorFormat( "[Element Grid] Tried to lock element in closed position {0}", a_worldPos );
            return;
        }

        SetElement( newElementGridCoordinate, a_element );

        // check row
        for ( int x = 0; x < m_gridSize.x; ++x ) {
            var pos = new Vector2Int( x, newElementGridCoordinate.y );
            CheckClear( pos );
            if ( CheckPowerUp( pos ) )
                m_rendererGrid[pos.x, pos.y].color = m_gemDarkTint;
            else
                m_rendererGrid[pos.x, pos.y].color = Color.white;
        }

        // check column
        for ( int y = 0; y < m_gridSize.y; ++y ) {
            var pos = new Vector2Int( newElementGridCoordinate.x, y );
            CheckClear( pos );
            if ( CheckPowerUp( pos ) )
                m_rendererGrid[pos.x, pos.y].color = m_gemDarkTint;
            else
                m_rendererGrid[pos.x, pos.y].color = Color.white;
        }
    }

    private void Awake() {
        if ( instance != null ) {
            Debug.LogErrorFormat( "[Element Manager] Duplicate in {0}", name );
            Destroy( this );
            return;
        }
        instance = this;
    }

    private void OnDestroy() {
        if ( instance == this )
            instance = null;
    }


    private void Update() {
        m_levelTimeElapsedSec += Time.deltaTime;

        m_timeSinceLastColorChange += Time.deltaTime;
        if( m_gemLight && m_timeSinceLastColorChange > m_gemLightTimeSec ) {
            m_gemLight = false;
            m_timeSinceLastColorChange = 0.0f;
        }
        if( m_gemLight == false && m_timeSinceLastColorChange > m_gemDarkTimeSec ) {
            m_gemLight = true;
            m_timeSinceLastColorChange = 0.0f;
        }

        m_timeSinceLastCombo += Time.deltaTime;
        if( m_timeSinceLastCombo >= ElementManager.instance.ComboResetTimeSec) {
            m_comboMult = 1;
            m_timeSinceLastCombo = 0.0f;
        }

        if ( m_gridElement == null ) return;

        HandleClear();
        UpdateAllowedElements();

        // check for end of level
        var gemCount = 0;
        var gemTint = m_gemLight ? m_gemDarkTint : Color.white;
        for ( int x = 0; x < m_gridSize.x; ++x ) {
            for ( int y = 0; y < m_gridSize.y; ++y ) {
                if( m_gridLevel[x, y] >= ElementManager.instance.GemLevel ) {
                    ++gemCount;
                    m_rendererGrid[x, y].color = gemTint;
                }
            }
        }

        m_rendererGrid[BlockSpawnPoint.x, BlockSpawnPoint.y].color = Color.red;

        m_gemCountTextMesh.text = string.Format( "Gems {0}/{1}", gemCount, GemCountInitial );
        if ( gemCount == 0 ) {
            var allClear = true;
            for ( int x = 0; x < m_gridSize.x; ++x ) {
                for ( int y = 0; y < m_gridSize.y; ++y ) {
                    if( m_gridElement[x, y] != null ) {
                        allClear = false;
                        break;
                    }
                    if ( allClear == false ) break;
                }
            }

            ElementManager.instance.EndLevel( m_levelTimeElapsedSec, allClear );
            return;
        }

        // check gravity
        for ( int x = 0; x < m_gridSize.x; ++x ) {
            for ( int y = 0; y < m_gridSize.y; ++y ) {
                HandleGravity( x, y );
            }
        }
    }

    private void Clear(int a_x, int a_y) {
        m_gridElement[a_x, a_y] = null;
        m_gridLevel[a_x, a_y] = 0;
        m_rendererGrid[a_x, a_y].color = Color.white;
        m_rendererGrid[a_x, a_y].sprite = m_emptyTileSprite;
        m_gridClearFlag[a_x, a_y] = false;
    }

    private void HandleClear() {
        var gemClear = false;
        for ( int x = 0; x < m_gridSize.x; ++x ) {
            for ( int y = 0; y < m_gridSize.y; ++y ) {
                if ( m_gridClearFlag[x, y] ) {
                    if ( m_gridLevel[x, y] > 1 ) {
                        var score = m_gridLevel[x, y] * m_comboMult * 2;
                        ElementManager.instance.AddScore( score );
                        gemClear = true;
                    }
                    Clear( x, y );
                }
            }
        }

        if ( gemClear ) {
            GetComponent<AudioSource>().Play();
            ++m_comboMult;
            m_timeSinceLastCombo = 0.0f;
        }
    }

    private void HandleGravity(int a_x, int a_y) {
        if ( ( a_y < 1 || m_gridElement[a_x, a_y] == null ) || m_gridLevel[a_x, a_y] > 1 ) return;

        if( m_gridElement[a_x, a_y - 1] == null ) {
            Debug.LogFormat( "Free element at {0}, {1}", a_x, a_y );

            var element = m_gridElement[a_x, a_y];
            Clear( a_x, a_y );

            var block = GenerateBlock( new Vector2Int( a_x, a_y ), element );
            if ( block == null ) return;
            block.PlayerControlled = false;
        }
    }

    private void InitializeGrid( ) {
        InitializeGrid( m_gridSizeDefault );
    }

    private void InitializeGrid( Vector2Int a_gridSize ) {
        if ( m_gridElement != null ) {
            for ( int x = 0; x < m_gridSize.x; ++x ) {
                for ( int y = 0; y < m_gridSize.y; ++y ) {
                    if ( m_rendererGrid[x, y] != null )
                        Destroy( m_rendererGrid[x, y].gameObject );
                }
            }
        }

        m_gridSize = a_gridSize;

        m_gridClearFlag = new bool[m_gridSize.x, m_gridSize.y];
        m_gridElement = new Element[m_gridSize.x, m_gridSize.y];
        m_gridLevel = new int[m_gridSize.x, m_gridSize.y];

        m_rendererGrid = new SpriteRenderer[m_gridSize.x, m_gridSize.y];
        for ( int x = 0; x < m_gridSize.x; ++x ) {
            for ( int y = 0; y < m_gridSize.y; ++y ) {
                var go = new GameObject();
                go.transform.SetParent( transform );
                go.transform.localScale = Vector2.one * ElementManager.instance.TileSize;
                go.transform.position = ElementManager.instance.AlignToGrid( 
                    new Vector2( x, y ) * ElementManager.instance.TileSize );
                go.name = string.Format( "Grid Tile ({0}, {1})", x, y );

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = m_emptyTileSprite;

                m_rendererGrid[x, y] = sr;
            }
        }

        var pos = Camera.main.transform.position;
        pos.x = CenterWorldPoint.x * 0.5f;
        Camera.main.transform.position = pos;
    }

    private void SetElement( Vector2Int a_gridPos, Element a_element, int a_level = 1 ) {
        m_gridElement[a_gridPos.x, a_gridPos.y] = a_element;
        m_gridLevel[a_gridPos.x, a_gridPos.y] = a_level;

        var sr = m_rendererGrid[a_gridPos.x, a_gridPos.y];
        sr.sprite = a_element.GetSprite(a_level);

        sr.color = a_element.Color * m_lockedColorMult;
    }

    private void UpdateAllowedElements() {
        for ( int i = 0; i < ElementManager.instance.ElementCount; ++i ) {
            var element = ElementManager.instance.GetElement( i );

            element.ResetCount();
            for ( int x = 0; x < m_gridSize.x; ++x ) {
                for ( int y = 0; y < m_gridSize.y; ++y ) {
                    if ( m_gridElement[x, y] == element && m_gridClearFlag[x, y] == false ) {
                        element.IncrementCount();
                    }
                }
            }
        }

        for ( int i = 0; i < ElementManager.instance.ElementCount; ++i ) {
            var element = ElementManager.instance.GetElement( i );
            if ( element.Count == 0 ) {
                if ( element.Opposite.Count == 0 ) {
                    ElementManager.instance.RemoveElement( element );
                    ElementManager.instance.RemoveElement( element.Opposite );
                }
            }
        }
    }

    private Vector2Int WorldToGridPos(Vector2Int a_worldPos) {
        return a_worldPos * Mathf.FloorToInt( 1.0f / ElementManager.instance.TileSize );
    }
}
