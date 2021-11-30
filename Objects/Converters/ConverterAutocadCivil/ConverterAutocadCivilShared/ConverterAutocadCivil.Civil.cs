#if (CIVIL2021 || CIVIL2022)
using System.Collections.Generic;
using System.Linq;

using Speckle.Core.Models;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.ApplicationServices;
using CivilDB = Autodesk.Civil.DatabaseServices;
using Acad = Autodesk.AutoCAD.Geometry;

using Alignment = Objects.BuiltElements.Alignment;
using Interval = Objects.Primitive.Interval;
using Polycurve = Objects.Geometry.Polycurve;
using Curve = Objects.Geometry.Curve;
using Featureline = Objects.BuiltElements.Featureline;
using Point = Objects.Geometry.Point;
using Brep = Objects.Geometry.Brep;
using Mesh = Objects.Geometry.Mesh;
using Pipe = Objects.BuiltElements.Pipe;
using Polyline = Objects.Geometry.Polyline;
using Station = Objects.BuiltElements.Station;
using Structure = Objects.BuiltElements.Structure;
using static Objects.BuiltElements.Alignment;
using Objects.BuiltElements;
using static Objects.BuiltElements.Profile;

namespace Objects.Converter.AutocadCivil
{
    public partial class ConverterAutocadCivil
    {
        // stations
        public Station StationToSpeckle(CivilDB.Station station)
        {
            var _station = new Station();
            _station.location = PointToSpeckle(station.Location);
            _station.type = station.StationType.ToString();
            _station.number = station.RawStation;
            _station.units = ModelUnits;

            return _station;
        }

        // alignments
        public Alignment AlignmentToSpeckle(CivilDB.Alignment alignment)
        {
            var _alignment = new Alignment();

            _alignment.baseCurve = CurveToSpeckle(alignment.BaseCurve, ModelUnits);
            if (alignment.DisplayName != null)
            _alignment.name = alignment.DisplayName;
            if (alignment.StartingStation != null)
            _alignment.startStation  = alignment.StartingStation;
            if (alignment.EndingStation != null)
            _alignment.endStation = alignment.EndingStation;

            // handle station equations
            var equations = new List<double>();
            var directions = new List<bool>();
            foreach (var stationEquation in alignment.StationEquations)
            {
            equations.AddRange(new List<double> { stationEquation.RawStationBack, stationEquation.StationBack, stationEquation.StationAhead });
            bool equationIncreasing = (stationEquation.EquationType.Equals(CivilDB.StationEquationType.Increasing)) ? true : false;
            directions.Add(equationIncreasing);
            }
            _alignment.stationEquations = equations;
            _alignment.stationEquationDirections = directions;

            _alignment.units = ModelUnits;
            _alignment.Length = alignment.Length;
            _alignment.Entities = GetEntities(alignment.Entities);
            


            return _alignment;
        }

