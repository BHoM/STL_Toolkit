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

using BH.oM.Base;
using BH.oM.Data.Requests;
using BH.oM.Reflection.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

using BH.oM.Adapters.STL.Settings;

namespace BH.Adapter.STL
{
    public partial class STLAdapter : BHoMAdapter
    {

        /***************************************************/
        /**** Constructors                              ****/
        /***************************************************/

        [Description("Produces a STL Adapter to allow interoperability with STL file representation of geometry")]
        [Input("stlSettings", "STL settings which define how the push or pull operation should operate")]
        [Output("adapter", "Adapter to STL file")]
        public STLAdapter(STLSettings stlSettings)
        {
            if (stlSettings == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Please set the STL Settings correctly to enable the STL Adapter to work correctly.");
                return;
            }

            m_AdapterSettings.DefaultPushType = oM.Adapter.PushType.CreateOnly;
            m_stlSettings = stlSettings;
        }

        /***************************************************/
        /**** Private Fields                            ****/
        /***************************************************/

        private STLSettings m_stlSettings { get; set; } = null;
    }
}
