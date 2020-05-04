using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [SerializeField]
    private bool m_isInitiallyVisible = false;

    [SerializeField]
    private GameObject m_topLevel = null;

    [SerializeField]
    private Button m_backButtonPrefab = null;

    public bool IsVisible { get { return m_menuStack.Count > 0; } }

    private GameObject ActiveMenu {  get { return m_menuStack.Count == 0 ? null : m_menuStack.Peek(); } }
    private Stack<GameObject> m_menuStack = new Stack<GameObject>();

    public void Back() {
        if( m_menuStack.Count == 0 ) {
            Debug.LogWarning( "Tried to go back but we're not in a menu." );
            return;
        }

        m_menuStack.Pop();
        UpdateMenu();
    }

    public void PushMenu(GameObject a_menu ) {
        m_menuStack.Push( a_menu );
        UpdateMenu();
    }

    private void UpdateMenu() {
        var stackStr = "";
        foreach ( var m in m_menuStack ) stackStr += m.name + " ";
        Debug.LogFormat( "Stack {0}: " + stackStr, m_menuStack.Count );
        foreach ( Transform child in transform ) {
            if ( child == transform ) continue;
            child.gameObject.SetActive( false );
        }

        if ( m_menuStack.Count == 0 ) return;

        var menu = m_menuStack.Peek();
        menu.SetActive( true );
    }

    public void ShowTop() {
        PushMenu( m_topLevel );
    }

    private void Start() {
        // create back buttons
        foreach ( Transform child in transform ) {
            //child.gameObject.SetActive( true );
            //Debug.LogFormat( "Add back button to {0}", child.name );
            if ( child.gameObject == m_topLevel ) continue;
            var backButton = Instantiate( m_backButtonPrefab );
            backButton.transform.SetParent( child.transform, false );
            backButton.onClick.AddListener( Back );
        }

        if ( m_isInitiallyVisible ) ShowTop();
        else UpdateMenu();
    }
}
