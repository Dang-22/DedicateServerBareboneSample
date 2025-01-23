using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{ private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        
        if (!IsOwner) return;
       
        float moveX = Input.GetAxis("Horizontal") * Time.deltaTime * 5f;
        float moveZ = Input.GetAxis("Vertical") * Time.deltaTime * 5f;
        transform.Translate(new Vector3(moveX, 0, moveZ));
        UpdatePositionServerRpc(transform.position);
    }
    [ServerRpc]
    private void UpdatePositionServerRpc(Vector3 newPosition)
    {
        transform.position = newPosition;
        UpdatePositionClientRpc(newPosition);
    }

    [ClientRpc]
    private void UpdatePositionClientRpc(Vector3 newPosition)
    {
        if (!IsOwner)
            transform.position = newPosition;
    }
}
