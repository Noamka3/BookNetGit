using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _BookNeT_.Models;
using _BookNeT_.Models.viewModels;

namespace _BookNeT_Project.Controllers
{
    public class UsersController : Controller
    {
        BookNeT_projectEntities db = new BookNeT_projectEntities();

        // GET: Users
        public ActionResult Users()
        {
            return View(db.Users.ToList());
        }

        [HttpGet]
        public ActionResult Details(int id)
        {
            var user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            // החזרת Partial View (רק התוכן של ה-Modal)
            return PartialView("admin_Details_user", user);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "UserID,FirstName,LastName,Email,Phone,Password,Role,Age,RegistrationDate")] Users user)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Users");
            }

            return View(user);
        }
        [HttpGet]
        public ActionResult Edits(int id)
        {
            var user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            // החזרת Partial View (רק התוכן של ה-Modal)
            return PartialView("admin_edit_user", user);
        }
        [HttpPost]
        public ActionResult SetEdits(Users updatedUser)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.FirstOrDefault(u => u.UserID == updatedUser.UserID);
                if (user != null)
                {
                    // עדכון הנתונים לפי הערכים החדשים
                    user.FirstName = updatedUser.FirstName;
                    user.LastName = updatedUser.LastName;
                    user.Email = updatedUser.Email;
                    user.Phone = updatedUser.Phone;
                    

                    // שמירה ב-Database
                    db.SaveChanges();

                    // חזרה לדף Users עם הודעת הצלחה
                    TempData["SuccessMessage"] = "Details updated successfully."; // שליחה של הודעה
                    return RedirectToAction("Users");
                }
            }
            return Json(new { success = false, message = "Failed to update details." });
        }


        // POST: Users/Edit/5
        [HttpPost]
        public ActionResult Edit([Bind(Include = "UserID,FirstName,LastName,Email,Phone,Password,Role,Age,RegistrationDate")] Users user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Users");
            }

            return View(user);
        }

        // GET: Users/Delete/5
        public ActionResult Delete(int? id)
        {
            // אם ה-ID לא קיים, נחזיר שגיאה
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // חפש את המשתמש ב-DB לפי ה-ID
            Users user = db.Users.Find(id);

            // אם המשתמש לא נמצא, נחזיר שגיאה
            if (user == null)
            {
                return HttpNotFound();
            }

            try
            {
                // מחיקת המשתמש מה-DB
                db.Users.Remove(user);
                db.SaveChanges();

                // הצגת הודעת הצלחה
                TempData["SuccessMessage"] = "The User " + user.FirstName + " " + user.LastName + " has been successfully deleted.";
            }
            catch (Exception ex)
            {
                // הצגת הודעת שגיאה במקרה של תקלה
                TempData["ErrorMessage"] = $"An error occurred while deleting the user: {ex.Message}";
            }

            // חזרה לדף הרשימה של המשתמשים
            return RedirectToAction("Users");
        }


        // GET: Users/ChangeAdmin/5
        [HttpGet]
        public ActionResult ChangeRole(int? id)
        {
            // אם ה-ID לא קיים, נחזיר שגיאה
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // חפש את המשתמש ב-DB לפי ה-ID
            Users user = db.Users.Find(id);

            // אם המשתמש לא נמצא, נחזיר שגיאה
            if (user == null)
            {
                return HttpNotFound();
            }
            if (user.Role == "Admin")
            {
                user.Role = "user";
            }
            else
            {
                user.Role = "Admin";
            }

            db.SaveChanges();

            // החזרה לדף הרשימה עם הודעת הצלחה
            TempData["SuccessMessage"] = "The User " + user.FirstName + " has been successfully updated to " + user.Role;
            return RedirectToAction("Users");
        }
        // GET: Users/Search


        // Dispose
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