        public IEnumerable<AlignmentEntity> GetEntities(CivilDB.AlignmentEntityCollection entities)
        {
            foreach(var entity in entities)
            {
                AlignmentEntity _entity = new AlignmentEntity(); 
                #region type select
                switch (entity.EntityType)
                {
                    case CivilDB.AlignmentEntityType.Line:
                        {
                            _entity = new AlignmentLine();
                            break;
                        }
                    case CivilDB.AlignmentEntityType.Arc:
                        {
                            _entity = new AlignmentArc();
                            break;
                        }
                    case CivilDB.AlignmentEntityType.Spiral:
                        {
                            _entity = new AlignmentSpiral();
                            break;
                        }
                    case CivilDB.AlignmentEntityType.SpiralCurveSpiral:
                        {
                            _entity = new AlignmentSCS();
                            break;
                        }
                    case CivilDB.AlignmentEntityType.SpiralLineSpiral:
                        {
                            _entity = new AlignmentSLS();
                            break;
                        }
                    case CivilDB.AlignmentEntityType.SpiralLine:
                        {
                            _entity = new AlignmentSL();
                            break;
                        }
                    case CivilDB.AlignmentEntityType.LineSpiral:
                        {
                            _entity = new AlignmentLS();
                            break;
                        }

                    case CivilDB.AlignmentEntityType.SpiralCurve:
                        {
                            _entity = new AlignmentSC();
                            break;
                        }
                    case CivilDB.AlignmentEntityType.CurveSpiral:
                        {
                            _entity = new AlignmentCS();
                            break;
                        }
                    case CivilDB.AlignmentEntityType.SpiralSpiralCurveSpiralSpiral:
                        {
                            _entity = new AlignmentSSCSS();
                            break;
                        }

                    case CivilDB.AlignmentEntityType.SpiralCurveSpiralCurveSpiral:
                        {
                            _entity = new AlignmentSCSCS();
                            break;
                        }
                    case CivilDB.AlignmentEntityType.SpiralCurveSpiralSpiralCurveSpiral:
                        {
                            _entity = new AlignmentSCSSCS();
                            break;
                        }
                    case CivilDB.AlignmentEntityType.SpiralSpiral:
                        {
                            _entity = new AlignmentSS();
                            break;
                        }
                    case CivilDB.AlignmentEntityType.SpiralSpiralCurve:
                        {
                            _entity = new AlignmentSSC();
                            break;
                        }
                    case CivilDB.AlignmentEntityType.CurveSpiralSpiral:
                        {
                            _entity = new AlignmentCSS();
                            break;
                        }
                    case CivilDB.AlignmentEntityType.MultipleSegments:
                        {
                            _entity = new AlignmentMultipleSegments();
                            break;
                        }
                    case CivilDB.AlignmentEntityType.CurveLineCurve:
                        {
                            _entity = new AlignmentCLC();
                            break;
                        }
                    case CivilDB.AlignmentEntityType.CurveReverseCurve:
                        {
                            _entity = new AlignmentCRC();
                            break;
                        }
                    case CivilDB.AlignmentEntityType.CurveCurveReverseCurve:
                        {
                            _entity = new AlignmentCCRC();
                            break;
                        }
                }


                #endregion
                _entity.subEntities = GetSubEntities(entity);
                _entity.Domain = new Interval((entity as CivilDB.AlignmentCurve).StartStation, (entity as CivilDB.AlignmentCurve).EndStation);                
                yield return _entity;
            }
        }

        public IEnumerable<AlignmentSubEntity> GetSubEntities(CivilDB.AlignmentEntity entity)
        {
            for (int i = 0; i < entity.SubEntityCount; i++)
            {
                AlignmentSubEntity _subEntity = new AlignmentSubEntity();
                #region Type select
                switch (entity[i].SubEntityType)
                {
                    case CivilDB.AlignmentSubEntityType.Arc:
                        {
                            _subEntity = new AlignmentSubEntityArc();
                            break;
                        }
                    case CivilDB.AlignmentSubEntityType.Line:
                        {
                            _subEntity = new AlignmentSubEntityLine();
                            break;
                        }
                    case CivilDB.AlignmentSubEntityType.Spiral:
                        {
                            _subEntity = new AlignmentSubEntitySpiral();
                            break;
                        }

                }
                #endregion
                _subEntity.Domain = new Interval(entity[i].StartStation, entity[i].EndStation);
                yield return _subEntity;
            }
        }

        // profiles
        public Profile ProfileToSpeckle(CivilDB.Profile profile)
        {
            //var curve = CurveToSpeckle(profile.BaseCurve, ModelUnits) as Base;
            Profile _profile = new Profile()
            {
                baseCurve = CurveToSpeckle(profile.BaseCurve, ModelUnits),
                units = ModelUnits,
                Alignment = AlignmentToSpeckle(new CivilDB.Alignment(profile.AlignmentId)),
                Entities = GetEntities(profile.Entities),
                ElevationDomain = new Interval(profile.ElevationAt(profile.StartingStation),profile.ElevationAt(profile.EndingStation)),
                Offset = profile.Offset
            };
            if (profile.DisplayName != null)
                _profile.name = profile.Name;
            if (profile.StartingStation != null)
                _profile.startStation = profile.StartingStation;
            if (profile.EndingStation != null)
                _profile.endStation = profile.EndingStation;
            return _profile;
        }

