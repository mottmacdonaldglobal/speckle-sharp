﻿using Grasshopper.Kernel.Types;
using Objects.Geometry;
using Objects.Primitive;
using Objects.Other;
using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Alignment = Objects.BuiltElements.Alignment;
using Arc = Objects.Geometry.Arc;
using Box = Objects.Geometry.Box;
using Brep = Objects.Geometry.Brep;
using Circle = Objects.Geometry.Circle;
using Curve = Objects.Geometry.Curve;
using DirectShape = Objects.BuiltElements.Revit.DirectShape;
using Ellipse = Objects.Geometry.Ellipse;
using Hatch = Objects.Other.Hatch;
using Interval = Objects.Primitive.Interval;
using Line = Objects.Geometry.Line;
using Mesh = Objects.Geometry.Mesh;
using ModelCurve = Objects.BuiltElements.Revit.Curve.ModelCurve;
using Plane = Objects.Geometry.Plane;
using Point = Objects.Geometry.Point;
using Polyline = Objects.Geometry.Polyline;
using View3D = Objects.BuiltElements.View3D;

using RH = Rhino.Geometry;

using Surface = Objects.Geometry.Surface;
using Vector = Objects.Geometry.Vector;

namespace Objects.Converter.RhinoGh
{
  public partial class ConverterRhinoGh : ISpeckleConverter
  {
#if RHINO6
    public static string RhinoAppName = Applications.Rhino6;
    public static string GrasshopperAppName = Applications.Grasshopper;
#elif RHINO7
    public static string RhinoAppName = Applications.Rhino7;
    public static string GrasshopperAppName = Applications.Grasshopper;
#endif

    public ConverterRhinoGh()
    {
      var ver = System.Reflection.Assembly.GetAssembly(typeof(ConverterRhinoGh)).GetName().Version;
      Report.Log($"Using converter: {Name} v{ver}");
    }
    public string Description => "Default Speckle Kit for Rhino & Grasshopper";
    public string Name => nameof(ConverterRhinoGh);
    public string Author => "Speckle";
    public string WebsiteOrEmail => "https://speckle.systems";

    public ProgressReport Report { get; private set; } = new ProgressReport();

    public IEnumerable<string> GetServicedApplications()
    {

#if RHINO6
      return new string[] { RhinoAppName, Applications.Grasshopper };
#elif RHINO7
      return new string[] {RhinoAppName};
#endif   
    }

    public HashSet<Exception> ConversionErrors { get; private set; } = new HashSet<Exception>();

    public RhinoDoc Doc { get; private set; } = Rhino.RhinoDoc.ActiveDoc ?? null;

    public List<ApplicationPlaceholderObject> ContextObjects { get; set; } = new List<ApplicationPlaceholderObject>();

    public void SetContextObjects(List<ApplicationPlaceholderObject> objects) => ContextObjects = objects;

    public void SetPreviousContextObjects(List<ApplicationPlaceholderObject> objects) => throw new NotImplementedException();

    public void SetContextDocument(object doc)
    {
      Doc = (RhinoDoc)doc;
      Report.Log($"Using document: {Doc.Path}");
      Report.Log($"Using units: {ModelUnits}");
    }

    // speckle user string for custom schemas
    string SpeckleSchemaKey = "SpeckleSchema";

