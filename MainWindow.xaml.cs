using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using PdfiumViewer;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace JustView
{
    public sealed partial class MainWindow : Window
    {
        private PdfDocument? _document;
        private int _currentPage;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(this));
            picker.FileTypeFilter.Add(".pdf");

            var file = await picker.PickSingleFileAsync();
            if (file == null) return;

            _document?.Dispose();
            _document = PdfDocument.Load(file.Path);
            _currentPage = 0;

            UpdateToolbar();
            await RenderPageAsync();
        }

        private async void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (_document == null || _currentPage <= 0) return;
            _currentPage--;
            UpdateToolbar();
            await RenderPageAsync();
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (_document == null || _currentPage >= _document.PageCount - 1) return;
            _currentPage++;
            UpdateToolbar();
            await RenderPageAsync();
        }

        private async Task RenderPageAsync()
        {
            if (_document == null) return;

            const float dpi = 144f;
            using var bitmap = _document.Render(_currentPage, dpi, dpi, true);

            using var ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;

            using var ras = new InMemoryRandomAccessStream();
            using (var writer = new DataWriter(ras.GetOutputStreamAt(0)))
            {
                writer.WriteBytes(ms.ToArray());
                await writer.StoreAsync();
            }
            ras.Seek(0);

            var bitmapImage = new BitmapImage();
            await bitmapImage.SetSourceAsync(ras);
            PdfPageImage.Source = bitmapImage;
        }

        private void UpdateToolbar()
        {
            if (_document == null) return;
            PageInfoText.Text = $"{_currentPage + 1} / {_document.PageCount}";
            PrevButton.IsEnabled = _currentPage > 0;
            NextButton.IsEnabled = _currentPage < _document.PageCount - 1;
        }
    }
}
