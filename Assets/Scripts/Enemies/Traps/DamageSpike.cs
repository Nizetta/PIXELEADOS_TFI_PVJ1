using UnityEngine;

public class DamageSpike : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Jugador recibió daño");
    }

}
