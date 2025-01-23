using Unity.Netcode;
using UnityEngine;

public class CubeCustom : NetworkBehaviour
{
    
    private NetworkVariable<Color> cubeColor = new NetworkVariable<Color>(Color.white, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    [SerializeField] private Renderer cubeRenderer;
    

    private void Start()
    {
        
        cubeColor.OnValueChanged += (oldColor, newColor) =>
        {
            cubeRenderer.material.color = newColor;
        };

        
        if (IsOwner)
        {
            cubeRenderer.material.color = cubeColor.Value;
        }
    }

    
    public void SetColor(Color newColor)
    {
        if (IsOwner)
        {
            cubeColor.Value = newColor;
        }
    }
}