using System;
using System.Net.Sockets;
using System.Text;

class ClientCode
{
    private const int Port = 5000;

    static void Main()
    {
        try
        {
            var client = new TcpClient("127.0.0.1", Port);
            Console.WriteLine("Connected to server");
            var stream = client.GetStream();

            // Lire la demande de nom d'utilisateur du serveur
            var buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string serverRequest = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.Write(serverRequest); // "Enter your username: "

            // Envoyer le nom d'utilisateur
            string? username = Console.ReadLine();
            if (string.IsNullOrEmpty(username))
            {
                username = "Anonymous";
            }

            byte[] usernameData = Encoding.UTF8.GetBytes(username);
            stream.Write(usernameData, 0, usernameData.Length);

            // Lire la confirmation du serveur
            bytesRead = stream.Read(buffer, 0, buffer.Length);
            string welcomeMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine(welcomeMessage);

            // Boucle de communication
            while (true)
            {
                Console.Write("Enter message: ");
                string? message = Console.ReadLine();
                if (string.IsNullOrEmpty(message)) continue;

                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);

                var response = new byte[1024 * 1024];
                bytesRead = stream.Read(response, 0, response.Length);
                string serverMessage = Encoding.UTF8.GetString(response, 0, bytesRead);
                Console.WriteLine(serverMessage);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Error: " + ex.Message);
        }
    }
}