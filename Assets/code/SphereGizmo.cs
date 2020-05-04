using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereGizmo : MonoBehaviour
{
    [SerializeField]
    private Color m_color = Color.white;

    [SerializeField]
    private float m_radius = 1.0f;

    private void OnDrawGizmos() {
        Gizmos.color = m_color;
        Gizmos.DrawWireSphere( transform.position, m_radius );
    }
}
