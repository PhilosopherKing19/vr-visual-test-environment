using System.Collections.Generic;
using UnityEngine;

// Utility class that instantiates and positions virtual screens from an
// explicit list of world-space positions. Used by the task controllers
// (MatchingTaskController, ComparativeSearchTask, and now also the single-
// screen tasks VisualAcuityTest and NASATLXTask) so that screen generation
// and per-frame transform updates do not need to be duplicated in each task.
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

    // Updates every managed screen each frame from per-screen positions and
    // rotations, scaling each screen in proportion to its depth
    // (z * globalScale) so that screens at different distances keep a
    // comparable visual angle. Used by the multi-screen tasks (Matching,
    // Comparative Search). The positions and rotations lists are expected to
    // have at least as many entries as there are managed screens.
    public void UpdateTransforms(List<Vector3> positions, List<Vector3> rotations, float globalScale)
    {
        for (int i = 0; i < screens.Count; i++)
        {
            screens[i].transform.position = positions[i];
            float scale = positions[i].z * globalScale;
            screens[i].transform.localScale = new Vector3(scale, scale, 1f);
            screens[i].transform.rotation = Quaternion.Euler(rotations[i]);
        }
    }

    // Updates a single managed screen each frame with a fixed scale, so the
    // rendered size stays independent of depth. This preserves the calibrated
    // screen size the single-screen tasks (Visual Acuity, NASA-TLX) rely on;
    // in particular the Visual Acuity task's pixel-to-metre calibration
    // depends on this fixed scale, so it must NOT be replaced by the depth-
    // based scaling used in UpdateTransforms.
    public void UpdateTransform(Vector3 position, Vector3 rotation, float scale)
    {
        GameObject screen = screens[0];
        screen.transform.position = position;
        screen.transform.rotation = Quaternion.Euler(rotation);
        screen.transform.localScale = new Vector3(scale, scale, 1f);
    }
}
