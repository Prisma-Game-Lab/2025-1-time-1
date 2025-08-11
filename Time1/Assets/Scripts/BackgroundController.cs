using System.Collections.Generic;
using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    [System.Serializable]
    public class BackgroundElement
    {
        public GameObject prefab;
        public int quantity;
        public bool isMoon; // true se for lua (pra evitar que fiquem lado a lado)
    }

    [SerializeField] private RectTransform container;
    [SerializeField] private List<BackgroundElement> elements;
    [SerializeField] private int linesCount = 3;
    [SerializeField] private float minDistance = 100f;
    [SerializeField] private float minSpeed = 35f;
    [SerializeField] private float maxSpeed = 50f;

    private class ElementData
    {
        public GameObject obj;
        public Vector3 direction;
        public float speed;
        public bool isMoon;
    }

    private List<ElementData> activeElements = new List<ElementData>();
    private float minX, maxX, minY, maxY;
    private List<float> lineYPositions;

    void Start()
    {
        minX = -container.rect.width / 2f;
        maxX = container.rect.width / 2f;
        minY = -container.rect.height / 2f;
        maxY = container.rect.height / 2f;

        GenerateLinePositions();
        SpawnElements();
    }

    void GenerateLinePositions()
    {
        lineYPositions = new List<float>();
        float spacing = (maxY - minY) / (linesCount - 1);
        for (int i = 0; i < linesCount; i++)
        {
            lineYPositions.Add(minY + i * spacing);
        }
    }

    void SpawnElements()
    {
        List<Vector3> usedPositions = new List<Vector3>();
        Dictionary<float, bool> lineHasMoon = new Dictionary<float, bool>();

        foreach (float y in lineYPositions)
            lineHasMoon[y] = false;

        foreach (var element in elements)
        {
            for (int i = 0; i < element.quantity; i++)
            {
                Vector3 pos = Vector3.zero;
                bool valid = false;
                int attempts = 0;
                float chosenY = 0f;

                while (!valid && attempts < 20)
                {
                    chosenY = lineYPositions[Random.Range(0, lineYPositions.Count)];

                    // Se for lua, não pode ter outra lua nessa linha
                    if (element.isMoon && lineHasMoon[chosenY])
                    {
                        attempts++;
                        continue;
                    }

                    float x = Random.Range(minX, maxX);
                    pos = new Vector3(x, chosenY, 0f);

                    valid = true;
                    foreach (var used in usedPositions)
                    {
                        if (Vector3.Distance(new Vector3(used.x, pos.y, 0f), pos) < minDistance)
                        {
                            valid = false;
                            break;
                        }
                    }

                    attempts++;
                }

                if (element.isMoon) lineHasMoon[chosenY] = true;

                GameObject obj = Instantiate(element.prefab, container);
                obj.GetComponent<RectTransform>().localPosition = pos;

                activeElements.Add(new ElementData
                {
                    obj = obj,
                    direction = Vector3.up,
                    speed = Random.Range(minSpeed, maxSpeed),
                    isMoon = element.isMoon
                });

                usedPositions.Add(pos);
            }
        }
    }

    void Update()
    {
        foreach (var element in activeElements)
        {
            RectTransform rt = element.obj.GetComponent<RectTransform>();
            rt.localPosition += element.direction * element.speed * Time.deltaTime;

            if (rt.localPosition.y > maxY)
            {
                float newY = minY - Random.Range(0f, 30f);
                float newX = Random.Range(minX, maxX);

                rt.localPosition = new Vector3(newX, newY, 0f);
            }
        }
    }
}
