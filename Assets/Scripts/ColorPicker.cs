using UnityEngine;
using UnityEngine.UI;

public class ColorPicker : MonoBehaviour
{
    [SerializeField] private Button redButton;
    [SerializeField] private Button greenButton;
    [SerializeField] private Button blueButton;

    private CubeCustom cubeCustom;

    private void Start()
    {
        
        cubeCustom = FindObjectOfType<CubeCustom>();
        redButton.onClick.AddListener(() => SetPlayerColor(Color.red));
        greenButton.onClick.AddListener(() => SetPlayerColor(Color.green));
        blueButton.onClick.AddListener(() => SetPlayerColor(Color.blue));
    }

    private void SetPlayerColor(Color color)
    {
        if (cubeCustom != null)
        {
            Debug.Log("Setting color to " + color);
            cubeCustom.SetColor(color);
        }
    }
}