using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RestaurantReview.Models;
using System.Web.Security;
using RestaurantReview.Validators;
using System.IO;

namespace RestaurantReview.Controllers
{
    public class AccountController : Controller
    {
        private RestaurantReviewsDB db = new RestaurantReviewsDB();

        // GET: Account
        [Authorize]
        public ActionResult Index()
        {
            var users = db.Users.Include(u => u.UserDetail);
          
            return View(users.ToList()); // shows all registered users
        }

        // GET: Account/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            if (User.Identity.Name.Equals(user.username) || User.Identity.Name.Equals("Admin")) // user can see his own file only (except he is the Admin)  
                return View(user);
            else
            {
                ModelState.AddModelError("", "Unauthorized attempt! You have to log in as Admin.");             
                return View(db.Users.Find(User.Identity.Name)); // gives alert and brings user to his own details
            }
        }

        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register([Bind(Include = "username,password")] User user, 
            [Bind(Include = "firstname,lastname,email,city,country,imgPath")] UserDetail userdetail)
        {
            if (ModelState.IsValid)
            {
                User aUser = (from u in db.Users
                              where u.username.Equals(user.username)
                              select u).FirstOrDefault();
                if(aUser==null)
                {
                    db.Users.Add(user);
                    userdetail.username = user.username;
                    
                    //upload the file to pic
                    HttpPostedFileBase pic = Request.Files["imgPath"];
                    if (pic != null && pic.ContentLength>0)
                    {
                        //validate size
                        if (pic.ContentLength > 10240)
                        {
                            ModelState.AddModelError("", "The size of the file should not exceed 10 KB");
                            return View();
                        }
                        //validate filetype
                        var supportedTypes = new[] { "jpg", "jpeg", "png" };
                        var fileExt = System.IO.Path.GetExtension(pic.FileName).Substring(1);
                        if (!supportedTypes.Contains(fileExt))
                        {
                            ModelState.AddModelError("", "Invalid type. Only the following types (jpg, jpeg, png) are supported.");
                            return View();
                        }
                        //get the path to append on the image, and rename image to userId
                        string relPath = @"~\Images\";

                        userdetail.imgPath = relPath + userdetail.username + Path.GetExtension(pic.FileName);
                        pic.SaveAs(Server.MapPath(userdetail.imgPath));

                    }
                    else
                        userdetail.imgPath = null; // end of Jaya's code

                    db.UserDetails.Add(userdetail);              
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Existing User Name! Choose another one and try.");
                }
            }
            return View(user);
        }

        //GET: Account/Login
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Account/Login
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                User aUser = (from u in db.Users
                              where u.username.Equals(username) &&
                                    u.password.Equals(password)
                              select u).FirstOrDefault();
                if (aUser != null)
                    FormsAuthentication.RedirectFromLoginPage(username, false);
                else
                    ModelState.AddModelError("", "Invalid username or password!");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Restaurants");
        }

        // GET: Account/Edit/5
        [Authorize]
        public ActionResult Edit(string username)
        {
            if (username == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(username);
            if (user == null)
            {
                return HttpNotFound();
            }

            if (User.Identity.Name.Equals(user.username) || User.Identity.Name.Equals("Admin"))
                return View(user);
            else
            {
                ModelState.AddModelError("", "Unauthorized attempt! You have to log in as Admin.");
                return View(db.Users.Find(User.Identity.Name));
            }
        }

        // POST: Account/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserDetail userdetail, User user)
        {
            if (ModelState.IsValid)
            {
                userdetail.username = user.username;
                db.Entry(userdetail).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View();
        }

        // GET: Account/Delete/5
        [AdminAuthorize (Users="Admin")]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Account/Delete/5
        [HttpPost, ActionName("Delete")]
        [AdminAuthorize(Users = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
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
