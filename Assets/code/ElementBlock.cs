using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementBlock : MonoBehaviour
{
    [SerializeField]
    private AudioSource m_lockSoundSource = null;

    [SerializeField]
    private AudioSource m_moveSoundSource = null;

    public Element Element {
        set {
            m_element = value;
            if ( m_element == null ) return;

            var sr = GetComponent<SpriteRenderer>();
            sr.color = m_element.Color;
            sr.sprite = m_element.GetSprite( 1 );
            sr.sortingOrder = 10;

            if( m_shadowSprite == null ) {
                var shadowGo = new GameObject();
                m_shadowSprite = shadowGo.AddComponent<SpriteRenderer>();
            }

            var shadowColor = sr.color;
            shadowColor.a = 0.5f;
            m_shadowSprite.color = shadowColor;

            m_shadowSprite.sprite = sr.sprite;
            m_shadowSprite.sortingOrder = sr.sortingOrder;

            name = string.Format( "{0} Block", m_element.ToString() );
        }
    }
    public bool PlayerControlled { set { m_playerControlled = value; } }

    private Element m_element = null;
    private bool m_locked = false;
    private Vector2Int m_pos = Vector2Int.zero;
    private bool m_quickDrop = false;
    private float m_speedMult = 1.0f;
    private float m_timeSinceLastFall = 0.0f;

    private float m_horizontal = 0.0f;
    private bool m_isHorizontalHeld = false;

    private bool m_playerControlled = true;

    private SpriteRenderer m_shadowSprite = null;

    public void RandomizeElement() {
        Element = ElementManager.instance.RandomElement;
    }

    private void OnDestroy() {
        if ( m_shadowSprite == null ) return;
        Destroy( m_shadowSprite.gameObject );
    }

    private void Start() {
        m_pos = ElementManager.instance.AlignToGrid( Vector2Int.RoundToInt( transform.position ) );
        transform.position = (Vector2)m_pos;
        transform.localScale = Vector3.one * ElementManager.instance.TileSize;
    }

    private void Update() {
        if ( Time.timeScale < 1.0f ) return;
        if ( m_locked ) return;

        m_timeSinceLastFall += Time.deltaTime * m_speedMult * ( m_playerControlled ? 1.0f : 10.0f );

        if ( m_playerControlled )
            HandleInput();

        var floorPos = ElementGrid.instance.GetFloorPos( m_pos );

        if ( m_shadowSprite == null ) return;
        m_shadowSprite.transform.position = (Vector2)floorPos;

        var gravityStep = ElementManager.instance.SecPerGravityStep;
        if ( m_pos == floorPos ) gravityStep *= 0.5f;
        if ( m_timeSinceLastFall < gravityStep )
            return;

        Move( Vector2Int.down, true );
        m_timeSinceLastFall = 0.0f;
    }

    private void HandleInput() {
        m_speedMult = 1.0f;

        var horizontal = Input.GetAxis( "Horizontal" );
        m_horizontal = horizontal;
        if ( m_isHorizontalHeld ) {
            if ( horizontal == 0.0f ) m_isHorizontalHeld = false;
        } else {
            if ( horizontal > float.Epsilon ) {
                Move( Vector2Int.right );
                m_isHorizontalHeld = true;
            } else if ( horizontal < -float.Epsilon ) {
                Move( Vector2Int.left );
                m_isHorizontalHeld = true;
            }

            if ( m_isHorizontalHeld && m_pos == ElementGrid.instance.GetFloorPos( m_pos ) )
                m_timeSinceLastFall = 0.0f;
        }

        if ( m_locked ) return;

        if ( m_quickDrop == false && Input.GetButtonDown( "Quickdrop" ) ) {
            m_pos = ElementGrid.instance.GetFloorPos( m_pos );
            transform.position = (Vector2)m_pos;

            m_quickDrop = true;

            m_timeSinceLastFall = 0.0f;
            return;
        }

        var vertical = Input.GetAxis( "Vertical" );
        if ( vertical < 0.0f ) m_speedMult = ElementManager.instance.FastSpeedMultiplier;
    }

    private bool Move( Vector2Int a_move, bool a_lockIfCannotMove = false ) {
        var targetPos = ElementManager.instance.AlignToGrid( m_pos + a_move * ElementManager.instance.TileSize );
        if ( ElementGrid.instance.IsOpen( targetPos ) == false ) {
            if ( a_lockIfCannotMove ) {
                m_lockSoundSource.Play();

                ElementGrid.instance.Lock( m_element, m_pos );
                if ( m_playerControlled )
                    ElementGrid.instance.GenerateBlock();
                m_locked = true;
                Destroy( gameObject, 0.1f );
            }
            return false;
        }

        var center = (Vector2)transform.position + Vector2.one * 0.5f;
        var halfTileSize = ElementManager.instance.TileSize * 0.5f;
        var hit = Physics2D.CircleCast( center, halfTileSize, a_move, halfTileSize );
        if ( hit ) return false;

        m_moveSoundSource.Play();
        m_pos = targetPos;
        transform.position = (Vector2)m_pos;
        return true;
    }
}
