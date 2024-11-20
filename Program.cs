using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Threading;

class Program1
{
    static Dictionary<int, string> loadouts = new Dictionary<int, string>(); // Key: slot number, Value: item name
    static string[] weapons = { "AK-47", "LR300", "MP5A4", "SMG", "M249" };
    static string[] items = { "Medkit", "Bandage", "Grenade" }; // Example additional items

    static void Main()
    {
        if (!LoginPage())
        {
            Console.WriteLine("Login failed. Exiting program...");
            Thread.Sleep(2000);
            return;
        }

        int currentIndex = 0;
        bool isSettingsMenu = false;
        bool isLoadoutMenu = false;
        bool recoilThreadRunning = false;
        Thread recoilThread = null;

        while (true)
        {
            Console.Clear();
            if (isSettingsMenu)
                DrawSettingsMenu(currentIndex);
            else if (isLoadoutMenu)
                DrawLoadoutMenu(currentIndex);
            else
                DrawInterface(currentIndex);

            var key = Console.ReadKey(true).Key;

            switch (key)
            {
                case ConsoleKey.UpArrow:
                    currentIndex = Math.Max(currentIndex - 1, 0);
                    break;
                case ConsoleKey.DownArrow:
                    if (isSettingsMenu)
                        currentIndex = Math.Min(currentIndex + 1, 7); // Assuming there are 8 settings
                    else if (isLoadoutMenu)
                        currentIndex = Math.Min(currentIndex + 1, weapons.Length + items.Length - 1);
                    else
                        currentIndex = Math.Min(currentIndex + 1, weapons.Length - 1);
                    break;
                case ConsoleKey.Spacebar:
                    if (isSettingsMenu)
                    {
                        // Toggle setting
                    }
                    else if (isLoadoutMenu)
                    {
                        AssignLoadout(currentIndex);
                    }
                    else
                    {
                        // Select weapon
                        SelectWeapon(currentIndex);
                    }
                    break;
                case ConsoleKey.Tab:
                    if (isLoadoutMenu)
                    {
                        isLoadoutMenu = false;
                        isSettingsMenu = false;
                    }
                    else if (isSettingsMenu)
                    {
                        isSettingsMenu = false;
                        isLoadoutMenu = true;
                    }
                    else
                    {
                        isSettingsMenu = true;
                        isLoadoutMenu = false;
                    }
                    currentIndex = 0;
                    break;
                case ConsoleKey.Escape:
                    if (recoilThreadRunning && recoilThread != null && recoilThread.IsAlive)
                    {
                        recoilThread.Abort();
                    }
                    ExitProgram();
                    return;
                case ConsoleKey.R:
                    if (!isSettingsMenu && !isLoadoutMenu && loadouts.ContainsValue("Selected Weapon"))
                    {
                        if (!recoilThreadRunning)
                        {
                            recoilThread = new Thread(() => RecoilManager.SimulateRecoil(Array.IndexOf(weapons, loadouts['Selected Weapon'])));
                            recoilThread.IsBackground = true;
                            recoilThread.Start();
                            recoilThreadRunning = true;
                        }
                    }
                    else
                    {
                        Console.WriteLine("\nPlease select a weapon first!");
                        Console.ReadKey();
                    }
                    break;
                default:
                    break;
            }

            if (recoilThreadRunning && !RecoilManager.IsRecoilActive())
            {
                recoilThreadRunning = false;
                recoilThread = null;
            }
        }
    }

    static void DrawSettingsMenu(int currentIndex)
    {
        Console.Title = "Rust No-Recoil Macro - Settings Menu";
        Console.ForegroundColor = ConsoleColor.Red;

        PrintBanner();

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("\n" + new string('═', 70));
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("SETTINGS MENU".PadLeft(45));
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(new string('═', 70));

        string[] settings = {
            "Randomizer X: 0  Y: 0",
            "Recoil-Ctrl X: 100  Y: 100",
            "AutoGunDetect: False",
            "AutoModDetect: False",
            "Burst-F Mode: False",
            "S-RUN: False",
            "RAPID: False",
            "HIP-F: False",
            "A-AFK: False"
        };

        for (int i = 0; i < settings.Length; i++)
        {
            if (i == currentIndex) Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(settings[i]);
            Console.ResetColor();
        }

        Console.WriteLine("\n" + new string('═', 70));
        Console.WriteLine("\nUse arrow keys to navigate, SPACE to toggle a setting.");
        Console.WriteLine("Press TAB to switch to Loadout menu, ESC to exit.");
    }

    static void DrawLoadoutMenu(int currentIndex)
    {
        Console.Title = "Rust No-Recoil Macro - Loadout Menu";
        Console.ForegroundColor = ConsoleColor.Red;

        PrintBanner();

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("\n" + new string('═', 70));
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("LOADOUT MENU".PadLeft(45));
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(new string('═', 70));

        string[] allItems = new string[weapons.Length + items.Length];
        weapons.CopyTo(allItems, 0);
        items.CopyTo(allItems, weapons.Length);

        for (int i = 0; i < allItems.Length; i++)
        {
            if (i == currentIndex) Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(allItems[i]);
            Console.ResetColor();
        }

        Console.WriteLine("\n" + new string('═', 70));
        Console.WriteLine("\nUse arrow keys to navigate, SPACE to assign an item to a slot.");
        Console.WriteLine("Press TAB to switch menus, ESC to exit.");
    }

