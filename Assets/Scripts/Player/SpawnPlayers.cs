using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
public class SpawnPlayers : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The radius the players spawn around the object this is attatched to")]
    private float spawnRadius = 5;
    [SerializeField]
    [Tooltip("The spawn point the players spawn around")]
    private Transform spawnPoint;
    [SerializeField]
    private GameEvent onSceneReady;

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
            PlayerComponents components = player.Key.gameObject.GetComponent<PlayerComponents>();
            components.JackhammerRB.constraints = RigidbodyConstraints.FreezeRotation;
            components.JackhammerRB.isKinematic = false;
            components.JackhammerRB.useGravity = true;
            components.PlayerRoot.gameObject.transform.LookAt(spawnPoint);
            components.JackhammerRB.transform.position = spawnPoint.position + spawnPoints[player.Value];
            LayerMask viewLayers = new();
            viewLayers |= (1 << 0);
            viewLayers |= (1 << 1);
            viewLayers |= (1 << 2);
            viewLayers |= (1 << 4);
            viewLayers |= (1 << 5);
            viewLayers |= (1 << 6);
            viewLayers |= (1 << 7);
            viewLayers |= (1 << 8);
            viewLayers |= (1 << 9);
            viewLayers |= (1 << 10 + player.Value);
            components.PlayerCamera.cullingMask = viewLayers;
            components.VirtualCamera.GetComponent<CinemachineInputProvider>().PlayerIndex = player.Value;
            components.VirtualCamera.layer = player.Value + 10;

            Debug.Log("Joined player " + (player.Value+1));
        }

        SceneReady = true;
        onSceneReady.TriggerEvent();
        onSceneReady.TriggerEvent(true);
    }
}