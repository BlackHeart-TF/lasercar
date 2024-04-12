namespace lasercar
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnNextClicked(object sender, EventArgs e)
        {
            // Navigate to the stream page with the URL
            await Navigation.PushAsync(new StreamPage(urlEntry.Text));
        }
    }

}
