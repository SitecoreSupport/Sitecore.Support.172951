require.config({
  baseUrl: '/sitecore/shell/client/Applications/ContentTesting/Common/lib'
});

define(["sitecore", "BindingUtil", "EditUtil", "DataUtil"], function (_sc, bindingUtil, editUtil, dataUtil) {
  return {
    ExecutedTestsList: function (options) {
      var mod = {
          _host: options.host,
          _enableClickEvent: options.enableClickEvent === undefined ? true : options.enableClickEvent,
        init: function () {
          this._host.TestsList.on("change:selectedItemId change:selectedLanguage", this.selectionChanged, this);
        },

        selectionChanged: function () {
          var selected = this._host.TestsList.get("selectedItem");

          var hostUri = selected.get("HostPageUri");
           
          /* Added code Patch 172951*/
            var contextSite = selected.get("ContextSite"); 
           /* Added code Patch 172951*/

          if (!hostUri || !mod._enableClickEvent) {
              $("[data-sc-id='TestsList']").find('tr').removeClass("active");
            return;
          }
          
          // Check if the URL starts with the xOptimization app URL
          if (window.location.pathname.indexOf(editUtil.xOppAppPath) === 0 && selected.get("TestType") == "Page") {
              editUtil.customOpenPageTestPage(hostUri, false, true, contextSite);
          }
          else {
              editUtil.customOpenExperienceEditor(hostUri, selected.get("DeviceId"), contextSite);
          }
        }
      };

      mod.init();
      return mod;
    }
  };
});