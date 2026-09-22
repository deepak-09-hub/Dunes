using System.Collections.Generic;
using UnityEngine;

public class Terrain : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Ground Prefabs")]
    [SerializeField] private GroundPiece[] groundPrefabs;

    [Header("Spawning")]
    [SerializeField] private int startingPieces = 8;

    [Tooltip("How far in front of the player ground should exist.")]
    [SerializeField] private float spawnAheadDistance = 80f;

    [Tooltip("How far behind the player pieces are removed.")]
    [SerializeField] private float destroyBehindDistance = 40f;

    private readonly Queue<GameObject> activePieces = new();

    private float nextSpawnX;
    private float fixedGroundY;

    private int nextGroundIndex;

    private void Start()
    {
        if (!player)
        {
            Player foundPlayer = FindFirstObjectByType<Player>();

            if (foundPlayer)
                player = foundPlayer.transform;
        }

        // Terrain object's X is where the first piece begins.
        nextSpawnX = transform.position.x;

        // Every ground piece uses this same Y.
        fixedGroundY = transform.position.y;

        // Begin from index 0.
        nextGroundIndex = 0;

        for (int i = 0; i < startingPieces; i++)
        {
            SpawnGroundPiece();
        }
    }

    private void Update()
    {
        if (!player)
            return;

        SpawnGroundAhead();
        RemoveGroundBehind();
    }

    private void SpawnGroundAhead()
    {
        int safetyCounter = 0;
        const int maxSpawnsPerFrame = 10;

        while (player.position.x + spawnAheadDistance > nextSpawnX)
        {
            float previousX = nextSpawnX;

            bool spawned = SpawnGroundPiece();

            // Something went wrong. Stop immediately instead
            // of allowing an infinite loop.
            if (!spawned)
                break;

            // Ground did not move the spawn point forward.
            if (nextSpawnX <= previousX)
            {
                Debug.LogError(
                    "GROUND SPAWNING STOPPED! " +
                    "EndPoint X must be greater than the GroundPiece root X."
                );

                break;
            }

            safetyCounter++;

            if (safetyCounter >= maxSpawnsPerFrame)
            {
                Debug.LogWarning(
                    "Terrain reached maximum ground spawns this frame."
                );

                break;
            }
        }
    }

    private bool SpawnGroundPiece()
    {
        if (groundPrefabs == null ||
            groundPrefabs.Length == 0)
        {
            Debug.LogError(
                "Terrain has no Ground Pieces assigned."
            );

            return false;
        }

        GroundPiece prefab =
            GetGroundPieceByIndex(nextGroundIndex);

        if (!prefab)
        {
            Debug.LogError(
                "No GroundPiece found with index: " +
                nextGroundIndex
            );

            return false;
        }

        Vector3 spawnPosition = new Vector3(
            nextSpawnX,
            fixedGroundY,
            transform.position.z
        );

        GroundPiece spawnedPiece = Instantiate(
            prefab,
            spawnPosition,
            Quaternion.identity,
            transform
        );

        activePieces.Enqueue(
            spawnedPiece.gameObject
        );

        float newEndX = spawnedPiece.EndX;

        // Critical protection.
        if (newEndX <= nextSpawnX)
        {
            Debug.LogError(
                spawnedPiece.name +
                " has an invalid EndPoint!\n" +
                "Root X: " + nextSpawnX +
                "\nEndPoint X: " + newEndX
            );

            return false;
        }

        nextSpawnX = newEndX;

        AdvanceGroundIndex();

        return true;
    }

    private GroundPiece GetGroundPieceByIndex(int index)
    {
        for (int i = 0; i < groundPrefabs.Length; i++)
        {
            if (!groundPrefabs[i])
                continue;

            if (groundPrefabs[i].Index == index)
            {
                return groundPrefabs[i];
            }
        }

        return null;
    }

    private void AdvanceGroundIndex()
    {
        nextGroundIndex++;

        // If that index doesn't exist,
        // loop back to index 0.
        if (!GroundIndexExists(nextGroundIndex))
        {
            nextGroundIndex = 0;
        }
    }

    private bool GroundIndexExists(int index)
    {
        for (int i = 0; i < groundPrefabs.Length; i++)
        {
            if (!groundPrefabs[i])
                continue;

            if (groundPrefabs[i].Index == index)
            {
                return true;
            }
        }

        return false;
    }

    private void RemoveGroundBehind()
    {
        while (activePieces.Count > 0)
        {
            GameObject oldest =
                activePieces.Peek();

            if (!oldest)
            {
                activePieces.Dequeue();
                continue;
            }

            float distanceBehind =
                player.position.x -
                oldest.transform.position.x;

            if (
                distanceBehind >
                destroyBehindDistance
            )
            {
                Destroy(
                    activePieces.Dequeue()
                );
            }
            else
            {
                break;
            }
        }
    }
}