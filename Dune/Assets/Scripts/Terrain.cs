using System.Collections.Generic;
using UnityEngine;

public class Terrain : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] float segmentWidth = 12f;
    [SerializeField] int startingSegments = 10;
    [SerializeField] int segmentsAhead = 5;

    [SerializeField] float bottom = -8f;
    [SerializeField] float minimumHeight = 0f;
    [SerializeField] float maximumHeight = 4f;

    [SerializeField] Material material;

    readonly Queue<GameObject> segments = new();

    float nextX;
    float previousHeight = 1f;

    void Start()
    {
        if (!player)
            player = FindFirstObjectByType<Player>().transform;

        for (int i = 0; i < startingSegments; i++)
            CreateSegment();
    }

    void Update()
    {
        if (!player) return;

        while (player.position.x + segmentWidth * segmentsAhead > nextX)
            CreateSegment();

        while (segments.Count > startingSegments + 2)
        {
            GameObject first = segments.Peek();

            if (first.transform.position.x + segmentWidth <
                player.position.x - segmentWidth)
            {
                Destroy(segments.Dequeue());
            }
            else
            {
                break;
            }
        }
    }

    void CreateSegment()
    {
        float x0 = nextX;
        float x1 = nextX + segmentWidth;

        float nextHeight = Random.Range(
            minimumHeight,
            maximumHeight
        );

        // Occasionally make a lower valley.
        if (Random.value < .2f)
            nextHeight = Random.Range(0f, 1.2f);

        GameObject segment = new GameObject("Dune");
        segment.transform.SetParent(transform);

        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer >= 0)
            segment.layer = groundLayer;

        MeshFilter filter = segment.AddComponent<MeshFilter>();
        MeshRenderer renderer = segment.AddComponent<MeshRenderer>();
        PolygonCollider2D collider = segment.AddComponent<PolygonCollider2D>();

        const int points = 9;

        Vector3[] vertices = new Vector3[points + 2];

        for (int i = 0; i < points; i++)
        {
            float t = i / (float)(points - 1);

            // Smooth hill interpolation.
            float smooth = (1f - Mathf.Cos(t * Mathf.PI)) * .5f;

            float x = Mathf.Lerp(x0, x1, t);
            float y = Mathf.Lerp(
                previousHeight,
                nextHeight,
                smooth
            );

            vertices[i] = new Vector3(
                x,
                y,
                0f
            );
        }

        vertices[points] = new Vector3(x1, bottom, 0);
        vertices[points + 1] = new Vector3(x0, bottom, 0);

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;

        int[] triangles = new int[(vertices.Length - 2) * 3];

        int index = 0;

        for (int i = 1; i < vertices.Length - 1; i++)
        {
            triangles[index++] = 0;
            triangles[index++] = i;
            triangles[index++] = i + 1;
        }

        mesh.triangles = triangles;
        mesh.RecalculateBounds();

        filter.sharedMesh = mesh;

        if (material)
        {
            renderer.sharedMaterial = material;
        }
        else
        {
            Shader shader = Shader.Find("Sprites/Default");

            if (shader)
                renderer.sharedMaterial = new Material(shader);
        }

        Vector2[] path = new Vector2[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
            path[i] = vertices[i];

        collider.SetPath(0, path);

        segments.Enqueue(segment);

        nextX = x1;
        previousHeight = nextHeight;
    }
}
