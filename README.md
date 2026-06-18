# vr-visual-test-environment
# Inspector Setup Guide

This document describes how to configure the four task scripts in the Unity Inspector. The sprites, colors, and prefab fields are already assigned in the included prefabs and scene; you only need to select the matching assets from the project when setting up a new scene or duplicating a task.

## General Notes

A few settings apply across all tasks and are easy to overlook:

- **Z position must not be left at 0.** Both `ScreenManager1.GenerateScreens()` and the per-frame scaling logic in `MatchingTaskController` and `ComparativeSearchTask` derive each screen's scale directly from its z-coordinate. A z value of 0 results in a screen scaled to 0, which is invisible even though it is technically present in the scene.
- **Global Scale must not be left at 0**, for the same reason: the final scale is the z position multiplied by Global Scale, so either being 0 results in an invisible screen.
- **Don't forget to assign the screen prefab and the sprite/color arrays.** These fields are not filled in automatically when adding the component to a new GameObject, and a missing reference will throw a null reference exception as soon as the task tries to use it.

## Visual Acuity Task (`VisualAcuityTest`)

This task uses a single screen, instantiated directly without a ScreenManager.

| Field | Description |
|---|---|
| `screenPrefab` | The screen prefab to instantiate. |
| `position`, `rotation`, `scale` | Transform of the single screen. |
| `currentStimuliSet` | Landolt C, Tumbling E, Sloan Letters, or Geometric Shapes. |
| `initialSize` | Starting stimulus size in px (200 in the study). |
| `finishingSize` | Minimum size at which the staircase terminates (15 in the study). |
| `totalTrials` | Used to compute the fixed step size between sizes. |
| `correctThreshold` | Number of consecutive correct responses required to shrink the stimulus (3 in the study). |
| `incorrectThreshold` | Number of incorrect responses required to grow the stimulus back (1 in the study). |
| `endOnFinishingSize` | If checked, the task ends once `finishingSize` is reached rather than after `totalTrials`. |

The study configuration used a 3-down-1-up staircase (`correctThreshold = 3`, `incorrectThreshold = 1`), 7 trials, and a finishing size of 15.

## Matching Task (`MatchingTaskController`)

This task uses three screens, generated through `ScreenManager1`.

| Field | Description |
|---|---|
| `screenPrefab` | The screen prefab to instantiate. |
| `Screen1` / `Screen2` / `Screen3` | World positions of the near, middle, and far screen. The z-component drives both the screen's distance and its scale (see General Notes above). |
| `rotation1` / `rotation2` / `rotation3` | Rotation applied to each screen. |
| `globalScale` | Multiplier applied to each screen's z position to compute its final scale. |
| `screenOneStimuliSet` / `screenTwoStimuliSet` | Stimuli set shown on screen 1 (query) and screen 2 (symbol). The study used Landolt C on screen 1 and a symbol set on screen 2. |
| `randomGrayscale` | If checked, all stimuli use randomly generated gray tones instead of the configured `colors` array. The study used a single fixed color (black), so this should remain unchecked. |
| `colors` | Color palette used for the stimuli when `randomGrayscale` is unchecked. For the study setup, this should contain a single entry: black. |
| `matchByColor` | Switches the task into a color-matching mode instead of shape matching. Not used in the study; leave unchecked. |

The study used z positions of -0.5, -1, and -4 for screen 1, 2, and 3, respectively.

## Comparative Search Task (`ComparativeSearchTask`)

This task uses an arbitrary number of screens, also generated through `ScreenManager1`.

| Field | Description |
|---|---|
| `screenPrefab` | The screen prefab to instantiate. |
| `screenPositions` | World positions of all screens, one per list entry. The z-component of each position drives both its distance and its scale (see General Notes above). |
| `screenRotations` | Rotation applied to each screen, one entry per position. |
| `globalScale` | Multiplier applied to each screen's z position to compute its final scale. |
| `currentStimuliSet` | Stimuli set used for the displayed objects. |
| `objectsPerScreen` | Number of objects shown per screen (30 in the study). |
| `stimuliSpacing` | Minimum spacing between objects in px for the Random distribution, or the spacing between objects for the Row/Column distributions (64 in the study). |
| `distribution` | Random, Row, or Column placement of objects within a screen. |
| `currentMismatchType` | Color, Shape, Position, or Missing; determines how the mismatch object differs from the reference set. |
| `randomGrayscale` | If checked, objects use randomly generated gray tones instead of the `objectColors` palette defined in code. |

The study used three screens with z positions of -0.5, -1, and -4, an `objectsPerScreen` of 30, and a `stimuliSpacing` of 64.

## NASA-TLX Questionnaire (`NASATLXTask`)

| Field | Description |
|---|---|
| `screenPrefab` | The screen prefab to instantiate. |
| `position`, `rotation`, `scale` | Transform of the questionnaire screen. |
| `questions` | The six subscale questions, displayed one at a time in the order given. |
| `headers` | Column headers written to the CSV file; should correspond one-to-one with `questions`. |

This task does not display a numerical scale alongside each question. In the study, participants were told verbally that ratings should be given on a scale from 1 to 20, and the experimenter entered the spoken rating on the keyboard on the participant's behalf. The script itself does not validate the entered value against this range, so an out-of-range number can still be typed and confirmed.
