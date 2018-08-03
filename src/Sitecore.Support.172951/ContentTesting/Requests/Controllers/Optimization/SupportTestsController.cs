using System;
using Sitecore.ContentTesting.Extensions;
using Sitecore.ContentTesting.Helpers;
using Sitecore.ContentTesting.Reports;
using Sitecore;
using Sitecore.ContentTesting;
using Sitecore.ContentTesting.ContentSearch.Models;
using Sitecore.ContentTesting.Data;
using Sitecore.ContentTesting.Extensions;
using Sitecore.ContentTesting.Helpers;
using Sitecore.ContentTesting.Intelligence;
using Sitecore.ContentTesting.Model;
using Sitecore.ContentTesting.Model.Data.Items;
using Sitecore.ContentTesting.Models;
using Sitecore.ContentTesting.Reports;
using Sitecore.ContentTesting.Requests.Controllers;
using Sitecore.ContentTesting.ViewModel;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Web.Http.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using Sitecore.Links;
using Sitecore.Sites;
using Sitecore.Configuration;

namespace Sitecore.Support.ContentTesting.Requests.Controllers.Optimization
{

    using SuggestedTestViewModel = ViewModel.SuggestedTestViewModel;
    using ExecutedTestViewModel = ViewModel.ExecutedTestViewModel;

    [ValidateHttpAntiForgeryToken, Authorize]
    public class SupportTestsController : ContentTestingControllerBase
    {
        private const string DateFormat = "dd-MMM-yyyy";

        private readonly IContentTestStore _contentTestStore;

        public SupportTestsController() : this(ContentTestingFactory.Instance.ContentTestStore)
        {
        }

        public SupportTestsController(IContentTestStore contentTestStore)
        {
            this._contentTestStore = contentTestStore;
        }
        [HttpGet]
        public JsonResult<TestListViewModel> GetSuggestedTests(int? page = null, int? pageSize = null, string hostItemId = null, string searchText = null)
        {
            page = new int?(page ?? 1);
            pageSize = new int?(pageSize ?? 20);
            DataUri hostItemDataUri = null;
            if (!string.IsNullOrEmpty(hostItemId))
            {
                hostItemDataUri = DataUriParser.Parse(hostItemId, "");
            }
            SuggestedTestSearchResultItem[] array = base.ContentTestStore.GetSuggestedTests(hostItemDataUri, searchText).ToArray<SuggestedTestSearchResultItem>();
            List<SuggestedTestViewModel> list = new List<SuggestedTestViewModel>();
            int num = (page.Value - 1) * pageSize.Value;
            while (list.Count < pageSize && num < array.Length)
            {
                SuggestedTestSearchResultItem suggestedTestSearchResultItem = array[num];
                Item item = base.Database.GetItem(suggestedTestSearchResultItem.ItemId);
                if (item != null)
                {
                    list.Add(new SuggestedTestViewModel()
                    {
                        HostPageId = item.ID.ToString(),
                        HostPageUri = item.Uri.ToDataUri(),
                        HostPageName = item.DisplayName,
                        Language = item.Language.Name,
                        Impact = suggestedTestSearchResultItem.Impact,
                        Potential = suggestedTestSearchResultItem.Potential,
                        Recommendation = suggestedTestSearchResultItem.Recommendation,
                        #region ModifiedCode
                        ContextSite = ResolveContextSite(item)
                        #endregion
                    });
                }
                num++;
            }
            return base.Json<TestListViewModel>(new TestListViewModel
            {
                Items = list,
                TotalResults = list.Count<SuggestedTestViewModel>()
            });
        }


