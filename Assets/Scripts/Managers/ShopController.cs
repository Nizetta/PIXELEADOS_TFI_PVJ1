using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopController : MonoBehaviour
{
    [Header("Shop Canvas / Root")]
    [SerializeField] private GameObject shopCanvasRoot;

    [Header("Costs & Values")]
    [SerializeField] private int healthCost = 50;
    [SerializeField] private int healthBonus = 20;

    [SerializeField] private int attackCost = 75;
    [SerializeField] private int attackBonus = 5;

    [SerializeField] private int speedCost = 60;
    [SerializeField] private float speedBonus = 1.5f;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] private Button buyHealthButton;
    [SerializeField] private Button buyAttackButton;
    [SerializeField] private Button buySpeedButton;

    [SerializeField] private TextMeshProUGUI healthInfoText;
    [SerializeField] private TextMeshProUGUI attackInfoText;
    [SerializeField] private TextMeshProUGUI speedInfoText;

    private bool openedFromHud;

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += HandleScoreChanged;
        }
        RefreshUI();
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= HandleScoreChanged;
        }
    }

    private void Start()
    {
        if (shopCanvasRoot != null)
        {
            shopCanvasRoot.SetActive(false);
        }
        RefreshUI();
    }

    public void ToggleShopFromHUD()
    {
        if (shopCanvasRoot == null) return;

        bool willOpen = !shopCanvasRoot.activeSelf;
        if (willOpen)
        {
            OpenShop();
        }
        else
        {
            CloseShop();
        }
    }

    public void OpenShop()
    {
        if (shopCanvasRoot == null) return;

        openedFromHud = true;
        shopCanvasRoot.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        RefreshUI();
    }

    public void CloseShop()
    {
        if (shopCanvasRoot == null) return;

        shopCanvasRoot.SetActive(false);
        if (openedFromHud && GameManager.Instance != null && GameManager.Instance.State == GameState.Playing)
        {
            Time.timeScale = 1f;
        }
        openedFromHud = false;
        RefreshUI();
    }

    public void OnClickBuyHealth()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.HasEnoughScore(healthCost))
        {
            GameManager.Instance.BuyHealthUpgrade(healthCost, healthBonus);
            AudioManager.Instance?.PlayBuySuccess(transform);
        }
        else
        {
            AudioManager.Instance?.PlayBuyDenied(transform); 
        }
        RefreshUI();
    }

    public void OnClickBuyAttack()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.HasEnoughScore(attackCost))
        {
            GameManager.Instance.BuyAttackUpgrade(attackCost, attackBonus);
            AudioManager.Instance?.PlayBuySuccess(transform);
        }
        else
        {
            AudioManager.Instance?.PlayBuyDenied(transform);
        }
        RefreshUI();
    }

    public void OnClickBuySpeed()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.HasEnoughScore(speedCost))
        {
            GameManager.Instance.BuySpeedUpgrade(speedCost, speedBonus);
            AudioManager.Instance?.PlayBuySuccess(transform);
        }
        else
        {
            AudioManager.Instance?.PlayBuyDenied(transform);
        }
        RefreshUI();
    }

    private void HandleScoreChanged(int _)
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        int currentPoints = GameManager.Instance != null ? GameManager.Instance.Score : 0;
        PlayerStats stats = GameManager.Instance != null ? GameManager.Instance.Stats : null;

        if (pointsText != null)
            pointsText.text = $"Puntos: {currentPoints}";

        if (buyHealthButton != null)
            buyHealthButton.interactable = currentPoints >= healthCost;

        if (buyAttackButton != null)
            buyAttackButton.interactable = currentPoints >= attackCost;

        if (buySpeedButton != null)
            buySpeedButton.interactable = currentPoints >= speedCost;

        if (healthInfoText != null)
            healthInfoText.text = $"+{healthBonus} Vida (Max: {(stats != null ? stats.MaxHealth : 100)}) - {healthCost} pts";

        if (attackInfoText != null)
            attackInfoText.text = $"+{attackBonus} Golpe (Daño: {(stats != null ? stats.AttackDamage : 10)}) - {attackCost} pts";

        if (speedInfoText != null)
            speedInfoText.text = $"+{speedBonus:0.#} Vel. (Vel: {(stats != null ? stats.MovementSpeed : 5f):0.#}) - {speedCost} pts";
    }


}
