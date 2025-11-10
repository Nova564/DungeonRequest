using UnityEngine;

public class BSPNode
{
    public RectInt bounds;
    public BSPNode left;
    public BSPNode right;
    public RectInt room;

    public BSPNode(RectInt bounds)
    {
        this.bounds = bounds;
    }

    public bool IsLeaf()
    {
        return left == null && right == null;
    }
}