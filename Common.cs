using System;
using System.Collections.Generic;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;

namespace DateBetweenLocator
{
	/// <summary>
	/// Summary description for Common.
	/// </summary>
	public class Common
	{

		private static string _featureLayerClassID = "{40A9E885-5533-11D0-98BE-00805F7CED21}";

        #region Constructors / Destructors

        public Common()
		{

		}
        public static Dictionary<string, IFeatureLayer> _layerNamesToFeatureLayer = new Dictionary<string, IFeatureLayer>();
        #endregion

        public static  IEnumLayer GetFeatureLayers(IMap map)
		{
			ESRI.ArcGIS.esriSystem.UID filterUID = new ESRI.ArcGIS.esriSystem.UIDClass();
			filterUID.Value = _featureLayerClassID;

			return map.get_Layers(filterUID, true);
		}

		public static IFeatureClass GetFeatureClass(ILayer layer)
		{
			if (layer == null || !layer.Valid) return null;

			IFeatureLayer featLyr = layer as IFeatureLayer;
			if (featLyr == null) return null;

			return featLyr.FeatureClass;
		}
	}
}
