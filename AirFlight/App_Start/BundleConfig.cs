using System.Web;
using System.Web.Optimization;

namespace AirFlight
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            //Наши скриптиы
           
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            //Бандл для jQueryUIHelpers.Mvc5 
            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                          "~/Scripts/jquery-ui-{version}.js",
                          "~/Scripts/jquery-ui.unobtrusive-{version}.js"));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                         "~/Content/themes/base/all.css",
                         "~/Content/themes/base/core.css",
                         "~/Content/themes/base/myTooltip.css",
                         "~/Content/themes/base/jquery.ui.timepicker.css",                         
                         "~/Content/themes/base/theme.css"));       

            bundles.Add(new ScriptBundle("~/bundles/inputmask").Include(
            //~/Scripts/Inputmask/dependencyLibs/inputmask.dependencyLib.js",  //if not using jquery
            "~/Scripts/Inputmask/inputmask.js",
            "~/Scripts/Inputmask/jquery.inputmask.js",            
            "~/Scripts/Inputmask/inputmask.extensions.js",
            "~/Scripts/Inputmask/inputmask.date.extensions.js",
            //and other extensions you want to include
            "~/Scripts/Inputmask/inputmask.numeric.extensions.js"));
            ////////////////////////////////////
            // Используйте версию Modernizr для разработчиков, чтобы учиться работать. Когда вы будете готовы перейти к работе,           
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));
            bundles.Add(new ScriptBundle("~/bundles/CostumScript").Include(
                      "~/Scripts/CostumScript/*.js"));


        }
    }
}
