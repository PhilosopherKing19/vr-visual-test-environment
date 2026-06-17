using System.Collections.Generic;
using UnityEngine;

// Utility class that instantiates and positions virtual screens from an
// explicit list of world-space positions. Used by the task controllers
// (MatchingTaskController, ComparativeSearchTask) so that screen
// generation does not need to be duplicated in each task.
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

    // Instantiates one copy of the screen prefab per given position, scales
    // each screen in proportion to its depth (z-coordinate) so that all
    // screens subtend approximately the same visual angle regardless of
    // distance, and applies the configured default rotation to each one.
    public List<GameObject> GenerateScreens(List<Vector3> positions)
    {
        screens = new List<GameObject>();
        for (int i = 0; i < positions.Count; i++)
        {
            GameObject newScreen = Object.Instantiate(screenPrefab);
            newScreen.transform.position = positions[i];

            // If z is negative, its absolute value is used instead;
            // otherwise z is used directly.
            float scale = positions[i].z < 0 ? -1 * positions[i].z : positions[i].z;
            newScreen.transform.localScale = new Vector3(scale, scale, 1f);

            newScreen.transform.rotation = Quaternion.Euler(defaultRotation);
            screens.Add(newScreen);
        }
        return screens;
    }
}
