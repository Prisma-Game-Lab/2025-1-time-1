using UnityEngine;
using System.Collections;

public class Item3DViewer : MonoBehaviour
{
    [SerializeField] private Transform itemPrefab;
    [SerializeField] private float rotationSpeed = 30f;
    [SerializeField] private float autoRotationSpeed = 20f;
    [SerializeField] private Vector3 initialPosition = new Vector3(0, 0, -2);
    [SerializeField] private Vector3 initialRotation = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 initialScale = new Vector3(5, 5, 5);
    
    // Camera settings
    [SerializeField] private bool useCustomCamera = true;
    [SerializeField] private Camera itemCamera;
    [SerializeField] private int itemLayerIndex = 8; // Layer 8 is the first free user layer
    
    private Transform currentItem;
    private bool isDragging = false;
    private Vector3 previousMousePosition;
    private bool isAutoRotating = true;
    private Vector3 originalCameraPosition;
    private bool originalCameraOrthographic;
    private float originalCameraFOV;

    private void Start()
    {
        SetupCamera();
    }

    private void SetupCamera()
    {
        if (useCustomCamera)
        {
            if (itemCamera == null)
            {
                // Create a dedicated camera for the item viewer
                GameObject cameraObj = new GameObject("ItemViewerCamera");
                itemCamera = cameraObj.AddComponent<Camera>();
                itemCamera.transform.parent = transform;
                itemCamera.transform.localPosition = new Vector3(0, 0, -10);
                itemCamera.orthographic = false;
                itemCamera.fieldOfView = 60f;
                itemCamera.depth = Camera.main ? Camera.main.depth + 1 : 1; // Render on top of main camera
                itemCamera.clearFlags = CameraClearFlags.Depth; // Only render the item
                itemCamera.cullingMask = 1 << itemLayerIndex;
            }
        }
        else
        {
            itemCamera = Camera.main;
            if (itemCamera != null)
            {
                // Store original camera settings
                originalCameraPosition = itemCamera.transform.position;
                originalCameraOrthographic = itemCamera.orthographic;
                originalCameraFOV = itemCamera.fieldOfView;
            }
        }
    }

    public void ShowItem()
    {
        // Cleanup any existing item first
        if (currentItem != null)
        {
            Destroy(currentItem.gameObject);
            currentItem = null;
        }

        // Make sure camera is set up
        SetupCamera();

        // Create new item
        if (itemPrefab != null)
        {
            currentItem = Instantiate(itemPrefab, transform);
            currentItem.localPosition = initialPosition;
            currentItem.localRotation = Quaternion.Euler(initialRotation);
            currentItem.localScale = initialScale;
            
            // Set the item to the correct layer
            SetLayerRecursively(currentItem.gameObject, itemLayerIndex);

            // Position camera to view item
            if (itemCamera != null)
            {
                // Calculate bounds of the object
                Bounds bounds = new Bounds(currentItem.position, Vector3.one);
                Renderer[] renderers = currentItem.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    if (renderer != null)
                    {
                        bounds.Encapsulate(renderer.bounds);
                    }
                }

                // Adjust camera to fit object
                float objectSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                float distance = objectSize / Mathf.Tan(itemCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
                itemCamera.transform.position = bounds.center - itemCamera.transform.forward * distance;
            }

            isAutoRotating = true;
            Debug.Log($"Item shown: Position={currentItem.position}, Scale={currentItem.localScale}, Layer={currentItem.gameObject.layer}");
        }
        else
        {
            Debug.LogWarning("Item3DViewer: No item prefab assigned!");
        }
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        if (obj == null) return;
        
        try
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
            {
                if (child != null)
                {
                    SetLayerRecursively(child.gameObject, layer);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error setting layer for object {obj.name}: {e.Message}");
        }
    }

    private void Update()
    {
        if (currentItem == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            isAutoRotating = false;
            previousMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            isAutoRotating = true;
        }

        if (isDragging)
        {
            Vector3 delta = Input.mousePosition - previousMousePosition;
            currentItem.Rotate(Vector3.up, -delta.x * rotationSpeed * Time.deltaTime, Space.World);
            currentItem.Rotate(Vector3.right, delta.y * rotationSpeed * Time.deltaTime, Space.World);
            previousMousePosition = Input.mousePosition;
        }
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

        // Restore original camera settings if using main camera
        if (!useCustomCamera && itemCamera != null)
        {
            itemCamera.transform.position = originalCameraPosition;
            itemCamera.orthographic = originalCameraOrthographic;
            itemCamera.fieldOfView = originalCameraFOV;
        }
    }

    private void OnDestroy()
    {
        // Cleanup: restore camera settings if using main camera
        if (!useCustomCamera && itemCamera != null)
        {
            itemCamera.transform.position = originalCameraPosition;
            itemCamera.orthographic = originalCameraOrthographic;
            itemCamera.fieldOfView = originalCameraFOV;
        }
    }
} 