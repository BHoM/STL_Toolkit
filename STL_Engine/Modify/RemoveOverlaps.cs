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

using BH.Engine.Geometry;
using BH.Engine.Environment;

using BH.oM.Reflection.Attributes;
using System.ComponentModel;

using BH.oM.STL;

namespace BH.Engine.STL
{
    public static partial class Modify
    {
        [Description("Resolve overlapping geometry in geometry groups by triangulating the geometry")]
        [Input("geometryGroups", "A collection of Geometry Groups which contain geometry that may overlap. These will be checked to ensure none of the geometry overlaps each other")]
        [Output("geometryGroups", "The modified collection of Geometry Groups with geometry triangulated")]
        public static List<GeometryGroup> RemoveOverlaps(List<GeometryGroup> geometryGroups)
        {
            List<Polyline> potentialOverlappingLines = geometryGroups.SelectMany(a => a.Geometry.SelectMany(b => b.IToPolyline())).ToList();
            potentialOverlappingLines = potentialOverlappingLines.Where(x => x != null).ToList();
            List<GeometryGroup> newSTLObjects = new List<GeometryGroup>();

            for(int x = 0; x < geometryGroups.Count; x++)
            {
                List<Polyline> polylines = geometryGroups[x].Geometry.Select(a => a as Polyline).ToList();

                List<IGeometry> replacementPolylines = new List<IGeometry>();
                foreach(Polyline p in polylines)
                {
                    List<Polyline> overlappingLines = potentialOverlappingLines.Where(a => p.PartiallyContains(a.ControlPoints)).ToList(); //To simplify the triangulation calculation by removing unnecessary polylines from the calculations
                    foreach (Polyline p1 in BH.Engine.Geometry.Triangulation.Compute.DelaunayTriangulation(p, overlappingLines.Where(a => a != p).ToList()))
                        replacementPolylines.Add(p1);
                }

                GeometryGroup replacement = new GeometryGroup
                {
                    Name = geometryGroups[x].Name,
                    FileName = geometryGroups[x].FileName,
                    Geometry = replacementPolylines,
                };

                newSTLObjects.Add(replacement);
            }

            return newSTLObjects;
        }
    }
}
