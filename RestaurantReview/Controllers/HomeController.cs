using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RestaurantReview.Models;
using System.Data.Entity.Spatial;
using RestaurantReview.BusinessClasses;
using System.Xml.Linq;

namespace RestaurantReview.Controllers
{
    public class HomeController : Controller
    {
        private RestaurantReviewsDB db = new RestaurantReviewsDB();

        // GET: Home
        // renders a form asking postal code if geolocation acquiring is compromised, 
        //user will be diverted to the home page that includes 5 restaurants
        public ActionResult Index() 
        {         
            return View();
        }
     
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
