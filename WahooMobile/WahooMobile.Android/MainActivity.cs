using Android.App;
using Android.OS;
using Xamarin.Forms.Labs;
using Xamarin.Forms.Labs.Droid;
using Xamarin.Forms.Labs.Services;

namespace WahooMobile.Droid {
  [Activity(Label = "WahooMobile", MainLauncher = true)]
  public class MainActivity : XFormsApplicationDroid {
    static bool _initialized;

    protected override void OnCreate(Bundle bundle) {
      base.OnCreate(bundle);
      if (!_initialized) { SetIoC(); }
      Xamarin.Forms.Forms.Init(this, bundle);
      SetPage(App.GetMainPage());
    }

    void SetIoC() {
      var resolverContainer = new SimpleContainer();
      resolverContainer
        .Register<IDevice>(t => AndroidDevice.CurrentDevice)
        .Register<IDisplay>(t => t.Resolve<IDevice>().Display)
        .Register<IDependencyContainer>(resolverContainer)
      ;

      Resolver.SetResolver(resolverContainer.GetResolver());
      _initialized = true;
    }
  }
}

