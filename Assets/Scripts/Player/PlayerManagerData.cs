using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "PlayerManagerData", menuName = "ScriptableObjects/PlayerManagerData")]
public class PlayerManagerData : ScriptableObject
{
    [SerializeField]
    private bool debugMessages = true;
    [SerializeField]
    private PlayerInputManager inputManager;
    [SerializeField]
    private List<GameObject> prefabs;
    [SerializeField]
    [Tooltip("Distance players will spawn from the center")]
    private float spawnDistance;

    public Dictionary<int, PlayerInput> players { get; private set; }
    private int playerCount = 1;
    public void AddPlayer(PlayerInput input)
    {
        if (debugMessages) { Debug.Log("Attempting join"); }

        while (!players.TryAdd(playerCount, input))
        {
            if (debugMessages) { Debug.Log("Could not join at player number: " + playerCount); }
            playerCount++;
        }
        playerCount++;
        if (debugMessages) { Debug.Log("Joined player number " + playerCount); }
    }

    public void RemovePLayer(PlayerInput input)
    {
        if (debugMessages) { Debug.Log("Attempting remove"); }
        if (players.Remove(playerCount))
        {
            if (debugMessages) { Debug.Log("Removed player number " + playerCount); }
            playerCount--;
        }
        else { if (debugMessages) { Debug.Log("Could not remove at player number: " + playerCount); } }
    }

    public void SpawnPlayers()
    {
        List<Vector3> spawnPoints = GenerateSpawnPoints(spawnDistance, playerCount);
        foreach(KeyValuePair<int, PlayerInput> player in players)
        {
            inputManager.playerPrefab = prefabs[player.Key];
            PlayerInput temp = inputManager.JoinPlayer(player.Key, player.Key, player.Value.currentControlScheme, player.Value.devices.ToArray());
            temp.transform.position = spawnPoints[playerCount];
        }
    }

    private List<Vector3> GenerateSpawnPoints(float spawnRadius, int spawnPointsCount)
    {
        List<Vector3> spawnPoints = new();
        float angleBetween = 360 / spawnPointsCount * Mathf.Deg2Rad;

        float angle = 0;
        for(int i = 1; i <= spawnPointsCount; i ++)
        {
            spawnPoints.Add(new Vector3(spawnRadius * Mathf.Cos(angle), 0, spawnRadius * Mathf.Sin(angle)));
            if (debugMessages) { Debug.Log("Spawn point: " + i + " at " + spawnPoints[i]); }
            angle += angleBetween;
        }

        return spawnPoints;
    }
}