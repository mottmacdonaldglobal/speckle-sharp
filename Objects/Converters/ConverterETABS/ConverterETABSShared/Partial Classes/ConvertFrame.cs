﻿using System;
using System.Collections.Generic;
using Objects.Geometry;
using Objects.Structural.Geometry;
using ETABSv1;

namespace Objects.Converter.ETABS
{
    public partial class ConverterETABS
    {
        public void FrameToNative()
        {
            throw new NotImplementedException();
        }
        public Element1D FrameToSpeckle(string name)
        {
            var speckleStructFrame = new Element1D();
            speckleStructFrame.name = name;
            string pointI, pointJ;
            pointI = pointJ = null;
            int v = Model.FrameObj.GetPoints(name,ref pointI,ref pointJ);
            var pointINode = PointToSpeckle(pointI);
            var pointJNode = PointToSpeckle(pointJ);
            var speckleLine = new Line(pointINode.basePoint, pointJNode.basePoint);
            speckleStructFrame.baseLine = speckleLine;
            eFrameDesignOrientation frameDesignOrientation = eFrameDesignOrientation.Null;
            Model.FrameObj.GetDesignOrientation(name, ref frameDesignOrientation);
            switch (frameDesignOrientation)
            {
                case eFrameDesignOrientation.Column:
                    {
                        speckleStructFrame.type = ElementType1D.Column;
                        break;
                    }
                case eFrameDesignOrientation.Beam:
                    {
                        speckleStructFrame.type = ElementType1D.Beam;
                        break;
                    }
                case eFrameDesignOrientation.Brace:
                    {
                        speckleStructFrame.type = ElementType1D.Brace;
                        break;
                    }
                case eFrameDesignOrientation.Null:
                    {
                        speckleStructFrame.type = ElementType1D.Null;
                        break;  
                    }
                case eFrameDesignOrientation.Other:
                    {
                        speckleStructFrame.type = ElementType1D.Other;
                        break;
                    }
            }

            string property, SAuto;
            property = SAuto = null;
            Model.FrameObj.GetSection(name, ref property, ref SAuto);
            speckleStructFrame.property = Property1DToSpeckle(property);


            return speckleStructFrame;
        }
    }
}
