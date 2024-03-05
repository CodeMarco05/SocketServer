using System.Net.Sockets;
using System.Text;

namespace Shared;

public class SocketUtils{
    private const int SizeForLengthTransmition = 5;
    private const int MaxTransmitableBytes = 99_999;

    public static void SendAck(Socket socket) {
        //Send ack
        string ack = "0";
        byte[] responseBytes = Encoding.ASCII.GetBytes(ack);
        socket.Send(responseBytes);
    }

    public static void RecieveAck(Socket socket) {
        //recieve Ack
        byte[] buffer = new byte[1];
        int bytesRead = socket.Receive(buffer);
        string response = (Encoding.ASCII.GetString(buffer, 0, bytesRead));
        if (response != "0") {
            Utils.PrintErrorMessage("Method(RecieveAck) Error during sending or recieving data. Exiting...");
            //Environment.FailFast("Critical error occured.");
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
        byte[] buffer = new byte[SizeForLengthTransmition];
        int bytesRead = socket.Receive(buffer);
        string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
        SendAck(socket);
        
        // Trim leading zeros
        response = response.TrimStart('0');

        // Check if the string is empty or whitespace before parsing
        if (string.IsNullOrWhiteSpace(response)) {
            //return -1 because no message was sent
            return -1;
            //throw new FormatException("Method: RecieveLengthOfNextTransmition, there was no message, or response is empty or whitespace. Exiting...");
        }

        return int.Parse(response);
    }



    public static void SendOverSocket(string message, Socket socket) {
        //send the size of the message
        SendLengthOfNextTransmition(message.Length, socket);

        byte[] data = Encoding.ASCII.GetBytes(message);
        socket.Send(data);
        RecieveAck(socket);
    }

    public static string? RecieveOverSocket(Socket socket) {
        //first get the size of the message
        int bufferSize = RecieveLengthOfNextTransmition(socket);
        
        //if the buffer size is -1, then there was no message to recieve
        if(bufferSize == -1) {
            return null!;
        }

        //Send the message
        byte[] buffer = new byte[bufferSize];
        int bytesRead = socket.Receive(buffer);
        //send ack
        SendAck(socket);
        //return the result
        return Encoding.ASCII.GetString(buffer, 0, bytesRead);
    }
}