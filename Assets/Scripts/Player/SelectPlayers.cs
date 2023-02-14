 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SelectPlayers : MonoBehaviour
{
    [SerializeField]
    private Transform leftBound;
    [SerializeField]
    private Transform rightBound;

    public void AddPlayer(PlayerInput playerInput)
    {
        GameObject root = PlayerManagerData.Instance.AddPlayer(playerInput);
        PlayerComponents component = root.GetComponent<PlayerComponents>();
        component.ModelSwapper.SwapJackhammerMaterial(PlayerManagerData.Instance.Materials[PlayerManagerData.Instance.PlayerCount-1]);
        AlignPlayers();
    }
    
    public void RemovePlayer(PlayerInput playerInput)
    {
        PlayerManagerData.Instance.RemovePlayer(playerInput);
    }

    private void AlignPlayers()
    {
        List<Vector3> newPositions = PlayerManagerData.Instance.GenerateSpawnPoints(leftBound.position, rightBound.position);
        foreach(KeyValuePair<PlayerInput, int> player in PlayerManagerData.Instance.Players)
        {
            player.Key.transform.position = newPositions[player.Value];
            player.Key.gameObject.GetComponent<PlayerComponents>().JackhammerRB.constraints = RigidbodyConstraints.FreezePosition;
        }
    }
}