        [HttpGet]
        public JsonResult<TestListViewModel> GetActiveTests(int? page = default(int?), int? pageSize = default(int?), string hostItemId = null, string searchText = null)
        {
            page = (page ?? 1);
            pageSize = (pageSize ?? 20);
            DataUri hostItemDataUri = null;
            if (!string.IsNullOrEmpty(hostItemId))
            {
                hostItemDataUri = DataUriParser.Parse(hostItemId, "");
            }
            TestingSearchResultItem[] array = base.ContentTestStore.GetActiveTests(hostItemDataUri, searchText, null).ToArray();
            List<ExecutedTestViewModel> list = new List<ExecutedTestViewModel>();
            Dictionary<ID, ITestConfiguration> dictionary = new Dictionary<ID, ITestConfiguration>();
            TestingSearchResultItem[] array2 = array;
            foreach (TestingSearchResultItem testingSearchResultItem in array2)
            {
                Item item = Database.GetItem(testingSearchResultItem.Uri);
                if (item != null)
                {
                    TestDefinitionItem testDefinitionItem = TestDefinitionItem.Create(item); 
                    if (testDefinitionItem != null)
                    {
                        Item item2 = (testingSearchResultItem.HostItemUri != null) ? item.Database.GetItem(testingSearchResultItem.HostItemUri) : null;
                        if (item2 != null)
                        {
                            ITestConfiguration testConfiguration = _contentTestStore.LoadTestForItem(item2, testDefinitionItem);
                            if (testConfiguration != null)
                            {
                                dictionary.Add(testConfiguration.TestDefinitionItem.ID, testConfiguration);
                                list.Add(new ExecutedTestViewModel
                                {
                                    HostPageId = item2.ID.ToString(),
                                    HostPageUri = item2.Uri.ToDataUri(),
                                    HostPageName = item2.DisplayName,
                                    DeviceId = testConfiguration.DeviceId.ToString(),
                                    DeviceName = testConfiguration.DeviceName,
                                    Language = testConfiguration.LanguageName,
                                    CreatedBy = FormattingHelper.GetFriendlyUserName(item.Security.GetOwner()),
                                    Date = DateUtil.ToServerTime(testDefinitionItem.StartDate).ToString("dd-MMM-yyyy"),
                                    ExperienceCount = testConfiguration.TestSet.GetExperienceCount(),
                                    Days = GetEstimatedDurationDays(item2, testConfiguration.TestSet.GetExperienceCount(), testDefinitionItem),
                                    ItemId = testDefinitionItem.ID.ToString(),
                                    ContentOnly = (testConfiguration.TestSet.Variables.Count == testDefinitionItem.PageLevelTestVariables.Count),
                                    TestType = testConfiguration.TestType,
                                    TestId = testConfiguration.TestDefinitionItem.ID,
                                    #region ModifiedCode
                                    ContextSite = ResolveContextSite(item)
                                    #endregion
                                });
                            }
                        }
                    }
                }
            }
            list = (from x in list
                    orderby x.Days
                    select x).Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value).ToList();
            foreach (ExecutedTestViewModel item3 in list)
            {
                if (base.Database.GetItem(item3.HostPageUri) != null)
                {
                    item3.Effect = GetWinningEffect(dictionary[item3.TestId]);
                    if (item3.Effect < 0.0)
                    {
                        item3.EffectCss = "value-decrease";
                    }
                    else if (item3.Effect == 0.0)
                    {
                        item3.EffectCss = "value-nochange";
                    }
                    else
                    {
                        item3.EffectCss = "value-increase";
                    }
                }
            }
            return Json(new TestListViewModel
            {
                Items = list,
                TotalResults = array.Count()
            });
        }

        private int GetEstimatedDurationDays(Item hostItem, int experienceCount, TestDefinitionItem testDef)
        {
            string deviceName = string.Empty;
            if (testDef.Device.TargetItem != null)
            {
                deviceName = testDef.Device.TargetItem.Name;
            }
            TestRunEstimator testRunEstimator = ContentTestingFactory.Instance.GetTestRunEstimator(testDef.Language, deviceName);
            testRunEstimator.HostItem = hostItem;
            TestRunEstimate estimate = testRunEstimator.GetEstimate(experienceCount, 0.8, testDef.TrafficAllocationPercentage, testDef.ConfidenceLevelPercentage, testDef, TestMeasurement.Undefined);
            int num = (int)Math.Ceiling((testDef.StartDate.AddDays(estimate.EstimatedDayCount.HasValue ? ((double)estimate.EstimatedDayCount.Value) : 0.0) - DateTime.UtcNow).TotalDays);
            TimeSpan timeSpan = DateTime.UtcNow - testDef.StartDate;
            if (num < 1)
            {
                return int.Parse(testDef.MaxDuration);
            }
            num = Math.Min(num, int.Parse(testDef.MaxDuration) - timeSpan.Days);
            return Math.Max(num, int.Parse(testDef.MinDuration) - timeSpan.Days);
        }

        private double GetWinningEffect(Item hostItem)
        {
            Assert.ArgumentNotNull(hostItem, "hostItem");
            ITestConfiguration testConfiguration = base.ContentTestStore.LoadTestForItem(hostItem, true);
            if (testConfiguration == null)
            {
                return 0.0;
            }
            return GetWinningEffect(testConfiguration);
        }


        private double GetWinningEffect(ITestConfiguration test)
        {
            IContentTestPerformance performanceForTest = base.PerformanceFactory.GetPerformanceForTest(test);
            if (performanceForTest.BestExperiencePerformance != null)
            {
                return performanceForTest.GetExperienceEffect(performanceForTest.BestExperiencePerformance.Combination, false);
            }
            return 0.0;
        }

        private double GetWinningValue(Item hostItem)
        {
            Assert.ArgumentNotNull(hostItem, "hostItem");
            ITestConfiguration testConfiguration = base.ContentTestStore.LoadTestForItem(hostItem, true);
            if (testConfiguration == null)
            {
                return 0.0;
            }
            return GetWinningValue(testConfiguration);
        }

        private double GetWinningValue(ITestConfiguration test)
        {
            IContentTestPerformance performanceForTest = base.PerformanceFactory.GetPerformanceForTest(test);
            if (performanceForTest.BestExperiencePerformance != null)
            {
                return performanceForTest.BestExperiencePerformance.Value;
            }
            return 0.0;
        }


        #region AddedCode
        private string ResolveContextSite(Item item)
        {
            SiteContext siteContext = null;
            siteContext = LinkManager.GetPreviewSiteContext(item);
            siteContext = (siteContext ?? Factory.GetSite(Sitecore.Configuration.Settings.Preview.DefaultSite));
            if (siteContext == null)
            {
                Log.Error("Cannot resolve site for suggested test item", this);
                return string.Empty;
            }
            return siteContext.Name;
        }
        #endregion

    }
}