    public Base ConvertToSpeckle(object @object)
    {
      RenderMaterial material = null;
      DisplayStyle style = null;
      Base @base = null;
      Base schema = null;
      if (@object is RhinoObject ro)
      {
        material = GetMaterial(ro);
        style = GetStyle(ro);

        if (ro.Attributes.GetUserString(SpeckleSchemaKey) != null) // schema check - this will change in the near future
           schema = ConvertToSpeckleBE(ro) ?? ConvertToSpeckleStr(ro);

        if (!(@object is InstanceObject)) // block instance check
          @object = ro.Geometry;
      }

      switch (@object)
      {
        case Point3d o:
          @base = PointToSpeckle(o);
          Report.Log($"Converted Point {o}");
          break;
        case Rhino.Geometry.Point o:
          @base = PointToSpeckle(o);
          Report.Log($"Converted Point {o}");
          break;
        case PointCloud o:
          @base = PointcloudToSpeckle(o);
          Report.Log($"Converted PointCloud");
          break;
        case Vector3d o:
          @base = VectorToSpeckle(o);
          Report.Log($"Converted Vector3d {o}");
          break;
        case RH.Interval o:
          @base = IntervalToSpeckle(o);
          Report.Log($"Converted Interval {o}");
          break;
        case UVInterval o:
          @base = Interval2dToSpeckle(o);
          Report.Log($"Converted Interval2d {o}");
          break;
        case RH.Line o:
          @base = LineToSpeckle(o);
          Report.Log($"Converted Line");
          break;
        case LineCurve o:
          @base = LineToSpeckle(o);
          Report.Log($"Converted LineCurve");
          break;
        case RH.Plane o:
          @base = PlaneToSpeckle(o);
          Report.Log($"Converted Plane");
          break;
        case Rectangle3d o:
          @base = PolylineToSpeckle(o);
          Report.Log($"Converted Polyline");
          break;
        case RH.Circle o:
          @base = CircleToSpeckle(o);
          Report.Log($"Converted Circle");
          break;
        case RH.Arc o:
          @base = ArcToSpeckle(o);
          Report.Log($"Converted Arc");
          break;
        case ArcCurve o:
          @base = ArcToSpeckle(o);
          Report.Log($"Converted Arc");
          break;
        case RH.Ellipse o:
          @base = EllipseToSpeckle(o);
          Report.Log($"Converted Ellipse");
          break;
        case RH.Polyline o:
          @base = PolylineToSpeckle(o) as Base;
          Report.Log($"Converted Polyline");
          break;
        case NurbsCurve o:
          if (o.TryGetEllipse(out RH.Ellipse ellipse))
          {
            @base = EllipseToSpeckle(ellipse);
            Report.Log($"Converted NurbsCurve as Ellipse");
          }
          else
          {
            @base = CurveToSpeckle(o) as Base;
            Report.Log($"Converted NurbsCurve");
          }
          break;
        case PolylineCurve o:
          @base = PolylineToSpeckle(o);
          Report.Log($"Converted Polyline Curve");
          break;
        case PolyCurve o:
          @base = PolycurveToSpeckle(o);
          Report.Log($"Converted PolyCurve");
          break;
        case RH.Box o:
          @base = BoxToSpeckle(o);
          Report.Log($"Converted Box");
          break;
        case RH.Hatch o:
          @base = HatchToSpeckle(o);
          Report.Log($"Converted Hatch");
          break;
        case RH.Mesh o:
          @base = MeshToSpeckle(o);
          Report.Log($"Converted Mesh");
          break;
#if RHINO7
        case RH.SubD o:
          if (o.HasBrepForm)
          {
            @base = BrepToSpeckle(o.ToBrep(new SubDToBrepOptions()));
            Report.Log($"Converted SubD as BREP");
          }
          else
          {
            @base = MeshToSpeckle(o);
            Report.Log($"Converted SubD as Mesh");
          }
          break;
#endif
        case RH.Extrusion o:
          @base = BrepToSpeckle(o.ToBrep());
          Report.Log($"Converted Extrusion as Brep");
          break;
        case RH.Brep o:
          @base = BrepToSpeckle(o.DuplicateBrep());
          Report.Log($"Converted Brep");
          break;
        case NurbsSurface o:
          @base = SurfaceToSpeckle(o);
          Report.Log($"Converted NurbsSurface");
          break;
        case ViewInfo o:
          @base = ViewToSpeckle(o);
          Report.Log($"Converted ViewInfo");
          break;
        case InstanceDefinition o:
          @base = BlockDefinitionToSpeckle(o);
          Report.Log($"Converted InstanceDefinition {o.Id}");
          break;
        case InstanceObject o:
          @base = BlockInstanceToSpeckle(o);
          Report.Log($"Converted BlockInstance {o.Id}");
          break;
        case TextEntity o:
          @base = TextToSpeckle(o);
          Report.Log($"Converted TextEntity");
          break;
        default:
          Report.Log($"Skipped not supported type: {@object.GetType()}");
          throw new NotSupportedException();
      }

      if (material != null)
        @base["renderMaterial"] = material;

      if (style != null)
        @base["displayStyle"] = style;

      if (schema != null)
      {
        schema["renderMaterial"] = material;
        @base["@SpeckleSchema"] = schema;
      }

      return @base;
    }

