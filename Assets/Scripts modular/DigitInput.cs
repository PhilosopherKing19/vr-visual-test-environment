using UnityEngine.InputSystem;

// Shared helper for reading a single 0-9 digit press from the keyboard,
// covering both the number row and the numpad. It replaces the long, per-
// digit if-chains that were duplicated in the task scripts: the NASA-TLX
// task uses it to build up a typed rating string, while the Comparative
// Search task uses it to map a key directly to a response. The number-row
// digit keys are listed explicitly because their order in the Key enum does
// not follow 0-9, so they cannot be addressed by simple index arithmetic.
public static class DigitInput
{
    private static readonly Key[] rowKeys =
    {
        Key.Digit0, Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4,
        Key.Digit5, Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9
    };

    private static readonly Key[] numpadKeys =
    {
        Key.Numpad0, Key.Numpad1, Key.Numpad2, Key.Numpad3, Key.Numpad4,
        Key.Numpad5, Key.Numpad6, Key.Numpad7, Key.Numpad8, Key.Numpad9
    };

    // Returns true and sets digit to 0-9 if the matching number-row or numpad
    // key was pressed this frame; returns false and sets digit to -1
    // otherwise. At most one digit is reported per frame.
    public static bool TryGetDigit(out int digit)
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
            for (int d = 0; d <= 9; d++)
                if (keyboard[rowKeys[d]].wasPressedThisFrame ||
                    keyboard[numpadKeys[d]].wasPressedThisFrame)
                {
                    digit = d;
                    return true;
                }

        digit = -1;
        return false;
    }
}
