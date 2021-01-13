/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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

using BH.oM.Base;
using BH.oM.Geometry;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.oM.Adapters.STL
{
    [Description("This object can be used to group geometry to push to a specific file. Useful if pushing groups of geometry related to specific representations, for example, a group of panels to one file, and a group of bars to another.")]
    public class GeometryGroup : BHoMObject
    {
        /***************************************************/
        /**** Properties                                ****/
        /***************************************************/

        [Description("The container of Geometry which is grouped to this file. This can be any BHoM Geometry and can be mixed types.")]
        public virtual List<IGeometry> Geometry { get; set; } = new List<IGeometry>();

        [Description("The name of the file which this geometry should be stored (pushed) to. Do not include a directory on this file name, the save directory should be specified on the STLSettings object for the Adapter. Do not provide a file extension either, this will be handled by the Adapter.")]
        public virtual string FileName { get; set; } = "";

        /***************************************************/
    }
}


