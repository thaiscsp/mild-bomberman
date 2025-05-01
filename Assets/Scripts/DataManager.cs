using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    public int Lives { get; set; } = 5;
    public int Score { get; set; } = 0;

    public int level = 1;

    [Header("Player")]
    public int totalBombs = 1;
    public int explosionRadius = 1;
    public float speed = 3;

    private void Awake()
    {
        SetAsSingleton();
    }

    private void SetAsSingleton()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

}
