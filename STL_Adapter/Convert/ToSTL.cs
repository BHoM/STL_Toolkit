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

using BH.oM.Geometry;
using BH.oM.Reflection;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;
using BH.Engine.Reflection;
using BH.oM.Environment.Elements;
using BH.Engine.Geometry;

using BH.oM.Adapters.STL;

namespace BH.Adapter.STL
{
    public static partial class Convert
    {
        [Description("Convert a BHoM Geometry Polyline into an STL string representation for STL files.")]
        [Input("polyline", "BHoM Geometry Polyline to convert.")]
        [Output("polylineString", "The STL string representation of the Polyline.")]
        public static List<string> ToSTL(this Polyline polyine, STLSettings settings)
        {
            List<string> facetString = new List<string>();

            foreach (Polyline p in BH.Engine.Geometry.Triangulation.Compute.DelaunayTriangulation(polyine as Polyline))
            {
                facetString.Add("  facet normal " + p.Normal().ToSTL(settings));
                facetString.Add("    outer loop");
                foreach (Point point in p.DiscontinuityPoints())
                    facetString.Add("      vertex " + point.ToSTL(settings));
                facetString.Add("    endloop");
                facetString.Add("  endfacet");
            }

            return facetString;
        }

        [Description("Convert a BHoM Geometry Point into an STL string representation for STL files.")]
        [Input("point", "BHoM Geometry Point to convert.")]
        [Output("PointString", "The STL string representation of the point.")]
        private static string ToSTL(this Point point, STLSettings settings)
        {
            if (point == null) return "0.0 0.0 0.0";

            return Math.Round(point.X, settings.DecimalPlaces).ToString() + " " + Math.Round(point.Y, settings.DecimalPlaces).ToString() + " " + Math.Round(point.Z, settings.DecimalPlaces).ToString();
        }

        [Description("Convert a BHoM Geometry Vector into an STL string representation for STL files.")]
        [Input("vector", "BHoM Geometry Vector to convert.")]
        [Output("VectorString", "The STL string representation of the Vector.")]
        private static string ToSTL(this Vector vector, STLSettings settings)
        {
            if (vector == null) return "0.0 0.0 0.0";

            return Math.Round(vector.X, settings.DecimalPlaces).ToString() + " " + Math.Round(vector.Y, settings.DecimalPlaces).ToString() + " " + Math.Round(vector.Z, settings.DecimalPlaces).ToString();
        }
    }
}