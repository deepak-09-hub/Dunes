using UnityEngine;
using UnityEngine.UI;

public class StoreBallItemUI : MonoBehaviour
{
    [SerializeField] private Button button;

    [SerializeField] private Image ballImage;

    [SerializeField] private GameObject lockedImage;

    [SerializeField] private GameObject selectedImage;

    private BallSkinData data;
    private BallStoreManager store;

    public BallSkinData Data => data;

    public void Setup(
        BallSkinData newData,
        BallStoreManager manager)
    {
        data = newData;
        store = manager;

        if (ballImage)
            ballImage.sprite = data.shopIcon;

        if (button)
        {
            button.onClick.RemoveAllListeners();

            button.onClick.AddListener(
                OnPressed
            );
        }

        Refresh();
    }

    private void OnPressed()
    {
        if (!store || !data)
            return;

        store.SelectPreview(data);
    }

    public void Refresh()
    {
        if (!store || !data)
            return;

        bool owned =
            store.IsOwned(data);

        bool selected =
            store.IsSelected(data);

        if (lockedImage)
            lockedImage.SetActive(!owned);

        if (selectedImage)
            selectedImage.SetActive(selected);
    }
}