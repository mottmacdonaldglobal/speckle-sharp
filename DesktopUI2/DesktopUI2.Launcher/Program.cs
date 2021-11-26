﻿using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesktopUI2.ViewModels;
using DesktopUI2.Views.Pages;
using ReactiveUI;
using Splat;
using System.Diagnostics;
using System.Threading;

namespace DesktopUI2.Launcher
{
  class Program
  {
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {


      //Debugger.Launch();
      //while (!Debugger.IsAttached)
      //  Thread.Sleep(100);

      string path = Path.GetDirectoryName(typeof(App).Assembly.Location);

      string nativeLib = Path.Combine(path, "Native", "libAvalonia.Native.OSX.dylib");
      return AppBuilder.Configure<App>()
      .UsePlatformDetect()
      .With(new X11PlatformOptions { UseGpu = false })
      .With(new MacOSPlatformOptions { ShowInDock = false })
      .With(new AvaloniaNativePlatformOptions
      {
        AvaloniaNativeLibraryPath = nativeLib
      })
      .With(new SkiaOptions { MaxGpuResourceSizeBytes = 8096000 })
      .With(new Win32PlatformOptions { AllowEglInitialization = true, EnableMultitouch = false })
      .LogToTrace()
      .UseReactiveUI();
    }

  }
}