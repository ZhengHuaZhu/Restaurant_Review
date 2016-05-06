using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RestaurantReview.Validators
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AdminAuthorize: AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {   
            // checks user's authentication
            if (filterContext.RequestContext.HttpContext.User.Identity.IsAuthenticated)
            {
                // tells the responding controller to pass the name-value pair in TempData for its view to show an error message
                // filterContext.Controller.TempData.Clear();
                filterContext.Controller.TempData.Add("RedirectionReason", "Unauthorized");
            }
            base.HandleUnauthorizedRequest(filterContext);
        }
    }
}