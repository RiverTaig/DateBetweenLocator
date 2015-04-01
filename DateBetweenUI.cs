//This comment was added to the working branch
using System;
using System.Collections;
using System.Collections.Generic;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Globalization;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
//using ESRI.ArcGIS.ArcMapUI;
using Miner.ComCategories;
using Miner.Framework.Search;
using Miner.Interop;

namespace DateBetweenLocator
{
    [ComVisible(true)]
    [Guid("5f24274e-9a55-4adc-b462-4dd6170c2fb6")]
    [ProgId("SE.DateBetweenLocator")]
    [ComponentCategory(ComCategory.MMLocatorSearchStrategyUI)]
    public partial class DateBetweenUI : System.Windows.Forms.UserControl, IMMSearchStrategyUI//, IMMResultsProcessor
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Private Fields

        private IMap _map;
        private IActiveView _activeView;
        private IScreenDisplay _screenDisplay;
        private IObjectClass _classFilter = null;
        private string _caption = "SE Between Date Search";
        private int _priority = 0;


        #endregion



        #region Constructor / Destructor

        public DateBetweenUI()
        {
            try
            {
                // This call is required by the Windows.Forms Form Designer.
                InitializeComponent();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failure in Ctor: " + ex.ToString());
                _log.Error("ERROR - ", ex);
            }
        }

        #endregion



        #region IMMSearchStrategyUI Members

        public string Caption
        {
            get
            {
                return _caption;
            }
        }

        public int Priority
        {
            get
            {
                return _priority;
            }
        }

        public void InitializeStrategyUI(ESRI.ArcGIS.Carto.IMap pMap, ESRI.ArcGIS.Geodatabase.IObjectClass pClassFilter)
        {
            _map = pMap;
            _activeView = pMap as IActiveView;
            _screenDisplay = _activeView.ScreenDisplay;
            _classFilter = pClassFilter;
            PopulateDropdowns();
        }

        public IMMSearchStrategy SearchStrategy
        {
            get
            {
                IMMSearchStrategy strategy = new DateBetweenSearch() as IMMSearchStrategy;
                if (null == strategy)
                {
                    throw new Exception("Failed to create new DateBetweenSearch");
                }
                return strategy;
            }
        }

        public IMMSearchConfiguration GetSearchConfiguration(mmSearchOptionFlags optionFlags)
        {
            return GetUserSettings(optionFlags);
        }

        public void Shutdown()
        {
            //do nothing
        }

        public string COMProgID
        {
            get
            {
                return null;
            }
        }

        public IMMResultsProcessor ResultsProcessor
        {
            get
            {
                return null;// this;
            }
        }

        public void Reset()
        {
            //			txtXCoord.Text = "";
            //			txtYCoord.Text = "";
            //			txtDistance.Text = "";
        }

        public void Deactivated()
        {

        }

        #endregion

        #region Private Methods

     
        
        private void PopulateDropdowns()
        {
            try
            {
                IMMArcGISRuntimeEnvironment rte = new Miner.Framework.ArcGISRuntimeEnvironment();
                IMap map = rte.FocusMap;
                Common._layerNamesToFeatureLayer = new Dictionary<string,IFeatureLayer>();
                this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
                //Type t = Type.GetTypeFromCLSID(typeof(AppRefClass).GUID);
                //System.Object obj = Activator.CreateInstance(t);
                //IApplication app = obj as IApplication;
                //IMxDocument mxDoc = app.Document as IMxDocument;
                //IMap map = mxDoc.FocusMap;
                IEnumLayer enLyr = map.get_Layers(null, true);
                enLyr.Reset();
                cboFeatureClass.Items.Clear();
                ILayer lyr = enLyr.Next();
                while (lyr != null)
                {
                    if (lyr is IFeatureLayer)
                    {
                        cboFeatureClass.Items.Add(lyr.Name);
                        Common._layerNamesToFeatureLayer.Add(lyr.Name, lyr as IFeatureLayer);
                    }
                    lyr = enLyr.Next();
                }
            }
            catch (Exception ex)
            {
                _log.Error("ERROR - ", ex);
            }
            finally
            {
                this.Cursor = System.Windows.Forms.Cursors.Arrow;
            }
        }

