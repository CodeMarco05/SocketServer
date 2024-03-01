using System.Net.Sockets;
using System.Text;

namespace Shared;

public class UtilsSocket{
    private const int SizeForLengthTransmition = 3;
    private const int MaxTransmitableBytes = 999;

    public static void SendAck(Socket socket) {
        //TODO comments
        //Send ack
        string ack = "0";
        byte[] responseBytes = Encoding.ASCII.GetBytes(ack);
        socket.Send(responseBytes);
    }

    public static void RecieveAck(Socket socket) {
        //TODO cooments
        //recieve Ack
        byte[] buffer = new byte[1];
        int bytesRead = socket.Receive(buffer);
        string response = (Encoding.ASCII.GetString(buffer, 0, bytesRead));
        if (response != "0") {
            Utils.PrintErrorMessage("Error during sending username");
            Environment.FailFast("Critical error occured.");
        }
    }

    private static void SendLengthOfNextTransmition(int length, Socket socket) {
        string size = length.ToString();

        for (int i = size.Length; i < SizeForLengthTransmition; i++) {
            size = size.Insert(0, "0");
        }

        byte[] responseBytes = Encoding.ASCII.GetBytes(size);
        socket.Send(responseBytes);
        
        RecieveAck(socket);
    }

    private static int RecieveLengthOfNextTransmition(Socket socket) {
        byte[] buffer = new byte[MaxTransmitableBytes];
        int bytesRead = socket.Receive(buffer);
        String response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
        SendAck(socket);
        return int.Parse(response);
    }

    public static void SendOverSocket(string message, Socket socket) {
        //send the size of the message
        SendLengthOfNextTransmition(message.Length, socket);

        byte[] data = Encoding.ASCII.GetBytes(message);
        socket.Send(data);
        RecieveAck(socket);
    }

    public static string RecieveOverSocket(Socket socket) {
        //first get the size of the message
        int bufferSize = RecieveLengthOfNextTransmition(socket);

        //Send the message
        byte[] buffer = new byte[bufferSize];
        int bytesRead = socket.Receive(buffer);
        //wait for ack
        SendAck(socket);
        //return the result
        return Encoding.ASCII.GetString(buffer, 0, bytesRead);
    }
}