require.config({
  baseUrl: '/sitecore/shell/client/Applications/ContentTesting/Common/lib'
});

define(["sitecore", "BindingUtil", "EditUtil", "DataUtil"], function (_sc, bindingUtil, editUtil, dataUtil) {
  return {
    SuggestedTestsList: function (options) {
      var mod = {
        _host: options.host,

        init: function () {
          this._host.TestsList.on("change:selectedItemId", this.selectionChanged, this);

          if (this._host.Settings && this._host.Settings.get("ContentTesting.SuggestedTests.Maximum")) {
            this._host.TestsDataSource.set("pageSize", this._host.Settings.get("ContentTesting.SuggestedTests.Maximum"));
          }
        },

        selectionChanged: function () {
          var selected = this._host.TestsList.get("selectedItem");

          var hostUri = selected.get("HostPageUri");
          /* Added code Patch 172951*/
          var contextSite = selected.get("ContextSite");
          /* Added code Patch 172951*/
          if (!hostUri) {
            return;
          }
          /* Modified code Patch 172951 editUtil.openExperienceEditor(hostUri); */
          /* Added code Patch 172951*/
          this.customOpenExperienceEditor(hostUri, undefined, contextSite);
          /* Added code Patch 172951*/
        },
        /* Added code Patch 172951*/
        customOpenExperienceEditor: function (uri, device, contextSite) {
          var url = "/?sc_mode=edit";
          var parsedUri = new dataUtil.DataUri(uri);

          url = _sc.Helpers.url.addQueryParameters(url, {
            sc_itemid: parsedUri.id,
            sc_lang: parsedUri.lang,
            sc_version: parsedUri.ver,
            sc_device: device,
            sc_site: contextSite
          });
          editUtil.setExperienceEditorDeviceCookie(window, device);
          window.location.href = url;
        }
        /* Added code Patch 172951*/


      }

      mod.init();
      return mod;
    }





  };
});