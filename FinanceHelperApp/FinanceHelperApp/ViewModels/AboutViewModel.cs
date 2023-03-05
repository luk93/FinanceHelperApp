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
        private string _someText = string.Empty;
        private string _filePath = string.Empty;
        private MediaFile _mediaFile;

        private readonly ITesseractApi _tesseract;
        private readonly IMediaPicker _mediaPicker;
        public string SomeText
        {
            get
            {
                return _someText;
            }
            set
            {
                SetProperty(ref _someText, value);
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
            SomeText = "Hello";
            //FilePath = "File Path";
            SelectFileCommand = new Command(async () => FilePath = await LoadImageAsync());
            DetectTextCommand = new Command(async () => await Recognise(_mediaFile));
        }

        public ICommand DetectTextCommand { get; }
        public ICommand SelectFileCommand { get; }
        private async Task<string> SelectFileAsync()
        {
            try
            {
                var fileResult = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.iOS, new[] { "public.jpeg" } },
                        { DevicePlatform.Android, new[] { "image/jpeg" } },
                        { DevicePlatform.UWP, new[] { ".jgp" } }
                    }),
                    PickerTitle = "Wybierz plik jpg"
                });

                if (fileResult != null)
                {
                    // Plik został wybrany, zwracamy jego ścieżkę
                    return fileResult.FullPath;
                }
                else
                {
                    // Użytkownik anulował wybór pliku
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Wystąpił błąd podczas wybierania pliku
                await Application.Current.MainPage.DisplayAlert("Błąd", $"Wystąpił błąd podczas wybierania pliku: {ex.Message}", "OK");
                return null;
            }
        }
        private async Task<string> LoadImageAsync()
        {
            var result = await _mediaPicker.SelectPhotoAsync(new CameraMediaStorageOptions());
            _mediaFile = result;
            return result.Path;
            //await Recognise(result);
        }

        private async Task GetPhotoAsync()
        {
            var result = await _mediaPicker.TakePhotoAsync(new CameraMediaStorageOptions());
            await Recognise(result);
        }
        async Task Recognise(MediaFile result)
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
                        SomeText = _tesseract.Text;
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