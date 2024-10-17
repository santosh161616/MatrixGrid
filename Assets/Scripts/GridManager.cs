using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    [Header("General")]
    public Canvas parentCanvas;
    public float cellSize = 75f;
    public float cellSpacing = 5f;

    [Header("References")]
    public GameObject cellPrefab;

    [Header("List & Array")]
    public float[,] grid;
    public List<GameObject> spawnedCells;
    public int[] userArray;

    [Header("UI")]
    public TMP_InputField columns, rows, userDefinedArray;
    public Button generateBtn, resetBtn;

    private void Start()
    {
        generateBtn.onClick.AddListener(ButtonAction);
        resetBtn.onClick.AddListener(ClearOldMatrix);
    }
    
    void ButtonAction()
    {
        int x = ConvertToInt(columns.text);
        int y = ConvertToInt(rows.text);

        GenerateMatrix(x, y);
    }

    //Clear Old Matrix data
    void ClearOldMatrix()
    {
        if (spawnedCells != null)
        {
            foreach (var cell in spawnedCells)
            {
                cell.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack);

                CanvasGroup canvasGroup = cell.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.DOFade(0f, 0.5f);
                }


                Destroy(cell, 0.5f);
            }
            spawnedCells.Clear();
        }
    }

    //Converting String to int --> value that We're taking in input field.
    int ConvertToInt(string value)
    {
        if (int.TryParse(value, out int result))
        {
            return result;
        }
        else
        {
            Debug.Log("Parsing Failed!!");
            return 0;
        }
    }

    //Simply converting the user defined Array into Int
    int[] GetUserDefinedInput(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            int[] userInput = value.Split(',')
                .Select(int.Parse)
                .ToArray();
            return userInput;
        }
        else
        {
            Debug.Log("Parsing Failed!!");
            return null;
        }
    }

    //Generating the Matrix --> With user defined Columns & Rows
    void GenerateMatrix(int columns, int rows)
    {
        grid = new float[columns, rows];
        ClearOldMatrix();

        userArray = GetUserDefinedInput(userDefinedArray.text);

        if (userArray == null || userArray.Length == 0)
        {
            Debug.LogWarning("User array is empty. Please provide a valid input.");
            return;
        }


        int[] shuffledArray = userArray.OrderBy(n => Random.value).ToArray();
        int currentIndex = 0;

        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {

                grid[i, j] = GetValidNumberForPosition(i, j, shuffledArray, ref currentIndex);
                SpawnTiles(i, j, grid[i, j]);
            }
        }
    }

    //Checking if  the Value is valid Depending on Posioin.
    float GetValidNumberForPosition(int x, int y, int[] shuffledArray, ref int currentIndex)
    {
        int initialIndex = currentIndex;
        do
        {
            int value = shuffledArray[currentIndex];
            currentIndex = (currentIndex + 1) % shuffledArray.Length;

            if (IsValidPlacement(x, y, value))
            {
                return value;
            }


            if (currentIndex == initialIndex)
            {
                return value;
            }
        } while (true);
    }


    // This portion of the code have bug, works sometimes but still there is issue needs to checked again.
    bool IsValidPlacement(int x, int y, int value)
    {
        int diamondSize = Mathf.Min(2, userArray.Length - 1);
        for (int dx = -diamondSize; dx <= diamondSize; dx++)
        {
            for (int dy = -diamondSize; dy <= diamondSize; dy++)
            {
                int neighborX = x + dx;
                int neighborY = y + dy;

                if (dx == 0 && dy == 0) continue;

                if (Mathf.Abs(dx) + Mathf.Abs(dy) > diamondSize) continue;


                if (neighborX >= 0 && neighborX < grid.GetLength(0) && neighborY >= 0 && neighborY < grid.GetLength(1))
                {
                    if (grid[neighborX, neighborY] == value)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    //Spawing tiles based on grid Values
    public void SpawnTiles(int x, int y, float value)
    {
        GameObject obj = Instantiate(cellPrefab, parentCanvas.transform);

        Image imageComponent = obj.GetComponent<Image>();

        if (imageComponent != null)
        {
            imageComponent.color = GetColorFromValue(value);
        }

        spawnedCells.Add(obj);

        float posX = x * (cellSize + cellSpacing);
        float posY = y * (cellSize + cellSpacing);
        obj.transform.localScale = Vector3.zero;
        obj.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBounce);

        obj.transform.localPosition = new Vector3(posX, posY, 0);

        TextMeshProUGUI textComponent = obj.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = value.ToString();
        }

        RectTransform rectTransform = obj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(cellSize, cellSize);
        }
    }

    // Just Random Colors for Tiles --> Between light Red & light Yellow
    private Color GetColorFromValue(float value)
    {        
        float minValue = 1f; 
        float maxValue = 10f; 
        float normalizedValue = Mathf.InverseLerp(minValue, maxValue, value);

        Color startColor = new Color(1f, 0.8f, 0.8f);  //Light Red
        Color endColor = new Color(1f, 1f, 0.8f);  //Light yellow

        return Color.Lerp(startColor, endColor, normalizedValue);
    }
}
