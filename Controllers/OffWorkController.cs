﻿using CooperativeOffice.Entity;
using CooperativeOffice.Models;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Web.Mvc;

namespace CooperativeOffice.Controllers
{
    public class OffWorkController : Controller
    {
        /// <summary>
        /// 请假填写界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 请假显示界面
        /// </summary>
        /// <param name="code">审批编号</param>
        /// <param name="euid">当前用户编号</param>
        /// <param name="returnurl">上级路径</param>
        /// <param name="key">显示类型(0、默认,1、未处理,2、已处理,3、抄送)</param>
        /// <returns></returns>
        public ActionResult Show(string code,string euid,string returnurl, int key = 0)
        {
            ViewBag.approvalcode = code;
            ViewBag.euid = euid;
            ViewBag.key = key;
            ViewBag.returnurl = returnurl;
            if (key == 1) {
                new DBopreator().isRead(code, euid);
            }
            var model = new DBopreator().GetOffworkById(code);
            ViewData["content"] = model;
            if (model != null)
            {
                ViewBag.Tilte = model.title;
            }
            else {
                ViewBag.title = "请假申请";
            }
            return View();
        }
        /// <summary>
        /// 获取联系人列表
        /// </summary>
        /// <param name="orgid">企业编号</param>
        /// <param name="key">关键字</param>
        /// <returns></returns>
        public JsonResult GetMemberList(string orgid,string key) {
            return Json(new DBopreator().GetMemberList(orgid,key), JsonRequestBehavior.AllowGet);
        }
        
        /// <summary>
        /// 获取请假类型
        /// </summary>
        /// <returns></returns>
        public JsonResult GetOffWorkType()
        {
            return Json(new DBopreator().GetOffWorkType(), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 获取指定的请假内容
        /// </summary>
        /// <returns></returns>
        public JsonResult GetOffworkById(string id) {
            return Json(new DBopreator().GetOffworkById(id), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 获取请假草稿
        /// </summary>
        /// <param name="orgeuid">企业编号</param>
        /// <param name="euid">申请人编号</param>
        /// <returns></returns>
        public JsonResult GetDraft(string orgeuid, string euid)
        {
            return Json(new DBopreator().GetOffworkDraft(orgeuid, euid), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 保存审批操作
        /// </summary>
        /// <param name="id">审批单编号</param>
        /// <param name="opinion">意见</param>
        /// <param name="key">同意与否</param>
        /// <returns></returns>
        public JsonResult ProcessApproval(ApprovalProcessModels param) {
            return Json(new DBopreator().ProcessApproval(param));
        }
        /// <summary>
        /// 设置已读状态
        /// </summary>
        /// <param name="approvalcode"></param>
        /// <param name="euid"></param>
        /// <returns></returns>
        public JsonResult SetIsRead(string approvalcode, string euid)
        {
            return Json(new DBopreator().isRead(approvalcode, euid));
        }
        /// <summary>
        /// 保存请假内容
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public JsonResult Save(OffWorkModels param) {
            return Json(new DBopreator().SaveOffWork(param));
        }
        /// <summary>
        /// 保存请假草稿
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public JsonResult SaveDraft(OffWorkModels param)
        {
            return Json(new DBopreator().SaveOffWorkDraft(param));
        }
        /// <summary>
        /// 发送催办通知
        /// </summary>
        /// <param name="receverid">接收人编号</param>
        /// <param name="sendername">发送者姓名</param>
        /// <param name="noticetitle">通知内容</param>
        /// <returns></returns>
        public JsonResult Sendmsg(string receverid, string sendername, string noticetitle)
        {
            return Json(new DBopreator().Send(receverid, sendername, noticetitle));
        }
    }
}