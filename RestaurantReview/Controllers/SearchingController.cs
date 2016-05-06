using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RestaurantReview.Models;
using RestaurantReview.BusinessClasses;

namespace RestaurantReview.Controllers
{
    public class SearchingController : Controller
    {
        private RestaurantReviewsDB db = new RestaurantReviewsDB();

        // GET: Searching
        public ActionResult Index(string searchingword, string choice) // searches restaurants based on searching condition and words given by users
        {
            List<Restaurant> result = CalculationOfElementsForViews.CalReviewsAndAvgrating();
            List<Restaurant> list = new List<Restaurant>();
            switch (choice)
            {
                case "name":
                    list = (from r in result
                              where r.name.ToUpper().Contains(searchingword.ToUpper())
                             select r).ToList();
                    break;
                case "genre":
                    list = (from r in result
                              where r.genre.ToUpper().Contains(searchingword.ToUpper())
                               select r).ToList();
                    break;
                case "city":
                    list = (from r in result
                              where r.city.ToUpper().Contains(searchingword.ToUpper())
                            select r).ToList();
                    break;
                case "username":               
                    var reviewResult=(from rw in db.Reviews
                          where rw.reviewed_by.Equals(searchingword, StringComparison.InvariantCultureIgnoreCase)
                          select rw).ToList();

                    for (var i = 0; i < reviewResult.Count; i++)
                    {
                        for (var j = 0; j < result.Count; j++ )
                            if (result[j].restaurantId == reviewResult[i].restaurantId)
                                list.Add(result[j]);
                    }
                    break;
            }

            if (list == null)
            {
                ModelState.AddModelError("Error", "No results.");
                return View();
            }
                       
            return View(list);
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
