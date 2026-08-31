using UnityEngine;

public class BreakableMaterial : MonoBehaviour
{
    [Header("Material")]
    public string materialType;

    [Header("Health")]
    public int maxHealth = 3;

    private int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public bool TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log(
            gameObject.name +
            " HP: " +
            currentHealth +
            "/" +
            maxHealth
        );

        if (currentHealth <= 0)
        {
            Break();
            return true;
        }

        return false;
    }

    private void Break()
    {
        Debug.Log(gameObject.name + " BROKE!");

        Destroy(gameObject);
    }
}