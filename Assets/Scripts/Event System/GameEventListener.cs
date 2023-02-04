using UnityEngine;
using UnityEngine.Events;
public class GameEventListener : MonoBehaviour
{
    public GameEvent gameEvent;
    public UnityEvent onEventTriggered;
    [System.Serializable]
    public class OneParameterUnityEvent<T> : UnityEvent<T> { }
    [SerializeField]
    private OneParameterUnityEvent<bool> onBoolTrigger;
    [SerializeField]
    private OneParameterUnityEvent<int> onIntTrigger;
    [SerializeField]
    private OneParameterUnityEvent<float> onFloatTrigger;
    [SerializeField]
    private OneParameterUnityEvent<string> onStringTrigger;
    [SerializeField]
    private OneParameterUnityEvent<Vector2> onVector2Trigger;
    [SerializeField]
    private OneParameterUnityEvent<Vector2Int> onVector2IntTrigger;
    [SerializeField]
    private OneParameterUnityEvent<Vector3> onVector3Trigger;
    [SerializeField]
    private OneParameterUnityEvent<Vector3Int> onVector3IntTrigger;
    [SerializeField]
    private OneParameterUnityEvent<Transform> onTransformTrigger;
    [SerializeField]
    private OneParameterUnityEvent<GameObject> onGameObjectTrigger;
    void OnEnable()
    {
        gameEvent.AddListener(this);
    }
    void OnDisable()
    {
        gameEvent.RemoveListener(this);
    }

    public void OnEventTriggered()
    {
        onEventTriggered.Invoke();
    }

    public void OnEventTriggered<T>(T firstArgument)
    {
        switch(firstArgument)
        {
            case bool boolArgument:
                onBoolTrigger.Invoke(boolArgument);
                break;
            case int intArgument:
                onIntTrigger.Invoke(intArgument);
                break;
            case float floatArgument:
                onFloatTrigger.Invoke(floatArgument);
                break;
            case string stringArgument:
                onStringTrigger.Invoke(stringArgument);
                break;
            case Vector2 vector2Argument:
                onVector2Trigger.Invoke(vector2Argument);
                break;
            case Vector2Int vector2IntArgument:
                onVector2IntTrigger.Invoke(vector2IntArgument);
                break;
            case Vector3 vector3Argument:
                onVector3Trigger.Invoke(vector3Argument);
                break;
            case Vector3Int vector3IntArgument:
                onVector3IntTrigger.Invoke(vector3IntArgument);
                break;
            case Transform transformArgument:
                onTransformTrigger.Invoke(transformArgument);
                break;
            case GameObject gameObjectArgument:
                onGameObjectTrigger.Invoke(gameObjectArgument);
                break;
        }
    }
}