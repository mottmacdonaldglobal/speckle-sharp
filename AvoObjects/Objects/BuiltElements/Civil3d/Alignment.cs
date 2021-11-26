using Objects.Geometry;
using Objects.Primitive;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objects.BuiltElements
{
    public class Alignment : Base
    {
        public ICurve baseCurve { get; set; }

        public string name { get; set; }

        public double startStation { get; set; }

        public double endStation { get; set; }

        /// <summary>
        /// Station equation list contains doubles indicating raw station back, station back, and station ahead for each station equation
        /// </summary>
        public List<double> stationEquations { get; set; }

        /// <summary>
        /// Station equation direction for the corresponding station equation should be true for increasing or false for decreasing
        /// </summary>
        public List<bool> stationEquationDirections { get; set; }

        public string units { get; set; }

        public Alignment() { }

        public float Length { get; set; }
        public List<Profile> Profiles { get; set; }

        public Interval ChainageDomain { get; set; }

        public double Tolerance { get; set; }

        public Plane ReferencePlane { get; set; }

        public List<Entity> Entities { get; set; }

        public AlignmentType Type { get; set; }

        public List<SECurve> SuperElevationCurves { get; set; }

        public class SECurve
        {
            Interval Domain { get; set; }
        }

        public enum AlignmentType
        {
            Centerline,
            Offset,
            CurbReturn,
            Utility,
            Rail,
        }

        public class AlignmentEntity : Entity
        {
            Interval Domain { get; set;  }
            List<AlignmentSubEntity> subEntities { get; set;  }
        }

        #region AlignmentEntity Types

        public class AlignmentArc: AlignmentEntity{}

        public class AlignmentLine : AlignmentEntity { }
        public class AlignmentSpiral : AlignmentEntity { }
        public class AlignmentMulitpleSegments : AlignmentEntity { }
        public class AlignmentSCS : AlignmentEntity { }
        public class AlignmentSCSSC : AlignmentEntity { }
        public class AlignmentSCSSCS : AlignmentEntity { }
        public class AlignmentSSCSS : AlignmentEntity { }
        public class AlignmentSTS : AlignmentEntity { }

        #endregion

        public class AlignmentSubEntity
        {
            Interval Domain { get; set; }
        }

        #region AlignmentSubEntity Types
        public class AlignmentSubEntityArc : AlignmentSubEntity { }
        public class AlignmentSubEntityLine : AlignmentSubEntity { }
        public class AlignmentSubEntitySpiral : AlignmentSubEntity { }
        #endregion



    }
}
