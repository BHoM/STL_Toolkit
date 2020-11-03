/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Base;
using BH.oM.Geometry;
using BH.Engine.Geometry;

namespace BH.Engine.Geometry
{
    public static partial class Convert
    {
        public static List<Polyline> IToPolyline(this IGeometry geom)
        {
            return ToPolyline(geom as dynamic);
        }

        public static List<Polyline> ToPolyline(this Polyline pline)
        {
            return new List<Polyline> { pline };
        }

        //Solids
        public static List<Polyline> ToPolyline(this BoundaryRepresentation brep)
        {
            return brep.Surfaces.SelectMany(y => y.IExternalEdges().Select(z => z.ICollapseToPolyline(BH.oM.Geometry.Tolerance.Angle))).ToList();
        }

        //Surfaces
        public static List<Polyline> ToPolyline(this Extrusion extrusion)
        {
            return extrusion.IExternalEdges().Select(x => x.ICollapseToPolyline(BH.oM.Geometry.Tolerance.Angle)).ToList();
        }

        public static List<Polyline> ToPolyline(this ISurface isurface)
        {
            return isurface.Edges().Select(x => x.ICollapseToPolyline(BH.oM.Geometry.Tolerance.Angle)).ToList();
        }

        public static List<Polyline> ToPolyline(this Loft loft)
        {
            return loft.IExternalEdges().Select(x => x.ICollapseToPolyline(BH.oM.Geometry.Tolerance.Angle)).ToList();
        }

        public static List<Polyline> ToPolyline(this NurbsSurface nsurface)
        {
            return nsurface.IExternalEdges().Select(x => x.ICollapseToPolyline(BH.oM.Geometry.Tolerance.Angle)).ToList();
        }

        public static List<Polyline> ToPolyline(this PlanarSurface surface)
        {
            return surface.IExternalEdges().Select(x => x.ICollapseToPolyline(BH.oM.Geometry.Tolerance.Angle)).ToList();
        }

        public static List<Polyline> ToPolyline(this PolySurface psurface)
        {
            return psurface.IExternalEdges().Select(x => x.ICollapseToPolyline(BH.oM.Geometry.Tolerance.Angle)).ToList();
        }

        public static List<Polyline> ToPolyline(this Mesh mesh)
        {
            List<Polyline> polylines = new List<Polyline>();
            foreach (Face face in mesh.Faces)
            {
                List<Point> controlPoints = new List<Point>();
                controlPoints.Add(mesh.Vertices[face.A]);
                controlPoints.Add(mesh.Vertices[face.B]);
                controlPoints.Add(mesh.Vertices[face.C]);
                if (face.D != -1)
                {
                    controlPoints.Add(mesh.Vertices[face.D]);
                }
                Polyline polyline = new Polyline() { ControlPoints = controlPoints };
                polylines.Add(polyline);
            }

            return polylines;
        }

        public static List<Polyline> ToPolyline(this IObject obj)
        {
            BH.Engine.Reflection.Compute.RecordError("This geometry type is not currently supported by STL Toolkit");
            return new List<Polyline>();
        }
    }
}
