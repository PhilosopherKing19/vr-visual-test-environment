// Shared rotation table for the Landolt C stimulus. This is the only
// orientation table that is byte-for-byte identical across all task scripts
// (Visual Acuity, Matching, Comparative Search), so it is centralized here to
// remove the duplicated copies. The Tumbling E table is intentionally NOT
// shared: the Visual Acuity and Comparative Search tasks use four
// orientations (0/90/180/270), whereas the Matching task drives its eight-
// cell mapping table from this same eight-entry Landolt C table, so each task
// keeps its own Tumbling E handling unchanged.
public static class StimulusRotations
{
    // The eight gap orientations of the Landolt C, in degrees.
    public static readonly float[] LandoltC = { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };
}
