using UnityEngine;
using System.Collections;

public class Item3DViewer : MonoBehaviour
{
    [SerializeField] private Transform itemPrefab;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private Vector3 initialPosition = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 initialRotation = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 initialScale = new Vector3(1, 1, 1);

    private Transform currentItem;
    private bool isDragging = false;
    private Vector3 previousMousePosition;

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
    }

    private void Update()
    {
        if (currentItem == null) return;

        // Inicia a rotação quando o botão esquerdo do mouse é pressionado
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            previousMousePosition = Input.mousePosition;
        }
        // Termina a rotação quando o botão é solto
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        // Rotaciona o item enquanto arrasta
        if (isDragging)
        {
            Vector3 delta = Input.mousePosition - previousMousePosition;
            currentItem.Rotate(Vector3.up, -delta.x * rotationSpeed * Time.deltaTime, Space.World);
            currentItem.Rotate(Vector3.right, delta.y * rotationSpeed * Time.deltaTime, Space.World);
            previousMousePosition = Input.mousePosition;
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