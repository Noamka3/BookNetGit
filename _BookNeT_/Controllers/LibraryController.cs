using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using _BookNeT_.Models;
using System.Collections.Generic;

namespace _BookNeT_.Controllers
{
    public class LibraryController : Controller
    {
        private readonly BookNeT_projectEntities db = new BookNeT_projectEntities();

        public ActionResult Index(string searchTerm, string sortBy, string author, string genre, 
            decimal? minPrice, decimal? maxPrice, bool? isBorrowable, 
            bool? showDiscounted, int? publicationYear)
        {
            var books = db.Books.AsQueryable();

            // search book in the search field
            if (!string.IsNullOrEmpty(searchTerm))
            {
                books = books.Where(b => b.Title.Contains(searchTerm));
            }
            
            //filtering
            if (!string.IsNullOrEmpty(author))
            { books = books.Where(b => b.Author.Contains(author)); }

            if (!string.IsNullOrEmpty(genre))
            { books = books.Where(b => b.Genre == genre); }

            if (minPrice.HasValue)
            { books = books.Where(b => b.PurchasePrice >= minPrice.Value); }

            if (maxPrice.HasValue)
            { books = books.Where(b => b.PurchasePrice <= maxPrice.Value); }

            if (isBorrowable.HasValue)
            { books = books.Where(b => b.IsBorrowable == isBorrowable.Value); }

            if (showDiscounted.HasValue && showDiscounted.Value)
            {
                var currentDate = DateTime.Now;
                books = books.Where(b => b.IsDiscounted == true && 
                                       b.DiscountEndDate.HasValue && 
                                       b.DiscountEndDate.Value >= currentDate);
            }

            if (publicationYear.HasValue)
            { books = books.Where(b => b.YearOfPublication == publicationYear.Value); }

            //sorting
            switch (sortBy)
            {
                case "PriceAsc":
                    books = books.OrderBy(b => b.PurchasePrice); //Sort price from lowest to highest
                    break;
                case "PriceDesc":
                    books = books.OrderByDescending(b => b.PurchasePrice); //Sort price from highest to lowest
                    break;
                
                //still not available - to fix it with shopping of users
                case "MostPurchased":
                    books = books.OrderByDescending(b => b.Purchases.Count); 
                    break;
                case "Genre":
                    books = books.OrderBy(b => b.Genre);
                    break;
                case "YearDesc":
                    books = books.OrderByDescending(b => b.YearOfPublication); //sort year from oldest to newest
                    break;
                case "YearAsc":
                    books = books.OrderBy(b => b.YearOfPublication);//sort year from oldest to newest
                    break;
                default:
                    books = books.OrderByDescending(b => b.BookID); //defult - sort by bookID
                    break;
            }

            ViewBag.Authors = db.Books
                .Where(b => b.Author != null)
                .Select(b => b.Author)
                .Distinct()
                .ToList();

            ViewBag.Genres = db.Books
                .Where(b => b.Genre != null)
                .Select(b => b.Genre)
                .Distinct()
                .ToList();

            var maxYear = db.Books.Max(b => b.YearOfPublication ?? DateTime.Now.Year);
            var minYear = db.Books.Min(b => b.YearOfPublication ?? DateTime.Now.Year - 100);

            ViewBag.MaxYear = maxYear;
            ViewBag.MinYear = minYear;

            return View(books.ToList());
        }

        public ActionResult Details(int id)
        {
            var book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            
            var reviews = db.ServiceFeedback
                .Where(r => r.BookID == id)
                .OrderByDescending(r => r.FeedbackDate)
                .ToList();
        
            ViewBag.Reviews = reviews;
    
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]  
        public ActionResult AddReview(ServiceFeedback feedback)
        {
            if (ModelState.IsValid)
            {
                feedback.FeedbackDate = DateTime.Now;
                db.ServiceFeedback.Add(feedback);

                try
                {
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Your review has been added successfully!";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "There was an error adding your review.";
                }

                return RedirectToAction("Details", new { id = feedback.BookID });
            }

            return RedirectToAction("Details", new { id = feedback.BookID });
        }

        
        
        // cleaning the resources
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