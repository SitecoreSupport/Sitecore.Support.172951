namespace Sitecore.Support.ContentTesting.Requests.Controllers.Optimization
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Web.Http;
  using System.Web.Http.Results;
  using Data;
  using Data.Items;
  using Sitecore.ContentTesting;
  using Sitecore.ContentTesting.ContentSearch.Models;
  using Sitecore.ContentTesting.Data;
  using Sitecore.ContentTesting.Requests.Controllers;
  using Sitecore.ContentTesting.ViewModel;
  using Web.Http.Filters;

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
          list.Add(new SuggestedTestViewModel
          {
            HostPageId = item.ID.ToString(),
            HostPageUri = item.Uri.ToDataUri(),
            HostPageName = item.DisplayName,
            Language = item.Language.Name,
            Impact = suggestedTestSearchResultItem.Impact,
            Potential = suggestedTestSearchResultItem.Potential,
            Recommendation = suggestedTestSearchResultItem.Recommendation
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
  }
}