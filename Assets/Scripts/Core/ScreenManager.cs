using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    [SerializeField] private GameObject screenPrefab;
    [SerializeField] private int screenCount = 2;
    [SerializeField] private float horizontalSpacing = 1.5f;
    [SerializeField] private float depthSpacing = 1.0f;
    [SerializeField] private Vector3 startPosition = new Vector3(0f, 1.5f, 1.5f);
    [SerializeField] private Vector3 defaultRotation = new Vector3(0f,180f,0f);
    [SerializeField] private Transform screenContainer;
    [SerializeField] private GameObject screenContainerPrefab;
    [SerializeField] private bool centerLayout = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 1; i <= screenCount; i++)
        {   // Werte der Screens werden berrechnet
            float y = startPosition.y;
            float z = startPosition.z + i * depthSpacing;
            float centerOffset = (screenCount - 1) / 2;
            float x = (i - centerOffset) * horizontalSpacing;
            Vector3 position = new Vector3(x, y, z);

            // Screens werden erzeugt und Werte, werden übergeben
            GameObject neuerScreen = Instantiate(screenPrefab);
            neuerScreen.transform.position = position;

            // muss in Quaternion umgewandelt werden, da unity Quaternion erwartet und nicht Vector3
            neuerScreen.transform.rotation = Quaternion.Euler(defaultRotation);

        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