    public List<Base> ConvertToSpeckle(List<object> objects)
    {
      return objects.Select(x => ConvertToSpeckle(x)).ToList();
    }

    public Base ConvertToSpeckleBE(object @object)
    {
      // get schema if it exists
      RhinoObject obj = @object as RhinoObject;
      string schema = GetSchema(obj, out string[] args);

      Base schemaBase = null;
      if (obj is InstanceObject)
      {
        if (schema == "AdaptiveComponent")
          schemaBase = InstanceToAdaptiveComponent(obj as InstanceObject, args);
        else
          Report.Log($"Skipping Instance conversion to unsupported schema {schema}");
      }

      switch (obj.Geometry)
      {
        case RH.Curve o:
          switch (schema)
          {
            case "Column":
              schemaBase = CurveToSpeckleColumn(o);
              Report.Log($"Converted Curve to Column");
              break;

            case "Beam":
              schemaBase = CurveToSpeckleBeam(o);
              Report.Log("Converted Curve to Beam");
              break;

            default:
              Report.Log($"Skipping Curve conversion to schema {schema}");
              break;
          }
          break;

        case RH.Brep o:
          switch (schema)
          {
            case "Floor":
              schemaBase = BrepToSpeckleFloor(o);
              Report.Log($"Converted Brep to Floor");
              break;

            case "Roof":
              schemaBase = BrepToSpeckleRoof(o);
              Report.Log($"Converted Brep to Roof");
              break;

            case "Wall":
              schemaBase = BrepToSpeckleWall(o);
              Report.Log($"Converted Brep to Wall");
              break;

            case "FaceWall":
              schemaBase = BrepToFaceWall(o, args);
              Report.Log($"Converted Brep to Face Wall");
              break;

            case "DirectShape":
              schemaBase = BrepToDirectShape(o, args);
              Report.Log($"Converted Brep to DirectShape");
              break;

            default:
              Report.Log($"Skipping Brep Conversion to unsupported schema {schema}");
              break;
          }
          break;

        case RH.Extrusion o:
          switch (schema)
          {
            case "Floor":
              schemaBase = BrepToSpeckleFloor(o.ToBrep());
              Report.Log($"Converted Extrusion to Floor");
              break;

            case "Roof":
              schemaBase = BrepToSpeckleRoof(o.ToBrep());
              Report.Log($"Converted Extrusion to Roof");
              break;

            case "Wall":
              schemaBase = BrepToSpeckleWall(o.ToBrep());
              Report.Log($"Converted Extrusion to Wall");
              break;

            case "FaceWall":
              schemaBase = BrepToFaceWall(o.ToBrep(), args);
              Report.Log($"Converted Extrusion to FaceWall");
              break;

            case "DirectShape":
              schemaBase = ExtrusionToDirectShape(o, args);
              Report.Log($"Converted Extrusion to DirectShape");
              break;

            default:
              Report.Log($"Skipping Extrusion conversion to unsupported schema {schema}");
              break;
          }
          break;

        case RH.Mesh o:
          switch (schema)
          {
            case "DirectShape":
              schemaBase = MeshToDirectShape(o, args);
              Report.Log($"Converted Mesh to DirectShape");
              break;

            default:
              Report.Log($"Skipping Mesh conversion to unsupported schema {schema}");
              break;
          }
          break;

        default:
          Report.Log($"{obj.GetType()} is not supported in schema conversions.");
          break;
      }
      return schemaBase;
    }

