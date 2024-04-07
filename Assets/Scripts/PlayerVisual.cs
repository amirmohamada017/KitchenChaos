using System;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private MeshRenderer headMeshRenderer;
    [SerializeField] private MeshRenderer bodyMeshRenderer;

    private Material _material;
    
    private void Awake()
    {
        _material = new Material(headMeshRenderer.material);
        headMeshRenderer.material = _material;
        bodyMeshRenderer.material = _material;
    }

    private void Start()
    {
        
    }

    public void SetPlayerColor(Color color)
    {
        _material.color = color;
    }
}
