using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance { get; private set; }
    
    public Color PlayerColor { get; set; } = Color.white;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this object across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }
} 