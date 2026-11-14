using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Stats/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("Item Data")]
    [SerializeField] private string itemName;
    [SerializeField] private Sprite itemIcon;

    [Header("Rewards")]
    [SerializeField] private int scoreValue = 20;
    [SerializeField] private int healthRestoreValue = 0;

    /*[Header("Audio")]
    [SerializeField] private AudioClip pickupSound;
    [Header("Visual Effects")]
    [SerializeField] private GameObject pickupVFXPrefab;
    */
    public string ItemName => itemName;
    public Sprite ItemIcon => itemIcon;
    public int ScoreValue => scoreValue;
    public int HealthRestoreValue => healthRestoreValue;
    //public AudioClip PickupSound => pickupSound;
    //public GameObject PickupVFXPrefab => pickupVFXPrefab;
}
