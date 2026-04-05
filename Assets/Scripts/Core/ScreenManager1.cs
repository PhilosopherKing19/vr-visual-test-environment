using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class ScreenManager1
{
    private GameObject screenPrefab;
    private Vector3 defaultRotation;
    private List<GameObject> screens;

    public ScreenManager1(GameObject screenPrefab, Vector3 defaultRotation)
    {
        this.screenPrefab = screenPrefab;
        this.defaultRotation = defaultRotation;
    }

    public List<GameObject> GenerateScreens(List<Vector3> positions)
    {
        
        screens = new List<GameObject>();
        for (int i = 0; i < positions.Count; i++)
        {
            GameObject neuerScreen = Object.Instantiate(screenPrefab);
            neuerScreen.transform.position = positions[i];

            float scale = positions[i].z < 0 ? -1 * positions[i].z : positions[i].z; // falls z negativ, dann wird mit -1 verrechnet, anderrnfalls wird z direkt übernommen

            neuerScreen.transform.localScale = new Vector3(scale, scale, 1f);
   
            neuerScreen.transform.rotation = Quaternion.Euler(defaultRotation);
            screens.Add(neuerScreen);
        }

        return screens;
    }

}