    public List<Base> ConvertToSpeckleBE(List<object> objects)
    {
      return objects.Select(x => ConvertToSpeckleBE(x)).ToList();
    }

    public Base ConvertToSpeckleStr(object @object)
    {
      // get schema if it exists
      RhinoObject obj = @object as RhinoObject;
      string schema = GetSchema(obj, out string[] args);

      switch (obj.Geometry)
      {

        //case RH.Point o:
        //    switch (schema)
        //    {
        //        case "Node":
        //            return PointToSpeckleNode(o);

        //        default:
        //            throw new NotSupportedException();
        //    }

        //case RH.Curve o:
        //    switch (schema)
        //    {
        //        case "Element1D":
        //            return CurveToSpeckleElement1D(o);

        //        default:
        //            throw new NotSupportedException();
        //    }

        //case RH.Mesh o:
        //    switch (schema)
        //    {
        //        case "Element2D":
        //            return MeshToSpeckleElement2D(o);

        //    case "Element3D":
        //        return MeshToSpeckleElement3D(o);

        //            default:
        //            throw new NotSupportedException();
        //    }

        default:
          throw new NotSupportedException();
      }
    }

    public List<Base> ConvertToSpeckleStr(List<object> objects)
    {
      return objects.Select(x => ConvertToSpeckleStr(x)).ToList();
    }


    public object ConvertToNative(Base @object)
    {
      object rhinoObj = null;
      switch (@object)
      {
        case Point o:
          rhinoObj = PointToNative(o);
          Report.Log($"Created Point {o.id}");
          break;

        case Pointcloud o:
          rhinoObj = PointcloudToNative(o);
          Report.Log($"Created PointCloud {o.id}");
          break;

        case Vector o:
          rhinoObj = VectorToNative(o);
          Report.Log($"Created Vector {o.id}");
          break;

        case Hatch o:
          rhinoObj = HatchToNative(o);
          Report.Log($"Created Hatch {o.id}");
          break;

        case Interval o:
          rhinoObj = IntervalToNative(o);
          Report.Log($"Created Interval {o.id}");
          break;

        case Interval2d o:
          rhinoObj = Interval2dToNative(o);
          Report.Log($"Created Interval2d {o.id}");
          break;

        case Line o:
          rhinoObj = LineToNative(o);
          Report.Log($"Created Line {o.id}");
          break;

        case Plane o:
          rhinoObj = PlaneToNative(o);
          Report.Log($"Created Plane {o.id}");
          break;

        case Circle o:
          rhinoObj = CircleToNative(o);
          Report.Log($"Created Circle {o.id}");
          break;

        case Arc o:
          rhinoObj = ArcToNative(o);
          Report.Log($"Created Arc {o.id}");
          break;

        case Ellipse o:
          rhinoObj = EllipseToNative(o);
          Report.Log($"Created Ellipse {o.id}");
          break;

        case Polyline o:
          rhinoObj = PolylineToNative(o);
          Report.Log($"Created Polyline {o.id}");
          break;

        case Polycurve o:
          rhinoObj = PolycurveToNative(o);
          Report.Log($"Created PolyCurve {o.id}");
          break;

        case Curve o:
          rhinoObj = CurveToNative(o);
          Report.Log($"Created Curve {o.id}");
          break;

        case Box o:
          rhinoObj = BoxToNative(o);
          Report.Log($"Created Box {o.id}");
          break;

        case Mesh o:
          rhinoObj = MeshToNative(o);
          Report.Log($"Created Mesh {o.id}");
          break;

        case Brep o:
          // Brep conversion should always fallback to mesh if it fails.
          var b = BrepToNative(o);
          if (b == null)
          {
            rhinoObj = (o.displayMesh != null) ? MeshToNative(o.displayMesh) : null;
            Report.Log($"Created Brep {o.id} as Mesh");
          }
          else
          {
            rhinoObj = b;
            Report.Log($"Created Brep {o.id}");
          }
          break;
        case Surface o:
          rhinoObj = SurfaceToNative(o);
          Report.Log($"Created Surface {o.id}");
          break;

        case Alignment o:
          rhinoObj = CurveToNative(o.baseCurve);
          Report.Log($"Created Alignment {o.id}");
          break;

        case ModelCurve o:
          rhinoObj = CurveToNative(o.baseCurve);
          Report.Log($"Created ModelCurve {o.id}");
          break;

        case DirectShape o:
          if (o.displayMesh != null)
          {
            rhinoObj = MeshToNative(o.displayMesh);
            Report.Log($"Created DirectShape {o.id}");
          }
          Report.Log($"Skipping DirectShape {o.id} because it has no displayMesh");
          break;

        case View3D o:
          rhinoObj = ViewToNative(o);
          Report.Log($"Created View3D {o.id}");
          break;

        case BlockDefinition o:
          rhinoObj = BlockDefinitionToNative(o);
          Report.Log($"Created BlockDefinition {o.id}");
          break;

        case BlockInstance o:
          rhinoObj = BlockInstanceToNative(o);
          Report.Log($"Created BlockInstance {o.id}");
          break;

        case Text o:
          rhinoObj = TextToNative(o);
          Report.Log($"Created Text {o.id}");
          break;

        default:
          Report.Log($"Skipped not supported type: {@object.GetType()} {@object.id}");
          throw new NotSupportedException();
      }