        public IEnumerable<ProfileEntity> GetEntities(CivilDB.ProfileEntityCollection entities)
        {
            foreach(var entity in entities)
            {
                ProfileEntity _entity = new ProfileEntity();
                #region Type select
                switch (entity.EntityType)
                {
                    case CivilDB.ProfileEntityType.Circular:
                        {
                            _entity = new ProfileCircular();
                            break;
                        }
                    case CivilDB.ProfileEntityType.ParabolaAsymmetric:
                        {
                            _entity = new ProfileParabolaAssymetric();
                            break;
                        }
                    case CivilDB.ProfileEntityType.None:
                        {
                            _entity = new ProfileNone();
                            break;
                        }
                    case CivilDB.ProfileEntityType.ParabolaSymmetric:
                        {
                            _entity = new ProfileParabolaSymmetric();
                            break;
                        }
                    case CivilDB.ProfileEntityType.Tangent:
                        {
                            _entity = new ProfileTangent();
                            break;
                        }

                }
                #endregion
                _entity.Domain = new Interval(entity.StartStation, entity.EndStation);
                _entity.ElevationDomain = new Interval(entity.StartElevation, entity.EndElevation);
                _entity.length = entity.Length;
                yield return _entity;
            }
        }

        // featurelines
        public Featureline FeatureLineToSpeckle(CivilDB.FeatureLine featureline)
        {
            var _featureline = new Featureline();

            _featureline.baseCurve = CurveToSpeckle(featureline.BaseCurve, ModelUnits);
            _featureline.name = (featureline.DisplayName != null) ? featureline.DisplayName : "";
            _featureline["description"] = (featureline.Description != null) ? featureline.Description : "";
            _featureline.units = ModelUnits;

            List<Point> piPoints = new List<Point>();
            List<Point> elevationPoints = new List<Point>();
      
            foreach (Autodesk.AutoCAD.Geometry.Point3d point in featureline.GetPoints(Autodesk.Civil.FeatureLinePointType.PIPoint))
            piPoints.Add(PointToSpeckle(point));
            foreach (Autodesk.AutoCAD.Geometry.Point3d point in featureline.GetPoints(Autodesk.Civil.FeatureLinePointType.ElevationPoint))
            elevationPoints.Add(PointToSpeckle(point));
            if (piPoints.Count > 0)
            _featureline[@"piPoints"] = piPoints;
            if (elevationPoints.Count > 0)
            _featureline[@"elevationPoints"] = elevationPoints;

            try { _featureline["site"] = featureline.SiteId; } catch { }

            return _featureline;
        }

        public CivilDB.FeatureLine FeatureLineToNative(Polycurve polycurve)
        {
            return null;
        }

        // surfaces
        public Mesh SurfaceToSpeckle(CivilDB.TinSurface surface)
        {
            Mesh mesh = null;

            // output vars
            var _vertices = new List<Acad.Point3d>();
            var _faces = new List<int[]>();

            foreach (var triangle in surface.GetTriangles(false))
            {
            // get vertices
            var faceIndices = new List<int>();
            foreach (var vertex in new List<CivilDB.TinSurfaceVertex>() {triangle.Vertex1, triangle.Vertex2, triangle.Vertex3})
            {
                if (!_vertices.Contains(vertex.Location))
                {
                faceIndices.Add(_vertices.Count);
                _vertices.Add(vertex.Location);
                }
                else
                {
                faceIndices.Add(_vertices.IndexOf(vertex.Location));
                }
                vertex.Dispose();
            }

            // get face
            _faces.Add(new int[] { 0, faceIndices[0], faceIndices[1], faceIndices[2] });

            triangle.Dispose();
            }

            var vertices = PointsToFlatArray(_vertices);
            var faces = _faces.SelectMany(o => o).ToArray();
            mesh = new Mesh(vertices, faces);
            mesh.units = ModelUnits;
            mesh.bbox = BoxToSpeckle(surface.GeometricExtents);

            // add tin surface props
            try{
            mesh["name"] = surface.DisplayName;
            mesh["description"] = surface.Description;
            }
            catch{}

            return mesh;
        }

