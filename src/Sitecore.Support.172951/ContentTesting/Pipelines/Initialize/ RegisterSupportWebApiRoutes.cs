namespace Sitecore.Support.ContentTesting.Pipelines.Initialize
{
  using System.Web.Http;
  using System.Web.Routing;
  using Diagnostics;
  using Sitecore.ContentTesting.Configuration;
  using Sitecore.Pipelines;

  #region MyRegion
  public class RegisterSupportWebApiRoutes
  {
    public virtual void Process(PipelineArgs args)
    {
      Assert.ArgumentNotNull(args, "args"); 
      if (!Settings.IsAutomaticContentTestingEnabled)
      {
        return;
      }
      RouteTable.Routes.MapHttpRoute("Sitecore.SupportContentTesting", Settings.CommandRoutePrefix + "Tests/GetSuggestedTests", new { controller = "SupportTests", action = "GetSuggestedTests" });
      RouteTable.Routes.MapHttpRoute("Sitecore.SupportContentTestingActiveTest", "sitecore/shell/api/ct/SupportTests/{action}", new { controller = "SupportTests" });
    }
  }
  #endregion

}