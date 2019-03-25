using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Blog.Models;

namespace Blog.Controllers
{
    public class ArticleController : Controller
    {
        // GET: Article
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        // GET: Articles/List
        public ActionResult List()
        {
            using (var db = new BlogDbContext())
            {
                var articles = db.Articles.Include(a => a.Author).ToList();

                return View(articles);
            }
        }

        // GET Article/Details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new BlogDbContext())
            {
                var article = db.Articles.Where(a => a.Id == id).Include(a => a.Author).SingleOrDefault();

                if (article == null)
                {
                    //return HttpNotFound($"Cannot find article with ID {id}");
                    return new HttpNotFoundResult($"Cannot find article with ID {id}");
                }

                return View(article);
            }
        }

        //GET Article/Create
        [HttpGet]
        [Authorize]
        public ActionResult Create()
        {
            return View();
        }

        //POST Article/Create
        [HttpPost]
        [Authorize]
        public ActionResult Create(Article article)
        {
            if (!ModelState.IsValid)
            {
                return View(article);
            }

            using (var db = new BlogDbContext())
            {
                //Get author from DB
                var author = db.Users.SingleOrDefault(u => u.UserName == this.User.Identity.Name);

                //Check if author exists in DB
                if (author == null)
                {
                    return View(article);
                }

                //Get author ID
                var authorId = author.Id;

                //Set article author
                article.AuthorId = authorId;

                //Save article into DB
                db.Articles.Add(article);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
        }

        //GET Article/Delete/{Id}
        [HttpGet]
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new BlogDbContext())
            {
                var article = db.Articles.SingleOrDefault(a => a.Id == id);

                if (article == null)
                {
                    return new HttpNotFoundResult($"Cannot find article with ID {id}");
                }

                if (!IsUserAuthorizedToEdit(article))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }

                return View(article);
            }
        }

        //POST Article/Delete/{Id}
        [HttpPost]
        [Authorize]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new BlogDbContext())
            {
                var article = db.Articles.SingleOrDefault(a => a.Id == id);

                if (article == null)
                {
                    return new HttpNotFoundResult($"Cannot find article with ID {id}");
                }

                db.Articles.Remove(article);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
        }

        //GET Article/Edit/{Id}
        [HttpGet]
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new BlogDbContext())
            {
                var article = db.Articles.SingleOrDefault(a => a.Id == id);

                if (article == null)
                {
                    return new HttpNotFoundResult($"Cannot find article with ID {id}");
                }

                if (!IsUserAuthorizedToEdit(article))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }

                var model = new ArticleViewModel
                {
                    Id = article.Id,
                    Title = article.Title,
                    Content = article.Content
                };

                return View(model);
            }
        }

        //GET Article/Edit/{Id}
        [HttpPost]
        [Authorize]
        public ActionResult Edit(ArticleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var db = new BlogDbContext())
            {
                var article = db.Articles.SingleOrDefault(a => a.Id == model.Id);

                if (article == null)
                {
                    return new HttpNotFoundResult($"Cannot find article with ID {model.Id}");
                }

                article.Title = model.Title;
                article.Content = model.Content;

                db.Entry(article).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
        }

        public bool IsUserAuthorizedToEdit(Article article)
        {
            bool isAdmin = this.User.IsInRole("Admin");
            bool isAuthor = article.IsAuthor(this.User.Identity.Name);

            return isAdmin || isAuthor;
        }
    }
}