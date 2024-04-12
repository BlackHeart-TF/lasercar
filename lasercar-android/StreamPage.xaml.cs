namespace lasercar;

public partial class StreamPage : ContentPage
{
    private string hostname;
    private HttpClient _client = new HttpClient();
    private CancellationTokenSource _cts = new CancellationTokenSource();
    public StreamPage(string url)
	{
		InitializeComponent();
        hostname = url;
        StartStreaming();
    }

    private async void StartStreaming()
    {
        MjpegStream client = new MjpegStream("192.168.4.1"); // Replace with your camera's IP address
        await client.ConnectAsync();

        byte[] frame;
        while (!_cts.IsCancellationRequested)
        {
            frame = await client.GetNextFrameAsync(_cts.Token);
            if (frame == null)
                continue;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                streamImage.Source = ImageSource.FromStream(() => new MemoryStream(frame));
            });
        }
    }


    private void printSample(MemoryStream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);
        var buff = new byte[4];
        stream.ReadExactly(buff);
        string outputstr = BitConverter.ToString(buff);
        stream.Seek(-4, SeekOrigin.End);
        stream.ReadExactly(buff);
        outputstr += " ... " + BitConverter.ToString(buff);
        Console.WriteLine(outputstr);
        Console.WriteLine($"{stream.Position}, {stream.Length}");
        stream.Seek(0, SeekOrigin.Begin);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Ensure the streaming stops when the page is not visible
        _cts.Cancel();
    }
}