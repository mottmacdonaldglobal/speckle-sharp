﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Speckle.DesktopUI.Utils
{
  public class StringToUpperConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if ( value != null && value is string )
      {
        return ( ( string ) value ).ToUpper();
      }

      return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return null;
    }
  }

  public class TimeAgoConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if ( value != null )
      {
        return Formatting.TimeAgo(( string ) value);
      }

      return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return null;
    }
  }
}