    static void AssignLoadout(int itemIndex)
    {
        string[] allItems = new string[weapons.Length + items.Length];
        weapons.CopyTo(allItems, 0);
        items.CopyTo(allItems, weapons.Length);

        Console.Clear();
        Console.WriteLine("Enter slot number (1-9) to assign this item:");
        string input = Console.ReadLine();

        if (int.TryParse(input, out int slotNumber) && slotNumber >= 1 && slotNumber <= 9)
        {
            loadouts[slotNumber] = allItems[itemIndex];
            Console.WriteLine($"Assigned {allItems[itemIndex]} to slot {slotNumber}.");
        }
        else
        {
            Console.WriteLine("Invalid slot number.");
        }

        Console.WriteLine("Press any key to return to the loadout menu...");
        Console.ReadKey();
    }

    static void SelectWeapon(int currentIndex)
    {
        if (loadouts.ContainsValue("Selected Weapon"))
        {
            foreach (var key in loadouts.Keys)
            {
                if (loadouts[key] == "Selected Weapon")
                {
                    loadouts[key] = weapons[currentIndex];
                    break;
                }
            }
        }
        else
        {
            loadouts[1] = weapons[currentIndex]; // Default to slot 1 if no weapon is currently selected
        }
        loadouts['Selected Weapon'] = weapons[currentIndex];
    }

    static bool LoginPage()
    {
        string correctUsername = "admin";
        string correctPassword = "password123";

        while (true)
        {
            Console.Clear();
            Console.WriteLine("LOGIN PAGE");
            Console.WriteLine(new string('═', 30));

            Console.Write("Username: ");
            string username = Console.ReadLine();

            Console.Write("Password: ");
            string password = MaskPassword();

            if (username == correctUsername && password == correctPassword)
            {
                Console.Clear();
                Console.WriteLine("Login successful! Press any key to continue...");
                Console.ReadKey();
                return true;
            }
            else
            {
                Console.WriteLine("\nInvalid credentials or token. Press ESC to exit or any other key to retry.");
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Escape)
                {
                    return false;
                }
            }
        }
    }

    static string MaskPassword()
    {
        string password = "";
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(true);

            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                password += key.KeyChar;
                Console.Write("*");
            }
            else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password[..^1];
                Console.Write("\b \b");
            }
        } while (key.Key != ConsoleKey.Enter);

        Console.WriteLine(); // Move to next line after Enter
        return password;
    }

    private static bool ValidateToken(string token)
    {
        string databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "auth_tokens.db");

        if (!File.Exists(databasePath))
        {
            Console.WriteLine("Token database not found.");
            return false;
        }

        using (var connection = new SQLiteConnection($"Data Source={databasePath};Version=3;"))
        {
            connection.Open();

            string query = "SELECT expiration FROM tokens WHERE token = @token";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@token", token);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        long expiration = reader.GetInt64(0);
                        if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() < expiration)
                        {
                            Console.WriteLine("Token is valid.");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("Token has expired.");
                            return false;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Token not found.");
                        return false;
                    }
                }
            }
        }
    }

    static void DrawInterface(int currentIndex)
    {
        Console.Title = "Rust No-Recoil Macro - Weapon Menu";
        Console.ForegroundColor = ConsoleColor.Red;

        PrintBanner();

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("\n" + new string('═', 70));
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("SELECT A WEAPON TO SIMULATE RECOIL".PadLeft(45));
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(new string('═', 70));

        for (int i = 0; i < weapons.Length; i++)
        {
            if (i == currentIndex) Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[{(loadouts.ContainsValue(weapons[i]) ? "x" : " ")}] {weapons[i]}");
            Console.ResetColor();
        }

        Console.WriteLine("\n" + new string('═', 70));
        DrawHotbar();
        Console.WriteLine("\nUse arrow keys to navigate, SPACE to select a weapon.");
        Console.WriteLine("Press R to simulate recoil, TAB to switch menus, ESC to exit.");
    }

    static void DrawHotbar()
    {
        Console.WriteLine("\nHotbar:");
        for (int i = 1; i <= 9; i++)
        {
            if (loadouts.ContainsKey(i))
            {
                Console.Write($"[{loadouts[i]}] ");
            }
            else
            {
                Console.Write("[] ");
            }
        }
        Console.WriteLine();
    }

    static void ExitProgram()
    {
        Console.Clear();
        Console.WriteLine("Thank you for using the Rust No-Recoil Macro! Press any key to exit...");
        Console.ReadKey();
    }

    static void PrintBanner()
    {
        Console.WriteLine(" ██████╗ ██╗   ██╗███████╗██████╗     ███╗   ███╗███████╗███╗   ██╗");
        Console.WriteLine("██╔════╝ ██║   ██║██╔════╝██╔══██╗    ████╗ ████║██╔════╝████╗  ██║");
        Console.WriteLine("██║  ███╗██║   ██║█████╗  ██████╔╝    ██╔████╔██║█████╗  ██╔██╗ ██║");
        Console.WriteLine("██║   ██║██║   ██║██╔══╝  ██╔═══╝     ██║╚██╔╝██║██╔══╝  ██║╚██╗██║");
        Console.WriteLine("╚██████╔╝╚██████╔╝███████╗██║         ██║ ╚═╝ ██║███████╗██║ ╚████║");
        Console.WriteLine(" ╚═════╝  ╚═════╝ ╚══════╝╚═╝         ╚═╝     ╚═╝╚══════╝╚═╝  ╚═══╝");
    }
}
