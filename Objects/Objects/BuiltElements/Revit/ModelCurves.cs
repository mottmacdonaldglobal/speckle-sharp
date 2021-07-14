﻿using Speckle.Core.Kits;
using Speckle.Core.Models;
using System.Collections.Generic;

namespace Objects.BuiltElements.Revit.Curve
{
  public class ModelCurve : Base
  {
    public ICurve baseCurve { get; set; }
    public string lineStyle { get; set; }
    public Base parameters { get; set; }
    public string elementId { get; set; }

    public ModelCurve() { }

    [SchemaInfo("ModelCurve", "Creates a Revit model curve", "Revit", "Curves")]
    public ModelCurve([SchemaMainParam] ICurve baseCurve, string lineStyle, Base parameters = null)
    {
      this.baseCurve = baseCurve;
      this.lineStyle = lineStyle;
      this.parameters = parameters;
    }
  }

  public class DetailCurve : Base
  {
    public ICurve baseCurve { get; set; }
    public string lineStyle { get; set; }
    public Base parameters { get; set; }
    public string elementId { get; set; }

    public DetailCurve() { }

    [SchemaInfo("DetailCurve", "Creates a Revit detail curve", "Revit", "Curves")]
    public DetailCurve([SchemaMainParam] ICurve baseCurve, string lineStyle, Base parameters = null)
    {
      this.baseCurve = baseCurve;
      this.lineStyle = lineStyle;
      this.parameters = parameters;
    }
  }

  public class RoomBoundaryLine : Base
  {
    public ICurve baseCurve { get; set; }
    public Base parameters { get; set; }
    public string elementId { get; set; }

    public RoomBoundaryLine() { }

    [SchemaInfo("RoomBoundaryLine", "Creates a Revit room boundary line", "Revit", "Curves")]
    public RoomBoundaryLine([SchemaMainParam] ICurve baseCurve, Base parameters = null)
    {
      this.baseCurve = baseCurve;
      this.parameters = parameters;
    }
  }
}
