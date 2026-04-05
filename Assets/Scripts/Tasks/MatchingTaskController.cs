using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEditor.ShaderGraph.Internal;
using UnityEditor.VersionControl;


public class NewMonoBehaviourScript : MonoBehaviour
{
    
    [SerializeField] private Vector3 Screen1;
    [SerializeField] private Vector3 Screen2;
    [SerializeField] private Vector3 Screen3;
    [SerializeField] private GameObject screenPrefab;
    private ScreenManager1 screenManager;
    private List<GameObject> screens;
    private List<Vector3> positions = new List<Vector3>();
    

    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        positions.Add(Screen1);
        positions.Add(Screen2);
        positions.Add(Screen3);
        screenManager = new ScreenManager1(screenPrefab, new Vector3(0f, 180f, 0f));
        screens = screenManager.GenerateScreens(positions);
    }

    // Update is called once per frame
    void Update()
    {
        screens[0].transform.position = Screen1;
        float scale1 = Screen1.z < 0 ? -1 * Screen1.z : Screen1.z;
        screens[0].transform.localScale = new Vector3 (scale1, scale1, 1f);
        
        screens[1].transform.position = Screen2;
        float scale2 = Screen2.z < 0 ? -1 * Screen2.z : Screen2.z;
        screens[1].transform.localScale = new Vector3(scale2, scale2, 1f);

        screens[2].transform.position = Screen3;
        float scale3 = Screen3.z < 0 ? -1 * Screen3.z : Screen3.z;
        screens[2].transform.localScale = new Vector3(scale3, scale3, 1f);

    }
}
