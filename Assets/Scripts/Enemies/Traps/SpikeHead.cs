using System.Collections;
using UnityEngine;

public class SpikeHead : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private Vector2 direction = Vector2.down;
    [SerializeField] private float distance = 4f;
    [SerializeField] private float attackSpeed = 12f;
    [SerializeField] private float returnSpeed = 3f;

    [Header("Tiempos")]
    [SerializeField] private float waitTime = 1.5f;
    [SerializeField] private float warningTime = 0.6f;

    [Header("Aviso")]
    [SerializeField] private Animator animator;

    private Rigidbody2D rb;
    private Vector2 startPoint;
    private Vector2 endPoint;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPoint = rb.position;
        endPoint = startPoint + direction.normalized * distance;
    }

    private void Start()
    {
        StartCoroutine(Cycle());
    }

    private IEnumerator Cycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTime);

            if (animator != null) animator.SetTrigger("warning");
            yield return new WaitForSeconds(warningTime);
   
            yield return MoveTo(endPoint, attackSpeed);
            if (animator != null) animator.SetTrigger("hit");
            yield return new WaitForSeconds(0.3f);

            yield return MoveTo(startPoint, returnSpeed);
        }
    }

    private IEnumerator MoveTo(Vector2 target, float speed)
    {
        while ((rb.position - target).sqrMagnitude > 0.0001f)
        {
            rb.MovePosition(Vector2.MoveTowards(
                rb.position, target, speed * Time.fixedDeltaTime));
            yield return new WaitForFixedUpdate();
        }
    }

}
