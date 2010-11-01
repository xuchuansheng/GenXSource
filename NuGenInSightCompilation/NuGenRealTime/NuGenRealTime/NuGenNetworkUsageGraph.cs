using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Genetibase.UI.NuGenMeters;

namespace NuGenRealTime
{
    [ToolboxItem(true)]
    public class NuGenNetworkUsageGraph : NuGenGraphBase
    {
        #region Declarations

        private IContainer components = null;

        #endregion

        #region Properties.Overriden

        private static PerformanceCounter GetCounter()
        {
            return new PerformanceCounter(
                        "Network Interface",
                        @"Bytes Total/sec",
                        "",
                        true);
        }

        private PerformanceCounter counter = GetCounter();

        /// <summary>
        /// Gets the type of the counter.
        /// </summary>
        /// <value></value>
        protected override PerformanceCounter CounterType
        {
            get
            {                                
                return this.counter;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NuGenPercentProcessorTimeGraph"/> class.
        /// </summary>
        public NuGenNetworkUsageGraph()
        {
            InitializeComponent();
        }

        #endregion

        #region Dispose

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null) components.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }

        #endregion
    }
}