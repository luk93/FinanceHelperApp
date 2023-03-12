using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Tesseract;
using Xamarin.Essentials;
using Xamarin.Forms;
using XLabs.Ioc;
using XLabs.Platform.Services.Media;

namespace FinanceHelperApp.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        private string _detectedText = string.Empty;
        private string _filePath = string.Empty;
        private MediaFile _mediaFile = null;

        private readonly ITesseractApi _tesseract;
        private readonly IMediaPicker _mediaPicker;
        public string DetectedText
        {
            get
            {
                return _detectedText;
            }
            set
            {
                SetProperty(ref _detectedText, value, DetectedText);
            }
        }
        public string FilePath
        {
            get
            {
                return _filePath;
            }
            set
            {
                SetProperty(ref _filePath, value, FilePath);
            }
        }

        public MediaFile MediaFile
        {
            get
            {
                return _mediaFile;
            }
            set
            {
                SetProperty(ref _mediaFile, value, "MediaFile");
            }
        }
        public ICommand DetectTextCommand { get; }
        public ICommand LoadImageCommand { get; }
        public ICommand TakePhotoCommand { get; }
        public AboutViewModel()
        {
            try
            {
                _mediaPicker = Resolver.Resolve<IMediaPicker>();
                _tesseract = Resolver.Resolve<ITesseractApi>();
            }
            catch (Exception ex)
            {
                FilePath = ex.Message;
            }

            Title = "Detect text in image";
            DetectedText = "Detected Text";
            FilePath = "File Path";
            LoadImageCommand = new Command(async () => await LoadImageAsync());
            DetectTextCommand = new Command(async () => await GetText(_mediaFile));
            TakePhotoCommand = new Command(async () => await TakePhotoAsync());
        }

        private async Task LoadImageAsync()
        {
            try
            {
                var result = await _mediaPicker.SelectPhotoAsync(new CameraMediaStorageOptions());
                MediaFile = result;
                FilePath = result.Path;
            }
            catch (TaskCanceledException ex)
            {
                // handle cancellation exception
            }
        }

        private async Task TakePhotoAsync()
        {
            try
            {
                var result = await _mediaPicker.TakePhotoAsync(new CameraMediaStorageOptions()
                {
                    DefaultCamera = CameraDevice.Front,
                    SaveMediaOnCapture = true,
                    Name = string.Format("FinanceApp_{0}", DateTime.Now.ToString("yyMMddhhmmss")),
                    MaxPixelDimension = 1024,
                    PercentQuality = 85
                });
                MediaFile = result;
                FilePath = result.Path;
            }
            catch (TaskCanceledException ex)
            {
                var stack = ex.StackTrace;
                var msg = ex.Message;
                var inner = ex.InnerException;
                var t = ex.Task.ToString();
                var x = ex.TargetSite.ToString();
            }
        }
        async Task GetText(MediaFile result)
        {
            if (result.Source == null)
                return;
            if (!_tesseract.Initialized)
            {
                try
                {
                    var initialised = await _tesseract.Init("pol");
                    if (!initialised)
                        return;
                }
                catch (Exception ex)
                {
                    var msg = ex.Message;
                    var stack = ex.StackTrace;
                    return;
                }
            }
            _tesseract.SetImage(result.Source)
                .ContinueWith((t) =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        DetectedText = _tesseract.Text;
                        var words = _tesseract.Results(PageIteratorLevel.Word);
                        var symbols = _tesseract.Results(PageIteratorLevel.Symbol);
                        var blocks = _tesseract.Results(PageIteratorLevel.Block);
                        var paragraphs = _tesseract.Results(PageIteratorLevel.Paragraph);
                        var lines = _tesseract.Results(PageIteratorLevel.Textline);
                    });
                });
        }
    }
}