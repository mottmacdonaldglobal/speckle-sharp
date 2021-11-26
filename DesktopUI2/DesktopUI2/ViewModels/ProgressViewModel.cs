﻿using ReactiveUI;
using Speckle.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;

namespace DesktopUI2.ViewModels
{


  public class ProgressViewModel : ReactiveObject
  {
    public CancellationTokenSource CancellationTokenSource { get; set; } = new CancellationTokenSource();

    public ProgressReport Report { get; set; } = new ProgressReport();


    private ConcurrentDictionary<string, int> _progressDict;
    public ConcurrentDictionary<string, int> ProgressDict
    {
      get => _progressDict;
      set
      {
        ProgressSummary = "";
        foreach (var kvp in value)
        {
          ProgressSummary += $"{kvp.Key}: {kvp.Value} ";
        }
        //NOTE: progress set to indeterminate until the TotalChildrenCount is correct
        ProgressSummary += $"Total: {Max}";
        _progressDict = value;
        this.RaiseAndSetIfChanged(ref _progressDict, value);
      }
    }


    private string _progressSummary;
    public string ProgressSummary
    {
      get => _progressSummary;
      set
      {
        this.RaiseAndSetIfChanged(ref _progressSummary, value);
      }
    }

    private int _value = 0;
    public int Value
    {
      get => _value;
      set
      {
        this.RaiseAndSetIfChanged(ref _value, value);
        this.RaisePropertyChanged(nameof(IsIndeterminate));
      }
    }

    private int _max = 0;
    public int Max
    {
      get => _max;
      set
      {
        this.RaiseAndSetIfChanged(ref _max, value);
        this.RaisePropertyChanged(nameof(IsIndeterminate));
      }
    }

    public bool IsIndeterminate { get => Value == 0 || Max == Value; }

    private bool _isProgressing = false;
    public bool IsProgressing
    {
      get => _isProgressing;
      set
      {
        this.RaiseAndSetIfChanged(ref _isProgressing, value);
      }
    }



    public void Update(ConcurrentDictionary<string, int> pd)
    {
      //Avalonia.Threading.Dispatcher.UIThread.Post(() =>
      //{
      ProgressDict = pd;
      Value = pd.Values.Last();
      //}, Avalonia.Threading.DispatcherPriority.MaxValue);
    }

    public void GetHelpCommand()
    {
      var report = "";
      if (Report.OperationErrorsCount > 0)
      {
        report += "OPERATION ERRORS\n\n";
        report += Report.OperationErrorsString;
      }

      if (Report.ConversionErrorsCount > 0)
      {
        if (Report.OperationErrorsCount > 0)
          report += "\n\n";
        report += "CONVERSION ERRORS\n\n";
        report += Report.ConversionErrorsString;
      }
      var safeReport = HttpUtility.UrlEncode(report);
      Process.Start(new ProcessStartInfo($"https://speckle.community/new-topic?title=I%20need%20help%20with...&body={safeReport}&category=help") { UseShellExecute = true });
    }
  }
}