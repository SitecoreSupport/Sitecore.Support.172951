using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.ContentTesting.ViewModel;


namespace Sitecore.Support.ContentTesting.ViewModel
{
    public class ExecutedTestViewModel : BaseTestViewModel
    {
        public int Days
        {
            get;
            set;
        }

        public double TestScore
        {
            get;
            set;
        }

        public double Effect
        {
            get;
            set;
        }

        public string EffectCss
        {
            get;
            set;
        }

        #region AddedCode

        public string ContextSite
        {
            get;
            set;
        }
        #endregion
    }

} 