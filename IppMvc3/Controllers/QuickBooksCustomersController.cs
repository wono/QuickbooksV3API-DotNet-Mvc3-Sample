using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Intuit.Ipp.Core;
using Intuit.Ipp.Core.Configuration;
using Intuit.Ipp.Data;
using Intuit.Ipp.DataService;
using Intuit.Ipp.Exception;
using Intuit.Ipp.QueryFilter;
using Intuit.Ipp.Security;
using Intuit.Ipp.LinqExtender;
using DevDefined.OAuth.Consumer;
using DevDefined.OAuth.Framework;
using IntuitSampleMVC.utils;

namespace IntuitSampleMVC.Controllers
{
    /// <summary>
    /// Controller which connects to QuickBooks and Pulls customer Info.
    /// This flow will make use of Data Service SDK V2 to create OAuthRequest and connect to 
    /// Customer Data under the service context and display data inside the grid.
    /// </summary>
    public class QuickBooksCustomersController : Controller
    {
        /// <summary>
        /// RealmId, AccessToken, AccessTokenSecret, ConsumerKey, ConsumerSecret, DataSourceType
        /// </summary>
        private String realmId, accessToken, accessTokenSecret, consumerKey, consumerSecret, dataSourcetype;

        /// <summary>
        /// Action Results for Index
        /// </summary>
        /// <returns>Action Result.</returns>
        public ActionResult Index()
        {
            realmId = Session["realm"].ToString();
            accessToken = Session["accessToken"].ToString();
            accessTokenSecret = Session["accessTokenSecret"].ToString();
            consumerKey = ConfigurationManager.AppSettings["consumerKey"].ToString();
            consumerSecret = ConfigurationManager.AppSettings["consumerSecret"].ToString();
            dataSourcetype = Session["dataSource"].ToString().ToLower();


            try
            {
                IntuitServicesType intuitServicesType = new IntuitServicesType();
                switch (dataSourcetype)
                {
                    case "qbo":
                        intuitServicesType = IntuitServicesType.QBO;
                        break;
                    case "qbd":
                        intuitServicesType = IntuitServicesType.QBD;
                        break;
                    default:
                        throw new Exception("Data source type not found.");
                        break;

                }

                OAuthRequestValidator oauthValidator = new OAuthRequestValidator(accessToken, accessTokenSecret, consumerKey, consumerSecret);
                ServiceContext context = new ServiceContext(realmId, intuitServicesType, oauthValidator);
                DataService dataService = new DataService(context);
                List<Customer> customers = dataService.FindAll(new Customer(), 1, 100).ToList();
                ViewBag.MyCollection = customers;
                ViewBag.CustomerCount = customers.Count();

            }
            catch (InvalidTokenException exp)
            {
                //Remove the Oauth access token from the OauthAccessTokenStorage.xml
                OauthAccessTokenStorageHelper.RemoveInvalidOauthAccessToken(Session["FriendlyEmail"].ToString(), this);

                Session["show"] = true;
                return Redirect("/Home/index");
            }
            catch (System.Exception exp)
            {
                throw exp;
            }

            return View();
        }
    }
}
