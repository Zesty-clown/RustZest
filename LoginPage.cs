using System;
using System.Data.SQLite;
using System.IO;
using System.Threading;

public static class LoginPage
{
    private const string CorrectUsername = "admin";
    private const string CorrectPassword = "password123";

    public static bool AuthenticateUser()
    {
        Console.Clear();
        Console.WriteLine("LOGIN PAGE");
        Console.WriteLine(new string('═', 30));

        Console.Write("Username: ");
        string username = Console.ReadLine();

        Console.Write("Password: ");
        string password = MaskPassword();


        if (username == CorrectUsername && password == CorrectPassword)
        {
            SaveHWIDToDesktop();
            Console.Clear();
            Console.WriteLine("Login successful!");
            Console.WriteLine($"Your HWID has been saved to the desktop as 'user_hwid.txt'.");
            return true;
        }
        else
        {
            Console.WriteLine("\nInvalid credentials or token. Try again.");
            Thread.Sleep(2000); // Pause to show error message
            return AuthenticateUser();
        }
    }

    private static string MaskPassword()
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

    private static string GetHWID()
    {
        return Environment.MachineName;
    }

    private static void SaveHWIDToDesktop()
    {
        string hwid = GetHWID();

        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string filePath = Path.Combine(desktopPath, "user_hwid.txt");

        try
        {
            File.WriteAllText(filePath, hwid);
            Console.WriteLine($"HWID saved successfully to: {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving HWID to file: {ex.Message}");
        }
    }
}
