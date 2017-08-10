using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Mod.ClientSettings.Models;
using Orchard;
using Orchard.Core.Settings.ViewModels;
using Orchard.Localization;
using Orchard.ContentManagement;
using Orchard.Localization.Services;
using Orchard.Settings;
using Orchard.UI.Notify;

namespace Mod.ClientSettings.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel {
        private readonly ISiteService _siteService;
        private readonly ICultureManager _cultureManager;
        public IOrchardServices Services { get; private set; }

        public AdminController(
            ISiteService siteService,
            IOrchardServices services,
            ICultureManager cultureManager) {
            _siteService = siteService;
            _cultureManager = cultureManager;
            Services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Index(string groupInfoId) {
            if (!Services.Authorizer.Authorize(Permissions.ManageSettings, T("Not authorized to manage settings")))
                return new HttpUnauthorizedResult();

            var site = _siteService.GetSiteSettings();
            if (String.IsNullOrWhiteSpace(groupInfoId)) {
                dynamic sitemodel = Services.New.SiteSettingsEditor();
                return View(sitemodel);
            }

            // perform check to make sure they are not trying to access a secure setting
            if (!CanAccess(groupInfoId))
                return new HttpUnauthorizedResult();

            dynamic model;            
            model = Services.ContentManager.BuildEditor(site, groupInfoId);

            if (model == null)
                return HttpNotFound();

            var groupInfo = Services.ContentManager.GetEditorGroupInfo(site, groupInfoId);
            if (groupInfo == null)
                return HttpNotFound();

            model.GroupInfo = groupInfo;

            return View(model);
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST(string groupInfoId) {
            if (!Services.Authorizer.Authorize(Permissions.ManageSettings, T("Not authorized to manage settings")))
                return new HttpUnauthorizedResult();

            // perform check to make sure they are not trying to access a secure setting
            if (!CanAccess(groupInfoId))
                return new HttpUnauthorizedResult();

            var site = _siteService.GetSiteSettings();
            var model = Services.ContentManager.UpdateEditor(site, this, groupInfoId);

            GroupInfo groupInfo = null;

            if (!string.IsNullOrWhiteSpace(groupInfoId)) {
                if (model == null) {
                    Services.TransactionManager.Cancel();
                    return HttpNotFound();
                }

                groupInfo = Services.ContentManager.GetEditorGroupInfo(site, groupInfoId);
                if (groupInfo == null) {
                    Services.TransactionManager.Cancel();
                    return HttpNotFound();
                }
            }

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                model.GroupInfo = groupInfo;

                return View(model);
            }

            Services.Notifier.Information(T("Settings updated"));
            return RedirectToAction("Index", new { groupInfoId = groupInfoId });
        }

        private bool CanAccess(string groupId) {
            var editors = Services.WorkContext.CurrentSite.As<EditorGroupSettingsPart>().EditorGroups;
            if (String.IsNullOrWhiteSpace(editors))
                return false;

            var editorArray = editors.Split(',');
            if (editorArray.Contains(groupId, StringComparer.InvariantCultureIgnoreCase))
                return true;

            return false;
        }

        #region Culture
        public ActionResult Culture() {
            //todo: class and/or method attributes for our auth?
            if (!Services.Authorizer.Authorize(Permissions.ManageSettings, T("Not authorized to manage settings")))
                return new HttpUnauthorizedResult();

            var model = new SiteCulturesViewModel {
                CurrentCulture = _cultureManager.GetCurrentCulture(HttpContext),
                SiteCultures = _cultureManager.ListCultures(),
            };
            model.AvailableSystemCultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .Select(ci => ci.Name)
                .Where(s => !model.SiteCultures.Contains(s));

            return View(model);
        }

        [HttpPost]
        public ActionResult AddCulture(string systemCultureName, string cultureName) {
            if (!Services.Authorizer.Authorize(Permissions.ManageSettings, T("Not authorized to manage settings")))
                return new HttpUnauthorizedResult();

            cultureName = string.IsNullOrWhiteSpace(cultureName) ? systemCultureName : cultureName;

            if (!string.IsNullOrWhiteSpace(cultureName)) {
                _cultureManager.AddCulture(cultureName);
            }
            return RedirectToAction("Culture");
        }

        [HttpPost]
        public ActionResult DeleteCulture(string cultureName) {
            if (!Services.Authorizer.Authorize(Permissions.ManageSettings, T("Not authorized to manage settings")))
                return new HttpUnauthorizedResult();

            _cultureManager.DeleteCulture(cultureName);
            return RedirectToAction("Culture");
        }

        #endregion

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}
