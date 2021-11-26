using Objects.Geometry;
using Objects.Primitive;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objects.BuiltElements
{
    public class Profile : Base
    {
        public ICurve baseCurve { get; set; }

        public string name { get; set; }

        public double startStation { get; set; }

        public double endStation { get; set; }

        public string units { get; set; }

        public Profile() { }
        public Interval ChainageDomain { get; set; }
        public Interval ElevationDomain { get; set; }

        public float Offset { get; set; }


        public double Tolerance { get; set; }

        public Alignment Alignment { get; set; }

        public ProfileType Type { get; set; }

        public enum ProfileType 
        { 
            EG,
            FG,
            SuperImposed,
            File,
            CorridorFeature,
            OffsetProfile,
            CurbReturnProfile,
        }
        
        public List<ProfileEntity> Entities { get; set; }

        public class ProfileEntity : Entity 
        { 
            Interval Domain { get; set; }
            Interval ElevationDomain { get; set; }
            float length { get; set; }

        }

        #region ProfileEntity Types
        public class ProfileCircular : ProfileEntity { }
        public class ProfileParabolaAssymetric : ProfileEntity { }
        public class ProfileParabolaSymmetric : ProfileEntity { }
        public class ProfileTangent : ProfileEntity { }
        #endregion


    }
}
