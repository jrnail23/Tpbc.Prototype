using System.Linq;
using System.Web.Mvc;
using Tpbc.Web.Application.DomainModel;
using Tpbc.Web.Application.DomainModel.Impl;
using Tpbc.Web.Areas.Admin.Models;

namespace Tpbc.Web.Areas.Admin.Controllers
{
    public class MembersController : Controller
    {
        private readonly IMemberRepository _members;

        public MembersController() : this(new InMemoryMemberRepository())
        {
        }

        public MembersController(IMemberRepository members)
        {
            _members = members;
        }

        // GET: Admin/Members
        public ActionResult Index()
        {
            var members = _members.Select(m => new MemberModel { UserName = m.UserName, FullName = m.FullName });
            return View(members);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View(new MemberModel());
        }

        [HttpPost]
        public ActionResult Create(MemberModel member)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            _members.Add(new Member(member.UserName, member.FullName));

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            return View(GetMemberByUserName(id));
        }

        [HttpPost]
        public ActionResult Edit(string id, MemberModel member)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var model = _members.GetByUserName(id);

            model.FullName = member.FullName;

            return RedirectToAction("Details", routeValues: new { id });
        }

        [HttpGet]
        public ActionResult Details(string id)
        {
            return View(GetMemberByUserName(id));
        }

        private MemberModel GetMemberByUserName(string userName)
        {
            var model = _members.GetByUserName(userName);
            return new MemberModel { FullName = model.FullName, UserName = model.UserName };
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            return View(GetMemberByUserName(id));
        }

        [HttpPost]
        public ActionResult DoDelete(string id)
        {
            var member = _members.GetByUserName(id);
            _members.Remove(member);
            return RedirectToAction("Index");
        }
    }
}