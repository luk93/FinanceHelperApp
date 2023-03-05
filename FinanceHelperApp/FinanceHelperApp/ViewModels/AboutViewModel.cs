using System;
using System.Collections.Generic;
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
        private MediaFile _mediaFile;

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
                SetProperty(ref _detectedText, value);
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
                SetProperty(ref _filePath, value);
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

            Title = "About";
            DetectedText = "Hello";
            //FilePath = "File Path";
            LoadImageCommand = new Command(async () => await LoadImageAsync());
            DetectTextCommand = new Command(async () => await GetText(_mediaFile));
            TakePhotoCommand = new Command(async () => await TakePhotoAsync());
        }


      
        private async Task LoadImageAsync()
        {
            var result = await _mediaPicker.SelectPhotoAsync(new CameraMediaStorageOptions());
            _mediaFile = result;
            FilePath = result.Path;
        }

        private async Task TakePhotoAsync()
        {
            var result = await _mediaPicker.TakePhotoAsync(new CameraMediaStorageOptions());
            _mediaFile = result;
            FilePath = result.Path;
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