using RestaurantReview.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace RestaurantReview.BusinessClasses
{
    public class HomePageRestaurantList
    {
        public static List<Restaurant> FindFiveRestos(string latitude, string longitude, List<Restaurant> restaurants)
        {
            DbGeography userLoc = DbGeography.FromText(string.Format("POINT({1} {0})", latitude, longitude), 4326);
            var closestFiveRestos = (from r in restaurants
                                     orderby r.location.Distance(userLoc)
                                     select r).Take(5).ToList();         
            return closestFiveRestos;
        }

        public static List<Restaurant> FindFiveRestos(XElement xe, List<Restaurant> restaurants)
        {
            RestaurantReviewsDB db = new RestaurantReviewsDB();

            var position = (from x in xe.Descendants("location")
                            select new
                            {
                                latitude = x.Element("lat").Value,
                                longitude = x.Element("lng").Value
                            }).FirstOrDefault();

            DbGeography userLoc = DbGeography.FromText(string.Format("POINT({1} {0})", position.latitude, position.longitude), 4326);
            var closestFiveRestos = (from r in restaurants
                                     orderby r.location.Distance(userLoc)
                                     select r).Take(5).ToList();
            return closestFiveRestos;
        }
    }
}