using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Game Event")]
public class GameEvent : ScriptableObject
{
    private List<GameEventListener> gameEventListeners = new List<GameEventListener>();
    public void AddListener(GameEventListener listener)
    {
        gameEventListeners.Add(listener);
    }

    public void RemoveListener(GameEventListener listener)
    {
        gameEventListeners.Remove(listener);
    }

    #region Game Event
    public void TriggerEvent()
    {
        for(int i = 0; i < gameEventListeners.Count; i++)
        {
            gameEventListeners[i].OnEventTriggered();
        }
    }
    #endregion

    #region Game Event; One Argument
    public void TriggerEvent<T>(T firstArgument)
    {
        for (int i = 0; i < gameEventListeners.Count; i++)
        {
            gameEventListeners[i].OnEventTriggered();
            switch(firstArgument)
            {
                case bool boolArgument:
                    gameEventListeners[i].OnEventTriggered(boolArgument);
                    break;
                case int intArgument:
                    gameEventListeners[i].OnEventTriggered(intArgument);
                    break;
                case float floatArgument:
                    gameEventListeners[i].OnEventTriggered(floatArgument);
                    break;
                case string stringArgument:
                    gameEventListeners[i].OnEventTriggered(stringArgument);
                    break;
                case Vector2 vector2Argument:
                    gameEventListeners[i].OnEventTriggered(vector2Argument);
                    break;
                case Vector2Int vector2IntArgument:
                    gameEventListeners[i].OnEventTriggered(vector2IntArgument);
                    break;
                case Vector3 vector3Argument:
                    gameEventListeners[i].OnEventTriggered(vector3Argument);
                    break;
                case Vector3Int vector3IntArgument:
                    gameEventListeners[i].OnEventTriggered(vector3IntArgument);
                    break;
                case Transform transformArgument:
                    gameEventListeners[i].OnEventTriggered(transformArgument);
                    break;
                case GameObject gameObjectArgument:
                    gameEventListeners[i].OnEventTriggered(gameObjectArgument);
                    break;
                case PlayerInput playerInputArgument:
                    gameEventListeners[i].OnEventTriggered(playerInputArgument);
                    break;
            }
        }
    }
    #endregion

    #region Game Event; Two Argument
    public void TriggerEvent<T, T2>(T firstArgument, T2 secondArgument)
    {
        for (int i = 0; i < gameEventListeners.Count; i++)
        {
            gameEventListeners[i].OnEventTriggered();
            switch ((firstArgument, secondArgument))
            {
                case (PlayerInput playerInput, float number) playerInputFloatArgument:
                    gameEventListeners[i].OnEventTriggered(playerInput, number);
                    break;
            }
        }
    }
    #endregion
}

