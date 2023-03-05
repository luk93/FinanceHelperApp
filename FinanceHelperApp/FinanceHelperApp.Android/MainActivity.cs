using System;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Autofac;
using Tesseract;
using Tesseract.Droid;
using XLabs.Ioc;
using XLabs.Ioc.Autofac;
using static AndroidX.Concurrent.Futures.CallbackToFutureAdapter;
using XLabs.Platform.Services.Media;

namespace FinanceHelperApp.Droid
{
    [Activity(Label = "FinanceHelperApp", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            #region Autofac definition
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(c => this).As<Context>();
            containerBuilder.RegisterType<MediaPicker>().As<IMediaPicker>();
            containerBuilder.RegisterType<AssetsDeployment>().As<AssetsDeployment>();
            containerBuilder.RegisterType<TesseractApi>().As<ITesseractApi>()
                .WithParameter("assetsDeployment", new AssetsDeployment());
            Resolver.SetResolver(new AutofacResolver(containerBuilder.Build()));
            #endregion

            #region Permissions
            if ((ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) != (int)Permission.Granted)
                || (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) != (int)Permission.Granted))
            {
                ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage }, 1);
            }
            #endregion


            LoadApplication(new App());
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}