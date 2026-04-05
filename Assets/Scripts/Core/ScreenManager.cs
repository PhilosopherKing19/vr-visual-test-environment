using NUnit.Framework;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;
using System.Collections.Generic;

public class ScreenManager : MonoBehaviour
{
    public List<GameObject> generatedScreens = new List<GameObject>();

    public List<GameObject> GetGeneratedScreens() { return generatedScreens; }
    public enum LayoutType
    {
        Linear,
        Grid
    }
    

    [SerializeField] private LayoutType layoutType;

    // Linear layout settings
    [SerializeField] private GameObject screenPrefab;
    [SerializeField] private int screenCount = 2;
    [SerializeField] private float horizontalSpacing = 1.5f;
    [SerializeField] private float depthSpacing = 1.0f;
    [SerializeField] private Vector3 startPosition = new Vector3(0f, 1.5f, 1.5f);
    [SerializeField] private Vector3 defaultRotation = new Vector3(0f,180f,0f);
    [SerializeField] private Transform screenContainer;
    [SerializeField] private GameObject screenContainerPrefab;
    [SerializeField] private bool centerLayout = true;

    // Grid layout settings
    [SerializeField] private float verticalSpacing;
    [SerializeField] private int rowCount;
    [SerializeField] private int columnCount;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(layoutType == LayoutType.Linear) GenerateLinearScreens();
        else if(layoutType == LayoutType.Grid) GenerateGridScreens();
        print(generatedScreens.Count);
    }

    
    void GenerateLinearScreens()
    {
        float centerOffset = (screenCount - 1) / 2;
        for (int i = 1; i <= screenCount; i++)
        {   // Werte der Screens werden berrechnet
            float y = startPosition.y;
            float z = startPosition.z + i * depthSpacing;
            float x = (i - centerOffset) * horizontalSpacing;
            Vector3 position = new Vector3(x, y, z);

            // Screens werden erzeugt und Werte, werden übergeben
            GameObject neuerScreen = Instantiate(screenPrefab);
            neuerScreen.transform.position = position;

            // muss in Quaternion umgewandelt werden, da unity Quaternion erwartet und nicht Vector3
            neuerScreen.transform.rotation = Quaternion.Euler(defaultRotation);
        }
    }


    void GenerateGridScreens()
    {
        float columnCenterOffset = (columnCount - 1) / 2f;
        float rowCenterOffset = (rowCount - 1) / 2f;

        for(int row = 0; row < rowCount; row++){
            for(int col = 0; col < columnCount; col++){

                // x berrechnung
                float xOffset = (col - columnCenterOffset) * horizontalSpacing;
                float x = startPosition.x + xOffset;
            
                // y berrechnung
                float yOffset = (rowCenterOffset - row) * verticalSpacing;
                float y = startPosition.y + yOffset;

                // z berrechnung
                float z = startPosition.z;

                Vector3 position = new Vector3(x, y, z);

                // Screens werden erzeugt und Werte, werden übergeben
                GameObject neuerScreen = Instantiate(screenPrefab);
                generatedScreens.Add(neuerScreen);
                print(generatedScreens.Count);
                print(gameObject.name);
                neuerScreen.transform.SetParent(screenContainer);
                neuerScreen.transform.position = position;

                // muss in Quaternion umgewandelt werden, da unity Quaternion erwartet und nicht Vector3
                neuerScreen.transform.rotation = Quaternion.Euler(defaultRotation);

            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