        public Mesh SurfaceToSpeckle(CivilDB.GridSurface surface)
        {
            Mesh mesh = null;

            // output vars
            var _vertices = new List<Acad.Point3d>();
            var _faces = new List<int[]>();

            foreach (var cell in surface.GetCells(false))
            {
            // get vertices
            var faceIndices = new List<int>();
            foreach (var vertex in new List<CivilDB.GridSurfaceVertex>() {cell.BottomLeftVertex, cell.BottomRightVertex, cell.TopLeftVertex, cell.TopRightVertex})
            {
                if (!_vertices.Contains(vertex.Location))
                {
                faceIndices.Add(_vertices.Count);
                _vertices.Add(vertex.Location);
                }
                else
                {
                faceIndices.Add(_vertices.IndexOf(vertex.Location));
                }
                vertex.Dispose();
            }

            // get face
            _faces.Add(new int[] { 1, faceIndices[0], faceIndices[1], faceIndices[2], faceIndices[3] });

            cell.Dispose();
            }

            var vertices = PointsToFlatArray(_vertices);
            var faces = _faces.SelectMany(o => o).ToArray();
            mesh = new Mesh(vertices, faces);
            mesh.units = ModelUnits;
            mesh.bbox = BoxToSpeckle(surface.GeometricExtents);

            // add grid surface props
            try{
            mesh["name"] = surface.DisplayName;
            mesh["description"] = surface.Description;
            }
            catch{}

            return mesh;
        }

        // structures
        public Structure StructureToSpeckle(CivilDB.Structure structure)
        {
            // get ids pipes that are connected to this structure
            var pipeIds = new List<string>();
            for (int i = 0; i < structure.ConnectedPipesCount; i++)
            pipeIds.Add(structure.get_ConnectedPipe(i).ToString());

            var _structure = new Structure();

            _structure.location = PointToSpeckle(structure.Location, ModelUnits);
            _structure.pipeIds = pipeIds;
            _structure.displayMesh = SolidToSpeckle(structure.Solid3dBody);
            _structure.units = ModelUnits;

            // assign additional structure props
            _structure["name"] = (structure.DisplayName != null) ? structure.DisplayName : "";
            _structure["description"] = (structure.Description != null) ? structure.Description : "";
            try{ _structure["grate"] = structure.Grate; } catch{ }
            try{ _structure["station"] = structure.Station; } catch{ }
            try{ _structure["network"] = structure.NetworkName; } catch{ }

            return _structure;
        }

        // pipes
        // TODO: add pressurepipes and pressure fittings when they become supported by C3D api
        public Pipe PipeToSpeckle(CivilDB.Pipe pipe)
        {
            // get the pipe curve
            ICurve curve = null;
            switch (pipe.SubEntityType)
            {
            case CivilDB.PipeSubEntityType.Straight:
                var line = new Line(pipe.StartPoint, pipe.EndPoint);
                curve = CurveToSpeckle(line);
                break;
            default:
                curve = CurveToSpeckle(pipe.Spline);
                break;
            }

            var _pipe = new Pipe();
            _pipe.baseCurve = curve;
            _pipe.diameter = pipe.InnerDiameterOrWidth;
            _pipe.length = pipe.Length3DToInsideEdge;
            _pipe.displayMesh = SolidToSpeckle(pipe.Solid3dBody);
            _pipe.units = ModelUnits;

            // assign additional structure props
            _pipe["name"] = (pipe.DisplayName != null) ? pipe.DisplayName : "";
            _pipe["description"] = (pipe.DisplayName != null) ? pipe.Description : "";
            try { _pipe["shape"] = pipe.CrossSectionalShape.ToString(); } catch { }
            try { _pipe["slope"] = pipe.Slope; } catch { }
            try { _pipe["flowDirection"] = pipe.FlowDirection.ToString(); } catch { }
            try { _pipe["flowRate"] = pipe.FlowRate; } catch { }
            try { _pipe["network"] = pipe.NetworkName; } catch { }
            try { _pipe["startOffset"] = pipe.StartOffset; } catch { }
            try { _pipe["endOffset"] = pipe.EndOffset; } catch { }
            try { _pipe["startStation"] = pipe.StartStation; } catch { }
            try { _pipe["endStation"] = pipe.EndStation; } catch { }
            try { _pipe["startStructure"] = pipe.StartStructureId.ToString(); } catch { }
            try { _pipe["endStructure"] = pipe.EndStructureId.ToString(); } catch { }

            // add start and end structure ids

            return _pipe;
        }

