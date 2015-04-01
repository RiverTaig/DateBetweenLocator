using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;

using Miner.Framework;
using Miner.FrameworkUI;
using Miner.Geodatabase;
using Miner.Interop;

namespace DateBetweenLocator
{
    [ComVisible(true)]
    [Guid("a0b11500-5e8f-4b8c-b859-77cdddb7ff74")]
    [ProgId("SE.DateBetweenSearch")]
    public class DateBetweenSearch : IMMSearchStrategy
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Private Fields

        private IMMRowSearchResults _results;
		private IMMSearchControl _searchControl;
		private IMap _map;

        #endregion

        #region Constructors / Destructors

        public DateBetweenSearch()
		{
		}

        #endregion

        #region IMMSearchStrategy Members

        public IMMSearchResults Find(IMMSearchConfiguration pSearchConfig, IMMSearchControl searchControl)
		{
			try
			{
				_searchControl = searchControl;
				ExtractSettings(pSearchConfig);
				ExecuteFind();

				if (Stopped)
				{
					return null;
				}
				else
				{
					return _results as IMMSearchResults; 
				}
			}
			catch(Exception ex)
			{
				_log.Error("ERROR - ", ex);
				return null;
			}
		}

		#endregion

		#region Private Methods
        string _layerName = "";
        string _dateField = "";
        DateTime _dtStart;
        DateTime _dtEnd;
        mmSearchOptionFlags _optionFlags;
		private void ExtractSettings(IMMSearchConfiguration config)
		{
			if (null == config) throw new InvalidOperationException("Null IMMSearchConfiguration; find cannot proceed.");
			
			IPropertySet properties = config.SearchParameters as IPropertySet;
			
			if (null == properties) throw new InvalidOperationException("No PropertySet in IMMSearchConfiguration; find cannot proceed.");
			
			_map = (IMap) properties.GetProperty("Map");
            _layerName = properties.GetProperty("LayerName").ToString();
            _dateField = properties.GetProperty("DateField").ToString();
            _dtStart = (DateTime)properties.GetProperty("DateStart");
            _dtEnd = (DateTime)properties.GetProperty("DateEnd");
            _optionFlags = (mmSearchOptionFlags)properties.GetProperty("OptionFlags");
			
		}

		private bool Stopped
		{
			get
			{
				if (_searchControl != null)
				{
					return _searchControl.Stopped;
				}
				else
				{
					return false;
				}

			}
		}

        private void ExecuteFind()
		{
			try
			{
                bool ChangeLayerSelect =  (_optionFlags & mmSearchOptionFlags.mmSOFAutoSelect) > 0;
                IFeatureLayer fl = Common._layerNamesToFeatureLayer[_layerName];
                IFeatureSelection fs = fl as IFeatureSelection;
                ISelectionSet ss = fs.SelectionSet;
                ISelectionEnvironment selEnv = new SelectionEnvironmentClass();
                var comboMethod = selEnv.CombinationMethod;

                IFeatureClass fc = Common._layerNamesToFeatureLayer[_layerName].FeatureClass;
                IQueryFilter qf = new QueryFilterClass();
                string dtStart = _dtStart.ToShortDateString();
                string dtEnd = _dtEnd.ToShortDateString();
                qf.WhereClause = _dateField + " >= '" + dtStart + "' AND " +
                    _dateField + " <= '" + dtEnd + "'";
                //MessageBox.Show("SQL  Server where clause = " + qf.WhereClause);
                IWorkspace ws = (fl.FeatureClass as IDataset).Workspace;
                if (ws.Type == esriWorkspaceType.esriLocalDatabaseWorkspace)
                {
                    //SELECT [DATECREATED] FROM PriOHElectricLineSegment WHERE [DATECREATED] between #5/11/2001# and #5/30/2001#;
                    qf.WhereClause = _dateField + " >= date '" + dtStart + "' AND " +
                    _dateField + " <= date '" + dtEnd + "'";
                    //MessageBox.Show("Access where clause = " + qf.WhereClause);
                }
                ICursor cur = null;
                ISelectionSet ss2 = fc.Select(qf, esriSelectionType.esriSelectionTypeHybrid, esriSelectionOption.esriSelectionOptionNormal, ws);
                if ((ChangeLayerSelect == false) || comboMethod == esriSelectionResultEnum.esriSelectionResultNew)
                {
                    ss2.Search(null, false, out cur);
                    if (ChangeLayerSelect)
                    {
                        fs.SelectionSet = ss2;
                    }
                }
                if ((ChangeLayerSelect)  && comboMethod == esriSelectionResultEnum.esriSelectionResultSubtract)
                {
                    ISelectionSet ss3 = null;
                    ss.Combine(ss2, esriSetOperation.esriSetDifference, out ss3);
                    int ssCount = ss.Count;
                    int ss2Count = ss2.Count;
                    int ss3Count = ss3.Count;
                    ss3.Search(null, false, out cur);
                    fs.SelectionSet = ss3;

                }
                if ((ChangeLayerSelect) && comboMethod == esriSelectionResultEnum.esriSelectionResultAnd)
                {
                    
                    ISelectionSet ss3 = null;
                    ss2.Combine(ss, esriSetOperation.esriSetIntersection,out ss3);
                    int ssCount = ss.Count;
                    int ss2Count = ss2.Count;
                    int ss3Count = ss3.Count;
                    ss3.Search(null, false, out cur);
                    fs.SelectionSet = ss3;

                }
                if ((ChangeLayerSelect) && comboMethod == esriSelectionResultEnum.esriSelectionResultAdd)
                {
                    ISelectionSet ss3 = null;
                    ss2.Combine(ss, esriSetOperation.esriSetUnion, out ss3);
                    ss3.Search(null, false, out cur);
                    fs.SelectionSet = ss3;
                }

                //IFeatureCursor cursor = fc.Search(qf, false); ;
				AddCursorToResults(cur as ICursor);
                
			}
			catch(Exception ex)
			{
                MessageBox.Show("Error " + ex.ToString());
				_log.Error("ERROR - ", ex);
			}


		}

		private void AddCursorToResults(ICursor cursor)
		{
			// simply take a cursor, and add it to the 
			// internal _results object.
			
			if (cursor == null) return;

			if (_results == null)
			{
				_results = new Miner.Framework.Search.RowSearchResults();
			}
			_results.AddCursor(cursor, false);
		}

		#endregion
    }
}
