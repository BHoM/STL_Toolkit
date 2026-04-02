/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.Geometry;
using BH.Engine.Geometry;

namespace BH.Engine.Geometry
{
    public static partial class Convert
    {
        [Description("Dispatches to the appropriate ToPolyline method for the given geometry type.")]
        [Input("geom", "The geometry to convert to a list of polylines.")]
        [Output("polylines", "The geometry converted to a list of polylines.")]
        public static List<Polyline> IToPolyline(this IGeometry geom)
        {
            return ToPolyline(geom as dynamic);
        }

        [Description("Wraps a Polyline in a list for interface consistency.")]
        [Input("pline", "The polyline to wrap in a list.")]
        [Output("polylines", "A list containing the input polyline.")]
        public static List<Polyline> ToPolyline(this Polyline pline)
        {
            return new List<Polyline> { pline };
        }

        //Solids
        [Description("Converts a BoundaryRepresentation solid to a list of polylines by collapsing the external edges of each surface.")]
        [Input("brep", "The boundary representation solid to convert.")]
        [Output("polylines", "A list of polylines representing the external edges of the solid's surfaces.")]
        public static List<Polyline> ToPolyline(this BoundaryRepresentation brep)
        {
            return brep.Surfaces.SelectMany(y => y.IExternalEdges().Select(z => z.ICollapseToPolyline(BH.oM.Geometry.Tolerance.Angle))).ToList();
        }

        //Surfaces
        [Description("Converts an Extrusion to a list of polylines by collapsing its external edges.")]
        [Input("extrusion", "The extrusion to convert.")]
        [Output("polylines", "A list of polylines representing the external edges of the extrusion.")]
        public static List<Polyline> ToPolyline(this Extrusion extrusion)
        {
            return extrusion.IExternalEdges().Select(x => x.ICollapseToPolyline(BH.oM.Geometry.Tolerance.Angle)).ToList();
        }

        [Description("Converts an ISurface to a list of polylines by collapsing its edges.")]
        [Input("isurface", "The surface to convert.")]
        [Output("polylines", "A list of polylines representing the edges of the surface.")]
        public static List<Polyline> ToPolyline(this ISurface isurface)
        {
            return isurface.Edges().Select(x => x.ICollapseToPolyline(BH.oM.Geometry.Tolerance.Angle)).ToList();
        }

        [Description("Converts a Loft to a list of polylines by collapsing its external edges.")]
        [Input("loft", "The loft to convert.")]
        [Output("polylines", "A list of polylines representing the external edges of the loft.")]
        public static List<Polyline> ToPolyline(this Loft loft)
        {
            return loft.IExternalEdges().Select(x => x.ICollapseToPolyline(BH.oM.Geometry.Tolerance.Angle)).ToList();
        }

        [Description("Converts a NurbsSurface to a list of polylines by collapsing its external edges.")]
        [Input("nsurface", "The NURBS surface to convert.")]
        [Output("polylines", "A list of polylines representing the external edges of the NURBS surface.")]
        public static List<Polyline> ToPolyline(this NurbsSurface nsurface)
        {
            return nsurface.IExternalEdges().Select(x => x.ICollapseToPolyline(BH.oM.Geometry.Tolerance.Angle)).ToList();
        }

        [Description("Converts a PlanarSurface to a list of polylines by collapsing its external edges.")]
        [Input("surface", "The planar surface to convert.")]
        [Output("polylines", "A list of polylines representing the external edges of the planar surface.")]
        public static List<Polyline> ToPolyline(this PlanarSurface surface)
        {
            return surface.IExternalEdges().Select(x => x.ICollapseToPolyline(BH.oM.Geometry.Tolerance.Angle)).ToList();
        }

        [Description("Converts a PolySurface to a list of polylines by collapsing its external edges.")]
        [Input("psurface", "The poly surface to convert.")]
        [Output("polylines", "A list of polylines representing the external edges of the poly surface.")]
        public static List<Polyline> ToPolyline(this PolySurface psurface)
        {
            return psurface.IExternalEdges().Select(x => x.ICollapseToPolyline(BH.oM.Geometry.Tolerance.Angle)).ToList();
        }

        [Description("Converts a Mesh to a list of polylines by creating a closed polyline for each face.")]
        [Input("mesh", "The mesh to convert to polylines.")]
        [Output("polylines", "A list of closed polylines, one per mesh face, with vertices corresponding to the face's control points.")]
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
                controlPoints.Add(controlPoints.First());
                Polyline polyline = new Polyline() { ControlPoints = controlPoints };
                polylines.Add(polyline);
            }

            return polylines;
        }

        [Description("Fallback conversion for unsupported geometry types. Records an error and returns an empty list.")]
        [Input("obj", "The object to convert, which is not a supported geometry type.")]
        [Output("polylines", "An empty list. An error is recorded indicating the geometry type is not supported by STL Toolkit.")]
        public static List<Polyline> ToPolyline(this IObject obj)
        {
            BH.Engine.Base.Compute.RecordError("This geometry type is not currently supported by STL Toolkit");
            return new List<Polyline>();
        }
    }
}