      return rhinoObj;
    }

    public List<object> ConvertToNative(List<Base> objects)
    {
      return objects.Select(x => ConvertToNative(x)).ToList();
    }

    public bool CanConvertToSpeckle(object @object)
    {
      if (@object is RhinoObject ro && !(@object is InstanceObject))
      {
        @object = ro.Geometry;
      }

      switch (@object)
      {
        case Point3d _:
        case Rhino.Geometry.Point _:
        case PointCloud _:
        case Vector3d _:
        case RH.Interval _:
        case UVInterval _:
        case RH.Line _:
        case LineCurve _:
        case Rhino.Geometry.Hatch _:
        case RH.Plane _:
        case Rectangle3d _:
        case RH.Circle _:
        case RH.Arc _:
        case ArcCurve _:
        case RH.Ellipse _:
        case RH.Polyline _:
        case PolylineCurve _:
        case PolyCurve _:
        case NurbsCurve _:
        case RH.Box _:
        case RH.Mesh _:
#if RHINO7
case RH.SubD _:
#endif
        case RH.Extrusion _:
        case RH.Brep _:
        case NurbsSurface _:
          return true;
        // TODO: This types are not supported in GH!
        case ViewInfo _:
        case InstanceDefinition _:
        case InstanceObject _:
        case TextEntity _:
          return true;

        default:

          return false;
      }
    }

    public bool CanConvertToNative(Base @object)
    {
      switch (@object)
      {
        case Point _:
        case Vector _:
        case Interval _:
        case Interval2d _:
        case Line _:
        case Plane _:
        case Circle _:
        case Arc _:
        case Ellipse _:
        case Polyline _:
        case Polycurve _:
        case Curve _:
        case Hatch _:
        case Box _:
        case Mesh _:
        case Brep _:
        case Surface _:
          return true;

        //TODO: This types are not supported in GH!
        case Pointcloud _:
        case ModelCurve _:
        case DirectShape _:
        case View3D _:
        case BlockDefinition _:
        case BlockInstance _:
        case Alignment _:
        case Text _:
          return true;

        default:
          return false;
      }
    }
  }
}