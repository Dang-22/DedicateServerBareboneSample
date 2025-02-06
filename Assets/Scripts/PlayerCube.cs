using Unity.Netcode;
using UnityEngine;

public class PlayerCube : NetworkBehaviour
{
    private NetworkVariable<Color> networkColor = new NetworkVariable<Color>(
        Color.white,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    private MeshRenderer meshRenderer;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Set initial color
            SetColor(Color.white);
        }

        // Subscribe to color changes
        networkColor.OnValueChanged += OnColorChanged;
    }

    public override void OnNetworkDespawn()
    {
        networkColor.OnValueChanged -= OnColorChanged;
    }

    private void OnColorChanged(Color previousValue, Color newValue)
    {
        UpdateMeshColor(newValue);
    }

    private void UpdateMeshColor(Color newColor)
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = newColor;
        }
    }

    public void SetColor(Color newColor)
    {
        if (IsOwner)
        {
            networkColor.Value = newColor;
            PlayerData.Instance.PlayerColor = newColor; // Store color in PlayerData
        }
    }
} 