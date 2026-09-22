using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private int value = 1;

    private bool collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected)
            return;

        Player player = other.GetComponentInParent<Player>();

        if (!player)
            return;

        Collect();
    }

    private void Collect()
    {
        collected = true;

        GameManager.I?.AddCoins(value);

        AudioManager.Instance?.PlayCoinCollect();

        Destroy(gameObject);
    }
}