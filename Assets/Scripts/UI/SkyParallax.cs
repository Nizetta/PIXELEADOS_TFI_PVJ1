using Unity.VisualScripting;
using UnityEngine;

public class SkyParallax : MonoBehaviour
{
    [Header("Sky Settings")]
    [SerializeField] private float scrollSpeed = 2f;
    [SerializeField] private float loopWidth = 40f;
    private Vector3 startPosition;
    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        float newX = Mathf.Repeat(Time.time * scrollSpeed, loopWidth);
        transform.position = new Vector3(startPosition.x - newX, startPosition.y, startPosition.z);
    }
}
