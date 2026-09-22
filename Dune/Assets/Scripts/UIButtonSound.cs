using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (button)
        {
            button.onClick.AddListener(
                PlayClickSound
            );
        }
    }

    private void PlayClickSound()
    {
        if (AudioManager.Instance)
        {
            AudioManager.Instance
                .PlayButtonClick();
        }
    }

    private void OnDestroy()
    {
        if (button)
        {
            button.onClick.RemoveListener(
                PlayClickSound
            );
        }
    }
}