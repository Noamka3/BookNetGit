using _BookNeT_.Models;
using _BookNeT_.Models.viewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using System.Data.Entity;
using System.Net.Mail;
using System.Net;
using _BookNeT_.Models.BookUser;

namespace BookNeT_.Controllers
{
    public class ProfileController : Controller
    {
        BookNeT_projectEntities db = new BookNeT_projectEntities();
        public ActionResult Tools()
        {
            ViewBag.Title = "כלים שימושיים";
            return View();
        }

        [HttpGet]

        public ActionResult Details()
        {
            // בדוק האם המשתמש מחובר על ידי בדיקת Session
            if (Session["Email"] == null)
            {
                return RedirectToAction("Login", "Account"); // אם לא מחובר, הפנה להתחברות
            }

            string email = Session["Email"].ToString();
            var user = db.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                return HttpNotFound("User not found.");
            }

            return View(user); // שליחת המידע ל-View
        }

        [HttpPost]
        public ActionResult SaveProfile(string email, string phone, int? age, string firstName, string lastName)
        {
            try
            {
                // בדיקת Session
                if (Session["UserID"] == null)
                {
                    return Json(new { success = false, message = "Session expired. Please log in again." });
                }

                var userId = Session["UserID"].ToString();
                var user = db.Users.FirstOrDefault(u => u.UserID.ToString() == userId);

                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                // בדיקת אם המספר טלפון קיים כבר במסד הנתונים
                if (!string.IsNullOrEmpty(phone) && Regex.IsMatch(phone, @"^\d{10}$"))
                {
                    var phoneExists = db.Users.Any(u => u.Phone == phone && u.UserID.ToString() != userId);
                    if (phoneExists)
                    {
                        return Json(new { success = false, message = "Phone number already exists." });
                    }
                    user.Phone = phone;
                }
                else
                {
                    return Json(new { success = false, message = "Phone number must be exactly 10 digits." });
                }


                // בדיקת אם המייל קיים כבר במסד הנתונים
                if (!string.IsNullOrEmpty(email) && IsValidEmail(email))
                {
                    var emailExists = db.Users.Any(u => u.Email == email && u.UserID.ToString() != userId);
                    if (emailExists)
                    {
                        return Json(new { success = false, message = "Email already exists." });
                    }
                    user.Email = email;
                }

                // בדיקת גיל
                if (age.HasValue && age > 10)
                {
                    user.Age = age;
                }

                if (!string.IsNullOrEmpty(firstName))
                {
                    Session["FirstName"] = firstName;
                    user.FirstName = firstName;
                }

                if (!string.IsNullOrEmpty(lastName))
                {
                    user.LastName = lastName;
                }
                db.SaveChanges();


                return Json(new
                {
                    success = true,
                    message = "Profile updated successfully.",
                    phone = user.Phone,
                    age = user.Age,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        // פונקציית בדיקת תקינות של מייל
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            // ביטוי רגולרי לבדוק האם המייל תקין
            var emailRegex = new Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$");
            return emailRegex.IsMatch(email);
        }



        [HttpPost]
        public ActionResult UploadProfileImage(HttpPostedFileBase ProfileImage)
        {
            try
            {
                // בדיקה אם ה-Session קיים
                if (Session["UserID"] == null)
                {
                    Debug.WriteLine("Session expired.");
                    return Json(new { success = false, message = "Session expired." });
                }

                // השגת ה-UserID מה-Session
                var userId = Session["UserID"].ToString();

                // שליפת המשתמש מה-Database
                var user = db.Users.FirstOrDefault(u => u.UserID.ToString() == userId);
                if (user == null)
                {
                    Debug.WriteLine("User not found.");
                    return Json(new { success = false, message = "User not found." });
                }
                // בדיקה האם קובץ התמונה הועלה
                if (ProfileImage != null && ProfileImage.ContentLength > 0)
                {
                    // העלאת התמונה ושמירת ה-URL
                    var imageUrl = UploadImageAndSaveToDatabase(ProfileImage);

                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        // עדכון השדה ProfileImageUrl עם ה-URL שנוצר
                        user.ImageUrl = imageUrl;

                        try
                        {
                            db.SaveChanges();
                            Debug.WriteLine("Image URL saved to database.");
                            return Json(new { success = true, message = "Profile image updated successfully." });
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error saving changes to database: {ex.Message}");
                            return Json(new { success = false, message = $"Database save error: {ex.Message}" });
                        }
                    }
                }

                Debug.WriteLine("Failed to upload image.");
                return Json(new { success = false, message = "No valid image uploaded." });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error: {ex.Message}");
                return Json(new { success = false, message = $"Unexpected error: {ex.Message}" });
            }
        }




        private string UploadImageAndSaveToDatabase(HttpPostedFileBase image)
        {
            try
            {
                // יצירת שם ייחודי לתמונה
                var newFileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";

                // הגדרת נתיב לשמירת הקובץ
                var savePath = Server.MapPath($"~/Images/{newFileName}");

                // שמירת התמונה במיקום הפיזי
                image.SaveAs(savePath);

                // יצירת URL לתמונה
                var imageUrl = $"/Images/{newFileName}";

                Debug.WriteLine($"Image saved successfully at: {imageUrl}");
                return imageUrl;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving image to server: {ex.Message}");
                return string.Empty;
            }
        }


        [HttpGet]
        public ActionResult Support()
        {
            // בדוק האם המשתמש מחובר על ידי בדיקת Session
            if (Session["Email"] == null)
            {
                return RedirectToAction("Login", "Account"); // אם לא מחובר, הפנה להתחברות
            }

            string email = Session["Email"].ToString();
            var user = db.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                return HttpNotFound("User not found.");
            }

            return View(user); // שליחת המידע ל-View

        }
        [HttpPost]
        public ActionResult SubmitSupportRequest(string subject, string message)
        {
            try
            {
                // בדיקה האם יש מידע מהטופס
                if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(subject))
                {
                    return Json(new { success = false, message = "You must select a subject and enter a message to submit." });
                }

                // בדיקה אם נבחר נושא או נכתבה הודעה רק לשם מניעת שליחת מיילים מיותרים
                if (subject == "Select a Subject..." || string.IsNullOrWhiteSpace(message))
                {
                    return Json(new { success = false, message = "Please select a valid subject and write a meaningful message." });
                }

                string userEmail = Session["Email"]?.ToString();
                string userId = Session["UserID"]?.ToString();
                string userFullName = $"{Session["FirstName"]} {Session["LastName"]}";

                // בדיקת אם יש מידע על המשתמש
                if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "Session expired or user data unavailable." });
                }

                // שליחת המייל
                SendSupportEmail(userEmail, userId, userFullName, subject, message);

                return Json(new { success = true, message = "Your request has been sent successfully to support." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }



        private void SendSupportEmail(string userEmail, string userId, string fullName, string subject, string messageContent)
        {
            var supportEmail = "Noamkadosh4444@gmail.com";
            var supportPassword = "rvxq ciqx mczq oncb"; // המפתח האפליקציה שלך

            try
            {
                using (var smtpClient = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtpClient.EnableSsl = true;
                    smtpClient.Credentials = new NetworkCredential(supportEmail, supportPassword);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(supportEmail, "BookNeT Support"),
                        Subject = $"Support Request: {subject} from {fullName}",
                        Body = $"User Full Name: {fullName}\nUser Email: {userEmail}\nUser ID: {userId}\n\nSubject: {subject}\nMessage Content:\n{messageContent}",
                        IsBodyHtml = false
                    };

                    mailMessage.To.Add("support@example.com"); // תחליף את הכתובת בכתובת התמיכה שצריכה לקבל את המייל

                    smtpClient.Send(mailMessage);
                }
            }
            catch (Exception ex)
            {
                // הדפסת השגיאה למקרה הצורך
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }
        public ActionResult Books()
        {
            ViewBag.Title = "User Details";
            return View();
        }

        [HttpGet]
        public ActionResult PurchasedBooks()
        {
            if (Session["Email"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string email = Session["Email"].ToString();

            var user = db.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return HttpNotFound("User not found.");
            }

            // שאילתת LINQ והמרת המידע בצד הלקוח
            var purchasedBooks = db.Purchases
                .Include(p => p.Books) // מבצע eager loading של הטבלת Books
                .Where(p => p.UserID == user.UserID)
                .AsEnumerable() // מעבר ל-Enumerable כדי לעבוד בצד הלקוח
                .Select(p => new PurchasedBookViewModel
                {
                    ImageUrl = p.Books.ImageUrl, // עכשיו עובד בצד הלקוח
                    PurchaseID = p.PurchaseID,
                    BookID = p.BookID,
                    Title = p.Books.Title,
                    Author = p.Books.Author,
                    PurchaseDate = p.PurchaseDate
                })
                .ToList();

            return View(purchasedBooks);
        }
        [HttpGet]
        public ActionResult BorrowingBooks()
        {
            if (Session["Email"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string email = Session["Email"].ToString();

            var user = db.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return HttpNotFound("User not found.");
            }

            // שאילתת LINQ והמרת המידע בצד הלקוח
            var BorrowBooks = db.Borrowing
                .Include(p => p.Books) // מבצע eager loading של הטבלת Books
                .Where(p => p.UserID == user.UserID)
                .AsEnumerable() // מעבר ל-Enumerable כדי לעבוד בצד הלקוח
                .Select(p => new BorrowingBookViewModel
                {
                    ImageUrl = p.Books.ImageUrl, // עכשיו עובד בצד הלקוח
                    BorrowingID = p.BorrowID,
                    BookID = p.BookID,
                    Title = p.Books.Title,
                    Author = p.Books.Author,
                    BorrowDate = p.BorrowDate,
                    DueDate = p.DueDate
                })
                .ToList();

            return View(BorrowBooks);
        }




    }
}