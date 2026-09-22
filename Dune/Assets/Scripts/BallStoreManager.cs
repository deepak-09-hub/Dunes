using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BallStoreManager : MonoBehaviour
{
    [Header("Store")]
    [SerializeField] private GameObject storePanel;

    [SerializeField] private Button storeButton;
    [SerializeField] private Button closeButton;

    [Header("Ball Data")]
    [SerializeField] private BallSkinData[] balls;

    [Header("Grid")]
    [SerializeField] private Transform contentParent;

    [SerializeField]
    private StoreBallItemUI itemPrefab;

    [Header("Preview")]
    [SerializeField] private Image previewImage;
    [SerializeField] private TMP_Text priceText;

    [Header("Action")]
    [SerializeField] private Button actionButton;
    [SerializeField] private TMP_Text actionButtonText;

    [Header("Player")]
    [SerializeField] private SpriteRenderer playerSprite;

    private const string SelectedBallKey =
        "SelectedBall";

    private readonly List<StoreBallItemUI>
        spawnedItems = new();

    private BallSkinData previewedBall;
    private string selectedBallId;

    private void Start()
    {
        if (storePanel)
            storePanel.SetActive(false);

        if (storeButton)
        {
            storeButton.onClick.AddListener(
                OpenStore
            );
        }

        if (closeButton)
        {
            closeButton.onClick.AddListener(
                CloseStore
            );
        }

        if (actionButton)
        {
            actionButton.onClick.AddListener(
                ActionPressed
            );
        }

        InitializeOwnership();
        LoadSelectedBall();
        CreateStoreItems();
        ApplySelectedBall();

        if (balls != null &&
            balls.Length > 0)
        {
            BallSkinData selected =
                FindBall(selectedBallId);

            SelectPreview(
                selected != null
                    ? selected
                    : balls[0]
            );
        }
    }

    private void InitializeOwnership()
    {
        if (balls == null)
            return;

        foreach (BallSkinData ball in balls)
        {
            if (!ball)
                continue;

            if (ball.unlockedByDefault)
            {
                PlayerPrefs.SetInt(
                    GetOwnedKey(ball),
                    1
                );
            }
        }

        PlayerPrefs.Save();
    }

    private void LoadSelectedBall()
    {
        selectedBallId =
            PlayerPrefs.GetString(
                SelectedBallKey,
                ""
            );

        if (!string.IsNullOrEmpty(
                selectedBallId))
        {
            return;
        }

        BallSkinData startingBall =
            GetFirstOwnedBall();

        if (startingBall)
        {
            selectedBallId =
                startingBall.ballId;

            SaveSelectedBall();
        }
    }

    private void CreateStoreItems()
    {
        if (!contentParent ||
            !itemPrefab ||
            balls == null)
        {
            return;
        }

        foreach (BallSkinData ball in balls)
        {
            if (!ball)
                continue;

            StoreBallItemUI item =
                Instantiate(
                    itemPrefab,
                    contentParent
                );

            item.Setup(
                ball,
                this
            );

            spawnedItems.Add(item);
        }
    }

    public void OpenStore()
    {
        if (storePanel)
            storePanel.SetActive(true);

        RefreshAll();
    }

    public void CloseStore()
    {
        if (storePanel)
            storePanel.SetActive(false);
    }

    public void SelectPreview(
        BallSkinData ball)
    {
        if (!ball)
            return;

        previewedBall = ball;

        if (previewImage)
            previewImage.sprite =
                ball.shopIcon;

        RefreshPreview();
    }

    private void ActionPressed()
    {
        if (!previewedBall)
            return;

        if (!IsOwned(previewedBall))
        {
            TryUnlock(previewedBall);
            return;
        }

        if (!IsSelected(previewedBall))
        {
            SelectBall(previewedBall);
        }
    }

    private void TryUnlock(
        BallSkinData ball)
    {
        if (!GameManager.I)
            return;

        bool purchased =
            GameManager.I.TrySpendCoins(
                ball.price
            );

        if (!purchased)
        {
            Debug.Log(
                "Not enough coins."
            );

            return;
        }

        PlayerPrefs.SetInt(
            GetOwnedKey(ball),
            1
        );

        PlayerPrefs.Save();

        // Automatically equip newly purchased ball.
        SelectBall(ball);

        RefreshAll();
    }

    private void SelectBall(
        BallSkinData ball)
    {
        selectedBallId =
            ball.ballId;

        SaveSelectedBall();

        ApplySelectedBall();

        RefreshAll();
    }

    private void SaveSelectedBall()
    {
        PlayerPrefs.SetString(
            SelectedBallKey,
            selectedBallId
        );

        PlayerPrefs.Save();
    }

    public bool IsOwned(
        BallSkinData ball)
    {
        if (!ball)
            return false;

        if (ball.unlockedByDefault)
            return true;

        return PlayerPrefs.GetInt(
            GetOwnedKey(ball),
            0
        ) == 1;
    }

    public bool IsSelected(
        BallSkinData ball)
    {
        if (!ball)
            return false;

        return ball.ballId ==
               selectedBallId;
    }

    private string GetOwnedKey(
        BallSkinData ball)
    {
        return "BallOwned_" +
               ball.ballId;
    }

    private void RefreshPreview()
    {
        if (!previewedBall)
            return;

        bool owned =
            IsOwned(previewedBall);

        bool selected =
            IsSelected(previewedBall);

        if (!owned)
        {
            if (priceText)
            {
                priceText.gameObject
                    .SetActive(true);

                priceText.text =
                    previewedBall.price +
                    " Coins";
            }

            if (actionButtonText)
            {
                actionButtonText.text =
                    "UNLOCK";
            }

            if (actionButton)
                actionButton.interactable = true;

            return;
        }

        if (priceText)
            priceText.gameObject.SetActive(false);

        if (selected)
        {
            if (actionButtonText)
                actionButtonText.text =
                    "ACTIVE";

            if (actionButton)
                actionButton.interactable = false;
        }
        else
        {
            if (actionButtonText)
                actionButtonText.text =
                    "USE";

            if (actionButton)
                actionButton.interactable = true;
        }
    }

    private void RefreshAll()
    {
        RefreshPreview();

        foreach (
            StoreBallItemUI item
            in spawnedItems)
        {
            if (item)
                item.Refresh();
        }
    }

    private BallSkinData
        GetFirstOwnedBall()
    {
        if (balls == null)
            return null;

        foreach (BallSkinData ball in balls)
        {
            if (ball && IsOwned(ball))
                return ball;
        }

        return null;
    }

    private BallSkinData FindBall(
        string id)
    {
        if (balls == null)
            return null;

        foreach (BallSkinData ball in balls)
        {
            if (ball &&
                ball.ballId == id)
            {
                return ball;
            }
        }

        return null;
    }

    private void ApplySelectedBall()
    {
        if (!playerSprite)
            return;

        BallSkinData ball =
            FindBall(selectedBallId);

        if (!ball ||
            !ball.gameplaySprite)
        {
            return;
        }

        playerSprite.sprite =
            ball.gameplaySprite;
    }
}