using CooperativeOffice.Entity;
using System.Web.Mvc;

namespace CooperativeOffice.Controllers
{
    public class ApprovalController : Controller
    {
        // GET: Approval
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult StartedWithMe()
        {
            return View();
        }
        public ActionResult ApprovalWithMe()
        {
            return View();
        }
        public ActionResult CopyForMe()
        {
            return View();
        }
        /// <summary>
        /// 获取未处理审批和抄送给我的数量
        /// </summary>
        /// <param name="orgid"></param>
        /// <param name="euid"></param>
        /// <returns></returns>
        public JsonResult GetCount(string orgid, string euid) {
            return Json(new DBopreator().GetCount(orgid, euid), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 获取待我审批数据
        /// </summary>
        /// <param name="orgid">企业编号</param>
        /// <param name="euid">员工编号</param>
        /// <returns></returns>
        public JsonResult GetUndecidedData(string orgid, string euid)
        {
            return Json(new DBopreator().GetUndecidedData(orgid, euid), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 获取待我审批数据
        /// </summary>
        /// <param name="orgid">企业编号</param>
        /// <param name="euid">员工编号</param>
        /// <returns></returns>
        public JsonResult GetProcessedData(string orgid, string euid)
        {
            return Json(new DBopreator().GetProcessedData(orgid, euid), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 获取待我审批数据
        /// </summary>
        /// <param name="orgid">企业编号</param>
        /// <param name="euid">员工编号</param>
        /// <returns></returns>
        public JsonResult GetCopyformeData(string orgid, string euid)
        {
            return Json(new DBopreator().GetCopyformeData(orgid, euid), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 获取待我审批数据
        /// </summary>
        /// <param name="orgid">企业编号</param>
        /// <param name="euid">员工编号</param>
        /// <returns></returns>
        public JsonResult GetStartWithMeData(string orgid, string euid)
        {
            return Json(new DBopreator().GetStartWithMeData(orgid, euid), JsonRequestBehavior.AllowGet);
        }
    }
}