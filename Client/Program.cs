using System;
using System.Net.Sockets;
using System.Text;

class ClientCode
{
    static void Main()
    {
        Console.Write("Enter ngrok address: ");
        string? input = Console.ReadLine();

        if (string.IsNullOrEmpty(input))
        {
            Console.WriteLine("Invalid input!");
            return;
        }

        string serverAddress;
        int port;

        // Gérer différents formats d'entrée
        if (input.Contains("tcp://"))
        {
            input = input.Replace("tcp://", "");
        }

        if (input.Contains(":"))
        {
            string[] parts = input.Split(':');
            serverAddress = parts[0];
            if (!int.TryParse(parts[1], out port))
            {
                Console.WriteLine("Port invalide!");
                return;
            }
        }
        else
        {
            serverAddress = input;
            Console.Write("Enter port: ");
            string? portInput = Console.ReadLine();
            if (!int.TryParse(portInput, out port))
            {
                Console.WriteLine("Port invalide!");
                return;
            }
        }

        try
        {
            var client = new TcpClient(serverAddress, port);
            Console.WriteLine($"Connected to server at {serverAddress}:{port}");
            var stream = client.GetStream();

            // Processus de connexion
            var buffer = new byte[1024];

            // Email
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string prompt = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.Write(prompt);

            string? email = Console.ReadLine();
            byte[] emailData = Encoding.UTF8.GetBytes(email ?? "");
            stream.Write(emailData, 0, emailData.Length);
            stream.Flush();

            // Password
            bytesRead = stream.Read(buffer, 0, buffer.Length);
            prompt = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.Write(prompt);

            string? password = Console.ReadLine();
            byte[] passwordData = Encoding.UTF8.GetBytes(password ?? "");
            stream.Write(passwordData, 0, passwordData.Length);
            stream.Flush();

            // Confirmation login
            bytesRead = stream.Read(buffer, 0, buffer.Length);
            string loginResult = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine(loginResult);

            // Display name
            bytesRead = stream.Read(buffer, 0, buffer.Length);
            string displayPrompt = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.Write(displayPrompt);

            string? displayName = Console.ReadLine();
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = email?.Split('@')[0] ?? "User";
            }

            byte[] displayData = Encoding.UTF8.GetBytes(displayName);
            stream.Write(displayData, 0, displayData.Length);
            stream.Flush();

            // Welcome message
            bytesRead = stream.Read(buffer, 0, buffer.Length);
            string welcomeMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine(welcomeMessage);
            Console.WriteLine(); // Ligne vide pour clarté

            // Boucle de communication
            while (client.Connected)
            {
                try
                {
                    Console.Write("Enter message: ");
                    string? message = Console.ReadLine();

                    if (string.IsNullOrEmpty(message))
                    {
                        continue;
                    }

                    if (message.ToLower() == "quit" || message.ToLower() == "exit")
                    {
                        break;
                    }

                    byte[] data = Encoding.UTF8.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                    stream.Flush();

                    // Lire la réponse
                    var response = new byte[1024 * 1024];
                    bytesRead = stream.Read(response, 0, response.Length);

                    if (bytesRead > 0)
                    {
                        string serverMessage = Encoding.UTF8.GetString(response, 0, bytesRead);
                        Console.WriteLine(serverMessage);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Connection error: " + ex.Message);
        }

        Console.WriteLine("Disconnected. Press any key to exit...");
        Console.ReadKey();
    }
}