using UnityEngine;
using System.Collections;

public class Item3DViewer : MonoBehaviour
{
    [SerializeField] private Transform itemPrefab;
    [SerializeField] private float rotationSpeed = 30f;
    [SerializeField] private float autoRotationSpeed = 20f;
    [SerializeField] private Vector3 initialPosition = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 initialRotation = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 initialScale = new Vector3(1, 1, 1);

    private Transform currentItem;
    private bool isDragging = false;
    private Vector3 previousMousePosition;
    private bool isAutoRotating = true;

    private void Start()
    {
        if (itemPrefab != null)
        {
            ShowItem();
        }
    }

    public void ShowItem()
    {
        if (currentItem != null)
        {
            Destroy(currentItem.gameObject);
        }

        currentItem = Instantiate(itemPrefab, transform);
        currentItem.localPosition = initialPosition;
        currentItem.localRotation = Quaternion.Euler(initialRotation);
        currentItem.localScale = initialScale;
        isAutoRotating = true;
    }

    private void Update()
    {
        if (currentItem == null) return;

        // Inicia a rotação quando o botão esquerdo do mouse é pressionado
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            isAutoRotating = false;
            previousMousePosition = Input.mousePosition;
        }
        // Termina a rotação quando o botão é solto
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            isAutoRotating = true;
        }

        // Rotaciona o item enquanto arrasta
        if (isDragging)
        {
            Vector3 delta = Input.mousePosition - previousMousePosition;
            currentItem.Rotate(Vector3.up, -delta.x * rotationSpeed * Time.deltaTime, Space.World);
            currentItem.Rotate(Vector3.right, delta.y * rotationSpeed * Time.deltaTime, Space.World);
            previousMousePosition = Input.mousePosition;
        }
        // Auto-rotação quando não está sendo manipulado
        else if (isAutoRotating)
        {
            currentItem.Rotate(Vector3.up, autoRotationSpeed * Time.deltaTime, Space.World);
        }
    }

    public void HideItem()
    {
        if (currentItem != null)
        {
            Destroy(currentItem.gameObject);
            currentItem = null;
        }
    }
} 