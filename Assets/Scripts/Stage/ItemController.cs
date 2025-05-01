using UnityEngine;

public class ItemController : MonoBehaviour
{
    public enum ItemName
    {
        BombUp,
        FireUp,
        SpeedUp,
        Kick,
        Vest,
        BlockPass,
        RemoteControl,
        Time,
        OneUp
    };
    public ItemName itemName;
    public int points;

    SFXManager sfxManager;

    private void Start()
    {
        sfxManager = FindFirstObjectByType<SFXManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            sfxManager.PlayClip(sfxManager.itemGet);

            PlayerOneController playerOneController = collision.GetComponent<PlayerOneController>();

            switch(itemName)
            {
                case ItemName.BombUp:
                    DataManager.instance.totalBombs++;
                    playerOneController.BombsRemaining++;
                    break;

                case ItemName.FireUp:
                    DataManager.instance.explosionRadius++;
                    break;

                case ItemName.SpeedUp:
                    DataManager.instance.speed += 0.5f;
                    break;

                case ItemName.BlockPass:
                    playerOneController.GetComponent<Collider2D>().excludeLayers |= LayerMask.GetMask("Destructible");
                    break;

                case ItemName.OneUp:
                    if (DataManager.instance.Lives < 9) DataManager.instance.Lives++;
                    break;
            }

            DataManager.instance.Score += points;

            Destroy(gameObject);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Explosion"))
        {
            Destroy(gameObject);
        }
    }

}
