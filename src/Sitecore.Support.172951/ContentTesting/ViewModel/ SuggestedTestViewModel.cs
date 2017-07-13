namespace Sitecore.Support.ContentTesting.ViewModel
{
  using Sitecore.ContentTesting.ViewModel;

  public class SuggestedTestViewModel : BaseTestViewModel
  {
    public double Potential
    {
      get;
      set;
    }

    public double Impact
    {
      get;
      set;
    }

    public double Recommendation
    {
      get;
      set;
    }

    public string Suggestion
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