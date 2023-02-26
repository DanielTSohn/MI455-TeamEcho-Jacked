using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModelSwapper : MonoBehaviour
{
    [SerializeField]
    private SkinnedMeshRenderer jackhammerRenderer;
    [SerializeField]
    private SkinnedMeshRenderer playerRenderer;

    public void SwapJackhammerMaterial(Material newMaterial)
    {
        SwapMaterial(jackhammerRenderer, newMaterial);
    }

    public void SwapPlayerMaterial(Material newMaterial)
    {
        SwapMaterial(playerRenderer, newMaterial);
    }

    private void SwapMaterial(SkinnedMeshRenderer meshRenderer, Material newMaterial)
    {
        meshRenderer.material = newMaterial;
    }
}
