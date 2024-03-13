using UnityEngine;

public class BridgeMaterialsSwitch : MonoBehaviour
{
    public Material opaqueMaterial;
    public Material transparentMaterial;

    private MeshRenderer meshRenderer;
    void Start()
    {
       
        meshRenderer = GetComponent<MeshRenderer>();

        meshRenderer.material = opaqueMaterial;
    }

    public void SwitchToTransparent()
    {
        meshRenderer.material = transparentMaterial;
    }

    public void SwitchToOpaque()
    {
        meshRenderer.material = opaqueMaterial;
    }
}