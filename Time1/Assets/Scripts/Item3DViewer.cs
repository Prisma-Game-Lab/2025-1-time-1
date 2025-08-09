using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Item3DViewer : MonoBehaviour
{
    [Header("Referências")]
    public Transform itemHolder;
    public Camera viewerCamera;
    public GameObject rawImageOverlay;
    public GameObject imageOverlay;
    public TextMeshProUGUI rewardText;

    [Header("Configurações")]
    public float rotationSpeed = 100f;
    public float minDisplayTime = 3f;
    public string nextSceneName;
    public string defaultMessage = "Você ganhou um item!";

    private GameObject currentItem;
    private float timer;
    private bool isViewing = false;
    private bool readyToExit = false;

    void Awake()
    {
        DesativarTodosOsItens();

        if (rawImageOverlay != null)
            rawImageOverlay.SetActive(false);

        if (imageOverlay != null)
            imageOverlay.SetActive(false);

        if (rewardText != null)
            rewardText.gameObject.SetActive(false);
    }

    public void ShowItem(string itemName)
    {
        DesativarTodosOsItens();

        Transform item = itemHolder.Find(itemName);
        if (item != null)
        {
            currentItem = item.gameObject;
            currentItem.SetActive(true);
            Debug.Log("[Item3DViewer] Exibindo item: " + itemName);
        }
        else
        {
            Debug.LogWarning("[Item3DViewer] Item não encontrado: " + itemName);
            return;
        }

        if (viewerCamera != null)
            viewerCamera.gameObject.SetActive(true);

        if (rawImageOverlay != null)
            rawImageOverlay.SetActive(true);

        if (imageOverlay != null)
            imageOverlay.SetActive(true);

        if (rewardText != null)
        {
            //rewardText.text = defaultMessage;
            rewardText.gameObject.SetActive(true);
        }

        timer = 0f;
        isViewing = true;
        readyToExit = false;
    }

    void Update()
    {
        if (!isViewing) return;

        timer += Time.deltaTime;

        if (currentItem != null)
            currentItem.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        if (timer >= minDisplayTime)
            readyToExit = true;

        if (readyToExit && Input.GetMouseButtonDown(0))
        {
            Debug.Log("[Item3DViewer] Clique detectado. Carregando próxima cena...");
            CarregarProximaCena();
        }
    }

    public void SetRewardMessage(string message)
    {
        if (rewardText != null)
            rewardText.text = message;
    }

    private void CarregarProximaCena()
    {
        if (currentItem != null)
            currentItem.SetActive(false);

        if (viewerCamera != null)
            viewerCamera.gameObject.SetActive(false);

        if (rawImageOverlay != null)
            rawImageOverlay.SetActive(false);

        if (imageOverlay != null)
            imageOverlay.SetActive(false);

        if (rewardText != null)
            rewardText.gameObject.SetActive(false);

        isViewing = false;
        readyToExit = false;

        SceneManager.LoadScene(nextSceneName);
    }

    private void DesativarTodosOsItens()
    {
        foreach (Transform child in itemHolder)
            child.gameObject.SetActive(false);
    }
}
