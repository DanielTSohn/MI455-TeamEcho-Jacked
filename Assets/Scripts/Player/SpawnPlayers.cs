using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnPlayers : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The radius the players spawn around the object this is attatched to")]
    private float spawnRadius = 5;

    public static SpawnPlayers Instance { get; private set; }
    public bool SceneReady { get; private set; } = false;

    private void Awake()
    {
        // Singleton handling
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            PlayerManagerData.Instance.InputManager.DisableJoining();
            SceneReady = false;
            SetupPlayers();
        }
    }

    private void SetupPlayers()
    {
        Debug.Log("Spawning " + PlayerManagerData.Instance.PlayerCount + " players");

        List<Vector3> spawnPoints = PlayerManagerData.Instance.GenerateSpawnPoints(spawnRadius);
        foreach(KeyValuePair<PlayerInput, int> player in PlayerManagerData.Instance.Players)
        {
            player.Key.transform.position = spawnPoints[player.Value];
            Rigidbody jackRB = player.Key.gameObject.GetComponent<PlayerComponents>().JackhammerRB;
            jackRB.constraints = RigidbodyConstraints.FreezeRotationY;
            jackRB.isKinematic = false;
            Debug.Log("Joined player " + (player.Value+1));
        }

        SceneReady = true;
    }
}