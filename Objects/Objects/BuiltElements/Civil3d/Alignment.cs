﻿using Objects.Geometry;
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

        public Polyline displayValue { get; set; }

        public Alignment() { }

        public double length { get; set; }
        public IEnumerable<Profile> profiles { get; set; }

        public IEnumerable<AlignmentEntity> entities { get; set; }

        // public AlignmentType Type { get; set; }

        //public List<SECurve> SuperElevationCurves { get; set; }

        //public class SECurve
        //{
        //    Interval Domain { get; set; }
        //}

        //public enum AlignmentType
        //{
        //    Centerline,
        //    Offset,
        //    CurbReturn,
        //    Utility,
        //    Rail,
        //}

        public class AlignmentEntity : Entity,ICurve,IHasBoundingBox
        {
            public Interval domain { get; set;  }
            public IEnumerable<AlignmentSubEntity> subEntities { get; set;  }
            double ICurve.length { get ; set ; }

            Box IHasBoundingBox.bbox { get; }

            public AlignmentEntity() { }
        }

        #region AlignmentEntity Types
        public class AlignmentLine : AlignmentEntity { }
        public class AlignmentArc: AlignmentEntity{}
        public class AlignmentSpiral : AlignmentEntity { }
        public class AlignmentSCS : AlignmentEntity { }
        public class AlignmentSLS : AlignmentEntity { }
        public class AlignmentSL : AlignmentEntity { }
        public class AlignmentLS : AlignmentEntity { }
        public class AlignmentSC : AlignmentEntity { }
        public class AlignmentCS : AlignmentEntity { }
        public class AlignmentSSCSS : AlignmentEntity { }
        public class AlignmentSCSCS : AlignmentEntity { }
        public class AlignmentSCSSCS : AlignmentEntity { }
        public class AlignmentSS : AlignmentEntity { }
        public class AlignmentSSC : AlignmentEntity { }
        public class AlignmentCSS : AlignmentEntity { }
        public class AlignmentMultipleSegments : AlignmentEntity { }
        
        public class AlignmentCLC : AlignmentEntity { }
        public class AlignmentCRC : AlignmentEntity { }
        
        public class AlignmentCCRC : AlignmentEntity { }

        #endregion

        public class AlignmentSubEntity : Base, ICurve, IHasBoundingBox

        {
            public Interval domain { get; set; }

            public Box bbox { get; set; }

            public double length { get ; set ; }
        }
    }
}