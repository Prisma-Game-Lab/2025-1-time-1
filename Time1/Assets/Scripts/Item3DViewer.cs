using UnityEngine;
using UnityEngine.Events;

public class Item3DViewer : MonoBehaviour
{
    [Header("Visualização do Item 3D")]
    public Camera viewerCamera; // Câmera auxiliar
    public Transform itemHolder; // Empty no centro da cena/câmera
    public float rotationSpeed = 500f;
    public float viewDuration = 7f; // Tempo de exibição (padrão 7s)
    public UnityEvent onViewFinished; // Evento chamado ao terminar
    public GameObject mainCanvas; // Referência ao Canvas principal (Canvas 1)

    private GameObject currentItemInstance;
    private Vector3 lastMousePosition;
    private float timer;
    private bool isViewing = false;

    void Awake()
    {
        // Garante que todos os filhos estejam desativados ao iniciar
        if (itemHolder != null && itemHolder.childCount > 0)
        {
            foreach (Transform child in itemHolder)
                child.gameObject.SetActive(false);
        }
    }

    public void ShowItem()
    {
        if (itemHolder == null || viewerCamera == null || mainCanvas == null)
        {
            Debug.LogError("Item3DViewer: Referências não atribuídas!");
            return;
        }
        // Ativa o primeiro filho do itemHolder
        if (itemHolder.childCount > 0)
        {
            currentItemInstance = itemHolder.GetChild(0).gameObject;
            currentItemInstance.SetActive(true);
        }
        timer = 0f;
        isViewing = true;
        gameObject.SetActive(true);
        mainCanvas.SetActive(false); // Desativa o Canvas principal
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
            currentItemInstance.SetActive(false);
        }
        isViewing = false;
        gameObject.SetActive(false);
        if (mainCanvas != null)
            mainCanvas.SetActive(true); // Reativa o Canvas principal
    }
} 