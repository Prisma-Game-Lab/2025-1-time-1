using UnityEngine;
using UnityEngine.Events;

public class Item3DViewer : MonoBehaviour
{
    [Header("Visualização do Item 3D")]
    public Camera viewerCamera; // Câmera auxiliar para RenderTexture
    public RenderTexture renderTexture; // RenderTexture exibida no RawImage
    public Transform itemHolder; // Empty no centro da cena/câmera
    public GameObject itemPrefab; // Prefab do item a ser exibido
    public float rotationSpeed = 100f;
    public float viewDuration = 7f; // Tempo de exibição (padrão 7s)
    public UnityEvent onViewFinished; // Evento chamado ao terminar

    private GameObject currentItemInstance;
    private Vector3 lastMousePosition;
    private float timer;
    private bool isViewing = false;

    void Awake()
    {
        if (viewerCamera != null && renderTexture != null)
        {
            viewerCamera.targetTexture = renderTexture;
        }
        if (itemHolder != null && itemHolder.childCount > 0)
        {
            foreach (Transform child in itemHolder)
                Destroy(child.gameObject);
        }
    }

    public void ShowItem(GameObject prefab = null)
    {
        if (itemHolder == null || viewerCamera == null || renderTexture == null)
        {
            Debug.LogError("Item3DViewer: Referências não atribuídas!");
            return;
        }
        if (currentItemInstance != null)
            Destroy(currentItemInstance);
        GameObject toSpawn = prefab != null ? prefab : itemPrefab;
        currentItemInstance = Instantiate(toSpawn, itemHolder);
        currentItemInstance.transform.localPosition = Vector3.zero;
        currentItemInstance.transform.localRotation = Quaternion.identity;
        currentItemInstance.transform.localScale = Vector3.one;
        timer = 0f;
        isViewing = true;
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (!isViewing) return;
        timer += Time.unscaledDeltaTime;
        if (timer >= viewDuration)
        {
            isViewing = false;
            HideItem();
            onViewFinished?.Invoke();
            return;
        }
        HandleRotation();
    }

    void HandleRotation()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            float rotX = delta.y * rotationSpeed * Time.unscaledDeltaTime;
            float rotY = -delta.x * rotationSpeed * Time.unscaledDeltaTime;
            if (currentItemInstance != null)
            {
                currentItemInstance.transform.Rotate(viewerCamera.transform.up, rotY, Space.World);
                currentItemInstance.transform.Rotate(viewerCamera.transform.right, rotX, Space.World);
            }
            lastMousePosition = Input.mousePosition;
        }
    }

    public void HideItem()
    {
        if (currentItemInstance != null)
        {
            Destroy(currentItemInstance);
        }
        isViewing = false;
        gameObject.SetActive(false);
    }
} 