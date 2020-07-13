using BH.oM.Base;
using BH.oM.Geometry;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.oM.STL
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

