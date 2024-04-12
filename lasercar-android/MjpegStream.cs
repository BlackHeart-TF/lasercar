using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class MjpegStream
{
    private readonly Socket _socket;
    private readonly string _host;
    private readonly int _port;

    public MjpegStream(string host, int port = 81)
    {
        _host = host;
        _port = port;
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    public async Task ConnectAsync()
    {
        IPAddress ipAddress = IPAddress.Loopback;
        if (!IPAddress.TryParse(_host,out ipAddress))
            ipAddress = Dns.GetHostEntry(_host).AddressList[0];
        IPEndPoint remoteEP = new IPEndPoint(ipAddress, _port);

        await _socket.ConnectAsync(remoteEP);
        Console.WriteLine("Socket connected to {0}", _socket.RemoteEndPoint.ToString());

        // Send an HTTP GET request
        SendHttpGetRequest("/stream");
    }

    private void SendHttpGetRequest(string path)
    {
        string getRequest = "GET " + path + " HTTP/1.1\r\n" +
                            "Host: " + _host + "\r\n" +
                            "Connection: keep-alive\r\n" +
                            "\r\n";

        byte[] byteData = Encoding.ASCII.GetBytes(getRequest);
        _socket.Send(byteData);
    }

    public async Task<byte[]> GetNextFrameAsync(CancellationToken token)
    {
        MemoryStream accumulatedBuffer = new MemoryStream();
        byte[] buffer = new byte[4096];

        while (true)
        {
            if (token.IsCancellationRequested)
                return null;
            
            int bytesRead = await _socket.ReceiveAsync(buffer, SocketFlags.None,token);
            if (bytesRead == 0)
            {
                Console.WriteLine("No more data received");
                break;
            }

            accumulatedBuffer.Write(buffer, 0, bytesRead);
            while (true)
            {
                if (token.IsCancellationRequested)
                    return null;

                var frameData = accumulatedBuffer.ToArray();
                int start = FindIndex(frameData, new byte[] { 0xff, 0xd8 });
                int end = FindIndex(frameData, new byte[] { 0xff, 0xd9 }, Math.Max(start, 0));

                if (start != -1 && end != -1)
                {
                    byte[] jpg = new byte[end + 2 - start];
                    Array.Copy(frameData, start, jpg, 0, jpg.Length);
                    accumulatedBuffer.SetLength(0);
                    accumulatedBuffer.Write(frameData, end + 2, frameData.Length - end - 2);
                    return jpg;
                }
                else if (start != -1 && end == -1)
                    // Start marker found but no end marker yet
                    // Need more data to complete the frame, break out of the inner loop to fetch more
                    break;
                else if (start == -1)
                    // No start market found in the buffer, clear buffer and keep looking
                    accumulatedBuffer.Position = 0;
                    accumulatedBuffer.SetLength(0);
                    break;
            }
        }

        return null;
    }

    private int FindIndex(byte[] data, byte[] pattern, int startIndex = 0)
    {
        int maxFirstCharSlot = data.Length - pattern.Length + 1;
        for (int i = startIndex; i < maxFirstCharSlot; i++)
        {
            if (data[i] != pattern[0]) continue;

            bool matched = true;
            for (int j = 1; j < pattern.Length; j++)
            {
                if (data[i + j] != pattern[j])
                {
                    matched = false;
                    break;
                }
            }

            if (matched) return i;
        }

        return -1; // Not found
    }
}