        // corridors
        // displaymesh: mesh representation corridor solid
        public Base CorridorToSpeckle(CivilDB.Corridor corridor)
        {
            var _corridor = new Base();

            var baselines = new List<Base>();
            using (Transaction tr = Doc.Database.TransactionManager.StartTransaction())
            {
            foreach (var baseline in corridor.Baselines)
            {
                Base convertedBaseline = new Base();

                /* this is just for construction, not relevant info
                if (baseline.IsFeatureLineBased()) // featurelines will only be created if assembly has point codes
                {
                var featureline = tr.GetObject(baseline.FeatureLineId, OpenMode.ForRead) as CivilDB.FeatureLine;
                convertedBaseline = FeatureLineToSpeckle(featureline);
                }
                else
                {
                var alignment = tr.GetObject(baseline.AlignmentId, OpenMode.ForRead) as CivilDB.Alignment;
                convertedBaseline = AlignmentToSpeckle(alignment);
                }
                */

                // get the collection of featurelines for this baseline
                var featurelines = new List<Featureline>();
                foreach (var mainFeaturelineCollection in baseline.MainBaselineFeatureLines.FeatureLineCollectionMap) // main featurelines
                foreach (var featureline in mainFeaturelineCollection)
                    featurelines.Add(GetCorridorFeatureline(featureline));
                foreach (var offsetFeaturelineCollection in baseline.OffsetBaselineFeatureLinesCol) // offset featurelines
                foreach (var featurelineCollection in offsetFeaturelineCollection.FeatureLineCollectionMap)
                    foreach (var featureline in featurelineCollection)
                    featurelines.Add(GetCorridorFeatureline(featureline, true));

                convertedBaseline[@"featurelines"] = featurelines;
                convertedBaseline["type"] = baseline.BaselineType.ToString();
                convertedBaseline.applicationId = baseline.baselineGUID.ToString();
                try { convertedBaseline["stations"] = baseline.SortedStations(); } catch { }

                baselines.Add(convertedBaseline);
            }

            tr.Commit();
            }
      
            _corridor["@baselines"] = baselines;
            _corridor["name"] = (corridor.DisplayName != null) ? corridor.DisplayName : "";
            _corridor["description"] = (corridor.Description != null) ? corridor.Description : "";
            _corridor["units"] = ModelUnits;

            return _corridor;
        }

        private Featureline GetCorridorFeatureline(CivilDB.CorridorFeatureLine featureline = null, bool isOffset = false)
        {
            // construct the 3d polyline
            var collection = new Acad.Point3dCollection();
            foreach (var point in featureline.FeatureLinePoints)
            collection.Add(point.XYZ);
            var polyline = new Polyline3d(Poly3dType.SimplePoly, collection, false);

            // create featureline
            var _featureline = new Featureline();
            _featureline.baseCurve = PolylineToSpeckle(polyline);
            _featureline.name = featureline.CodeName;
            _featureline.units = ModelUnits;
            _featureline["isOffset"] = isOffset;

            return _featureline;
        }
    }
}
#endif