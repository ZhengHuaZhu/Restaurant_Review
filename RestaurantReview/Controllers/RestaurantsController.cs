using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RestaurantReview.Models;
using System.Xml.Linq;
using RestaurantReview.BusinessClasses;
using System.Data.Entity.Spatial;
using RestaurantReview.Validators;

namespace RestaurantReview.Controllers
{
    public class RestaurantsController : Controller
    {
        private RestaurantReviewsDB db = new RestaurantReviewsDB();

        // GET: Restaurants
        public ActionResult Index()
        {   
            // fetch all restaurants with two additional properties: numOfReviews & avgRating
            return View(CalculationOfElementsForViews.CalReviewsAndAvgrating());  
        }

        [HttpPost]
        public ActionResult Render(int error, string latitude, string longitude, string postalCode)
        {
            // fetch all restaurants with two additional properties, numOfReviews & avgRating, from my business class
            var restaurants = CalculationOfElementsForViews.CalReviewsAndAvgrating();

            if (error == 0)
            {
                // fetches 5 restaurants with two additional properties, numOfReviews & avgRating, from my business class
                return View(HomePageRestaurantList.FindFiveRestos(latitude, longitude, restaurants
                    ));
            }
            else
            {
                // fetches XElement from my business class
                XElement xe = GeoPosition.GetGeocodingSearchResults(postalCode);
                int numOfResult = 0;
                if (xe.Elements("status").FirstOrDefault().Value.Equals("OK"))
                    numOfResult = xe.Elements("result").ToList().Count;

                if (numOfResult == 1)
                {
                    // fetches 5 restaurants with two additional properties, numOfReviews & avgRating, from my business class
                    return View(HomePageRestaurantList.FindFiveRestos(xe, restaurants));
                }
                else
                {
                    ModelState.AddModelError("", "Incorrect postal code! Default page is loaded.");
                    postalCode = "H3Z1A4"; // default postal code
                    XElement xe2 = GeoPosition.GetGeocodingSearchResults(postalCode);
                    return View(HomePageRestaurantList.FindFiveRestos(xe2, restaurants));
                }
            }
        }

        //GET: AddReviews
        [Authorize]
        public ActionResult AddReviews()
        {
            return View();
        }

        //POST: AddReviews
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        // adds reviews according to the restaurant id
        public ActionResult AddReviews([Bind(Include = "title, rating, content")] Review review, int id)
        {
            if (ModelState.IsValid)
            {
                review.restaurantId = id;
                review.reviewed_by = User.Identity.Name;
                review.date = DateTime.Now;
                var resto = (from r in db.Restaurants
                             where r.restaurantId == id
                             select r).FirstOrDefault();
                db.Reviews.Add(review);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View();
        }

        // GET: Restaurants/Reviews
        public ActionResult Reviews(int? id) // a resto id is passed in to see the detail of reviews 
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var list = (from rw in db.Reviews
                        where rw.restaurantId == id
                        select rw).ToList();

            return View(list);
        }

        // GET: Restaurants/EditReviews/
        [AdminAuthorize(Users = "Admin")]
        public ActionResult EditReviews(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Review review = db.Reviews.Find(id);
            if (review == null)
            {
                return HttpNotFound();
            }
            return View(review);
        }

