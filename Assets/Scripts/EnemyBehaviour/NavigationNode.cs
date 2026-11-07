using System.Collections.Generic;
using UnityEngine;

public class NavigationNode
{
    private Vector3 position;
    private List<NavigationNode> neighbours = new List<NavigationNode>();

    public Vector3 Position => position;
    public List<NavigationNode> Neighbours => neighbours;

    public NavigationNode(Vector3 position)
    {
        this.position = position;
    }
}
