using UnityEngine;

public class MovementPlayer : MonoBehaviour
{
    public float speed = 5f; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float velocidadx = Input.GetAxis("Horizontal")* speed * Time.deltaTime;
        Vector3 posicion= transform.position;
        transform.position = new Vector3(posicion.x + velocidadx, posicion.y, posicion.z);
    }
}
