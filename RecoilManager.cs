using System;
using System.Runtime.InteropServices;
using System.Threading;

public class RecoilManager
{
    // Import WinAPI functions
    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int X, int Y);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    private const int VK_RBUTTON = 0x02; // Right mouse button
    private const int VK_CONTROL = 0x11; // CTRL key

    // Flag to indicate if recoil is active
    private static bool recoilActive = false;

    // Define recoil patterns for each weapon (calibrated for Rust 2024 mechanics)
    private static readonly (int X, int Y)[][] recoilPatterns = new (int X, int Y)[][]
    {
        // AK-47: High vertical recoil with rightward drift, then stabilizes
        new (int X, int Y)[]
        {
            (0, 6), (1, 8), (2, 9), (1, 8), (0, 8), // Initial vertical rise with drift right
            (-1, 9), (-2, 9), (-3, 8), (-2, 8),    // Transition to leftward drift
            (-1, 7), (0, 6), (1, 5), (2, 4),       // Stabilizing rightward drift
            (2, 3), (1, 2), (0, 1), (-1, 1),       // Slowing down
            (0, 0)                                 // Finish
        },

        // LR300: Smooth and balanced recoil with light drift
        new (int X, int Y)[]
        {
            (0, 4), (1, 5), (1, 4), (0, 3), (-1, 3), // Small upward drift
            (-1, 2), (0, 2), (1, 1), (1, 1), (0, 0)  // Stabilize
        },

        // MP5A4: Small recoil with minor oscillations
        new (int X, int Y)[]
        {
            (0, 3), (1, 3), (0, 3), (-1, 3), (-1, 2), // Steady vertical with slight drift
            (0, 2), (1, 1), (0, 1), (-1, 1), (0, 0)   // Slow descent
        },

        // SMG: Compact vertical recoil with slight right drift
        new (int X, int Y)[]
        {
            (0, 2), (1, 3), (1, 2), (0, 2), (-1, 1), // Rapid rise and drift right
            (-1, 1), (0, 1), (0, 0)                 // Stabilize
        },

        // M249: Heavy recoil with large vertical movement
        new (int X, int Y)[]
        {
            (0, 8), (1, 8), (2, 9), (2, 8), (1, 7),  // Big vertical rise
            (-1, 7), (-2, 6), (-3, 5), (-3, 4),     // Leftward correction
            (-2, 3), (-1, 2), (0, 1), (0, 0)        // Slowdown and stabilize
        }
    };

    /// <summary>
    /// Starts the recoil simulation for the selected weapon.
    /// </summary>
    /// <param name="weaponIndex">Index of the selected weapon.</param>
    public static void SimulateRecoil(int weaponIndex)
    {
        if (weaponIndex < 0 || weaponIndex >= recoilPatterns.Length)
        {
            Console.WriteLine("Invalid weapon selected.");
            return;
        }

        var pattern = recoilPatterns[weaponIndex];
        int step = 0;

        recoilActive = true;

        Console.Clear();
        Console.WriteLine("Hold the right mouse button to simulate recoil...");
        Thread.Sleep(500); // Give the user a moment to prepare

        while ((GetAsyncKeyState(VK_RBUTTON) & 0x8000) != 0 && recoilActive)
        {
            // Use the current step, and loop back to start when the pattern ends
            var (x, y) = pattern[step];

            // Check if CTRL is held to modify recoil speed
            bool isCtrlHeld = (GetAsyncKeyState(VK_CONTROL) & 0x8000) != 0;
            int delay = isCtrlHeld ? 50 : 25; // Faster speed if CTRL is held

            MoveMouseBy(x, y);
            Thread.Sleep(delay);

            step++;

            if (step >= pattern.Length)
                step = 0; // Loop the pattern again
        }

        recoilActive = false;
        Console.WriteLine("\nRecoil simulation stopped.");
    }

    /// <summary>
    /// Moves the mouse cursor by the specified offsets.
    /// </summary>
    /// <param name="offsetX">Horizontal offset.</param>
    /// <param name="offsetY">Vertical offset.</param>
    private static void MoveMouseBy(int offsetX, int offsetY)
    {
        if (!GetCursorPos(out POINT cursorPos))
        {
            Console.WriteLine("Failed to get cursor position.");
            return;
        }

        int newX = cursorPos.X + offsetX;
        int newY = cursorPos.Y + offsetY;

        if (!SetCursorPos(newX, newY))
        {
            Console.WriteLine("Failed to set cursor position.");
        }
    }

    /// <summary>
    /// Checks if the recoil simulation should remain active.
    /// </summary>
    /// <returns>True if recoil should continue, otherwise false.</returns>
    public static bool IsRecoilActive()
    {
        return recoilActive;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }
}
