using System.Collections.Generic;
using UnityEngine;

public class NavigationNode
{
    private Vector3 position;
    private List<NavigationNode> neighbours = new List<NavigationNode>();

    private float gCost;
    private float hCost;
    private NavigationNode parent; 

    public Vector3 Position => position;
    public List<NavigationNode> Neighbours => neighbours;
    public NavigationNode Parent => parent;

    public NavigationNode(Vector3 position)
    {
        this.position = position;
    }

    public float GCost()
    {
        return gCost;
    }

    public void SetGCost(float gCost)
    {
        this.gCost = gCost;
    }

    public float HCost()
    {
        return hCost;
    }

    public void SetHCost(float hCost)
    {
        this.hCost = hCost;
    }

    public float FinalCost()
    {
        return gCost + hCost;
    }

    public void SetParent(NavigationNode parent)
    {
        this.parent = parent;
    }
}
