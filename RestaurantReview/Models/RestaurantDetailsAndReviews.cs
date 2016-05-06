using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace RestaurantReview.Models
{
    public class RestaurantDetailsAndReviews
    {
        public Restaurant Restaurant { get; set; }
        public List<Review> Reviews { get; set; }

        public RestaurantDetailsAndReviews(Restaurant resto)
        {
            RestaurantReviewsDB db=new RestaurantReviewsDB();
            Restaurant = resto;
            Reviews = (from rw in db.Reviews
                       where rw.restaurantId == resto.restaurantId
                       select rw).ToList();
        }
    }
}