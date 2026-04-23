using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using PdfiumViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System;

namespace JustView
{
    public sealed partial class MainWindow : Window
    {
        private PdfDocument? _document;
        private readonly List<Image> _pageImages = new();
        private bool _isUpdatingPageText;
        private bool _isScrollUnfocusing;

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

            TotalPagesText.Text = _document.PageCount.ToString();
            PageNumberBox.IsEnabled = true;

            await RenderAllPagesAsync();
        }

        private async Task RenderAllPagesAsync()
        {
            if (_document == null) return;

            PagesPanel.Children.Clear();
            _pageImages.Clear();

            const float dpi = 144f;
            for (int i = 0; i < _document.PageCount; i++)
            {
                using var bitmap = _document.Render(i, dpi, dpi, true);
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

                var image = new Image
                {
                    Source = bitmapImage,
                    Stretch = Microsoft.UI.Xaml.Media.Stretch.None,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                _pageImages.Add(image);
                PagesPanel.Children.Add(image);
            }

            PageNumberBox.Text = "1";
        }

        private void PageScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            if (PageNumberBox.FocusState != FocusState.Unfocused)
            {
                _isScrollUnfocusing = true;
                PageScrollViewer.Focus(FocusState.Pointer);
                _isScrollUnfocusing = false;
            }
        }

        private void PageScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (_pageImages.Count == 0) return;
            _isUpdatingPageText = true;
            PageNumberBox.Text = GetCurrentVisiblePage().ToString();
            _isUpdatingPageText = false;
        }

        private void PageNumberBox_GotFocus(object sender, RoutedEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() => PageNumberBox.SelectAll());
        }

        private void PageNumberBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            if (_isUpdatingPageText || _document == null) return;

            string digits = new(sender.Text.Where(char.IsDigit).ToArray());

            string corrected;
            if (string.IsNullOrEmpty(digits))
            {
                // 빈 문자열은 허용 (입력 중간 상태)
                corrected = digits;
            }
            else if (int.TryParse(digits, out int page))
            {
                corrected = page == 0 ? "1"
                    : page > _document.PageCount ? _document.PageCount.ToString()
                    : digits;
            }
            else
            {
                corrected = digits;
            }

            if (corrected == sender.Text) return;

            _isUpdatingPageText = true;
            sender.Text = corrected;
            sender.SelectionStart = corrected.Length;
            _isUpdatingPageText = false;
        }

        private void PageNumberBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != VirtualKey.Enter) return;
            NavigateToInputPage();
        }

        private void PageNumberBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_isScrollUnfocusing) return;
            NavigateToInputPage();
        }

        private void NavigateToInputPage()
        {
            if (_document == null) return;

            if (!int.TryParse(PageNumberBox.Text, out int page) || page < 1)
            {
                PageNumberBox.Text = GetCurrentVisiblePage().ToString();
                return;
            }

            int pageIndex = page - 1;
            var transform = _pageImages[pageIndex].TransformToVisual(PagesPanel);
            var pos = transform.TransformPoint(new Point(0, 0));
            PageScrollViewer.ChangeView(null, pos.Y - 16, null, disableAnimation: true);
        }

        private int GetCurrentVisiblePage()
        {
            if (_pageImages.Count == 0) return 1;
            double viewportCenter = PageScrollViewer.VerticalOffset + PageScrollViewer.ViewportHeight / 2;
            for (int i = 0; i < _pageImages.Count; i++)
            {
                var transform = _pageImages[i].TransformToVisual(PagesPanel);
                var pos = transform.TransformPoint(new Point(0, 0));
                if (pos.Y + _pageImages[i].ActualHeight > viewportCenter)
                    return i + 1;
            }
            return _pageImages.Count;
        }
    }
}
