using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;
using System.Collections.Generic;
using NUnit.Framework;
public class VarScreenManager
{
    
    public enum LayoutType
    {
        Linear,
        Grid
    }




    // Linear layout settings
    [SerializeField] private GameObject screenPrefab;
    [SerializeField] private float horizontalSpacing = 1.5f;
    [SerializeField] private float depthSpacing = 1.0f;
    [SerializeField] private Vector3 startPosition = new(0f, 1.5f, 1.5f);
    [SerializeField] private Vector3 defaultRotation = new(0f, 0f, 0f);
    [SerializeField] private Transform screenContainer;

    // Grid layout settings
    [SerializeField] private float verticalSpacing;
    //[SerializeField] private int rowCount;
    //[SerializeField] private int columnCount;

    public VarScreenManager(GameObject screenPrefab)
    {
        this.screenPrefab = screenPrefab;
    }

    public List<GameObject> GenerateLinearScreens(int count)
    {
        List<GameObject> screens = new();
        float centerOffset = (count - 1) / 2;
        for (int i = 1; i <= count; i++)
        {   // Werte der Screens werden berrechnet
            float y = startPosition.y;
            float z = startPosition.z + i * depthSpacing;
            float x = (i - centerOffset) * horizontalSpacing;
            Vector3 position = new(x, y, z);

            // Screens werden erzeugt und Werte, werden übergeben
            GameObject neuerScreen = Object.Instantiate(screenPrefab);
            
            // muss in Quaternion umgewandelt werden, da unity Quaternion erwartet und nicht Vector3
            neuerScreen.transform.SetPositionAndRotation(position, Quaternion.Euler(defaultRotation));
            screens.Add(neuerScreen);
        }
        return screens;
    }


    public List<GameObject> GenerateGridScreens(int rowCount, int columnCount)
    {
        float columnCenterOffset = (columnCount - 1) / 2f;
        float rowCenterOffset = (rowCount - 1) / 2f;
        List<GameObject> screens = new();

        for (int row = 0; row < rowCount; row++)
            for (int col = 0; col < columnCount; col++)
            {
                // x berrechnung
                float xOffset = (col - columnCenterOffset) * horizontalSpacing;
                float x = startPosition.x + xOffset;

                // y berrechnung
                float yOffset = (rowCenterOffset - row) * verticalSpacing;
                float y = startPosition.y + yOffset;

                // z berrechnung
                float z = startPosition.z;

                Vector3 position = new(x, y, z);

                // Screens werden erzeugt und Werte, werden übergeben
                GameObject neuerScreen = Object.Instantiate(screenPrefab);
                neuerScreen.transform.SetParent(screenContainer);
                neuerScreen.transform.position = position;

                // muss in Quaternion umgewandelt werden, da unity Quaternion erwartet und nicht Vector3
                neuerScreen.transform.rotation = Quaternion.Euler(defaultRotation);
                screens.Add(neuerScreen);
            }
        return screens;
    }


}
