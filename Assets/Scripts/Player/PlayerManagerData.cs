using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "PlayerManagerData", menuName = "ScriptableObjects/PlayerManagerData")]
public class PlayerManagerData : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Does the object send debug messages")]
    private bool debugMessages = true;

    public List<Material> Materials { get { return materials; } private set { materials = value; } }
    [SerializeField]
    [Tooltip("The index of player prefabs to automatically assign to")]
    private List<Material> materials;
    [SerializeField]
    [Tooltip("Distance players will spawn from the center")]
    private float spawnDistance;

    /// <summary>
    /// The input manager to store players into
    /// </summary>
    [HideInInspector]
    public PlayerInputManager inputManager;

    public Dictionary<PlayerInput, int> Players { get; private set; } = new();
    public int PlayerCount { get; private set; } = 0;

    public static PlayerManagerData Instance { get; private set; }

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
            transform.parent = null;
            DontDestroyOnLoad(this);

            ResetPlayers();
        }
    }

    private void ResetPlayers()
    {
        Players.Clear();
        PlayerCount = Players.Count;
    }

    public GameObject AddPlayer(PlayerInput playerInput)
    {
        if (debugMessages) { Debug.Log("Attempting join"); }

        while (!Players.TryAdd(playerInput, PlayerCount))
        {
            if (debugMessages) { Debug.Log("Could not join at player number: " + PlayerCount); }
            PlayerCount++;
        }
        PlayerCount++;
        if (debugMessages) { Debug.Log("Joined player number " + PlayerCount); }

        return playerInput.gameObject;
    }

    public GameObject RemovePlayer(PlayerInput playerInput)
    {
        if (debugMessages) { Debug.Log("Attempting remove"); }
        if (Players.Remove(playerInput))
        {
            if (debugMessages) { Debug.Log("Removed player number " + PlayerCount); }
            PlayerCount--;
        }
        else { if (debugMessages) { Debug.Log("Could not remove at player number: " + PlayerCount); } }

        return playerInput.gameObject;
    }

    public void SpawnPlayers()
    {
        foreach (KeyValuePair<PlayerInput, int> player in Players)
        {
            inputManager.JoinPlayer(player.Value, player.Value, player.Key.currentControlScheme, player.Key.devices.ToArray());
        }
    }

    /// <summary>
    /// Generates spawn points along a circle's circumference 
    /// </summary>
    /// <param name="spawnRadius">The radius of the circle to spawn in</param>
    /// <returns>List of spawn points along the circle</returns>
    public List<Vector3> GenerateSpawnPoints(float spawnRadius)
    {
        List<Vector3> spawnPoints = new();
        float angleBetween = 360 / PlayerCount * Mathf.Deg2Rad;

        float angle = 0;
        for (int i = 0; i < PlayerCount; i++)
        {
            spawnPoints.Add(new Vector3(spawnRadius * Mathf.Cos(angle), 0, spawnRadius * Mathf.Sin(angle)));
            if (debugMessages) { Debug.Log("Spawn point: " + (i + 1) + " at " + spawnPoints[i] + " proportion " + (i + 1) + "/" + PlayerCount + " = " + ((float)(i + 1) / (PlayerCount + 1))); }
            angle += angleBetween;
        }

        return spawnPoints;
    }

    /// <summary>
    /// Generates spawn points between begin point and end point (inclusive), distributes evenly between
    /// </summary>
    /// <param name="begin">Start point to spawn from</param>
    /// <param name="end">Ending point to spawning</param>
    /// <returns>List of spawn points along given begin and end locations</returns>
    public List<Vector3> GenerateSpawnPoints(Vector3 begin, Vector3 end)
    {
        List<Vector3> spawnPoints = new();
        for (int i = 0; i < PlayerCount; i++)
        {
            spawnPoints.Add(Vector3.Lerp(begin, end, (float)i+1 / (PlayerCount+1)));
            if (debugMessages) { Debug.Log("Spawn point: " + (i+1) + " at " + spawnPoints[i] + " proportion " + (i+1) + "/" + PlayerCount + " = " + ((float)(i+1)/(PlayerCount+1))); }
        }

        return spawnPoints;
    }
}