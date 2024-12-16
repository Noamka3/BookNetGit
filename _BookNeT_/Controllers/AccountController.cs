using System;
using System.Linq;
using System.Web.Mvc;
using _BookNeT_.Models;
using _BookNeT_.Models.viewModels;
using System.Net.Mail;
using System.Web;

namespace _BookNeT_.Controllers
{
    public class AccountController : Controller
    {
        BookNeT_projectEntities db = new BookNeT_projectEntities();

        // GET: Account
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // בדוק אם המייל כבר קיים בבסיס הנתונים
                var existingUser = db.Users.SingleOrDefault(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "The email already exists in the system.");
                    return View(model);
                }

                if (string.IsNullOrEmpty(model.Password))
                {
                    ModelState.AddModelError("Password", "Password is required.");
                    return View(model);
                }

                // יצירת משתמש חדש
                var user = new Users
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Phone = model.Phone,
                    Role = "User",
                    Age = model.Age,
                    RegistrationDate = DateTime.Now,
                    Email = model.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(model.Password)
                };

                try
                {
                    // הוסף את המשתמש לבסיס הנתונים
                    db.Users.Add(user);
                    db.SaveChanges();

                    // הפנה לדף הבית
                    return Redirect("/Home/Index");
                }
                catch
                {
                    // במידה וקרתה שגיאה, הוסף הודעת שגיאה ל-ModelState
                    ModelState.AddModelError("", "An error occurred during registration. Please try again later");
                    return View(model);
                }

            }

            // אם המודל לא תקין, החזר את המודל לתצוגה עם השגיאות
            return View(model);
        }

        public ActionResult Login()
        {
            return View();
        }


        // טיפול בהגשה של טופס ההתחברות
        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // חפש את המשתמש לפי כתובת האימייל
                var user = db.Users.FirstOrDefault(u => u.Email == model.Email);

                if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
                {

                    Session["Email"] = user.Email;
                    Session["UserID"] = user.UserID;
                    Session["Role"] = user.Role;
                    Session["FirstName"] = user.FirstName;
                    if (model.RememberMe)
                    {
                        HttpCookie authCookie = new HttpCookie("UserLogin")
                        {
                            Values = { ["Email"] = user.Email, ["Password"] = user.Password },
                            Expires = DateTime.Now.AddDays(7) // תקף למשך שבוע
                        };
                        Response.Cookies.Add(authCookie);
                    }
                    // הפנה את המשתמש לדף הבית
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // במקרה שהמשתמש לא קיים או הסיסמה לא נכונה
                    ModelState.AddModelError("", "Invalid email or password.");
                }
            }

            // אם המודל לא תקין, נשיב את הטופס עם הודעות שגיאה
            return View(model);
        }


        public ActionResult VerifyEmail()
        {
            return View();
        }

        [HttpPost]
        public ActionResult VerifyEmail(VerifyEmailModel model)
        {
            if (ModelState.IsValid)
            {
                // בדיקה אם האימייל קיים בבסיס הנתונים
                var user = db.Users.FirstOrDefault(u => u.Email == model.Email);

                if (user != null)
                {
                    try
                    {
                        // שליחת מייל עם קישור לאיפוס סיסמה
                        SendResetPasswordEmail(model.Email);

                        TempData["SuccessMessage"] = "A password reset email has been sent to the provided email address.";
                        Session["Email"] = model.Email;
                        return RedirectToAction("ChangePassword", "Account");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"An error occurred while sending the email: {ex.Message}");
                    }
                }
                else
                {
                    ModelState.AddModelError("Email", "The provided email does not exist in the system.");
                }
            }

            return View(model);
        }

        private void SendResetPasswordEmail(string email)
        {
            try
            {
                string smtpServer = "smtp.gmail.com";
                int smtpPort = 587;
                string smtpUsername = "Noamkadosh4444@gmail.com"; // המייל שלך ב-Gmail
                string smtpPassword = "rvxq ciqx mczq oncb";   // סיסמת אפליקציה של Gmail

                string resetLink = $"https://yourwebsite.com/Account/ResetPassword?email={email}";

                MailMessage mail = new MailMessage
                {
                    From = new MailAddress(smtpUsername),
                    Subject = "Password Reset - BookNeT",
                    Body = $"<p>Click the following link to reset your password:</p><p><a href='{resetLink}'>Reset Password</a></p>",
                    IsBodyHtml = true
                };
                mail.To.Add(email);

                using (SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort))
                {
                    smtpClient.Credentials = new System.Net.NetworkCredential(smtpUsername, smtpPassword);
                    smtpClient.EnableSsl = true;
                    smtpClient.Send(mail);  // שליחה
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while sending the email: " + ex.Message);

            }
        }
        public ActionResult ChangePassword()
        {

            return View();
        }
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var email = Session["Email"] as string;
                if (string.IsNullOrEmpty(email))
                {
                    // אם האימייל לא קיים, הפנה לדף אחר או הצג שגיאה
                    return RedirectToAction("ErrorPage");
                }
                // בדוק אם הסיסמאות תואמות
                if (model.NewPassword != model.ConfirmNewPassword)
                {
                    ModelState.AddModelError("", "The passwords do not match. Please try again.");

                    return View(model);  // החזר את הדף עם הודעת השגיאה
                }
                try
                {
                    // חפש את המשתמש לפי האימייל
                    var user = db.Users.FirstOrDefault(u => u.Email == email);
                    if (user != null)
                    {
                        user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                        // עדכן את הסיסמה
                       
                        db.SaveChanges();  // שמור את השינויים בבסיס הנתונים

                        TempData["SuccessMessage"] = "The password has been changed successfully!";

                        return RedirectToAction("Login", "Account");  // הפנה למסך הכניסה לאחר השינוי
                    }
                    else
                    {
                        ModelState.AddModelError("", "No user found with this email.");

                    }
                }
                catch (Exception ex)
                {
                    // טיפול בשגיאות במקרה של בעיה עם בסיס הנתונים או השינוי
                    ModelState.AddModelError("", $"An error occurred: {ex.Message}");

                }
            }

            // אם ה-model לא תקין, החזר את הטופס עם הודעות שגיאה
            return View(model);

        }

    }
}