        private IMMSearchConfiguration GetUserSettings(mmSearchOptionFlags optionFlags)
        {
            IPropertySet properties = new PropertySetClass();
            properties.SetProperty("Map", _map);
            properties.SetProperty("LayerName", cboFeatureClass.Text);
            properties.SetProperty("DateField", cboDateField.Text);
            properties.SetProperty("DateStart", dtStart.Value);
            properties.SetProperty("DateEnd", dtEnd.Value);
            properties.SetProperty("OptionFlags",optionFlags);
            IMMSearchConfiguration config = new SearchConfiguration();
            if (null == config) return null;

            config.SearchParameters = properties;

            return config;
        }



        #endregion

        private void cboFeatureClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            cboDateField.Items.Clear();
            IFeatureLayer fl = Common._layerNamesToFeatureLayer[cboFeatureClass.Text] as IFeatureLayer;
            IFields fields = fl.FeatureClass.Fields;
            for (int i = 0; i < fields.FieldCount; i++)
            {
                if (fields.get_Field(i).Type == esriFieldType.esriFieldTypeDate)
                {
                    cboDateField.Items.Add(fields.get_Field(i).Name);
                }
            }
        }


        private void DateBetweenUI_Load(object sender, EventArgs e)
        {

        }

        private void InitializeComponent()
        {
            this.cboFeatureClass = new System.Windows.Forms.ComboBox();
            this.cboDateField = new System.Windows.Forms.ComboBox();
            this.dtStart = new System.Windows.Forms.DateTimePicker();
            this.dtEnd = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cboFeatureClass
            // 
            this.cboFeatureClass.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFeatureClass.FormattingEnabled = true;
            this.cboFeatureClass.Location = new System.Drawing.Point(92, 25);
            this.cboFeatureClass.Name = "cboFeatureClass";
            this.cboFeatureClass.Size = new System.Drawing.Size(149, 24);
            this.cboFeatureClass.TabIndex = 0;
            this.cboFeatureClass.SelectedIndexChanged += new System.EventHandler(this.cboFeatureClass_SelectedIndexChanged);
            // 
            // cboDateField
            // 
            this.cboDateField.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDateField.FormattingEnabled = true;
            this.cboDateField.Location = new System.Drawing.Point(92, 64);
            this.cboDateField.Name = "cboDateField";
            this.cboDateField.Size = new System.Drawing.Size(149, 24);
            this.cboDateField.TabIndex = 0;
            // 
            // dtStart
            // 
            this.dtStart.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtStart.Location = new System.Drawing.Point(92, 101);
            this.dtStart.Name = "dtStart";
            this.dtStart.Size = new System.Drawing.Size(149, 22);
            this.dtStart.TabIndex = 1;
            // 
            // dtEnd
            // 
            this.dtEnd.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtEnd.Location = new System.Drawing.Point(92, 140);
            this.dtEnd.Name = "dtEnd";
            this.dtEnd.Size = new System.Drawing.Size(149, 22);
            this.dtEnd.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "Layer";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 67);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "Field";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 106);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(72, 17);
            this.label3.TabIndex = 2;
            this.label3.Text = "Start Date";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 145);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(67, 17);
            this.label4.TabIndex = 2;
            this.label4.Text = "End Date";
            // 
            // DateBetweenUI
            // 
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dtEnd);
            this.Controls.Add(this.dtStart);
            this.Controls.Add(this.cboDateField);
            this.Controls.Add(this.cboFeatureClass);
            this.Name = "DateBetweenUI";
            this.Size = new System.Drawing.Size(264, 190);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void label1_Click(object sender, EventArgs e)
        {
            PopulateDropdowns();

        }

  
   

    }
}
