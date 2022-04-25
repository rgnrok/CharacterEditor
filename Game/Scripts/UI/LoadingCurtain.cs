using UnityEngine;
using UnityEngine.UI;

public class LoadingCurtain : MonoBehaviour
{
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private GameObject loadingPanel;

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void SetLoading(float percentage)
    {
        if (loadingPanel == null)
            return;

        if (percentage >= 100)
        {
            loadingPanel.SetActive(false);

            return;
        }

        if (percentage < 100 && !loadingPanel.activeSelf)
            loadingPanel.SetActive(true);

        loadingSlider.value = percentage / 100f;
    }

}
