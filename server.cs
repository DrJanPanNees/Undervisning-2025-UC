using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class SocketListener
{
    public static int Main(String[] args)
    {
        StartServer(); // Start the server
        return 0; // a convention in C# console applications that signifies the exit status of the program.
    }

    public static void StartServer()
    {
        // Get the host's IP address (localhost in this case)
        IPHostEntry host = Dns.GetHostEntry("localhost"); // Get the local machine's IP
        IPAddress ipAddress = host.AddressList[0]; // Use the first available IP address
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000); // Define endpoint with port 11000

        try
        {
            // Create a TCP socket
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            // ipAddress.AddressFamily specifies the addressing scheme (IPv4 or IPv6) to match the IP address you're using.
            // SocketType.Stream specifies that the socket is a stream socket, meaning data is sent reliably (TCP) as a continuous stream of bytes.
            // ProtocolType.Tcp specifies that the socket uses the TCP protocol, ensuring reliable, ordered data transfer.
 
            //Socket udpSocket = new Socket(ipAddress.AddressFamily, SocketType.Dgram, ProtocolType.Udp); <- UDP


            // Bind the socket to the local endpoint
            listener.Bind(localEndPoint);
            // "Binding" means associating the socket with a specific local IP address and port number.
            // After binding, the socket knows exactly where (IP and port) it should listen for incoming connections.

            // Start listening for client connections (max queue = 10)
            listener.Listen(10);
            // The number (10) indicates the maximum number of incoming connection requests that can wait in line (queued) 
            // before being accepted.
            // If more than 10 clients attempt to connect simultaneously, additional clients may get a "server busy" response.


            Console.WriteLine("Waiting for a connection...");

            // Accept a connection (blocking call, waits until a client connects)
            Socket handler = listener.Accept();
            // This line waits ("blocks") until a client attempts to connect to the listener socket.
            // Once a client connects, Accept() creates a new socket ("handler") specifically dedicated to communicating with that client.
            // The original listener socket continues listening for new incoming connections.

            Console.WriteLine("Client connected!");

            // Buffer for incoming data
            byte[] bytes = new byte[1024];
            string data = "";
            // "bytes" is a byte array (size 1024 bytes) used as temporary storage for data received from the client.
            // "data" is a string that will hold the complete message received, built from the incoming bytes.


            while (true)
            {
                // Receive data from the client
                // This method returns the number of bytes actually received in this read operation    
                int bytesRec = handler.Receive(bytes);

                // Convert received bytes into ASCII text and append it to "data"
                data += Encoding.ASCII.GetString(bytes, 0, bytesRec);

                // Check if "<EOF>" is found in the message, signaling the end of the client's message
                // "<EOF>" is a custom marker indicating "End of File" or end of data transmission    
                if (data.IndexOf("<EOF>") > -1)
                {
                    break;
                }
            }

            // Display received message
            Console.WriteLine("Text received: {0}", data);

            // Send the same message back to the client (echo)
            byte[] msg = Encoding.ASCII.GetBytes(data);
            handler.Send(msg);

            // Close the connection
            handler.Shutdown(SocketShutdown.Both); // Stop both sending and receiving
            handler.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.ToString()); // Print error if something goes wrong
        }

        Console.WriteLine("\n Press any key to continue...");
        Console.ReadKey(); // Wait for key press before closing
    }
}
