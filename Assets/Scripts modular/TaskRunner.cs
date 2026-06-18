using UnityEngine;

// Centralizes task termination so the editor-stop vs. application-quit
// branching is defined in one place instead of being duplicated in every
// task script. Task scripts call TaskRunner.Exit() once their run is over.
public static class TaskRunner
{
    // Stops play mode when running inside the Unity editor, or quits the
    // built application otherwise.
    public static void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
