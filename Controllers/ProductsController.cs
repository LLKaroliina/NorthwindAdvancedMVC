using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using NorthwindAdvancedMVC.Models;
using PagedList;

namespace NorthwindAdvancedMVC.Controllers
{
    public class ProductsController : Controller
    {
        private NorthwindOriginalEntities db = new NorthwindOriginalEntities();

        // GET: Products
        public ActionResult Index(string searchString1, string currentFilter1, string sortOrder, string ProductCategory, int? page, int? pagesize)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.ProductNameSortParm = String.IsNullOrEmpty(sortOrder) ? "productname_desc" : "";
            ViewBag.UnitPriceSortParm = sortOrder == "UnitPrice" ? "UnitPrice_desc" : "UnitPrice";

            if (searchString1 != null)
            {
                page = 1;
            }
            else
            {
                searchString1 = currentFilter1;
            }

            ViewBag.currentFilter1=searchString1;

            var tuotteet = db.Products.Include(p => p.Categories).Include(p => p.Suppliers);
            if (!String.IsNullOrEmpty(searchString1)) //searchString1 on tuote mitä haetaan
            {
                tuotteet = tuotteet.Where(p => p.ProductName.Contains(searchString1)|| p.Suppliers.CompanyName.Contains(searchString1));
            }

            //Tehdään tuotehaku, jos se on asetettu käyttöliittymässä
            //Tuotehakuun tehdään rajaus tuotekategorialla
            if(!String.IsNullOrEmpty(ProductCategory) && (ProductCategory != "0"))
            {
                int para = int.Parse(ProductCategory);
                tuotteet = tuotteet.Where(p => p.CategoryID == para);
            }
            //Yhdistelmähaku: sekä suodatus että lajittelu
            if (!String.IsNullOrEmpty(searchString1)) //Jos hakufiltteri on käytössä,
                //niin käytetään sitä ja sen lisäksi lajitellaan tulokset
            {
                switch (sortOrder)
                {
                    case "productname_desc":
                        tuotteet = tuotteet.Where(p =>
                        p.ProductName.Contains(searchString1)).OrderByDescending(p => p.ProductName);
                        break;
                    case "UnitPrice":
                        tuotteet = tuotteet.Where(p =>
                        p.ProductName.Contains(searchString1)).OrderBy(p => p.UnitPrice);
                        break;
                    case "UnitPrice_desc":
                        tuotteet = tuotteet.Where(p =>
                        p.ProductName.Contains(searchString1)).OrderByDescending(p => p.UnitPrice);
                        break;
                    default:
                        tuotteet = tuotteet.Where(p =>
                        p.ProductName.Contains(searchString1)).OrderBy(p => p.ProductName);
                        break;
                }
                
            }
            else if (!String.IsNullOrEmpty(ProductCategory) && (ProductCategory!="0"))//ei ole tyhjä tai nolla
            {

           
            int para = int.Parse(ProductCategory); //määritellään para muuttuja
            switch (sortOrder)
            {
                case "productname_desc":
                    tuotteet = tuotteet.Where(p =>
                    p.CategoryID == para).OrderByDescending(p => p.ProductName);
                    break;
                case "UnitPrice":
                    tuotteet = tuotteet.Where(p =>
                    p.CategoryID == para).OrderBy(p => p.UnitPrice);
                    break;
                case "UnitPrice_desc":
                    tuotteet = tuotteet.Where(p =>
                    p.CategoryID == para).OrderByDescending(p => p.UnitPrice);
                    break;
                default:
                    tuotteet = tuotteet.Where(p =>
                    p.CategoryID == para).OrderBy(p => p.ProductName);
                    break;
            }
            }



            else
            { //Tässä hakufiltteri EI OLE käytössä, joten lajitellaan koko tulosjoukko ilman suodatuksia
                switch (sortOrder)
                {
                    case "productname_desc":
                        tuotteet = tuotteet.OrderByDescending(p => p.ProductName);
                        break;
                    case "UnitPrice":
                        tuotteet = tuotteet.OrderBy(p => p.UnitPrice);
                        break;
                    case "UnitPrice_desc":
                        tuotteet = tuotteet.OrderByDescending(p => p.UnitPrice);
                        break;
                    default:
                        tuotteet = tuotteet.OrderBy(p => p.ProductName);
                        break;
                }
            };

            List<Categories>lstCategories = new List<Categories>();

            var categoryList = from cat in db.Categories //tämä pitää pudotusvalikon tyhjänä
                               select cat; //cat aliasointu categories
            Categories tyhjaCategory = new Categories();
            tyhjaCategory.CategoryID = 0;
            tyhjaCategory.CategoryName = "";
            tyhjaCategory.CategoryIDCategoryName = ""; //edustaa kahden datatiedon yhdistelmää
            lstCategories.Add(tyhjaCategory);

            foreach (Categories category in categoryList) //listaaa pudotusvalikon kategoriat
            {
                Categories yksiCategory = new Categories();
                yksiCategory.CategoryID = category.CategoryID;
                yksiCategory.CategoryName = category.CategoryName;
                yksiCategory.CategoryIDCategoryName= category.CategoryID.ToString() + " - " + category.CategoryName;
                lstCategories.Add(yksiCategory);
            }
            ViewBag.CategoryID = new SelectList(lstCategories, "CategoryID", "CategoryIDCategoryName", ProductCategory);


            int pageSize = (pagesize ?? 10); //?? muuttaa int muotoon
            int pageNumber = (page ?? 1);

            return View(tuotteet.ToPagedList(pageNumber, pageSize));//Take() valitsee niin monta riviä kun on määritelty
        }
        

        


    // GET: Products/Details/5
    public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Products products = db.Products.Find(id);
            if (products == null)
            {
                return HttpNotFound();
            }
            return View(products);
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            ViewBag.SupplierID = new SelectList(db.Suppliers, "SupplierID", "CompanyName");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductID,ProductName,SupplierID,CategoryID,QuantityPerUnit,UnitPrice,UnitsInStock,UnitsOnOrder,ReorderLevel,Discontinued,ImageLink")] Products products)
        {
            if (ModelState.IsValid)
            {
                db.Products.Add(products);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", products.CategoryID);
            ViewBag.SupplierID = new SelectList(db.Suppliers, "SupplierID", "CompanyName", products.SupplierID);
            return View(products);
        }

        // GET: Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Products products = db.Products.Find(id);
            if (products == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", products.CategoryID);
            ViewBag.SupplierID = new SelectList(db.Suppliers, "SupplierID", "CompanyName", products.SupplierID);
            return View(products);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductID,ProductName,SupplierID,CategoryID,QuantityPerUnit,UnitPrice,UnitsInStock,UnitsOnOrder,ReorderLevel,Discontinued,ImageLink")] Products products)
        {
            if (ModelState.IsValid)
            {
                db.Entry(products).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", products.CategoryID);
            ViewBag.SupplierID = new SelectList(db.Suppliers, "SupplierID", "CompanyName", products.SupplierID);
            return View(products);
        }

        // GET: Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Products products = db.Products.Find(id);
            if (products == null)
            {
                return HttpNotFound();
            }
            return View(products);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Products products = db.Products.Find(id);
            db.Products.Remove(products);
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
