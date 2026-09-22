using UnityEngine;

public class GroundPiece : MonoBehaviour
{
    [Header("Ground Order")]
    [SerializeField] private int index;

    [Header("Connection")]
    [SerializeField] private Transform endPoint;

    public int Index => index;

    public float EndX
    {
        get
        {
            if (endPoint)
                return endPoint.position.x;

            return transform.position.x;
        }
    }
}