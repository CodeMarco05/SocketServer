using System.Net.Sockets;
using System.Text;

namespace Shared;

public class SocketUtils{
    private const int SizeForLengthTransmition = 5;
    private const int MaxTransmitableBytes = 99_999;

    public static bool SendAck(Socket socket) {
        //Send ack
        string ack = "0";
        byte[] responseBytes = Encoding.ASCII.GetBytes(ack);
        try {
            socket.Send(responseBytes);
        }
        catch (Exception e) {
            //custom error message
            Utils.PrintErrorMessage($"Method(SendAck) Error: {e.Message}");
            return false;
        }

        return true;
    }

    public static bool RecieveAck(Socket socket) {
        //recieve Ack
        byte[] buffer = new byte[1];
        try {
            int bytesRead = socket.Receive(buffer);
            
            //check bytes read
            if (bytesRead == 0) {
                Utils.PrintErrorMessage("Method(RecieveAck) Ack was not recieved. Exiting...");
                return false;
            }
            
            //string response = (Encoding.ASCII.GetString(buffer, 0, bytesRead));
            
            /*if (response != "0") {
                Utils.PrintErrorMessage("Method(RecieveAck) Something went wrong, Ack was. Exiting...");
                return false;
            }*/
        }
        catch (Exception e) {
            Console.WriteLine(e);
            return false;
        }

        return true;
    }

    private static bool SendLengthOfNextTransmition(int length, Socket socket) {
        string size = length.ToString();

        for (int i = size.Length; i < SizeForLengthTransmition; i++) {
            size = size.Insert(0, "0");
        }

        //send the size of the message
        byte[] responseBytes = Encoding.ASCII.GetBytes(size);
        socket.Send(responseBytes);
        
        //recieve ack
        bool ackSend = RecieveAck(socket);
        return ackSend;
    }

    private static int RecieveLengthOfNextTransmition(Socket socket) {
        byte[] buffer = new byte[SizeForLengthTransmition];
        try {
            int bytesRead = socket.Receive(buffer);
            
            //check bytes read
            if(bytesRead == 0) {
                Utils.PrintErrorMessage("Method: RecieveLengthOfNextTransmition, no bytes were read. Exiting...");
                return -1;
            }
            
            string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            
            bool success = SendAck(socket); 
            //check if the ack was sent
            if (!success) {
                return -1;
            }
            
            // Trim leading zeros
            response = response.TrimStart('0');

            return int.Parse(response);
        }
        catch (Exception e) {
            //print custom error message
            Utils.PrintErrorMessage($"Method: RecieveLengthOfNextTransmition, Error: {e.Message}");
            return -1;
        }
        
    }



    public static bool SendOverSocket(string message, Socket socket) {
        //send the size of the message
        bool success = SendLengthOfNextTransmition(message.Length, socket);
        
        //if success is false, then the message was not sent
        if (!success) {
            return false;
        }

        //Send the message
        byte[] data = Encoding.ASCII.GetBytes(message);
        socket.Send(data);
        
        //recieve ack
        success = RecieveAck(socket);
        return success;
    }

    public static string? RecieveOverSocket(Socket socket) {
        //first get the size of the message
        int bufferSize = RecieveLengthOfNextTransmition(socket);
        
        //if the buffer size is -1, then there was no message to recieve
        if(bufferSize == -1) {
            return null;
        }

        //recieve the message
        byte[] buffer = new byte[bufferSize];
        try {
            int bytesRead = socket.Receive(buffer);
            //send ack
            bool success = SendAck(socket);
            //check if the ack was sent
            if (!success) {
                return null;
            }
        
            //return the result
            return Encoding.ASCII.GetString(buffer, 0, bytesRead);
        }
        catch (Exception e) {
            //custom error message
            Utils.PrintErrorMessage($"Method: RecieveOverSocket, Error: {e.Message}");
            return null;
        }
    }
}