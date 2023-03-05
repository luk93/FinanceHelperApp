using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using Xamarin.Forms;
using XLabs.Platform.Services.Media;

namespace FinanceHelperApp.Converters
{
    public class MediaFileToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            MediaFile mediaFile = value as MediaFile;
            if (mediaFile != null)
            {
                using (Stream stream = mediaFile.Source)
                {
                    ImageSource imageSource = ImageSource.FromStream(() => stream);
                    return imageSource;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
