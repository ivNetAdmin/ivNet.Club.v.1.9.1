
using Orchard.UI.Resources;

namespace ivNet.Club
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(ResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            manifest.DefineScript("AngularJS")
                .SetUrl("ivNet/anjularJs/anjular.min.js")
                .SetVersion("1.2.9")
                .SetDependencies("jQueryUI");
            manifest.DefineScript("AngularJSResource")
                .SetUrl("ivNet/anjularJs/angular-resource.min.js")
                .SetVersion("1.2.9")
                .SetDependencies("AngularJS");
            manifest.DefineScript("AngularJSSanitize")
                .SetUrl("ivNet/anjularJs/angular-sanitize.js")
                .SetVersion("1.2.15")
                .SetDependencies("AngularJS");
            manifest.DefineScript("AngularRouter")
              .SetUrl("ivNet/anjularJs/angular-ui-router.js")
              .SetVersion("0.2.15")
              .SetDependencies("AngularJS");
            manifest.DefineScript("xEditable")
                .SetUrl("ivNet/anjularJs/xeditable.min.js")
                .SetVersion("1.2.9")
                .SetDependencies("AngularJS");
            manifest.DefineScript("trNgGrid")
                .SetUrl("ivNet/trNgGrid/trNgGrid.min.js")
                .SetVersion("1.2.9")
                .SetDependencies("AngularJSResource");
            manifest.DefineScript("Bootstrap.TPLS")
                .SetUrl("ivNet/bootstrap/ui-bootstrap-tpls.min.js")
                .SetVersion("1.2.9");
            manifest.DefineScript("Bootstrap.Bootbox")
               .SetUrl("ivNet/bootstrap/bootbox.min.js")
               .SetVersion("4.4.0");
            manifest.DefineScript("CKEditor").SetUrl("ivNet/ckeditor/ckeditor.js").SetVersion("1.0");

            manifest.DefineScript("ivNet.Admin.Membership")
                .SetUrl("ivNet/admin/membership.min.js")
                .SetVersion("1.0")
                .SetDependencies("trNgGrid");

            manifest.DefineScript("ivNet.Admin.TeamSelection")
                .SetUrl("ivNet/admin/teamselection.min.js")
                .SetDependencies("AngularJSResource")
                .SetVersion("1.0");

            manifest.DefineScript("ivNet.Admin.Fixture")
               .SetUrl("ivNet/admin/fixture.min.js")
               .SetVersion("1.0")
               .SetDependencies("trNgGrid");

            manifest.DefineScript("ivNet.Admin.Registration")
                .SetUrl("ivNet/admin/registration.min.js")
                .SetVersion("1.0")
                .SetDependencies("trNgGrid");

            manifest.DefineScript("ivNet.Carousel")
                .SetUrl("ivNet/carousel.min.js")
                .SetVersion("1.0")
                .SetDependencies("jQuery");

            manifest.DefineScript("ivNet.Configure")
                .SetUrl("ivNet/configure.min.js")
                .SetVersion("1.0")
                .SetDependencies("trNgGrid");

            manifest.DefineScript("ivNet.Stats")
                .SetUrl("ivNet/stats.min.js")
                .SetVersion("1.0")
                .SetDependencies("trNgGrid");

            manifest.DefineScript("ivNet.Fixtures")
                .SetUrl("ivNet/fixtures.min.js")
                .SetVersion("1.0")
                .SetDependencies("trNgGrid");

            manifest.DefineScript("ivNet.Registration")
                .SetUrl("ivNet/registration.min.js")
                .SetVersion("1.0")
                .SetDependencies("trNgGrid");

            manifest.DefineScript("ivNet.Members")
               .SetUrl("ivNet/members.min.js")
               .SetVersion("1.0")
               .SetDependencies("trNgGrid");

            manifest.DefineScript("ivNet.Availability")
                .SetUrl("ivNet/availability.min.js")
                .SetDependencies("AngularJSResource")
                .SetVersion("1.0");

            manifest.DefineStyle("xEditable").SetUrl("admin/xeditable.css");
          
            manifest.DefineStyle("ivNet.Admin.Membership")
                .SetUrl("admin/membership.min.css")
                .SetDependencies("Bootstrap.Responsive");
            manifest.DefineStyle("ivNet.Admin.Fixture")
                .SetUrl("admin/fixture.min.css")
                .SetDependencies("Bootstrap.Responsive");
            manifest.DefineStyle("ivNet.Admin.TeamSelection")
               .SetUrl("admin/teamselection.min.css")
               .SetDependencies("Bootstrap.Responsive");
            manifest.DefineStyle("ivNet.Admin.Registration")
                .SetUrl("admin/registrations.min.css")
                .SetDependencies("Bootstrap.Responsive");

            manifest.DefineStyle("ivNet.Carousel").SetUrl("carousel.min.css");
            manifest.DefineStyle("ivNet.Configure").SetUrl("configure.min.css").SetDependencies("Bootstrap.Responsive");
            manifest.DefineStyle("ivNet.Stats").SetUrl("stats.min.css").SetDependencies("Bootstrap.Responsive");
            manifest.DefineStyle("ivNet.Fixtures").SetUrl("fixtures.min.css").SetDependencies("Bootstrap.Responsive");
            manifest.DefineStyle("ivNet.Registration")
                .SetUrl("registration.min.css")
                .SetDependencies("Bootstrap.Responsive");
            manifest.DefineStyle("ivNet.Members")
               .SetUrl("members.min.css")
               .SetDependencies("Bootstrap.Responsive");
            manifest.DefineStyle("ivNet.Availability")
              .SetUrl("availability.min.css")
              .SetDependencies("Bootstrap.Responsive");
            manifest.DefineStyle("Font-Awesome").SetUrl("bootstrap/font-awesome.min.css");
            manifest.DefineStyle("Bootstrap").SetUrl("bootstrap/bootstrap.min.css").SetDependencies("Font-Awesome");
            manifest.DefineStyle("Bootstrap.Responsive")
                .SetUrl("bootstrap/bootstrap-responsive.min.css")
                .SetDependencies("Bootstrap");
        }
    }
}