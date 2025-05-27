using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class MultiClientServer
{
    private static TcpListener? server;
    private const int Port = 5000;

    static async Task Main()
    {
        server = new TcpListener(IPAddress.Any, Port);
        server.Start();
        Console.WriteLine($"Server started listening on port {Port}");

        while (true)
        {
            var client = await server.AcceptTcpClientAsync();
            Console.WriteLine("Client connected - waiting for login");
            _ = HandleClientAsync(client);
        }
    }

    private static async Task HandleClientAsync(TcpClient client)
    {
        var stream = client.GetStream();
        string userEmail = "";
        string username = "";
        
        try
        {
            // Processus de connexion (email + mot de passe)
            var loginInfo = await GetUserLogin(stream);
            userEmail = loginInfo.email;
            
            Console.WriteLine($"User logged in with email: {userEmail}");
            
            // Demander le nom d'affichage
            byte[] welcomeMsg = Encoding.UTF8.GetBytes("Enter your display name: ");
            await stream.WriteAsync(welcomeMsg, 0, welcomeMsg.Length);
            
            var buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            username = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
            
            Console.WriteLine($"User '{username}' ({userEmail}) connected");
            
            // Confirmer la connexion
            byte[] confirmMsg = Encoding.UTF8.GetBytes($"Welcome {username}! You are now connected. You can start sending messages.");
            await stream.WriteAsync(confirmMsg, 0, confirmMsg.Length);

            // Boucle de communication
            buffer = new byte[1024 * 1024];
            while (client.Connected)
            {
                try
                {
                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) 
                    {
                        Console.WriteLine($"Client {username} disconnected (0 bytes read)");
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                    if (string.IsNullOrEmpty(message)) continue;

                    Console.WriteLine($"Message from {username} ({userEmail}): {message}");

                    byte[] response = Encoding.UTF8.GetBytes($"Server to {username}: {message}");
                    await stream.WriteAsync(response, 0, response.Length);
                    await stream.FlushAsync(); // Forcer l'envoi
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading from {username}: {ex.Message}");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error with user {username}: {ex.Message}");
        }
        finally
        {
            try
            {
                client.Close();
            }
            catch { }
            Console.WriteLine($"User '{username}' ({userEmail}) disconnected");
        }
    }

    private static async Task<(string email, string password)> GetUserLogin(NetworkStream stream)
    {
        try
        {
            // Demander l'email
            byte[] emailPrompt = Encoding.UTF8.GetBytes("Enter your email: ");
            await stream.WriteAsync(emailPrompt, 0, emailPrompt.Length);
            await stream.FlushAsync();
            
            var buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            string email = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

            // Demander le mot de passe
            byte[] passwordPrompt = Encoding.UTF8.GetBytes("Enter your password: ");
            await stream.WriteAsync(passwordPrompt, 0, passwordPrompt.Length);
            await stream.FlushAsync();
            
            bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            string password = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

            // Toujours accepter (pas de vérification)
            byte[] successMsg = Encoding.UTF8.GetBytes("Login successful!");
            await stream.WriteAsync(successMsg, 0, successMsg.Length);
            await stream.FlushAsync();
            
            Console.WriteLine($"Login attempt - Email: {email}");
            
            return (email, password);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Login error: {ex.Message}");
            return ("", "");
        }
    }
}