        // POST: Restaurants/EditReviews/
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [AdminAuthorize(Users = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult EditReviews(Review review)
        {
            if (ModelState.IsValid)
            {
                db.Entry(review).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(review);
        }

        // GET: Restaurants/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Restaurant restaurant = db.Restaurants.Find(id);
            if (restaurant == null)
            {
                return HttpNotFound();
            }

            List<Review> reviews = (from rw in db.Reviews
                                    where rw.restaurantId == restaurant.restaurantId
                                    select rw).ToList();

            var resto = (from r in db.Restaurants
                         where r.restaurantId == id
                         select r).FirstOrDefault();

            resto.views++;
            db.Entry(resto).State = EntityState.Modified;
            db.SaveChanges();

            ViewBag.Reviews = reviews; // shows reviews with the restaurant

            return View(restaurant);
        }

        // GET: Restaurants/Create
        [Authorize]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Restaurants/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "name,streetNumber,streetName,city,postal_code,genre,price")] Restaurant restaurant)
        {
            if (ModelState.IsValid)
            {
                var existence = (from r in db.Restaurants
                                 where r.name.Equals(restaurant.name, StringComparison.InvariantCultureIgnoreCase) &&
                                 r.streetNumber.Equals(restaurant.streetNumber) &&
                                 r.streetName.Equals(restaurant.streetName, StringComparison.InvariantCultureIgnoreCase) &&
                                 r.postal_code.Equals(restaurant.postal_code, StringComparison.InvariantCultureIgnoreCase)
                                 select r).FirstOrDefault(); // checks if there is an existing restaurant in the db

                if (existence == null)
                {
                    restaurant.views = 0;
                    restaurant.added_by = User.Identity.Name;
                    restaurant.added_date = DateTime.Now;

                    XElement xe = GeoPosition.GetGeocodingSearchResults(restaurant.postal_code);
                    int numOfResult = 0;
                    if (xe.Elements("status").FirstOrDefault().Value.Equals("OK"))
                        numOfResult = xe.Elements("result").ToList().Count;

                    if (numOfResult == 1)
                    {
                        var position = (from x in xe.Descendants("location")
                                        select new
                                        {
                                            latitude = x.Element("lat").Value,
                                            longitude = x.Element("lng").Value
                                        }).FirstOrDefault();
                        restaurant.location = DbGeography.FromText(string.Format("POINT({1} {0})", position.latitude, position.longitude), 4326);

                        db.Restaurants.Add(restaurant);
                        db.SaveChanges();
                        var id = (from r in db.Restaurants
                                  where r.name == restaurant.name &&
                                        r.streetNumber == restaurant.streetNumber &&
                                        r.streetName == restaurant.streetName &&
                                        r.postal_code == restaurant.postal_code
                                  select r).FirstOrDefault().restaurantId;
                        return RedirectToAction("Details", new { id = id });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Incorrect postal code!");
                    }
                }
                else
                    ModelState.AddModelError("", "Existing restaurant!");
            }
            return View();
        }

        // GET: Restaurants/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Restaurant restaurant = db.Restaurants.Find(id);
            if (restaurant == null)
            {
                return HttpNotFound();
            }

            return View(restaurant);
        }

        // POST: Restaurants/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Restaurant restaurant)
        {
            if (ModelState.IsValid)
            {
                XElement xe = GeoPosition.GetGeocodingSearchResults(restaurant.postal_code);
                var position = (from x in xe.Descendants("location")
                                select new
                                {
                                    latitude = x.Element("lat").Value,
                                    longitude = x.Element("lng").Value
                                }).FirstOrDefault();
                restaurant.location = DbGeography.FromText(string.Format("POINT({1} {0})", position.latitude, position.longitude), 4326);
                restaurant.edited_by = User.Identity.Name;
                restaurant.edit_date = DateTime.Now;
                db.Entry(restaurant).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
           
            return View(restaurant);
        }

        // GET: Restaurants/Delete/5
        [AdminAuthorize(Users = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Restaurant restaurant = db.Restaurants.Find(id);
            if (restaurant == null)
            {
                return HttpNotFound();
            }
            return View(restaurant);
        }

        // POST: Restaurants/Delete/5 
        [AdminAuthorize(Users = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Restaurant restaurant = db.Restaurants.Find(id);
            db.Restaurants.Remove(restaurant);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Restaurants/DeleteReviews/
        [AdminAuthorize(Users = "Admin")]
        public ActionResult DeleteReviews(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Review review = db.Reviews.Find(id);
            if (review == null)
            {
                return HttpNotFound();
            }
            return View(review);
        }

        // POST: Restaurants/Delete/5 
        [AdminAuthorize(Users = "Admin")]
        [HttpPost, ActionName("DeleteReviews")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteReviewsConfirmed(int id)
        {
            Review review = db.Reviews.Find(id);
            db.Reviews.Remove(review);
            db.SaveChanges();
            return RedirectToAction("Index");
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
