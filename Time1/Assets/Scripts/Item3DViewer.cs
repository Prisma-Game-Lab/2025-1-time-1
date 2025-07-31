using UnityEngine;
using UnityEngine.Events;

public class Item3DViewer : MonoBehaviour
{
    [Header("Configuração de Visualização")]
    public Transform itemHolder; // Onde os itens 3D estão
    public float rotationSpeed = 100f;
    public float viewDuration = 5f;
    public UnityEvent onViewFinished;

    private GameObject currentItem;
    private float timer;
    private bool isViewing = false;

    void Awake()
    {
        DesativarTodosOsItens();
        gameObject.SetActive(false); // Viewer começa invisível
    }

    public void ShowItem(string itemName)
    {
        DesativarTodosOsItens();

        Transform item = itemHolder.Find(itemName);
        if (item != null)
        {
            currentItem = item.gameObject;
            currentItem.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Item3DViewer: Item não encontrado: " + itemName);
            onViewFinished?.Invoke(); // Avança mesmo sem item
            return;
        }

        timer = 0f;
        isViewing = true;
        gameObject.SetActive(true); // Ativa o objeto que contém o viewer
    }

    void Update()
    {
        if (!isViewing) return;

        timer += Time.deltaTime;

        if (currentItem != null)
        {
            currentItem.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }

        if (timer >= viewDuration)
        {
            HideItem();
        }
    }

    public void HideItem()
    {
        if (currentItem != null)
        {
            currentItem.SetActive(false);
        }

        isViewing = false;
        gameObject.SetActive(false);
        onViewFinished?.Invoke();
    }

    private void DesativarTodosOsItens()
    {
        if (itemHolder == null) return;

        foreach (Transform child in itemHolder)
        {
            child.gameObject.SetActive(false);
        }
    }
}
