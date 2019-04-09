using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using prjShoppingCar.Models;

namespace prjShoppingCar.Controllers
{
    public class HomeController : Controller
    {
       
        dbShoppingCarEntities db = new dbShoppingCarEntities();
       
        public ActionResult Index()
        {
            var products = db.tProduct.ToList();
            if (Session["Member"] == null)
            {
                return View("Index", "_Layout", products);
            }
            return View("Index", "_LayoutMember", products);
        }
        
        public ActionResult Login()
        {
            return View();
        }
        
        [HttpPost]
        public ActionResult Login(string fUserId, string fPwd)
        {
            var member = db.tMember
                .Where(m => m.fUserId == fUserId && m.fPwd == fPwd)
                .FirstOrDefault();
            if (member == null)
            {
                ViewBag.Message = "帳密錯誤，登入失敗";
                return View();
            }
            Session["WelCome"] = member.fName + "歡迎光臨";
            Session["Member"] = member;
            return RedirectToAction("Index");
        }
        
        public ActionResult Register()
        {
            return View();
        }
      
        [HttpPost]
        public ActionResult Register(tMember pMember)
        {
            if (ModelState.IsValid == false)
            {
                return View();
            }
            var member = db.tMember
                .Where(m => m.fUserId == pMember.fUserId)
                .FirstOrDefault();
            if (member == null)
            {
                db.tMember.Add(pMember);
                db.SaveChanges();
                return RedirectToAction("Login");
            }
            ViewBag.Message = "此帳號己有人使用，註冊失敗";
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear(); 
            return RedirectToAction("Index");
        }

        public ActionResult ShoppingCar()
        {
            string fUserId = (Session["Member"] as tMember).fUserId;
            var orderDetails = db.tOrderDetail.Where
                (m => m.fUserId == fUserId && m.fIsApproved == "否")
                .ToList();
            return View("ShoppingCar", "_LayoutMember", orderDetails);
        }

        public ActionResult AddCar(string fPId)
        {
            string fUserId = (Session["Member"] as tMember).fUserId;
            var currentCar = db.tOrderDetail
                .Where(m => m.fPId == fPId && m.fIsApproved == "否" && m.fUserId == fUserId)
                .FirstOrDefault();
            if (currentCar == null)
            {
                var product = db.tProduct.Where(m => m.fPId == fPId).FirstOrDefault();
                tOrderDetail orderDetail = new tOrderDetail();
                orderDetail.fUserId = fUserId;
                orderDetail.fPId = product.fPId;
                orderDetail.fName = product.fName;
                orderDetail.fPrice = product.fPrice;
                orderDetail.fQty = 1;
                orderDetail.fIsApproved = "否";
                db.tOrderDetail.Add(orderDetail);
            }
            else
            {
                currentCar.fQty += 1;
            }
            db.SaveChanges();
            return RedirectToAction("ShoppingCar");
        }

        public ActionResult DeleteCar(int fId)
        {
            var orderDetail = db.tOrderDetail.Where
                (m => m.fId == fId).FirstOrDefault();
            db.tOrderDetail.Remove(orderDetail);
            db.SaveChanges();
            return RedirectToAction("ShoppingCar");
        }

        [HttpPost]
        public ActionResult ShoppingCar(string fReceiver, string fEmail, string fAddress, string fPhone)
        {
            string fUserId = (Session["Member"] as tMember).fUserId;
            string guid = Guid.NewGuid().ToString();
            tOrder order = new tOrder();
            order.fOrderGuid = guid;
            order.fUserId = fUserId;
            order.fReceiver = fReceiver;
            order.fEmail = fEmail;
            order.fAddress = fAddress;
            order.fPhone = fPhone;
            order.fDate = DateTime.Now;
            db.tOrder.Add(order);
            var carList = db.tOrderDetail
                .Where(m => m.fIsApproved == "否" && m.fUserId == fUserId)
                .ToList();
            foreach (var item in carList)
            {
                item.fOrderGuid = guid;
                item.fIsApproved = "是";
            }
            db.SaveChanges();
            return RedirectToAction("OrderList");
        }
        
        public ActionResult OrderList()
        {
            string fUserId = (Session["Member"] as tMember).fUserId;
            var orders = db.tOrder.Where(m => m.fUserId == fUserId)
                .OrderByDescending(m => m.fDate).ToList();
            return View("OrderList", "_LayoutMember", orders);
        }

        public ActionResult OrderDetail(string fOrderGuid)
        {
            var orderDetails = db.tOrderDetail
                .Where(m => m.fOrderGuid == fOrderGuid).ToList();
            return View("OrderDetail", "_LayoutMember", orderDetails);
        }
    }
}