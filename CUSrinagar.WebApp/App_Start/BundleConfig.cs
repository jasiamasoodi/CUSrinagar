using System.Web;
using System.Web.Optimization;

namespace CUSrinagar.WebApp
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            #region javaScripts

            //var jqueryCDNPath = "https://ajax.googleapis.com/ajax/libs/jquery/3.2.1/jquery.min.js";
            //bundles.Add(new ScriptBundle("~/bundles/jquery", jqueryCDNPath).Include("~/Scripts/Libraries/jquery-{version}.min.js"));
            bundles.Add(new ScriptBundle("~/Content/ThemePublic/Libraries/jsJQuery").Include(
               "~/Content/ThemeAdmin/Content/Scripts/Libraries/jquery-2.1.4.min.js"));

            bundles.Add(new ScriptBundle("~/Content/ThemePublic/CloudPushNotification/PushNoti").Include(
             "~/Content/ThemePublic/CloudPushNotification/JSPushNotification.js"));



            bundles.Add(new ScriptBundle("~/Content/ThemePublic/Libraries/jsMains").Include(
                "~/Content/ThemePublic/Libraries/plugins/bootstrap/js/bootstrap.min.js",
                "~/Content/ThemePublic/Libraries/plugins/bootstrap-hover-dropdown.min.js",
                "~/Content/ThemePublic/Libraries/plugins/back-to-top.js",
                "~/Content/ThemePublic/Libraries/plugins/jquery-placeholder/jquery.placeholder.js",
                "~/Content/ThemePublic/Libraries/plugins/pretty-photo/js/jquery.prettyPhoto.js",
                "~/Content/ThemePublic/Libraries/plugins/flexslider/jquery.flexslider-min.js",
                "~/Content/ThemePublic/Libraries/plugins/jflickrfeed/jflickrfeed.min.js",
                "~/Content/ThemePublic/Libraries/plugins/main.js",
                "~/Content/ThemeAdmin/Content/Scripts/Libraries/chosen.jquery.min.js",
                "~/Content/ThemeAdmin/Content/Scripts/Libraries/bootstrap-datepicker.min.js",
                "~/Scripts/General/general.js",
                "~/Scripts/General/pager-table.js"
                ));
            bundles.Add(new ScriptBundle("~/Content/ThemePublic/Libraries/plugins/jsIndex").Include(
                "~/Content/ThemePublic/Libraries/plugins/jq-accordion/js/jquery-ui.js",
                "~/Content/ThemePublic/Libraries/plugins/tabs/js/src/tabs.js",
                "~/Content/ThemePublic/Libraries/plugins/news-ticker/assets/js/jquery.newsTicker.js"
                ));
            bundles.Add(new ScriptBundle("~/bundles/Libraries/jqueryval").Include(
                "~/Content/ThemeAdmin/Content/Scripts/Libraries/jquery.mask.min.js",
                "~/Content/ThemeAdmin/Content/Scripts/JSGeneral/GeneralJS.js",
                "~/Content/ThemeAdmin/Content/Scripts/Libraries/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/Libraries/jqueryvalidation").Include(
                "~/Content/ThemeAdmin/Content/Scripts/Libraries/jquery.validate*",
                "~/Content/ThemeAdmin/Content/Scripts/Libraries/jquery.mask.min.js",
                "~/Content/ThemeAdmin/Content/Scripts/JSGeneral/GeneralJS.js",
                "~/Scripts/General/general-validation.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/Libraries/jqueryForm").Include(
                        "~/Content/ThemeAdmin/Content/Scripts/JSAdmission/Combinations.js"));

            bundles.Add(new ScriptBundle("~/bundles/Libraries/SCForm").Include(
                        "~/Content/ThemeAdmin/Content/Scripts/JSAdmission/SearchCombinations.js"
                        , "~/Content/ThemeAdmin/Content/Scripts/JSAdmission/CombinationStatus.js"));

            bundles.Add(new ScriptBundle("~/bundles/Libraries/jqueryregIH").Include(
                        "~/Content/ThemePublic/JSRegistration/IntegratedHonours.js"
                        , "~/Content/ThemePublic/Libraries/disableBack.js"));

            bundles.Add(new ScriptBundle("~/bundles/Libraries/jqueryregUG").Include(
                        "~/Content/ThemePublic/JSRegistration/Graduation.js"
                        , "~/Content/ThemePublic/Libraries/disableBack.js"));


            bundles.Add(new ScriptBundle("~/bundles/Libraries/jqueryregNEPIH").Include(
                        "~/Content/ThemePublic/JSRegistration/NEP_IG.js"
                        , "~/Content/ThemePublic/Libraries/disableBack.js"));

            bundles.Add(new ScriptBundle("~/bundles/Libraries/jqueryregNEPPG").Include(
                        "~/Content/ThemePublic/JSRegistration/NEP_PG.js"
                        , "~/Content/ThemePublic/Libraries/disableBack.js"));


            bundles.Add(new ScriptBundle("~/bundles/Libraries/jqueryregNEP").Include(
                        "~/Content/ThemePublic/JSRegistration/NEP.js"
                        , "~/Content/ThemePublic/Libraries/disableBack.js"));

            bundles.Add(new ScriptBundle("~/bundles/Libraries/jqueryregPG").Include(
                        "~/Content/ThemePublic/JSRegistration/JSPostGraduation.js"
                        , "~/Content/ThemePublic/Libraries/disableBack.js"));


            bundles.Add(new ScriptBundle("~/bundles/JQAccount").Include(
                "~/Scripts/JSAccount/SignIn.js"));

            bundles.Add(new ScriptBundle("~/bundles/JQStudenZone").Include(
                "~/Content/ThemeAdmin/Content/Scripts/JSUsers/StudentLogin.js"));

            bundles.Add(new ScriptBundle("~/bundles/Libraries/JQAssignRN").Include(
                "~/Content/ThemeAdmin/Content/Scripts/JSAdmission/AssignClassRollNo.js"));


            bundles.Add(new ScriptBundle("~/bundles/Libraries/jqueryAdmitCard").Include(
                        "~/Content/ThemePublic/JSRegistration/AdmitCards.js"
                        , "~/Content/ThemePublic/Libraries/disableBack.js"));

            bundles.Add(new ScriptBundle("~/bundles/Libraries/jqueryregCollegeUG").Include(
                        "~/Content/ThemeAdmin/Content/Scripts/JSRegistration/CollegeGraduation.js"
                        , "~/Content/ThemePublic/Libraries/disableBack.js"));


            //payments
            bundles.Add(new ScriptBundle("~/bundles/Libraries/jqueryregMP").Include(
                        "~/Content/ThemePublic/Payment/LaterPayment.js"
                        , "~/Content/ThemePublic/Libraries/disableBack.js"));

            bundles.Add(new ScriptBundle("~/bundles/Libraries/jqueryregSFS").Include(
                        "~/Content/ThemePublic/Payment/SelfFinanced.js"
                        , "~/Content/ThemePublic/Libraries/disableBack.js"));

            //sem admit cards
            bundles.Add(new ScriptBundle("~/bundles/Libraries/jquerySemAdmitCard").Include(
                  "~/Content/ThemePublic/Examination/SemExamAdmitCards.js"
                  , "~/Content/ThemePublic/Libraries/disableBack.js"));
            #endregion

            #region Styles
            bundles.Add(new StyleBundle("~/Content/ThemePublic/Libraries/gencss").Include(
                 "~/Content/ThemeAdmin/Content/Libraries/css/jquery-ui.min.css",
                      "~/Content/ThemePublic/Libraries/plugins/bootstrap/css/bootstrap.min.css",
                      "~/Content/ThemePublic/Libraries/css/main-site-styles.css",
                      "~/Content/ThemePublic/Libraries/css/style.css",
                      "~/Content/ThemeAdmin/Content/Libraries/css/chosen.min.css",
                      "~/Content/ThemePublic/Libraries/plugins/pretty-photo/css/prettyPhoto.css",
                      "~/Content/ThemePublic/Libraries/plugins/news-ticker/assets/css/main.css",
                      "~/Content/ThemePublic/Libraries/plugins/tabs/css/style-tabs.css",
                      "~/Content/ThemePublic/Libraries/plugins/jq-accordion/css/style.css"
                      ));
            #endregion

            #region CollegePanel
            bundles.Add(new ScriptBundle("~/Content/ThemeAdmin/Content/Scripts/Libraries/jsmainsbasiccollege").Include(
               "~/Content/ThemeAdmin/Content/Scripts/Libraries/jquery-2.1.4.min.js",
               "~/Content/ThemeAdmin/Content/Scripts/Libraries/jquery-ui.min.js",
               "~/Content/ThemeAdmin/Content/Scripts/Libraries/bootstrap.min.js"
           ));
            bundles.Add(new ScriptBundle("~/Content/ThemeAdmin/Content/Scripts/Libraries/jsmainscollege").Include(
                "~/Content/ThemeAdmin/Content/Scripts/Libraries/chosen.jquery.min.js",
                "~/Content/ThemeAdmin/Content/Scripts/Libraries/bootstrap-datepicker.min.js",
                "~/Content/ThemeAdmin/Content/Scripts/Libraries/ace-extra.min.js",
                "~/Content/ThemeAdmin/Content/Scripts/Libraries/grid.locale-en.js",
                "~/Content/ThemeAdmin/Content/Scripts/Libraries/ace-elements.min.js",
                "~/Content/ThemeAdmin/Content/Scripts/Libraries/ace.min.js",
                "~/Content/ThemeAdmin/Content/Scripts/JSGeneral/CheckCheckBoxes.js",
                 "~/Content/ThemeAdmin/Content/Scripts/Libraries/bootstrap-multiselect.min.js",
                   "~/Scripts/JSChosen.js"
            ));
            bundles.Add(new StyleBundle("~/Content/ThemeAdmin/Content/Libraries/css/gencsscollege").Include(
                "~/Content/ThemeAdmin/Content/Libraries/css/bootstrap.min.css",
                "~/Content/ThemeAdmin/Content/Libraries/css/jquery-ui.min.css",
                "~/Content/ThemeAdmin/Content/Libraries/css/ui.jqgrid.min.css",
                "~/Content/ThemeAdmin/Content/Libraries/css/chosen.min.css",
                "~/Content/ThemeAdmin/Content/Libraries/css/bootstrap-datepicker3.min.css",
                "~/Content/ThemeAdmin/Content/Libraries/css/ace.min.css",
                "~/Content/ThemeAdmin/Content/Libraries/css/ace-skins.min.css",
                 "~/Content/ThemeAdmin/Content/Libraries/css/ace-rtl.min.css",
                 "~/Content/ThemeAdmin/Content/Libraries/css/style.css"
          ));
            #endregion

            #region CUSAdminPanel

            bundles.Add(new StyleBundle("~/Content/ThemeAdmin/Content/Libraries/css/gencssadmin").Include(
                "~/Content/ThemeAdmin/Content/Libraries/css/bootstrap.min.css",
                "~/Content/ThemeAdmin/Content/Libraries/css/bootstrap.min.css",
                "~/Content/ThemeAdmin/Content/Libraries/css/jquery-ui.min.css",
                "~/Content/ThemeAdmin/Content/Libraries/css/chosen.min.css",
                "~/Content/ThemeAdmin/Content/Libraries/css/bootstrap-datepicker3.min.css",

                "~/Content/ThemeAdmin/Content/Libraries/css/ace.min.css",
                "~/Content/ThemeAdmin/Content/Libraries/css/ace-skins.min.css",
                 "~/Content/ThemeAdmin/Content/Libraries/css/ace-rtl.min.css",
                 "~/Content/ThemeAdmin/Content/Libraries/css/style.css"
                 ));
            //"~/Content/ThemeAdmin/Content/Libraries/css/fonts.googleapis.com.css",

            bundles.Add(new ScriptBundle("~/Content/ThemeAdmin/Content/Scripts/Libraries/jsgenmainadmin").Include(
                "~/Content/ThemeAdmin/Content/Scripts/Libraries/jquery-3.2.1.min.js",
              "~/Content/ThemeAdmin/Content/Scripts/Libraries/jquery-2.1.4.min.js",
             "~/Content/ThemeAdmin/Content/Scripts/Libraries/jquery-1.11.3.min.js",
             "~/Content/ThemeAdmin/Content/Scripts/Libraries/jquery-ui.min.js",
             "~/Content/ThemeAdmin/Content/Scripts/Libraries/bootstrap.min.js",
             "~/Content/ThemeAdmin/Content/Scripts/Libraries/chosen.jquery.min.js",

             "~/Content/ThemeAdmin/Content/Scripts/Libraries/ace-extra.min.js",

             "~/Content/ThemeAdmin/Content/Scripts/Libraries/ace-elements.min.js",
             "~/Content/ThemeAdmin/Content/Scripts/Libraries/ace.min.js"

            ));


            bundles.Add(new ScriptBundle("~/Content/ThemeAdmin/Content/Scripts/Libraries/jsgensubadmin").Include(
                "~/Content/ThemeAdmin/Content/Scripts/Libraries/grid.locale-en.js",
                "~/Content/ThemeAdmin/Content/Scripts/Libraries/bootstrap-datepicker.min.js",
                 "~/Content/ThemeAdmin/Content/Scripts/JSGeneral/CheckCheckBoxes.js",
                  "~/Content/ThemeAdmin/Content/Scripts/Libraries/bootstrap-multiselect.min.js",
                   "~/Scripts/JSChosen.js"
            ));

            bundles.Add(new ScriptBundle("~/Scripts/General/jscustomsubadmin").Include(
                "~/Scripts/General/general.js",
                "~/Scripts/General/pager-table.js",
                "~/Scripts/General/custom-select.js"
            ));

            #endregion

            #region ace_master
            //css
            bundles.Add(new StyleBundle("~/Content/ThemeAdmin/Content/Libraries/css/ace-master").Include(
            "~/Content/ThemeAdmin/Content/Libraries/css/bootstrap.min.css",
            "~/Content/ThemeAdmin/Content/Libraries/css/jquery-ui.min.css",
            "~/Content/ThemeAdmin/Content/Libraries/css/chosen.min.css",
            "~/Content/ThemeAdmin/Content/Libraries/css/bootstrap-datepicker3.min.css",

            "~/Content/ThemeAdmin/Content/Libraries/css/ace.min.css",
            "~/Content/ThemeAdmin/Content/Libraries/css/ace-skins.min.css",
             "~/Content/ThemeAdmin/Content/Libraries/css/ace-rtl.min.css",
             "~/Content/ThemeAdmin/Content/Libraries/css/style.css"
             ));
            // "~/Content/ThemeAdmin/Content/Libraries/css/fonts.googleapis.com.css",

            //jquery
            bundles.Add(new ScriptBundle("~/Content/ThemeAdmin/Content/Scripts/Libraries/js-ace-master").Include(
                "~/Content/ThemeAdmin/Content/Scripts/Libraries/jquery-3.2.1.min.js",
                "~/Content/ThemeAdmin/Content/Scripts/Libraries/jquery-2.1.4.min.js",
                "~/Content/ThemeAdmin/Content/Scripts/Libraries/jquery-1.11.3.min.js",
                "~/Content/ThemeAdmin/Content/Scripts/Libraries/jquery-ui.min.js",
                 "~/Content/ThemeAdmin/Content/Scripts/Libraries/bootstrap.min.js",
                 "~/Content/ThemeAdmin/Content/Scripts/Libraries/chosen.jquery.min.js",
                 "~/Content/ThemeAdmin/Content/Scripts/Libraries/ace-extra.min.js",
                 "~/Content/ThemeAdmin/Content/Scripts/Libraries/ace-elements.min.js",
                 "~/Content/ThemeAdmin/Content/Scripts/Libraries/ace.min.js"
            ));
            bundles.Add(new ScriptBundle("~/Content/ThemeAdmin/Content/Scripts/Libraries/js-general").Include(
                "~/Content/ThemeAdmin/Content/Scripts/Libraries/grid.locale-en.js",
                "~/Content/ThemeAdmin/Content/Scripts/Libraries/bootstrap-datepicker.min.js",
                 "~/Content/ThemeAdmin/Content/Scripts/JSGeneral/CheckCheckBoxes.js",
                  "~/Content/ThemeAdmin/Content/Scripts/Libraries/bootstrap-multiselect.min.js",
                   "~/Scripts/JSChosen.js"
            ));

            #endregion

            #region general
            bundles.Add(new ScriptBundle("~/Scripts/General/js-custom").Include(
                "~/Scripts/General/general.js",
                "~/Scripts/General/pager-table.js",
                "~/Scripts/General/custom-select.js"
            ));

#if (!DEBUG)
                bundles.Add(new ScriptBundle("~/Content/ThemeAdmin/Content/Scripts/JSGeneral/preventDefault").Include(
                    "~/Content/ThemeAdmin/Content/Scripts/JSGeneral/preventDefaultJS.js"
                ));
#endif
            #endregion
        }
    }
}