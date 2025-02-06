using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ColorUIManager : MonoBehaviour
{
    [SerializeField] private Button redButton;
    [SerializeField] private Button greenButton;
    [SerializeField] private Button blueButton;

    private void Start()
    {
        redButton.onClick.AddListener(() => ChangeColor(Color.red));
        greenButton.onClick.AddListener(() => ChangeColor(Color.green));
        blueButton.onClick.AddListener(() => ChangeColor(Color.blue));
    }

    private void ChangeColor(Color color)
    {
        var playerObject = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (playerObject != null)
        {
            var playerCube = playerObject.GetComponent<PlayerCube>();
            if (playerCube != null)
            {
                playerCube.SetColor(color);
            }
        }
    }

    private void OnDestroy()
    {
        redButton.onClick.RemoveAllListeners();
        greenButton.onClick.RemoveAllListeners();
        blueButton.onClick.RemoveAllListeners();
    }
} 