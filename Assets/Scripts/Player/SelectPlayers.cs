 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SelectPlayers : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The data to write players into")]
    private PlayerManagerData managerData;
    [SerializeField]
    private Transform leftBound;
    [SerializeField]
    private Transform rightBound;

    public void AddPlayer(PlayerInput playerInput)
    {
        GameObject root = managerData.AddPlayer(playerInput);
        PlayerComponents component = root.GetComponent<PlayerComponents>();
        component.ModelSwapper.SwapJackhammerMaterial(managerData.Materials[managerData.PlayerCount-1]);
        AlignPlayers();
    }
    
    public void RemovePlayer(PlayerInput playerInput)
    {
        managerData.RemovePlayer(playerInput);
    }

    private void AlignPlayers()
    {
        List<Vector3> newPositions = managerData.GenerateSpawnPoints(leftBound.position, rightBound.position);
        foreach(KeyValuePair<int, PlayerInput> player in managerData.players)
        {
            player.Value.transform.position = newPositions[player.Key];
            player.Value.gameObject.GetComponent<PlayerComponents>().JackhammerRB.constraints = RigidbodyConstraints.FreezePosition;
        }
    }
}