using System;
using Mod.ClientSettings.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Navigation;
using System.Linq;

namespace Mod.ClientSettings {
    public class AdminMenu : INavigationProvider {
        private readonly ISiteService _siteService;

        public AdminMenu(ISiteService siteService, IOrchardServices orchardServices) {
            _siteService = siteService;
            Services = orchardServices;
        }

        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }
        public IOrchardServices Services { get; private set; }

        public void GetNavigation(NavigationBuilder builder) {
            var site = _siteService.GetSiteSettings();
            if (site == null)
                return;

            var settings = site.ContentItem.As<EditorGroupSettingsPart>().EditorGroups;
            if (String.IsNullOrWhiteSpace(settings))
                return;

            var editors = settings.Split(',');

            builder.AddImageSet("settings")
                .Add(T("Client Settings"), "95",
                    menu => menu.Add(T("General"), "0", item => item.Action("Index", "Admin", new { area = "Mod.ClientSettings", groupInfoId = "Index" })
                        .Permission(Permissions.ManageSettings)), new [] {"collapsed"});

            

            foreach (var groupInfo in Services.ContentManager.GetEditorGroupInfos(site.ContentItem)) {
                GroupInfo info = groupInfo;
                if (!editors.Contains(info.Id))
                    continue;

                builder.Add(T("Client Settings"),
                    menu => menu.Add(info.Name, info.Position, item => item.Action("Index", "Admin", new { area = "Mod.ClientSettings", groupInfoId = info.Id })
                        .Permission(Permissions.ManageSettings)));
            }
        }
    }
}
