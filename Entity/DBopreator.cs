﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;
using Dapper;
using NLog;
using CooperativeOffice.Models;
using Newtonsoft.Json;
using System.Data;
using Oracle.ManagedDataAccess.Types;
using Newtonsoft.Json.Converters;
using System.Text;

namespace CooperativeOffice.Entity
{
    public class DBopreator
    {
        /// <summary>
        /// 日志实例
        /// </summary>
        protected Logger log = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// 获取未审批的数据
        /// </summary>
        /// <param name="orgid"></param>
        /// <param name="euid"></param>
        /// <returns></returns>
        public List<ListItemModels> GetUndecidedData(string orgid, string euid)
        {
            try
            {
                using (var _conn = OpenConnection())
                {
                    var ConnectionString = ConfigurationManager.ConnectionStrings["BaseDbContext"].ConnectionString;
                    using (var myconn = OpenConnection(ConnectionString))
                    {
                        var basedata = _conn.Query<ListItemModels>("select a.approvalcode,c.approvaltype,c.approvalname,a.applyeuid euid, a.applytitle Title,a.applytime Time from APPROVAL a right join APPROVALER b" +
                            " on a.approvalcode = b.approvalcode right join APPROVAL_TYPE c on a.applytype = c.approvaltype " +
                            "where a.orgeuid = :orgid and b.approvaleuid = :euid and b.approvalresult = 0 and a.applytime > add_months(a.applytime, -3) order by a.applytime desc", new { orgid = orgid, euid = euid }).ToList();
                        for (var i = 0; i < basedata.Count; i++)
                        {
                            var mname = myconn.Query<string>("select name from ec_employee where ORGANIZATIONEUID = :orgid and euid = :euid ", new { orgid = orgid, euid = basedata[i].euid }).FirstOrDefault();
                            basedata[i].Name = mname;
                            basedata[i].ShortName = string.IsNullOrEmpty(mname) ? "" : (mname.Length <= 2 ? mname : mname.Substring(mname.Length - 2));
                            switch (basedata[i].approvaltype)
                            {
                                case 1:
                                    //请假
                                    basedata[i].Detail = _conn.Query<string>("select b.leavename || ' ' || floor(a.leavehours / 24) || '天' || mod(a.leavehours,24) || '小时' from APPROVAL_LEAVE a " +
                                        "join APPROVAL_LEAVE_TYPE b on a.leavetype = b.leavetype where a.approvalcode = :approvalcode",
                                        new { approvalcode = basedata[i].approvalcode }).FirstOrDefault();
                                    break;
                                case 2:
                                    //报销
                                    basedata[i].Detail = _conn.Query<string>("select '报销金额' || expensetotal from APPROVAL_EXPENSE where approvalcode = :approvalcode",
                                        new { approvalcode = basedata[i].approvalcode }).FirstOrDefault();
                                    break;
                                case 3:
                                    //出差
                                    basedata[i].Detail = _conn.Query<string>("select tripreason from APPROVAL_TRIP where approvalcode = :approvalcode",
                                        new { approvalcode = basedata[i].approvalcode }).FirstOrDefault();
                                    break;
                                case 4:
                                    //外出
                                    basedata[i].Detail = _conn.Query<string>("select '时长:' || outhours || '小时,事由:' || outreason from APPROVAL_OUT where approvalcode = :approvalcode",
                                        new { approvalcode = basedata[i].approvalcode }).FirstOrDefault();
                                    break;
                                case 5:
                                    //请示
                                    basedata[i].Detail = _conn.Query<string>("select requestreason from APPROVAL_REQUEST where approvalcode = :approvalcode",
                                        new { approvalcode = basedata[i].approvalcode }).FirstOrDefault();
                                    break;
                                case 6:
                                    //借款
                                    basedata[i].Detail = _conn.Query<string>("select  '借款:' || loan || '元用于:' || loanreson  from APPROVAL_LOAN where approvalcode = :approvalcode",
                                        new { approvalcode = basedata[i].approvalcode }).FirstOrDefault();
                                    break;
                                case 7:
                                    //加班
                                    break;
                            }
                        }
                        return basedata;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "获取未审批的数据时发生异常。");
                return new List<ListItemModels>();
            }
        }
        /// <summary>
        /// 获取已审批的数据
        /// </summary>
        /// <param name="orgid"></param>
        /// <param name="euid"></param>
        /// <returns></returns>
        public List<ListItemModels> GetProcessedData(string orgid, string euid)
        {
            try
            {
                using (var _conn = OpenConnection())
                {
                    var ConnectionString = ConfigurationManager.ConnectionStrings["BaseDbContext"].ConnectionString;
                    using (var myconn = OpenConnection(ConnectionString))
                    {
                        var basedata = _conn.Query<ListItemModels>("select a.approvalcode,c.approvaltype,c.approvalname,a.applyeuid euid,case when a.apply = 3 then '审批中' else '完成' end Status, a.applytitle Title,a.applytime Time,case when b.approvalresult = 1 then '同意' else '不同意' end Result from APPROVAL a right join APPROVALER b" +
                        " on a.approvalcode = b.approvalcode right join APPROVAL_TYPE c on a.applytype = c.approvaltype " +
                        "where a.orgeuid = :orgid and b.approvaleuid = :euid and b.approvalresult <> 0 and a.applytime > add_months(a.applytime, -3) order by a.applytime desc", new { orgid = orgid, euid = euid }).ToList();
                        for (var i = 0; i < basedata.Count; i++)
                        {
                            var mname = myconn.Query<string>("select name from ec_employee where ORGANIZATIONEUID = :orgid and euid = :euid ", new { orgid = orgid, euid = basedata[i].euid }).FirstOrDefault();
                            basedata[i].Name = mname;
                            basedata[i].ShortName = string.IsNullOrEmpty(mname) ? "" : (mname.Length <= 2 ? mname : mname.Substring(mname.Length - 2));
                            switch (basedata[i].approvaltype)
                            {
                                case 1:
                                    //请假
                                    basedata[i].Detail = _conn.Query<string>("select b.leavename || ' ' || floor(a.leavehours / 24) || '天' || mod(a.leavehours,24) || '小时' from APPROVAL_LEAVE a " +
                                        "join APPROVAL_LEAVE_TYPE b on a.leavetype = b.leavetype where a.approvalcode = :approvalcode",
                                        new { approvalcode = basedata[i].approvalcode }).FirstOrDefault();
                                    break;
                                case 2:
                                    //报销
                                    basedata[i].Detail = _conn.Query<string>("select '报销金额' || expensetotal from APPROVAL_EXPENSE where approvalcode = :approvalcode",
                                        new { approvalcode = basedata[i].approvalcode }).FirstOrDefault();
                                    break;
                                case 3:
                                    //出差
                                    basedata[i].Detail = _conn.Query<string>("select tripreason from APPROVAL_TRIP where approvalcode = :approvalcode",
                                        new { approvalcode = basedata[i].approvalcode }).FirstOrDefault();
                                    break;
                                case 4:
                                    //外出
                                    basedata[i].Detail = _conn.Query<string>("select '时长:' || outhours || '小时,事由:' || outreason from APPROVAL_OUT where approvalcode = :approvalcode",
                                        new { approvalcode = basedata[i].approvalcode }).FirstOrDefault();
                                    break;
                                case 5:
                                    //请示
                                    basedata[i].Detail = _conn.Query<string>("select requestreason from APPROVAL_REQUEST where approvalcode = :approvalcode",
                                        new { approvalcode = basedata[i].approvalcode }).FirstOrDefault();
                                    break;
                                case 6:
                                    //借款
                                    basedata[i].Detail = _conn.Query<string>("select  '借款:' || loan || '元用于:' || loanreson  from APPROVAL_LOAN where approvalcode = :approvalcode",
                                        new { approvalcode = basedata[i].approvalcode }).FirstOrDefault();
                                    break;
                                case 7:
                                    //加班
                                    break;
                            }
                        }
                        return basedata;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "获取已审批的数据时发生异常。");
                return new List<ListItemModels>();
            }
        }
        /// <summary>
        /// 获取抄送给我的数据
        /// </summary>
        /// <param name="orgid"></param>
        /// <param name="euid"></param>
        /// <returns></returns>
        public List<ListItemModels> GetCopyformeData(string orgid, string euid)
        {
            try
            {
                using (var _conn = OpenConnection())
                {
                    var ConnectionString = ConfigurationManager.ConnectionStrings["BaseDbContext"].ConnectionString;
                    using (var myconn = OpenConnection(ConnectionString))
                    {
                        var basedata = _conn.Query<ListItemModels>("select a.approvalcode,c.approvaltype,c.approvalname,a.applyeuid euid,a.applytitle Title,a.applytime Time,case when a.apply = 3 then '审批中' else '完成' end Status from APPROVAL a right join APPROVAL_CCLIST b" +
                        " on a.approvalcode = b.approvalcode right join APPROVAL_TYPE c on a.applytype = c.approvaltype " +
                        "where a.orgeuid = :orgid and b.cceuid = :euid and a.applytime > add_months(a.applytime, -3) order by a.applytime desc", new { orgid = orgid, euid = euid }).ToList();
                        for (var i = 0; i < basedata.Count; i++)
                        {
                            var mname = myconn.Query<string>("select name from ec_employee where ORGANIZATIONEUID = :orgid and euid = :euid ", new { orgid = orgid, euid = basedata[i].euid }).FirstOrDefault();
                            basedata[i].Name = mname;
                            basedata[i].ShortName = string.IsNullOrEmpty(mname) ? "" : (mname.Length <= 2 ? mname : mname.Substring(mname.Length - 2));
                            switch (basedata[i].approvaltype)
                            {
                                case 1:
                                    //请假
                                    basedata[i].Detail = _conn.Query<string>("select b.leavename || ' ' || floor(a.leavehours / 24) || '天' || mod(a.leavehours,24) || '小时' from APPROVAL_LEAVE a " +
                                        "join APPROVAL_LEAVE_TYPE b on a.leavetype = b.leavetype where a.approvalcode = :approvalcode",
                                        new { approvalcode = basedata[i].approvalcode }).FirstOrDefault();
                                    break;
                                case 2:
                                    //报销
                                    basedata[i].Detail = _conn.Query<string>("select '报销金额' || expensetotal from APPROVAL_EXPENSE where approvalcode = :approvalcode",
                                        new { approvalcode = basedata[i].approvalcode }).FirstOrDefault();
                                    break;
                                case 3:
                                    //出差
                                    basedata[i].Detail = _conn.Query<string>("select tripreason from APPROVAL_TRIP where approvalcode = :approvalcode",
                                        new { approvalcode = basedata[i].approvalcode }).FirstOrDefault();
                                    break;
                                case 4:
                                    //外出
                                    basedata[i].Detail = _conn.Query<string>("select '时长:' || outhours || '小时,事由:' || outreason from APPROVAL_OUT where approvalcode = :approvalcode",
                                        new { approvalcode = basedata[i].approvalcode }).FirstOrDefault();
                                    break;
                                case 5:
                                    //请示
                                    basedata[i].Detail = _conn.Query<string>("select requestreason from APPROVAL_REQUEST where approvalcode = :approvalcode",
                                        new { approvalcode = basedata[i].approvalcode }).FirstOrDefault();
                                    break;
                                case 6:
                                    //借款
                                    basedata[i].Detail = _conn.Query<string>("select  '借款:' || loan || '元用于:' || loanreson  from APPROVAL_LOAN where approvalcode = :approvalcode",
                                        new { approvalcode = basedata[i].approvalcode }).FirstOrDefault();
                                    break;
                                case 7:
                                    //加班
                                    break;
                            }
                        }
                        return basedata;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "获取抄送给我的数据时发生异常。");
                return new List<ListItemModels>();
            }
        }
        /// <summary>
        /// 获取我发起的数据
        /// </summary>
        /// <param name="orgid"></param>
        /// <param name="euid"></param>
        /// <returns></returns>
        public List<ListItemModels> GetStartWithMeData(string orgid, string euid)
        {
            try
            {
                using (var _conn = OpenConnection())
                {
                    var ConnectionString = ConfigurationManager.ConnectionStrings["BaseDbContext"].ConnectionString;
                    using (var myconn = OpenConnection(ConnectionString))
                    {
                        var mname = myconn.Query<string>("select name from ec_employee where ORGANIZATIONEUID = :orgid and euid = :euid ", new { orgid = orgid, euid = euid }).FirstOrDefault();
                        var basedata = _conn.Query<ListItemModels>("select a.approvalcode,c.approvaltype,c.approvalname,a.applyeuid euid,a.applytitle Title,case when a.apply = 3 then '审批中' else '完成' end Status," +
                            " a.applytime Time from APPROVAL a join APPROVAL_TYPE c on a.applytype = c.approvaltype " +
                            "where a.orgeuid = :orgid and a.applyeuid = :euid and a.applytime > add_months(a.applytime, -3) order by a.applytime desc", new { orgid = orgid, euid = euid }).ToList();
                        for (var i = 0; i < basedata.Count; i++)
                        {
                            basedata[i].Name = mname;
                            basedata[i].ShortName = string.IsNullOrEmpty(mname) ? "" : (mname.Length <= 2 ? mname : mname.Substring(mname.Length - 2));
                            switch (basedata[i].approvaltype)
                            {
                                case 1:
                                    //请假
                                    basedata[i].Detail = _conn.Query<string>("select b.leavename || ' ' || floor(a.leavehours / 24) || '天' || mod(a.leavehours,24) || '小时' from APPROVAL_LEAVE a " +
                                        "join APPROVAL_LEAVE_TYPE b on a.leavetype = b.leavetype where a.approvalcode = :approvalcode",
                                        new { approvalcode = basedata[i].approvalcode }).FirstOrDefault();
                                    break;
                                case 2:
                                    //报销
                                    basedata[i].Detail = _conn.Query<string>("select '报销金额' || expensetotal from APPROVAL_EXPENSE where approvalcode = :approvalcode",
                                        new { approvalcode = basedata[i].approvalcode }).FirstOrDefault();
                                    break;
                                case 3:
                                    //出差
                                    basedata[i].Detail = _conn.Query<string>("select tripreason from APPROVAL_TRIP where approvalcode = :approvalcode",
                                        new { approvalcode = basedata[i].approvalcode }).FirstOrDefault();
                                    break;
                                case 4:
                                    //外出
                                    basedata[i].Detail = _conn.Query<string>("select '时长:' || outhours || '小时,事由:' || outreason from APPROVAL_OUT where approvalcode = :approvalcode",
                                        new { approvalcode = basedata[i].approvalcode }).FirstOrDefault();
                                    break;
                                case 5:
                                    //请示
                                    basedata[i].Detail = _conn.Query<string>("select requestreason from APPROVAL_REQUEST where approvalcode = :approvalcode",
                                        new { approvalcode = basedata[i].approvalcode }).FirstOrDefault();
                                    break;
                                case 6:
                                    //借款
                                    basedata[i].Detail = _conn.Query<string>("select  '借款:' || loan || '元用于:' || loanreson  from APPROVAL_LOAN where approvalcode = :approvalcode",
                                        new { approvalcode = basedata[i].approvalcode }).FirstOrDefault();
                                    break;
                                case 7:
                                    //加班
                                    break;
                            }
                        }
                        return basedata;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "获取我发起的数据时发生异常。");
                return new List<ListItemModels>();
            }
        }
        /// <summary>
        /// 获取企业的联系人列表
        /// </summary>
        /// <param name="orgid">企业编号</param>
        /// <param name="key">关键字</param>
        /// <returns></returns>
        public List<EmployeeModels> GetMemberList(string orgid, string key)
        {
            try
            {
                var ConnectionString = ConfigurationManager.ConnectionStrings["BaseDbContext"].ConnectionString;
                using (var _conn = OpenConnection(ConnectionString))
                {
                    if (!string.IsNullOrEmpty(key))
                    {
                        return _conn.Query<EmployeeModels>("select euid,name,ORGANIZATIONEUID orgid from ec_employee where ORGANIZATIONEUID = :orgid and name like :keywords ", new { orgid = orgid, keywords = "%" + key + "%" }).ToList();
                    }
                    else {
                        return _conn.Query<EmployeeModels>("select euid,name,ORGANIZATIONEUID orgid from ec_employee where ORGANIZATIONEUID = :orgid ", new { orgid = orgid }).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "获取企业的联系人列表时发生异常。");
                return new List<EmployeeModels>();
            }
        }
        /// <summary>
        /// 保存出差内容
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public object SaveTrip(TripModels param)
        {
            using (var _conn = OpenConnection())
            {
                IDbTransaction tran = null;
                try
                {
                    var _com = new OracleCommand();
                    _com.Connection = (OracleConnection)_conn;
                    tran = _conn.BeginTransaction();
                    _com.Transaction = (OracleTransaction)tran;
                    var tempcode = _conn.Query<string>("select LPAD(to_char(SEQAPPROVALCODE.nextval), 6,'0') from dual").FirstOrDefault();
                    var now = DateTime.Now;
                    var year = now.Year.ToString();
                    var month = now.Month.ToString().Length > 1 ? now.Month.ToString() : "0" + now.Month.ToString();
                    var day = now.Day.ToString().Length > 1 ? now.Day.ToString() : "0" + now.Day.ToString();
                    var hour = now.Hour.ToString().Length > 1 ? now.Hour.ToString() : "0" + now.Hour.ToString();
                    var minute = now.Minute.ToString().Length > 1 ? now.Minute.ToString() : "0" + now.Minute.ToString();
                    //设置审批编号
                    param.approvalcode = year + month + day + hour + minute + tempcode;

                    //第一步:将附件图片入库
                    if (param.imgs != null)
                    {
                        for (var i = 0; i < param.imgs.Count; i++)
                        {
                            _com.CommandText = "insert into APPROVAL_PIC (approvalcode,picseq,pic) values (:approvalcode,:picseq,:pic)";
                            var body = param.imgs[i].body.Substring(param.imgs[i].body.IndexOf("base64,") + 7);
                            byte[] blob = Convert.FromBase64String(body);
                            var filelength = blob.Length;
                            _com.Parameters.Clear();
                            _com.Parameters.AddRange(new OracleParameter[]{
                                    new OracleParameter("approvalcode", param.approvalcode),
                                    new OracleParameter("picseq", i),
                                    new OracleParameter("pic",OracleDbType.Blob,filelength,blob,System.Data.ParameterDirection.Input),
                                });
                            if (_conn.State == System.Data.ConnectionState.Closed)
                            {
                                _conn.Open();
                            }
                            _com.ExecuteNonQuery();
                        }
                    }
                    //第二步：将明细入库
                    if (param.detail != null)
                    {
                        for (var i = 0; i < param.detail.Count; i++)
                        {
                            _conn.Execute("insert into APPROVAL_TRIP_DETAILS (orgeuid,approvalcode,tripseq,tripsite,tripstarttime,tripendtime) " +
                                "values (:orgeuid,:approvalcode,:tripseq,:tripsite,to_date(:tripstarttime,'yyyy-mm-dd hh24:mi:ss'),to_date(:tripendtime,'yyyy-mm-dd hh24:mi:ss'))", new
                                {
                                    orgeuid = param.detail[i].orgeuid,
                                    approvalcode = param.approvalcode,
                                    tripseq = i,
                                    tripsite = param.detail[i].tripsite,
                                    tripstarttime = param.detail[i].tripstarttime,
                                    tripendtime = param.detail[i].tripendtime
                                }, tran);
                        }
                    }
                    //第三步：将内容入库
                    _conn.Execute("insert into APPROVAL_TRIP (orgeuid,approvalcode,tripdays,tripreason) values (:orgeuid,:approvalcode,:tripdays,:tripreason) ",
                       new
                       {
                           orgeuid = param.orgeuid,
                           approvalcode = param.approvalcode,
                           tripdays = param.tripdays,
                           tripreason = param.tripreason
                       }, tran);
                    //第四步：插入审批表
                    _conn.Execute("insert into APPROVAL (orgeuid,approvalcode,applyeuid,applytype,applytitle,apply) values (:orgeuid,:approvalcode,:applyeuid,:applytype,:applytitle,:apply) ",
                        new
                        {
                            orgeuid = param.orgeuid,
                            approvalcode = param.approvalcode,
                            applyeuid = param.euid,
                            applytype = 3,
                            applytitle = param.name + "的出差申请",
                            apply = 3,
                        }, tran);
                    //第五步：插入审批人表
                    for (var j = 0; j < param.approvalList.Count; j++)
                    {
                        _conn.Execute("insert into APPROVALER (orgeuid,approvalcode,approvaleuid) values (:orgeuid,:approvalcode,:approvaleuid) ",
                       new
                       {
                           orgeuid = param.orgeuid,
                           approvalcode = param.approvalcode,
                           approvaleuid = param.approvalList[j].euid
                       }, tran);
                    }
                    if (param.copyto != null)
                    {
                        //第六步：插入抄送人表
                        for (var j = 0; j < param.copyto.Count; j++)
                        {
                            _conn.Execute("insert into APPROVAL_CCLIST (orgeuid,approvalcode,CCEUID) values (:orgeuid,:approvalcode,:approvaleuid) ",
                           new
                           {
                               orgeuid = param.orgeuid,
                               approvalcode = param.approvalcode,
                               approvaleuid = param.copyto[j].euid
                           }, tran);
                        }
                    }
                    //第七步：删除草稿
                    _conn.Execute("delete APPROVAL_DRAFT where ORGEUID = :orgeuid and APPLICANTEUID = :myeuid and APPROVAL_TYPE = 3", new
                    {
                        orgeuid = param.orgeuid,
                        myeuid = param.euid
                    }, tran);
                    tran.Commit();
                    return new { status = 1, msg = "保存成功！" };
                }
                catch (Exception ex)
                {
                    if (tran != null)
                    {
                        tran.Rollback();
                    }
                    log.Error(ex, "保存出差内容时发生异常。");
                    return new { status = -1, msg = "保存失败！" };
                }
            }
        }
        /// <summary>
        /// 保存出差草稿
        /// </summary>
        /// <param name="param">参数</param>
        /// <returns></returns>
        public object SaveTripDraft(TripModels param)
        {
            try
            {
                using (var _conn = OpenConnection())
                {
                    _conn.Execute("delete APPROVAL_DRAFT where ORGEUID = :orgeuid and APPLICANTEUID = :myeuid and APPROVAL_TYPE = 3", new
                    {
                        orgeuid = param.orgeuid,
                        myeuid = param.euid
                    });
                    var _com = new OracleCommand();
                    _com.Connection = (OracleConnection)_conn;
                    _com.CommandText = "insert into APPROVAL_Draft values (:orgeuid,:applicant,:type,:mybody)";
                    IsoDateTimeConverter timeConverter = new IsoDateTimeConverter();
                    timeConverter.DateTimeFormat = "yyyy'-'MM'-'dd' 'HH':'mm':'ss";
                    var body = JsonConvert.SerializeObject(param, timeConverter);
                    byte[] blob = Encoding.Default.GetBytes(body);
                    var filelength = blob.Length;
                    _com.Parameters.Clear();
                    _com.Parameters.AddRange(new OracleParameter[]{
                            new OracleParameter("orgeuid", param.orgeuid),
                                    new OracleParameter("applicant", param.euid),
                                    new OracleParameter("type", 3),
                                    new OracleParameter("mybody",OracleDbType.Blob,filelength,blob,System.Data.ParameterDirection.Input),
                                });
                    _com.ExecuteNonQuery();
                    return new { status = 1, msg = "保存成功！" };
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "保存请假内容时发生异常。");
                return new { status = -1, msg = "保存失败！" };
            }
        }
        /// <summary>
        /// 获取出差草稿
        /// </summary>
        /// <param name="orgeuid">企业编号</param>
        /// <param name="euid">申请人编号</param>
        /// <returns></returns>
        public TripModels GetTripDraft(string orgeuid, string euid)
        {
            try
            {
                using (var _conn = OpenConnection())
                {
                    var _com = new OracleCommand();
                    _com.Connection = (OracleConnection)_conn;
                    _com.CommandType = CommandType.Text;
                    _com.CommandText = "select DRAFTBODY from APPROVAL_DRAFT where ORGEUID = :orgeuid and APPLICANTEUID = :myeuid and APPROVAL_TYPE = 3";
                    _com.Parameters.Clear();
                    _com.Parameters.Add(new OracleParameter("orgeuid", orgeuid));
                    _com.Parameters.Add(new OracleParameter("myeuid", euid));
                    var myRead = _com.ExecuteReader();
                    if (myRead.Read())
                    {
                        OracleBlob myLob = myRead.GetOracleBlob(0);
                        int myLength = Convert.ToInt32(myLob.Length);
                        byte[] Buffer = new byte[myLength];
                        myLob.Read(Buffer, 0, myLength);
                        var body = Encoding.Default.GetString(Buffer);
                        IsoDateTimeConverter timeConverter = new IsoDateTimeConverter();
                        timeConverter.DateTimeFormat = "yyyy'-'MM'-'dd' 'HH':'mm':'ss";
                        var result = JsonConvert.DeserializeObject<TripModels>(body, timeConverter);
                        myRead.Close();
                        if (_conn.State == System.Data.ConnectionState.Closed)
                        {
                            _conn.Open();
                        }
                        _conn.Execute("delete APPROVAL_DRAFT where ORGEUID = :orgeuid and APPLICANTEUID = :myeuid and APPROVAL_TYPE = 3", new
                        {
                            orgeuid = orgeuid,
                            myeuid = euid
                        });
                        return result;
                    }
                    else {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "获取请假草稿时发生异常。");
                return null;
            }
        }

        /// <summary>
        /// 保存报销内容
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public object SaveExpense(ExpenseModels param)
        {
            using (var _conn = OpenConnection())
            {
                IDbTransaction tran = null;
                try
                {
                    tran = _conn.BeginTransaction();
                    var _com = new OracleCommand();
                    _com.Connection = (OracleConnection)_conn;
                    _com.Transaction = (OracleTransaction)tran;

                    var tempcode = _conn.Query<string>("select LPAD(to_char(SEQAPPROVALCODE.nextval), 6,'0') from dual").FirstOrDefault();
                    var now = DateTime.Now;
                    var year = now.Year.ToString();
                    var month = now.Month.ToString().Length > 1 ? now.Month.ToString() : "0" + now.Month.ToString();
                    var day = now.Day.ToString().Length > 1 ? now.Day.ToString() : "0" + now.Day.ToString();
                    var hour = now.Hour.ToString().Length > 1 ? now.Hour.ToString() : "0" + now.Hour.ToString();
                    var minute = now.Minute.ToString().Length > 1 ? now.Minute.ToString() : "0" + now.Minute.ToString();
                    //设置审批编号
                    param.approvalcode = year + month + day + hour + minute + tempcode;

                    //第一步:将附件图片入库
                    if (param.imgs != null)
                    {
                        for (var i = 0; i < param.imgs.Count; i++)
                        {
                            _com.CommandText = "insert into APPROVAL_PIC (approvalcode,picseq,pic) values (:approvalcode,:picseq,:pic)";
                            var body = param.imgs[i].body.Substring(param.imgs[i].body.IndexOf("base64,") + 7);
                            byte[] blob = Convert.FromBase64String(body);
                            var filelength = blob.Length;
                            _com.Parameters.Clear();
                            _com.Parameters.AddRange(new OracleParameter[]{
                                    new OracleParameter("approvalcode", param.approvalcode),
                                    new OracleParameter("picseq", i),
                                    new OracleParameter("pic",OracleDbType.Blob,filelength,blob,System.Data.ParameterDirection.Input),
                                });
                            if (_conn.State == System.Data.ConnectionState.Closed)
                            {
                                _conn.Open();
                            }
                            _com.ExecuteNonQuery();
                        }
                    }
                    //第二步：将明细入库
                    if (param.detail != null)
                    {
                        for (var i = 0; i < param.detail.Count; i++)
                        {
                            _conn.Execute("insert into APPROVAL_EXPENSE_DETAILS (orgeuid,approvalcode,expenseseq,expensetype,expense,expenseremark) " +
                                "values (:orgeuid,:approvalcode,:expenseseq,:expensetype,:expense,:expenseremark)", new
                                {
                                    orgeuid = param.detail[i].orgeuid,
                                    approvalcode = param.approvalcode,
                                    expenseseq = i,
                                    expensetype = param.detail[i].expensetype,
                                    expense = param.detail[i].expense,
                                    expenseremark = param.detail[i].expenseremark
                                }, tran);
                        }
                    }
                    //第三步：将内容入库
                    _conn.Execute("insert into APPROVAL_EXPENSE (orgeuid,approvalcode,expensetotal) values (:orgeuid,:approvalcode,:expensetotal) ",
                       new
                       {
                           orgeuid = param.orgeuid,
                           approvalcode = param.approvalcode,
                           expensetotal = param.expensetotal
                       }, tran);
                    //第四步：插入审批表
                    _conn.Execute("insert into APPROVAL (orgeuid,approvalcode,applyeuid,applytype,applytitle,apply) values (:orgeuid,:approvalcode,:applyeuid,:applytype,:applytitle,:apply) ",
                        new
                        {
                            orgeuid = param.orgeuid,
                            approvalcode = param.approvalcode,
                            applyeuid = param.euid,
                            applytype = 2,
                            applytitle = param.name + "的报销申请",
                            apply = 3,
                        }, tran);
                    //第五步：插入审批人表
                    for (var j = 0; j < param.approvalList.Count; j++)
                    {
                        _conn.Execute("insert into APPROVALER (orgeuid,approvalcode,approvaleuid) values (:orgeuid,:approvalcode,:approvaleuid) ",
                       new
                       {
                           orgeuid = param.orgeuid,
                           approvalcode = param.approvalcode,
                           approvaleuid = param.approvalList[j].euid
                       }, tran);
                    }
                    if (param.copyto != null)
                    {
                        //第六步：插入抄送人表
                        for (var j = 0; j < param.copyto.Count; j++)
                        {
                            _conn.Execute("insert into APPROVAL_CCLIST (orgeuid,approvalcode,cceuid) values (:orgeuid,:approvalcode,:cceuid) ",
                           new
                           {
                               orgeuid = param.orgeuid,
                               approvalcode = param.approvalcode,
                               cceuid = param.copyto[j].euid
                           }, tran);
                        }
                    }
                    //第七步：删除草稿
                    if (_conn.State == System.Data.ConnectionState.Closed)
                    {
                        _conn.Open();
                    }
                    _conn.Execute("delete APPROVAL_DRAFT where ORGEUID = :orgeuid and APPLICANTEUID = :myeuid  and APPROVAL_TYPE = 2", new
                    {
                        orgeuid = param.orgeuid,
                        myeuid = param.euid
                    }, tran);
                    tran.Commit();
                    return new { status = 1, msg = "保存成功！" };
                }
                catch (Exception ex)
                {
                    if (tran != null)
                    {
                        tran.Rollback();
                    }
                    log.Error(ex, "保存报销内容时发生异常。");
                    return new { status = -1, msg = "保存失败！" };
                }
            }
        }
        /// <summary>
        /// 保存报销草稿
        /// </summary>
        /// <param name="param">参数</param>
        /// <returns></returns>
        public object SaveExpenseDraft(ExpenseModels param)
        {
            try
            {
                using (var _conn = OpenConnection())
                {
                    _conn.Execute("delete APPROVAL_DRAFT where ORGEUID = :orgeuid and APPLICANTEUID = :myeuid  and APPROVAL_TYPE = 2", new
                    {
                        orgeuid = param.orgeuid,
                        myeuid = param.euid
                    });
                    var _com = new OracleCommand();
                    _com.Connection = (OracleConnection)_conn;
                    _com.CommandText = "insert into APPROVAL_Draft values (:orgeuid,:applicant,:type,:mybody)";
                    IsoDateTimeConverter timeConverter = new IsoDateTimeConverter();
                    timeConverter.DateTimeFormat = "yyyy'-'MM'-'dd' 'HH':'mm':'ss";
                    var body = JsonConvert.SerializeObject(param, timeConverter);
                    byte[] blob = Encoding.Default.GetBytes(body);
                    var filelength = blob.Length;
                    _com.Parameters.Clear();
                    _com.Parameters.AddRange(new OracleParameter[]{
                            new OracleParameter("orgeuid", param.orgeuid),
                                    new OracleParameter("applicant", param.euid),
                                    new OracleParameter("type", 2),
                                    new OracleParameter("mybody",OracleDbType.Blob,filelength,blob,System.Data.ParameterDirection.Input),
                                });
                    if (_conn.State == System.Data.ConnectionState.Closed)
                    {
                        _conn.Open();
                    }
                    _com.ExecuteNonQuery();
                    return new { status = 1, msg = "保存成功！" };
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "保存请假内容时发生异常。");
                return new { status = -1, msg = "保存失败！" };
            }
        }
        /// <summary>
        /// 获取报销草稿
        /// </summary>
        /// <param name="orgeuid">企业编号</param>
        /// <param name="euid">申请人编号</param>
        /// <returns></returns>
        public ExpenseModels GetExpenseDraft(string orgeuid, string euid)
        {
            try
            {
                using (var _conn = OpenConnection())
                {
                    var _com = new OracleCommand();
                    _com.Connection = (OracleConnection)_conn;
                    _com.CommandType = CommandType.Text;
                    _com.CommandText = "select DRAFTBODY from APPROVAL_DRAFT where ORGEUID = :orgeuid and APPLICANTEUID = :myeuid  and APPROVAL_TYPE = 2";
                    _com.Parameters.Clear();
                    _com.Parameters.Add(new OracleParameter("orgeuid", orgeuid));
                    _com.Parameters.Add(new OracleParameter("myeuid", euid));
                    OracleDataReader myRead = _com.ExecuteReader();
                    if (myRead.Read())
                    {
                        OracleBlob myLob = myRead.GetOracleBlob(0);
                        int myLength = Convert.ToInt32(myLob.Length);
                        byte[] Buffer = new byte[myLength];
                        myLob.Read(Buffer, 0, myLength);
                        var body = Encoding.Default.GetString(Buffer);
                        IsoDateTimeConverter timeConverter = new IsoDateTimeConverter();
                        timeConverter.DateTimeFormat = "yyyy'-'MM'-'dd' 'HH':'mm':'ss";
                        var result = JsonConvert.DeserializeObject<ExpenseModels>(body, timeConverter);
                        myRead.Close();
                        if (_conn.State == System.Data.ConnectionState.Closed)
                        {
                            _conn.Open();
                        }
                        _conn.Execute("delete APPROVAL_DRAFT where ORGEUID = :orgeuid and APPLICANTEUID = :myeuid and APPROVAL_TYPE = 2", new
                        {
                            orgeuid = orgeuid,
                            myeuid = euid
                        });
                        return result;
                    }
                    else {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "获取请假草稿时发生异常。");
                return null;
            }
        }
        /// <summary>
        /// 保存借款内容
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public object SaveBorrowing(BorrowingModels param)
        {
            using (var _conn = OpenConnection())
            {
                IDbTransaction tran = null;
                try
                {
                    tran = _conn.BeginTransaction();
                    var _com = new OracleCommand();
                    _com.Connection = (OracleConnection)_conn;
                    _com.Transaction = (OracleTransaction)tran;
                    var tempcode = _conn.Query<string>("select LPAD(to_char(SEQAPPROVALCODE.nextval), 6,'0') from dual").FirstOrDefault();
                    var now = DateTime.Now;
                    var year = now.Year.ToString();
                    var month = now.Month.ToString().Length > 1 ? now.Month.ToString() : "0" + now.Month.ToString();
                    var day = now.Day.ToString().Length > 1 ? now.Day.ToString() : "0" + now.Day.ToString();
                    var hour = now.Hour.ToString().Length > 1 ? now.Hour.ToString() : "0" + now.Hour.ToString();
                    var minute = now.Minute.ToString().Length > 1 ? now.Minute.ToString() : "0" + now.Minute.ToString();
                    //设置审批编号
                    param.approvalcode = year + month + day + hour + minute + tempcode;
                    //第一步:将附件图片入库
                    if (param.imgs != null)
                    {
                        for (var i = 0; i < param.imgs.Count; i++)
                        {
                            _com.CommandText = "insert into APPROVAL_PIC (approvalcode,picseq,pic) values (:approvalcode,:picseq,:pic)";
                            var body = param.imgs[i].body.Substring(param.imgs[i].body.IndexOf("base64,") + 7);
                            byte[] blob = Convert.FromBase64String(body);
                            var filelength = blob.Length;
                            _com.Parameters.Clear();
                            _com.Parameters.AddRange(new OracleParameter[]{
                                    new OracleParameter("approvalcode", param.approvalcode),
                                    new OracleParameter("picseq", i),
                                    new OracleParameter("pic",OracleDbType.Blob,filelength,blob,System.Data.ParameterDirection.Input),
                                });
                            if (_conn.State == System.Data.ConnectionState.Closed)
                            {
                                _conn.Open();
                            }
                            _com.ExecuteNonQuery();
                        }
                    }
                    //第二步：将内容入库
                    _conn.Execute("insert into APPROVAL_LOAN (orgeuid,approvalcode,loan,loanday,loanreson) values (:orgeuid,:approvalcode,:loan,to_date(:loanday,'yyyy-mm-dd hh24:mi:ss'),:loanreson)",
                       new
                       {
                           orgeuid = param.orgeuid,
                           approvalcode = param.approvalcode,
                           loan = param.loan,
                           loanday = param.loanday,
                           loanreson = param.loanreson
                       }, tran);
                    //第三步：插入审批表
                    _conn.Execute("insert into APPROVAL (orgeuid,approvalcode,applyeuid,applytype,applytitle,apply) values (:orgeuid,:approvalcode,:applyeuid,:applytype,:applytitle,:apply) ",
                        new
                        {
                            orgeuid = param.orgeuid,
                            approvalcode = param.approvalcode,
                            applyeuid = param.euid,
                            applytype = 6,
                            applytitle = param.name + "的借款申请",
                            apply = 3,
                        }, tran);
                    //第四步：插入审批人表
                    for (var j = 0; j < param.approvalList.Count; j++)
                    {
                        _conn.Execute("insert into APPROVALER (orgeuid,approvalcode,approvaleuid) values (:orgeuid,:approvalcode,:approvaleuid) ",
                       new
                       {
                           orgeuid = param.orgeuid,
                           approvalcode = param.approvalcode,
                           approvaleuid = param.approvalList[j].euid
                       }, tran);
                    }
                    if (param.copyto != null)
                    {
                        //第五步：插入抄送人表
                        for (var j = 0; j < param.copyto.Count; j++)
                        {
                            _conn.Execute("insert into APPROVAL_CCLIST (orgeuid,approvalcode,cceuid) values (:orgeuid,:approvalcode,:cceuid) ",
                           new
                           {
                               orgeuid = param.orgeuid,
                               approvalcode = param.approvalcode,
                               cceuid = param.copyto[j].euid
                           }, tran);
                        }
                    }
                    //第六步：删除草稿
                    if (_conn.State == System.Data.ConnectionState.Closed)
                    {
                        _conn.Open();
                    }
                    _conn.Execute("delete APPROVAL_DRAFT where ORGEUID = :orgeuid and APPLICANTEUID = :myeuid  and APPROVAL_TYPE = 6", new
                    {
                        orgeuid = param.orgeuid,
                        myeuid = param.euid
                    }, tran);
                    tran.Commit();
                    return new { status = 1, msg = "保存成功！" };
                }
                catch (Exception ex)
                {
                    if (tran != null)
                    {
                        tran.Rollback();
                    }
                    log.Error(ex, "保存请假内容时发生异常。");
                    return new { status = -1, msg = "保存失败！" };
                }
            }
        }
        /// <summary>
        /// 保存借款草稿
        /// </summary>
        /// <param name="param">参数</param>
        /// <returns></returns>
        public object SaveBorrowingDraft(BorrowingModels param)
        {
            try
            {
                using (var _conn = OpenConnection())
                {
                    _conn.Execute("delete APPROVAL_DRAFT where ORGEUID = :orgeuid and APPLICANTEUID = :myeuid  and APPROVAL_TYPE = 6", new
                    {
                        orgeuid = param.orgeuid,
                        myeuid = param.euid
                    });
                    var _com = new OracleCommand();
                    _com.Connection = (OracleConnection)_conn;
                    _com.CommandText = "insert into APPROVAL_Draft values (:orgeuid,:applicant,:type,:mybody)";
                    IsoDateTimeConverter timeConverter = new IsoDateTimeConverter();
                    timeConverter.DateTimeFormat = "yyyy'-'MM'-'dd' 'HH':'mm':'ss";
                    var body = JsonConvert.SerializeObject(param, timeConverter);
                    byte[] blob = Encoding.Default.GetBytes(body);
                    var filelength = blob.Length;
                    _com.Parameters.Clear();
                    _com.Parameters.AddRange(new OracleParameter[]{
                            new OracleParameter("orgeuid", param.orgeuid),
                                    new OracleParameter("applicant", param.euid),
                                    new OracleParameter("type", 6),
                                    new OracleParameter("mybody",OracleDbType.Blob,filelength,blob,System.Data.ParameterDirection.Input),
                                });
                    if (_conn.State == System.Data.ConnectionState.Closed)
                    {
                        _conn.Open();
                    }
                    _com.ExecuteNonQuery();
                    return new { status = 1, msg = "保存成功！" };
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "保存请假内容时发生异常。");
                return new { status = -1, msg = "保存失败！" };
            }
        }
        /// <summary>
        /// 获取借款草稿
        /// </summary>
        /// <param name="orgeuid">企业编号</param>
        /// <param name="euid">申请人编号</param>
        /// <returns></returns>
        public BorrowingModels GetBorrowingDraft(string orgeuid, string euid)
        {
            try
            {
                using (var _conn = OpenConnection())
                {
                    var _com = new OracleCommand();
                    _com.Connection = (OracleConnection)_conn;
                    _com.CommandType = CommandType.Text;
                    _com.CommandText = "select DRAFTBODY from APPROVAL_DRAFT where ORGEUID = :orgeuid and APPLICANTEUID = :myeuid  and APPROVAL_TYPE = 6";
                    _com.Parameters.Clear();
                    _com.Parameters.Add(new OracleParameter("orgeuid", orgeuid));
                    _com.Parameters.Add(new OracleParameter("myeuid", euid));
                    var myRead = _com.ExecuteReader();
                    if (myRead.Read())
                    {
                        OracleBlob myLob = myRead.GetOracleBlob(0);
                        int myLength = Convert.ToInt32(myLob.Length);
                        byte[] Buffer = new byte[myLength];
                        myLob.Read(Buffer, 0, myLength);
                        var body = Encoding.Default.GetString(Buffer);
                        IsoDateTimeConverter timeConverter = new IsoDateTimeConverter();
                        timeConverter.DateTimeFormat = "yyyy'-'MM'-'dd' 'HH':'mm':'ss";
                        var result = JsonConvert.DeserializeObject<BorrowingModels>(body, timeConverter);
                        myRead.Close();
                        if (_conn.State == System.Data.ConnectionState.Closed)
                        {
                            _conn.Open();
                        }
                        _conn.Execute("delete APPROVAL_DRAFT where ORGEUID = :orgeuid and APPLICANTEUID = :myeuid and APPROVAL_TYPE = 6", new
                        {
                            orgeuid = orgeuid,
                            myeuid = euid
                        });
                        return result;
                    }
                    else {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "获取请假草稿时发生异常。");
                return null;
            }
        }
        /// <summary>
        /// 保存外出内容
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public object SaveAway(AwayModels param)
        {
            using (var _conn = OpenConnection())
            {
                IDbTransaction tran = null;
                try
                {
                    var _com = new OracleCommand();
                    _com.Connection = (OracleConnection)_conn;
                    tran = _conn.BeginTransaction();
                    _com.Transaction = (OracleTransaction)tran;
                    var tempcode = _conn.Query<string>("select LPAD(to_char(SEQAPPROVALCODE.nextval), 6,'0') from dual").FirstOrDefault();
                    var now = DateTime.Now;
                    var year = now.Year.ToString();
                    var month = now.Month.ToString().Length > 1 ? now.Month.ToString() : "0" + now.Month.ToString();
                    var day = now.Day.ToString().Length > 1 ? now.Day.ToString() : "0" + now.Day.ToString();
                    var hour = now.Hour.ToString().Length > 1 ? now.Hour.ToString() : "0" + now.Hour.ToString();
                    var minute = now.Minute.ToString().Length > 1 ? now.Minute.ToString() : "0" + now.Minute.ToString();
                    //设置审批编号
                    param.approvalcode = year + month + day + hour + minute + tempcode;

                    //第一步:将附件图片入库
                    if (param.imgs != null)
                    {
                        for (var i = 0; i < param.imgs.Count; i++)
                        {
                            _com.CommandText = "insert into APPROVAL_PIC (approvalcode,picseq,pic) values (:approvalcode,:picseq,:pic)";

                            var body = param.imgs[i].body.Substring(param.imgs[i].body.IndexOf("base64,") + 7);
                            byte[] blob = Convert.FromBase64String(body);
                            var filelength = blob.Length;
                            _com.Parameters.Clear();
                            _com.Parameters.AddRange(new OracleParameter[]{
                                    new OracleParameter("approvalcode", param.approvalcode),
                                    new OracleParameter("picseq", i),
                                    new OracleParameter("pic",OracleDbType.Blob,filelength,blob,System.Data.ParameterDirection.Input),
                                });
                            if (_conn.State == System.Data.ConnectionState.Closed)
                            {
                                _conn.Open();
                            }
                            _com.ExecuteNonQuery();
                        }
                    }
                    //第二步：将内容入库
                    _conn.Execute("insert into APPROVAL_OUT (orgeuid,approvalcode,outstarttime,outendtime,outhours,outreason) " +
                       " values (:orgeuid,:approvalcode,to_date(:outstarttime,'yyyy-mm-dd hh24:mi:ss'),to_date(:outendtime,'yyyy-mm-dd hh24:mi:ss'),:outhours,:outreason) ",
                       new
                       {
                           orgeuid = param.orgeuid,
                           approvalcode = param.approvalcode,
                           outstarttime = year + "-" + month + "-" + day + " " + param.outstarttime,
                           outendtime = year + "-" + month + "-" + day + " " + param.outendtime,
                           outhours = param.outhours,
                           outreason = param.outreason
                       }, tran);
                    //第三步：插入审批表
                    _conn.Execute("insert into APPROVAL (orgeuid,approvalcode,applyeuid,applytype,applytitle,apply) " +
                        " values (:orgeuid,:approvalcode,:applyeuid,:applytype,:applytitle,:apply) ",
                        new
                        {
                            orgeuid = param.orgeuid,
                            approvalcode = param.approvalcode,
                            applyeuid = param.euid,
                            applytype = 4,
                            applytitle = param.name + "的外出申请",
                            apply = 3,
                        }, tran);
                    //第四步：插入审批人表
                    for (var j = 0; j < param.approvalList.Count; j++)
                    {
                        _conn.Execute("insert into APPROVALER (orgeuid,approvalcode,approvaleuid) values (:orgeuid,:approvalcode,:approvaleuid) ",
                       new
                       {
                           orgeuid = param.orgeuid,
                           approvalcode = param.approvalcode,
                           approvaleuid = param.approvalList[j].euid
                       }, tran);
                    }
                    if (param.copyto != null)
                    {
                        //第五步：插入抄送人表
                        for (var j = 0; j < param.copyto.Count; j++)
                        {
                            _conn.Execute("insert into APPROVAL_CCLIST (orgeuid,approvalcode,cceuid) values (:orgeuid,:approvalcode,:cceuid) ",
                           new
                           {
                               orgeuid = param.orgeuid,
                               approvalcode = param.approvalcode,
                               cceuid = param.copyto[j].euid
                           }, tran);
                        }
                    }
                    //第六步：删除草稿
                    if (_conn.State == System.Data.ConnectionState.Closed)
                    {
                        _conn.Open();
                    }
                    _conn.Execute("delete APPROVAL_DRAFT where ORGEUID = :orgeuid and APPLICANTEUID = :myeuid  and APPROVAL_TYPE = 4", new
                    {
                        orgeuid = param.orgeuid,
                        myeuid = param.euid
                    }, tran);
                    tran.Commit();
                    return new { status = 1, msg = "保存成功！" };
                }
                catch (Exception ex)
                {
                    if (tran != null)
                    {
                        tran.Rollback();
                    }
                    log.Error(ex, "保存请假内容时发生异常。");
                    return new { status = -1, msg = "保存失败！" };
                }
            }
        }
        /// <summary>
        /// 保存外出草稿
        /// </summary>
        /// <param name="param">参数</param>
        /// <returns></returns>
        public object SaveAwayDraft(AwayModels param)
        {
            try
            {
                using (var _conn = OpenConnection())
                {
                    _conn.Execute("delete APPROVAL_DRAFT where ORGEUID = :orgeuid and APPLICANTEUID = :myeuid  and APPROVAL_TYPE = 4", new
                    {
                        orgeuid = param.orgeuid,
                        myeuid = param.euid
                    });
                    var _com = new OracleCommand();
                    _com.Connection = (OracleConnection)_conn;
                    _com.CommandText = "insert into APPROVAL_Draft values (:orgeuid,:applicant,:type,:mybody)";
                    IsoDateTimeConverter timeConverter = new IsoDateTimeConverter();
                    timeConverter.DateTimeFormat = "yyyy'-'MM'-'dd' 'HH':'mm':'ss";
                    var body = JsonConvert.SerializeObject(param, timeConverter);
                    byte[] blob = Encoding.Default.GetBytes(body);
                    var filelength = blob.Length;
                    _com.Parameters.Clear();
                    _com.Parameters.AddRange(new OracleParameter[]{
                            new OracleParameter("orgeuid", param.orgeuid),
                                    new OracleParameter("applicant", param.euid),
                                    new OracleParameter("type", 4),
                                    new OracleParameter("mybody",OracleDbType.Blob,filelength,blob,System.Data.ParameterDirection.Input),
                                });
                    if (_conn.State == System.Data.ConnectionState.Closed)
                    {
                        _conn.Open();
                    }
                    _com.ExecuteNonQuery();
                    return new { status = 1, msg = "保存成功！" };
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "保存请假内容时发生异常。");
                return new { status = -1, msg = "保存失败！" };
            }
        }
        /// <summary>
        /// 获取外出草稿
        /// </summary>
        /// <param name="orgeuid">企业编号</param>
        /// <param name="euid">申请人编号</param>
        /// <returns></returns>
        public AwayModels GetAwayDraft(string orgeuid, string euid)
        {
            try
            {
                using (var _conn = OpenConnection())
                {
                    var _com = new OracleCommand();
                    _com.Connection = (OracleConnection)_conn;
                    _com.CommandType = CommandType.Text;
                    _com.CommandText = "select DRAFTBODY from APPROVAL_DRAFT where ORGEUID = :orgeuid and APPLICANTEUID = :myeuid  and APPROVAL_TYPE = 4";
                    _com.Parameters.Clear();
                    _com.Parameters.Add(new OracleParameter("orgeuid", orgeuid));
                    _com.Parameters.Add(new OracleParameter("myeuid", euid));
                    var myRead = _com.ExecuteReader();
                    if (myRead.Read())
                    {
                        OracleBlob myLob = myRead.GetOracleBlob(0);
                        int myLength = Convert.ToInt32(myLob.Length);
                        byte[] Buffer = new byte[myLength];
                        myLob.Read(Buffer, 0, myLength);
                        var body = Encoding.Default.GetString(Buffer);
                        IsoDateTimeConverter timeConverter = new IsoDateTimeConverter();
                        timeConverter.DateTimeFormat = "yyyy'-'MM'-'dd' 'HH':'mm':'ss";
                        var result = JsonConvert.DeserializeObject<AwayModels>(body, timeConverter);
                        myRead.Close();
                        if (_conn.State == System.Data.ConnectionState.Closed)
                        {
                            _conn.Open();
                        }
                        _conn.Execute("delete APPROVAL_DRAFT where ORGEUID = :orgeuid and APPLICANTEUID = :myeuid and APPROVAL_TYPE = 4", new
                        {
                            orgeuid = orgeuid,
                            myeuid = euid
                        });
                        return result;
                    }
                    else {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "获取请假草稿时发生异常。");
                return null;
            }
        }
        /// <summary>
        /// 保存请示内容
        /// </summary>
        /// <param name="param">参数</param>
        /// <returns></returns>
        public object SaveConsult(ConsultModels param)
        {
            using (var _conn = OpenConnection())
            {
                IDbTransaction tran = null;
                try
                {
                    tran = _conn.BeginTransaction();
                    var _com = new OracleCommand();
                    _com.Connection = (OracleConnection)_conn;
                    _com.Transaction = (OracleTransaction)tran;
                    var tempcode = _conn.Query<string>("select LPAD(to_char(SEQAPPROVALCODE.nextval), 6,'0') from dual").FirstOrDefault();
                    var now = DateTime.Now;
                    var year = now.Year.ToString();
                    var month = now.Month.ToString().Length > 1 ? now.Month.ToString() : "0" + now.Month.ToString();
                    var day = now.Day.ToString().Length > 1 ? now.Day.ToString() : "0" + now.Day.ToString();
                    var hour = now.Hour.ToString().Length > 1 ? now.Hour.ToString() : "0" + now.Hour.ToString();
                    var minute = now.Minute.ToString().Length > 1 ? now.Minute.ToString() : "0" + now.Minute.ToString();
                    //设置审批编号
                    param.approvalcode = year + month + day + hour + minute + tempcode;

                    //第一步:将附件图片入库
                    if (param.imgs != null)
                    {
                        for (var i = 0; i < param.imgs.Count; i++)
                        {
                            _com.CommandText = "insert into APPROVAL_PIC (approvalcode,picseq,pic) values (:approvalcode,:picseq,:pic)";

                            var body = param.imgs[i].body.Substring(param.imgs[i].body.IndexOf("base64,") + 7);
                            byte[] blob = Convert.FromBase64String(body);
                            var filelength = blob.Length;
                            _com.Parameters.Clear();
                            _com.Parameters.AddRange(new OracleParameter[]{
                                    new OracleParameter("approvalcode", param.approvalcode),
                                    new OracleParameter("picseq", i),
                                    new OracleParameter("pic",OracleDbType.Blob,filelength,blob,System.Data.ParameterDirection.Input),
                                });
                            if (_conn.State == System.Data.ConnectionState.Closed)
                            {
                                _conn.Open();
                            }
                            _com.ExecuteNonQuery();
                        }
                    }
                    //第二步：将内容入库
                    _conn.Execute("insert into APPROVAL_REQUEST (orgeuid,approvalcode,requestreason) values (:orgeuid,:approvalcode,:requestreason) ",
                       new
                       {
                           orgeuid = param.orgeuid,
                           approvalcode = param.approvalcode,
                           requestreason = param.reason
                       }, tran);
                    //第三步：插入审批表
                    _conn.Execute("insert into APPROVAL (orgeuid,approvalcode,applyeuid,applytype,applytitle,apply) " +
                        " values (:orgeuid,:approvalcode,:applyeuid,:applytype,:applytitle,:apply) ",
                        new
                        {
                            orgeuid = param.orgeuid,
                            approvalcode = param.approvalcode,
                            applyeuid = param.euid,
                            applytype = 5,
                            applytitle = param.name + "的请示申请",
                            apply = 3,
                        }, tran);
                    //第四步：插入审批人表
                    for (var j = 0; j < param.approvalList.Count; j++)
                    {
                        _conn.Execute("insert into APPROVALER (orgeuid,approvalcode,approvaleuid) values (:orgeuid,:approvalcode,:approvaleuid) ",
                       new
                       {
                           orgeuid = param.orgeuid,
                           approvalcode = param.approvalcode,
                           approvaleuid = param.approvalList[j].euid
                       }, tran);
                    }
                    if (param.copyto != null)
                    {
                        //第五步：插入抄送人表
                        for (var j = 0; j < param.copyto.Count; j++)
                        {
                            _conn.Execute("insert into APPROVAL_CCLIST (orgeuid,approvalcode,cceuid) values (:orgeuid,:approvalcode,:cceuid) ",
                           new
                           {
                               orgeuid = param.orgeuid,
                               approvalcode = param.approvalcode,
                               cceuid = param.copyto[j].euid
                           }, tran);
                        }
                    }
                    //第六步：删除草稿
                    if (_conn.State == System.Data.ConnectionState.Closed)
                    {
                        _conn.Open();
                    }
                    _conn.Execute("delete APPROVAL_DRAFT where ORGEUID = :orgeuid and APPLICANTEUID = :myeuid  and APPROVAL_TYPE = 5", new
                    {
                        orgeuid = param.orgeuid,
                        myeuid = param.euid
                    }, tran);
                    tran.Commit();
                    return new { status = 1, msg = "保存成功！" };
                }
                catch (Exception ex)
                {
                    if (tran != null)
                    {
                        tran.Rollback();
                    }
                    log.Error(ex, "保存请示内容时发生异常。");
                    return new { status = -1, msg = "保存失败！" };
                }
            }
        }
        /// <summary>
        /// 保存请示草稿
        /// </summary>
        /// <param name="param">参数</param>
        /// <returns></returns>
        public object SaveConsultDraft(ConsultModels param)
        {
            try
            {
                using (var _conn = OpenConnection())
                {
                    _conn.Execute("delete APPROVAL_DRAFT where ORGEUID = :orgeuid and APPLICANTEUID = :myeuid  and APPROVAL_TYPE = 5", new
                    {
                        orgeuid = param.orgeuid,
                        myeuid = param.euid
                    });
                    var _com = new OracleCommand();
                    _com.Connection = (OracleConnection)_conn;
                    _com.CommandText = "insert into APPROVAL_Draft values (:orgeuid,:applicant,:type,:mybody)";
                    IsoDateTimeConverter timeConverter = new IsoDateTimeConverter();
                    timeConverter.DateTimeFormat = "yyyy'-'MM'-'dd' 'HH':'mm':'ss";
                    var body = JsonConvert.SerializeObject(param, timeConverter);
                    byte[] blob = Encoding.Default.GetBytes(body);
                    var filelength = blob.Length;
                    _com.Parameters.Clear();
                    _com.Parameters.AddRange(new OracleParameter[]{
                            new OracleParameter("orgeuid", param.orgeuid),
                                    new OracleParameter("applicant", param.euid),
                                    new OracleParameter("type", 5),
                                    new OracleParameter("mybody",OracleDbType.Blob,filelength,blob,System.Data.ParameterDirection.Input),
                                });
                    if (_conn.State == System.Data.ConnectionState.Closed)
                    {
                        _conn.Open();
                    }
                    _com.ExecuteNonQuery();
                    return new { status = 1, msg = "保存成功！" };
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "保存请假内容时发生异常。");
                return new { status = -1, msg = "保存失败！" };
            }
        }
        /// <summary>
        /// 获取请示草稿
        /// </summary>
        /// <param name="orgeuid">企业编号</param>
        /// <param name="euid">申请人编号</param>
        /// <returns></returns>
        public ConsultModels GetConsultDraft(string orgeuid, string euid)
        {
            try
            {
                using (var _conn = OpenConnection())
                {
                    var _com = new OracleCommand();
                    _com.Connection = (OracleConnection)_conn;
                    _com.CommandType = CommandType.Text;
                    _com.CommandText = "select DRAFTBODY from APPROVAL_DRAFT where ORGEUID = :orgeuid and APPLICANTEUID = :myeuid  and APPROVAL_TYPE = 5";
                    _com.Parameters.Clear();
                    _com.Parameters.Add(new OracleParameter("orgeuid", orgeuid));
                    _com.Parameters.Add(new OracleParameter("myeuid", euid));
                    OracleDataReader myRead = _com.ExecuteReader();
                    if (myRead.Read())
                    {
                        OracleBlob myLob = myRead.GetOracleBlob(0);
                        int myLength = Convert.ToInt32(myLob.Length);
                        byte[] Buffer = new byte[myLength];
                        myLob.Read(Buffer, 0, myLength);
                        var body = Encoding.Default.GetString(Buffer);
                        IsoDateTimeConverter timeConverter = new IsoDateTimeConverter();
                        timeConverter.DateTimeFormat = "yyyy'-'MM'-'dd' 'HH':'mm':'ss";
                        var result = JsonConvert.DeserializeObject<ConsultModels>(body, timeConverter);
                        myRead.Close();
                        if (_conn.State == System.Data.ConnectionState.Closed)
                        {
                            _conn.Open();
                        }
                        _conn.Execute("delete APPROVAL_DRAFT where ORGEUID = :orgeuid and APPLICANTEUID = :myeuid and APPROVAL_TYPE = 5", new
                        {
                            orgeuid = orgeuid,
                            myeuid = euid
                        });
                        return result;
                    }
                    else {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "获取请假草稿时发生异常。");
                return null;
            }
        }
        /// <summary>
        /// 保存请假内容
        /// </summary>
        /// <param name="param">参数</param>
        /// <returns></returns>
        public object SaveOffWork(OffWorkModels param)
        {
            using (var _conn = OpenConnection())
            {
                IDbTransaction tran = null;
                try
                {
                    tran = _conn.BeginTransaction();
                    var _com = new OracleCommand();
                    _com.Connection = (OracleConnection)_conn;
                    _com.Transaction = (OracleTransaction)tran;
                    var tempcode = _conn.Query<string>("select LPAD(to_char(SEQAPPROVALCODE.nextval), 6,'0') from dual").FirstOrDefault();
                    var now = DateTime.Now;
                    var year = now.Year.ToString();
                    var month = now.Month.ToString().Length > 1 ? now.Month.ToString() : "0" + now.Month.ToString();
                    var day = now.Day.ToString().Length > 1 ? now.Day.ToString() : "0" + now.Day.ToString();
                    var hour = now.Hour.ToString().Length > 1 ? now.Hour.ToString() : "0" + now.Hour.ToString();
                    var minute = now.Minute.ToString().Length > 1 ? now.Minute.ToString() : "0" + now.Minute.ToString();
                    //设置审批编号
                    param.approvalcode = year + month + day + hour + minute + tempcode;

                    //第一步:将附件图片入库
                    if (param.imgs != null)
                    {
                        for (var i = 0; i < param.imgs.Count; i++)
                        {
                            _com.CommandText = "insert into APPROVAL_PIC (approvalcode,picseq,pic) values (:approvalcode,:picseq,:pic)";

                            var body = param.imgs[i].body.Substring(param.imgs[i].body.IndexOf("base64,") + 7);
                            byte[] blob = Convert.FromBase64String(body);
                            var filelength = blob.Length;
                            _com.Parameters.Clear();
                            _com.Parameters.AddRange(new OracleParameter[]{
                                    new OracleParameter("approvalcode", param.approvalcode),
                                    new OracleParameter("picseq", i),
                                    new OracleParameter("pic",OracleDbType.Blob,filelength,blob,System.Data.ParameterDirection.Input)
                                });
                            if (_conn.State == System.Data.ConnectionState.Closed)
                            {
                                _conn.Open();
                            }
                            _com.ExecuteNonQuery();
                        }
                    }
                    //第二步：将内容入库
                    _conn.Execute("insert into APPROVAL_LEAVE (orgeuid,approvalcode,leavetype,leavestarttime,leaveendtime,leavehours,leavereason) " +
                       " values (:orgeuid,:approvalcode,:leavetype,to_date(:leavestarttime,'yyyy-mm-dd hh24:mi:ss'),to_date(:leaveendtime,'yyyy-mm-dd hh24:mi:ss'),:leavehours,:leavereason) ",
                       new
                       {
                           orgeuid = param.orgeuid,
                           approvalcode = param.approvalcode,
                           leavetype = param.leavetype,
                           leavestarttime = param.leavestarttime,
                           leaveendtime = param.leaveendtime,
                           leavehours = double.Parse(param.leavehours) * 24,
                           leavereason = param.leavereason
                       }, tran);
                    //第三步：插入审批表
                    _conn.Execute("insert into APPROVAL (orgeuid,approvalcode,applyeuid,applytype,applytitle,apply) " +
                        " values (:orgeuid,:approvalcode,:applyeuid,:applytype,:applytitle,:apply) ",
                        new
                        {
                            orgeuid = param.orgeuid,
                            approvalcode = param.approvalcode,
                            applyeuid = param.euid,
                            applytype = 1,
                            applytitle = param.name + "的请假申请",
                            apply = 3,
                        }, tran);
                    //第四步：插入审批人表
                    for (var j = 0; j < param.approvalList.Count; j++)
                    {
                        _conn.Execute("insert into APPROVALER (orgeuid,approvalcode,approvaleuid) values (:orgeuid,:approvalcode,:approvaleuid) ",
                       new
                       {
                           orgeuid = param.orgeuid,
                           approvalcode = param.approvalcode,
                           approvaleuid = param.approvalList[j].euid
                       }, tran);
                    }
                    if (param.copyto != null)
                    {
                        //第五步：插入抄送人表
                        for (var j = 0; j < param.copyto.Count; j++)
                        {
                            _conn.Execute("insert into APPROVAL_CCLIST (orgeuid,approvalcode,cceuid) values (:orgeuid,:approvalcode,:cceuid) ",
                           new
                           {
                               orgeuid = param.orgeuid,
                               approvalcode = param.approvalcode,
                               cceuid = param.copyto[j].euid
                           }, tran);
                        }
                    }
                    //第六步：删除草稿
                    if (_conn.State == System.Data.ConnectionState.Closed)
                    {
                        _conn.Open();
                    }
                    _conn.Execute("delete APPROVAL_DRAFT where ORGEUID = :orgeuid and APPLICANTEUID = :myeuid and APPROVAL_TYPE = 1", new
                    {
                        orgeuid = param.orgeuid,
                        myeuid = param.euid
                    }, tran);
                    tran.Commit();
                    return new { status = 1, msg = "保存成功！" };
                }
                catch (Exception ex)
                {
                    if (tran != null)
                    {
                        tran.Rollback();
                    }
                    log.Error(ex, "保存请假内容时发生异常。");
                    return new { status = -1, msg = "保存失败！" };
                }
            }
        }
        /// <summary>
        /// 保存请假草稿
        /// </summary>
        /// <param name="param">参数</param>
        /// <returns></returns>
        public object SaveOffWorkDraft(OffWorkModels param)
        {
            try
            {
                using (var _conn = OpenConnection())
                {
                    _conn.Execute("delete APPROVAL_DRAFT where ORGEUID = :orgeuid and APPLICANTEUID = :myeuid  and APPROVAL_TYPE = 1", new
                    {
                        orgeuid = param.orgeuid,
                        myeuid = param.euid
                    });
                    var _com = new OracleCommand();
                    _com.Connection = (OracleConnection)_conn;
                    _com.CommandText = "insert into APPROVAL_Draft values (:orgeuid,:applicant,:type,:mybody)";
                    IsoDateTimeConverter timeConverter = new IsoDateTimeConverter();
                    timeConverter.DateTimeFormat = "yyyy'-'MM'-'dd' 'HH':'mm':'ss";
                    var body = JsonConvert.SerializeObject(param, timeConverter);
                    byte[] blob = Encoding.Default.GetBytes(body);
                    var filelength = blob.Length;
                    _com.Parameters.Clear();
                    _com.Parameters.AddRange(new OracleParameter[]{
                            new OracleParameter("orgeuid", param.orgeuid),
                                    new OracleParameter("applicant", param.euid),
                                    new OracleParameter("type", 1),
                                    new OracleParameter("mybody",OracleDbType.Blob,filelength,blob,System.Data.ParameterDirection.Input),
                                });
                    if (_conn.State == System.Data.ConnectionState.Closed)
                    {
                        _conn.Open();
                    }
                    _com.ExecuteNonQuery();
                    return new { status = 1, msg = "保存成功！" };
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "保存请假内容时发生异常。");
                return new { status = -1, msg = "保存失败！" };
            }
        }
        /// <summary>
        /// 获取请假草稿
        /// </summary>
        /// <param name="orgeuid">企业编号</param>
        /// <param name="euid">申请人编号</param>
        /// <returns></returns>
        public OffWorkModels GetOffworkDraft(string orgeuid, string euid)
        {
            try
            {
                using (var _conn = OpenConnection())
                {
                    var _com = new OracleCommand();
                    _com.Connection = (OracleConnection)_conn;
                    _com.CommandType = CommandType.Text;
                    _com.CommandText = "select DRAFTBODY from APPROVAL_DRAFT where ORGEUID = :orgeuid and APPLICANTEUID = :myeuid  and APPROVAL_TYPE = 1";
                    _com.Parameters.Clear();
                    _com.Parameters.Add(new OracleParameter("orgeuid", orgeuid));
                    _com.Parameters.Add(new OracleParameter("myeuid", euid));
                    var myRead = _com.ExecuteReader();
                    if (myRead.Read())
                    {
                        OracleBlob myLob = myRead.GetOracleBlob(0);
                        int myLength = Convert.ToInt32(myLob.Length);
                        byte[] Buffer = new byte[myLength];
                        myLob.Read(Buffer, 0, myLength);
                        var body = Encoding.Default.GetString(Buffer);
                        IsoDateTimeConverter timeConverter = new IsoDateTimeConverter();
                        timeConverter.DateTimeFormat = "yyyy'-'MM'-'dd' 'HH':'mm':'ss";
                        var result = JsonConvert.DeserializeObject<OffWorkModels>(body, timeConverter);
                        myRead.Close();
                        if (_conn.State == System.Data.ConnectionState.Closed)
                        {
                            _conn.Open();
                        }
                        _conn.Execute("delete APPROVAL_DRAFT where ORGEUID = :orgeuid and APPLICANTEUID = :myeuid and APPROVAL_TYPE = 1", new
                        {
                            orgeuid = orgeuid,
                            myeuid = euid
                        });
                        return result;
                    }
                    else {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "获取请假草稿时发生异常。");
                return null;
            }
        }

        /// <summary>
        /// 获取请假类型
        /// </summary>
        /// <returns></returns>
        public List<OffWorkTypeModels> GetOffWorkType()
        {
            try
            {
                using (var _conn = OpenConnection())
                {
                    return _conn.Query<OffWorkTypeModels>("select leavetype code, leavename name from APPROVAL_LEAVE_TYPE").ToList();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "获取请假类型时发生异常。");
                return new List<OffWorkTypeModels>();
            }
        }
        /// <summary>
        /// 获取报销费用类型
        /// </summary>
        /// <returns></returns>
        public List<ExpenseTypeModels> GetExpenseType()
        {
            try
            {
                using (var _conn = OpenConnection())
                {
                    return _conn.Query<ExpenseTypeModels>("select expense_type code, expense_name name from APPROVAL_EXPENSE_TYPE").ToList();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "获取报销费用类型时发生异常。");
                return new List<ExpenseTypeModels>();
            }
        }
        /// <summary>
        /// 获取指定的出差内容
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TripModels GetTripById(string id)
        {
            var result = new TripModels();
            try
            {
                using (var _conn = OpenConnection())
                {
                    var ConnectionString = ConfigurationManager.ConnectionStrings["BaseDbContext"].ConnectionString;
                    using (var myconn = OpenConnection(ConnectionString))
                    {
                        //第一步：获取基础数据
                        result = _conn.Query<TripModels>("select a.applytitle title,a.applyeuid euid,b.orgeuid,b.approvalcode,b.tripdays,b.tripreason from APPROVAL a join APPROVAL_TRIP b on a.approvalcode = b.approvalcode" +
                                " where a.approvalcode = :code", new { code = id }).FirstOrDefault();
                        result.name = myconn.Query<string>("select name from ec_employee where ORGANIZATIONEUID = :orgid and euid = :euid ", new { orgid = result.orgeuid, euid = result.euid }).FirstOrDefault();
                        result.approvalList = new List<ApprovalModels>();
                        result.copyto = new List<CopyToModels>();
                        result.imgs = new List<ImageModels>();
                        result.detail = new List<TripDetailModels>();
                        //第二步：获取图片
                        var _com = new OracleCommand();
                        _com.Connection = (OracleConnection)_conn;
                        _com.CommandType = CommandType.Text;
                        _com.CommandText = "select pic from APPROVAL_PIC where approvalcode = :code";
                        _com.Parameters.Clear();
                        _com.Parameters.Add(new OracleParameter("code", id));
                        var myRead = _com.ExecuteReader();
                        var i = 0;
                        while (myRead.Read())
                        {
                            OracleBlob myLob = myRead.GetOracleBlob(0);
                            int myLength = Convert.ToInt32(myLob.Length);
                            Byte[] Buffer = new byte[myLength];
                            myLob.Read(Buffer, 0, myLength);
                            result.imgs.Add(new ImageModels() { id = i.ToString(), body = Convert.ToBase64String(Buffer) });
                            i++;
                        }
                        myRead.Close();
                        //第三步：获取审批人列表
                        var approval = _conn.Query<ApprovalModels>("select approvaleuid euid,approvalreason reason,approvalresult result from APPROVALER where approvalcode = :code", new { code = id }).ToList();
                        for (i = 0; i < approval.Count; i++)
                        {
                            approval[i].name = myconn.Query<string>("select name from ec_employee where ORGANIZATIONEUID = :orgid and euid = :euid ", new { orgid = result.orgeuid, euid = approval[i].euid }).FirstOrDefault();
                            result.approvalList.Add(approval[i]);
                        }
                        //第四步：获取抄送人列表
                        var copyto = _conn.Query<CopyToModels>("select cceuid euid from APPROVAL_CCLIST where approvalcode = :code", new { code = id }).ToList();
                        for (i = 0; i < copyto.Count; i++)
                        {
                            copyto[i].name = myconn.Query<string>("select name from ec_employee where ORGANIZATIONEUID = :orgid and euid = :euid ", new { orgid = result.orgeuid, euid = copyto[i].euid }).FirstOrDefault();
                            result.copyto.Add(copyto[i]);
                        }
                        //第五步：获取明细列表
                        var detail = _conn.Query<TripDetailModels>("select orgeuid,approvalcode,tripseq,tripsite,to_char(tripstarttime,'yyyy-mm-dd') tripstarttime,to_char(tripendtime,'yyyy-mm-dd') tripendtime from APPROVAL_TRIP_DETAILS" +
                            " where approvalcode = :code", new { code = id }).ToList();
                        for (i = 0; i < detail.Count; i++)
                        {
                            result.detail.Add(detail[i]);
                        }
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "获取指定的出差内容时发生异常。");
                return null;
            }
        }
        /// <summary>
        /// 获取指定的报销内容
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ExpenseModels GetExpenseById(string id)
        {
            var result = new ExpenseModels();
            try
            {
                using (var _conn = OpenConnection())
                {
                    var ConnectionString = ConfigurationManager.ConnectionStrings["BaseDbContext"].ConnectionString;
                    using (var myconn = OpenConnection())
                    {
                        //第一步：获取基础数据
                        result = _conn.Query<ExpenseModels>("select a.applytitle title,a.applyeuid euid,b.orgeuid,b.approvalcode,b.expensetotal from APPROVAL a join APPROVAL_EXPENSE b on a.approvalcode = b.approvalcode" +
                                " where a.approvalcode = :code", new { code = id }).FirstOrDefault();
                        result.name = myconn.Query<string>("select name from ec_employee where ORGANIZATIONEUID = :orgid and euid = :euid ", new { orgid = result.orgeuid, euid = result.euid }).FirstOrDefault();
                        result.approvalList = new List<ApprovalModels>();
                        result.copyto = new List<CopyToModels>();
                        result.imgs = new List<ImageModels>();
                        result.detail = new List<ExpenseDetailModels>();
                        //第二步：获取图片
                        var _com = new OracleCommand();
                        _com.Connection = (OracleConnection)_conn;
                        _com.CommandType = CommandType.Text;
                        _com.CommandText = "select pic from APPROVAL_PIC where approvalcode = :code";
                        _com.Parameters.Clear();
                        _com.Parameters.Add(new OracleParameter("code", id));
                        var myRead = _com.ExecuteReader();
                        var i = 0;
                        while (myRead.Read())
                        {
                            OracleBlob myLob = myRead.GetOracleBlob(0);
                            int myLength = Convert.ToInt32(myLob.Length);
                            Byte[] Buffer = new byte[myLength];
                            myLob.Read(Buffer, 0, myLength);
                            result.imgs.Add(new ImageModels() { id = i.ToString(), body = Convert.ToBase64String(Buffer) });
                            i++;
                        }
                        myRead.Close();
                        //第三步：获取审批人列表
                        var approval = _conn.Query<ApprovalModels>("select approvaleuid euid,approvalreason reason,approvalresult result from APPROVALER where approvalcode = :code", new { code = id }).ToList();
                        for (i = 0; i < approval.Count; i++)
                        {
                            approval[i].name = myconn.Query<string>("select name from ec_employee where ORGANIZATIONEUID = :orgid and euid = :euid ", new { orgid = result.orgeuid, euid = approval[i].euid }).FirstOrDefault();
                            result.approvalList.Add(approval[i]);
                        }
                        //第四步：获取抄送人列表
                        var copyto = _conn.Query<CopyToModels>("select cceuid euid from APPROVAL_CCLIST where approvalcode = :code", new { code = id }).ToList();
                        for (i = 0; i < copyto.Count; i++)
                        {
                            copyto[i].name = myconn.Query<string>("select name from ec_employee where ORGANIZATIONEUID = :orgid and euid = :euid ", new { orgid = result.orgeuid, euid = copyto[i].euid }).FirstOrDefault();
                            result.copyto.Add(copyto[i]);
                        }
                        //第五步：获取明细列表
                        var detail = _conn.Query<ExpenseDetailModels>("select a.orgeuid,a.approvalcode,a.EXPENSESEQ,a.expense,a.expenseremark,b.EXPENSE_NAME EXPENSETYPE " +
                                    "from APPROVAL_EXPENSE_DETAILS a join APPROVAL_EXPENSE_TYPE b on a.EXPENSETYPE = b.EXPENSE_TYPE where approvalcode = :code order by a.EXPENSESEQ", new { code = id }).ToList();
                        for (i = 0; i < detail.Count; i++)
                        {
                            result.detail.Add(detail[i]);
                        }
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "获取指定的报销内容时发生异常。");
                return null;
            }
        }
        /// <summary>
        /// 获取指定的请示内容
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ConsultModels GetConsultById(string id)
        {
            var result = new ConsultModels();
            try
            {
                using (var _conn = OpenConnection())
                {
                    var ConnectionString = ConfigurationManager.ConnectionStrings["BaseDbContext"].ConnectionString;
                    using (var myconn = OpenConnection(ConnectionString))
                    {
                        //第一步：获取基础数据
                        result = _conn.Query<ConsultModels>("select a.applytitle title,a.applyeuid euid, b.orgeuid,b.approvalcode,b.requestreason reason from APPROVAL a join APPROVAL_REQUEST b on a.approvalcode = b.approvalcode" +
                                " where a.approvalcode = :code", new { code = id }).FirstOrDefault();
                        result.name = myconn.Query<string>("select name from ec_employee where ORGANIZATIONEUID = :orgid and euid = :euid ", new { orgid = result.orgeuid, euid = result.euid }).FirstOrDefault();
                        result.approvalList = new List<ApprovalModels>();
                        result.copyto = new List<CopyToModels>();
                        result.imgs = new List<ImageModels>();
                        //第二步：获取图片
                        var _com = new OracleCommand();
                        _com.Connection = (OracleConnection)_conn;
                        _com.CommandType = CommandType.Text;
                        _com.CommandText = "select pic from APPROVAL_PIC where approvalcode = :code";
                        _com.Parameters.Clear();
                        _com.Parameters.Add(new OracleParameter("code", id));
                        var myRead = _com.ExecuteReader();
                        var i = 0;
                        while (myRead.Read())
                        {
                            OracleBlob myLob = myRead.GetOracleBlob(0);
                            int myLength = Convert.ToInt32(myLob.Length);
                            Byte[] Buffer = new byte[myLength];
                            myLob.Read(Buffer, 0, myLength);
                            result.imgs.Add(new ImageModels() { id = i.ToString(), body = Convert.ToBase64String(Buffer) });
                            i++;
                        }
                        myRead.Close();
                        //第三步：获取审批人列表
                        var approval = _conn.Query<ApprovalModels>("select approvaleuid euid,approvalreason reason,approvalresult result from APPROVALER where approvalcode = :code", new { code = id }).ToList();
                        for (i = 0; i < approval.Count; i++)
                        {
                            approval[i].name = myconn.Query<string>("select name from ec_employee where ORGANIZATIONEUID = :orgid and euid = :euid ", new { orgid = result.orgeuid, euid = approval[i].euid }).FirstOrDefault();
                            result.approvalList.Add(approval[i]);
                        }
                        //第四步：获取抄送人列表
                        var copyto = _conn.Query<CopyToModels>("select cceuid euid from APPROVAL_CCLIST where approvalcode = :code", new { code = id }).ToList();
                        for (i = 0; i < copyto.Count; i++)
                        {
                            copyto[i].name = myconn.Query<string>("select name from ec_employee where ORGANIZATIONEUID = :orgid and euid = :euid ", new { orgid = result.orgeuid, euid = copyto[i].euid }).FirstOrDefault();
                            result.copyto.Add(copyto[i]);
                        }
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "获取指定的请示内容时发生异常。");
                return null;
            }
        }
        /// <summary>
        /// 获取指定的借款内容
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public BorrowingModels GetBorrowingById(string id)
        {
            var result = new BorrowingModels();
            try
            {
                using (var _conn = OpenConnection())
                {
                    var ConnectionString = ConfigurationManager.ConnectionStrings["BaseDbContext"].ConnectionString;
                    using (var myconn = OpenConnection(ConnectionString))
                    {
                        //第一步：获取基础数据
                        result = _conn.Query<BorrowingModels>("select a.applytitle title,a.applyeuid euid,b.orgeuid,b.approvalcode,b.loan,to_char(b.loanday,'yyyy-mm-dd') loanday,b.loanreson from APPROVAL a join APPROVAL_LOAN b on a.approvalcode = b.approvalcode" +
                        " where a.approvalcode = :code", new { code = id }).FirstOrDefault();
                        result.name = myconn.Query<string>("select name from ec_employee where ORGANIZATIONEUID = :orgid and euid = :euid ", new { orgid = result.orgeuid, euid = result.euid }).FirstOrDefault();
                        result.approvalList = new List<ApprovalModels>();
                        result.copyto = new List<CopyToModels>();
                        result.imgs = new List<ImageModels>();
                        //第二步：获取图片
                        var _com = new OracleCommand();
                        _com.Connection = (OracleConnection)_conn;
                        _com.CommandType = CommandType.Text;
                        _com.CommandText = "select pic from APPROVAL_PIC where approvalcode = :code";
                        _com.Parameters.Clear();
                        _com.Parameters.Add(new OracleParameter("code", id));
                        OracleDataReader myRead = _com.ExecuteReader();
                        var i = 0;
                        while (myRead.Read())
                        {
                            OracleBlob myLob = myRead.GetOracleBlob(0);
                            int myLength = Convert.ToInt32(myLob.Length);
                            Byte[] Buffer = new byte[myLength];
                            myLob.Read(Buffer, 0, myLength);
                            result.imgs.Add(new ImageModels() { id = i.ToString(), body = Convert.ToBase64String(Buffer) });
                            i++;
                        }
                        myRead.Close();
                        //第三步：获取审批人列表
                        var approval = _conn.Query<ApprovalModels>("select approvaleuid euid,approvalreason reason,approvalresult result from APPROVALER where approvalcode = :code", new { code = id }).ToList();
                        for (i = 0; i < approval.Count; i++)
                        {
                            approval[i].name = myconn.Query<string>("select name from ec_employee where ORGANIZATIONEUID = :orgid and euid = :euid ", new { orgid = result.orgeuid, euid = approval[i].euid }).FirstOrDefault();
                            result.approvalList.Add(approval[i]);
                        }
                        //第四步：获取抄送人列表
                        var copyto = _conn.Query<CopyToModels>("select cceuid euid from APPROVAL_CCLIST where approvalcode = :code", new { code = id }).ToList();
                        for (i = 0; i < copyto.Count; i++)
                        {
                            copyto[i].name = myconn.Query<string>("select name from ec_employee where ORGANIZATIONEUID = :orgid and euid = :euid ", new { orgid = result.orgeuid, euid = copyto[i].euid }).FirstOrDefault();
                            result.copyto.Add(copyto[i]);
                        }
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "获取指定的借款内容时发生异常。");
                return null;
            }
        }
        /// <summary>
        /// 获取指定的外出内容
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public AwayModels GetAwayById(string id)
        {
            var result = new AwayModels();
            try
            {
                using (var _conn = OpenConnection())
                {
                    var ConnectionString = ConfigurationManager.ConnectionStrings["BaseDbContext"].ConnectionString;
                    using (var myconn = OpenConnection(ConnectionString))
                    {
                        //第一步：获取基础数据
                        result = _conn.Query<AwayModels>("select a.applytitle title,a.applyeuid euid, b.orgeuid,b.approvalcode,to_char(b.outstarttime,'yyyy-mm-dd hh24:mi') outstarttime,to_char(b.outendtime,'yyyy-mm-dd hh24:mi') outendtime,b.outhours,b.outreason " +
                        "from APPROVAL a join APPROVAL_OUT b on a.approvalcode = b.approvalcode " +
                        " where a.approvalcode = :code", new { code = id }).FirstOrDefault();
                        result.name = myconn.Query<string>("select name from ec_employee where ORGANIZATIONEUID = :orgid and euid = :euid ", new { orgid = result.orgeuid, euid = result.euid }).FirstOrDefault();
                        result.approvalList = new List<ApprovalModels>();
                        result.copyto = new List<CopyToModels>();
                        result.imgs = new List<ImageModels>();
                        //第二步：获取图片
                        var _com = new OracleCommand();
                        _com.Connection = (OracleConnection)_conn;
                        _com.CommandType = CommandType.Text;
                        _com.CommandText = "select pic from APPROVAL_PIC where approvalcode = :code";
                        _com.Parameters.Clear();
                        _com.Parameters.Add(new OracleParameter("code", id));
                        OracleDataReader myRead = _com.ExecuteReader();
                        var i = 0;
                        while (myRead.Read())
                        {
                            OracleBlob myLob = myRead.GetOracleBlob(0);
                            int myLength = Convert.ToInt32(myLob.Length);
                            Byte[] Buffer = new byte[myLength];
                            myLob.Read(Buffer, 0, myLength);
                            result.imgs.Add(new ImageModels() { id = i.ToString(), body = Convert.ToBase64String(Buffer) });
                            i++;
                        }
                        myRead.Close();
                        //第三步：获取审批人列表
                        var approval = _conn.Query<ApprovalModels>("select approvaleuid euid,approvalreason reason,approvalresult result from APPROVALER where approvalcode = :code", new { code = id }).ToList();
                        for (i = 0; i < approval.Count; i++)
                        {
                            approval[i].name = myconn.Query<string>("select name from ec_employee where ORGANIZATIONEUID = :orgid and euid = :euid ", new { orgid = result.orgeuid, euid = approval[i].euid }).FirstOrDefault();
                            result.approvalList.Add(approval[i]);
                        }
                        //第四步：获取抄送人列表
                        var copyto = _conn.Query<CopyToModels>("select cceuid euid from APPROVAL_CCLIST where approvalcode = :code", new { code = id }).ToList();
                        for (i = 0; i < copyto.Count; i++)
                        {
                            copyto[i].name = myconn.Query<string>("select name from ec_employee where ORGANIZATIONEUID = :orgid and euid = :euid ", new { orgid = result.orgeuid, euid = copyto[i].euid }).FirstOrDefault();
                            result.copyto.Add(copyto[i]);
                        }
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "获取指定的外出内容时发生异常。");
                return null;
            }
        }
        /// <summary>
        /// 获取指定的请假内容
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public OffWorkModels GetOffworkById(string id)
        {
            var result = new OffWorkModels();
            try
            {
                using (var _conn = OpenConnection())
                {
                    var ConnectionString = ConfigurationManager.ConnectionStrings["BaseDbContext"].ConnectionString;
                    using (var myconn = OpenConnection(ConnectionString))
                    {
                        //第一步：获取基础数据
                        result = _conn.Query<OffWorkModels>(" select a.applytitle title,a.applyeuid euid,b.orgeuid,b.approvalcode,c.leavename leavetype,to_char(b.leavestarttime,'yyyy-mm-dd') leavestarttime,to_char(b.leaveendtime,'yyyy-mm-dd') leaveendtime, floor(b.leavehours / 24) || '天' || mod(b.leavehours,24) || '小时' leavehours,b.leavereason " +
                         "from approval a join approval_leave b on a.approvalcode = b.approvalcode join approval_leave_type c " +
                         " on b.leavetype = c.leavetype where a.approvalcode = :code", new { code = id }).FirstOrDefault();
                        result.name = myconn.Query<string>("select name from ec_employee where ORGANIZATIONEUID = :orgid and euid = :euid ", new { orgid = result.orgeuid, euid = result.euid }).FirstOrDefault();
                        result.approvalList = new List<ApprovalModels>();
                        result.copyto = new List<CopyToModels>();
                        result.imgs = new List<ImageModels>();
                        //第二步：获取图片
                        var _com = new OracleCommand();
                        _com.Connection = (OracleConnection)_conn;
                        _com.CommandType = CommandType.Text;
                        _com.CommandText = "select pic from APPROVAL_PIC where approvalcode = :code";
                        _com.Parameters.Clear();
                        _com.Parameters.Add(new OracleParameter("code", id));
                        OracleDataReader myRead = _com.ExecuteReader();
                        var i = 0;
                        while (myRead.Read())
                        {
                            OracleBlob myLob = myRead.GetOracleBlob(0);
                            int myLength = Convert.ToInt32(myLob.Length);
                            Byte[] Buffer = new byte[myLength];
                            myLob.Read(Buffer, 0, myLength);
                            result.imgs.Add(new ImageModels() { id = i.ToString(), body = Convert.ToBase64String(Buffer) });
                            i++;
                        }
                        myRead.Close();
                        //第三步：获取审批人列表
                        var approval = _conn.Query<ApprovalModels>("select a.approvaleuid euid,approvalreason reason, a.approvalresult result from APPROVALER a where a.approvalcode = :code", new { code = id }).ToList();
                        for (i = 0; i < approval.Count; i++)
                        {
                            approval[i].name = myconn.Query<string>("select name from ec_employee where ORGANIZATIONEUID = :orgid and euid = :euid ", new { orgid = result.orgeuid, euid = approval[i].euid }).FirstOrDefault();
                            result.approvalList.Add(approval[i]);
                        }
                        //第四步：获取抄送人列表
                        var copyto = _conn.Query<CopyToModels>("select cceuid euid from APPROVAL_CCLIST where approvalcode = :code", new { code = id }).ToList();
                        for (i = 0; i < copyto.Count; i++)
                        {
                            copyto[i].name = myconn.Query<string>("select name from ec_employee where ORGANIZATIONEUID = :orgid and euid = :euid ", new { orgid = result.orgeuid, euid = copyto[i].euid }).FirstOrDefault();
                            result.copyto.Add(copyto[i]);
                        }
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, " 获取指定的请假内容时发生异常。");
                return null;
            }
        }
        /// <summary>
        /// 处理申请
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public object ProcessApproval(ApprovalProcessModels param)
        {
            using (var _conn = OpenConnection())
            {
                IDbTransaction tran = null;
                try
                {
                    tran = _conn.BeginTransaction();
                    _conn.Execute("update APPROVALER set approvaltime = sysdate,approvalresult = :result,approvalreason = :opinion where approvalcode = :mycode and approvaleuid = :myid",
                       new
                       {
                           mycode = param.approvalcode,
                           myid = param.approvaleuid,
                           opinion = param.opinion,
                           result = param.result
                       }, tran);
                    var r = _conn.Query<int>("select count(*) from APPROVALER where approvalresult = 0 and approvalcode = :mycode",
                        new { mycode = param.approvalcode }, tran).FirstOrDefault();
                    if (r <= 0)
                    {
                        _conn.Execute("update APPROVAL set apply = 4, approvaltime = sysdate, approvaleuid = :euid, approvalresult = :result, approvalreason = :opinion where approvalcode = :mycode",
                       new
                       {
                           mycode = param.approvalcode,
                           euid = param.approvaleuid,
                           opinion = param.opinion,
                           result = param.result
                       }, tran);
                    }

                    tran.Commit();
                    return new { status = 1, msg = "保存成功！" };
                }
                catch (Exception ex)
                {
                    if (tran != null)
                    {
                        tran.Rollback();
                    }
                    log.Error(ex, "处理申请时发生异常。");
                    return new { status = -1, msg = "保存失败！" };
                }
            }
        }
        /// <summary>
        /// 设置已读
        /// </summary>
        /// <param name="approvalcode">审批编号</param>
        /// <param name="euid">审批人编号</param>
        /// <returns></returns>
        public object isRead(string approvalcode, string euid)
        {
            using (var _conn = OpenConnection())
            {
                IDbTransaction tran = null;
                try
                {
                    tran = _conn.BeginTransaction();
                    var flag = _conn.Query<int>("select count(*) from APPROVALER where approvalcode = :mycode and approvaleuid = :myid and isread = 1", new { mycode = approvalcode, myid = euid }).FirstOrDefault();
                    if (flag > 0)
                    {
                        _conn.Execute("update APPROVALER set isread = 2,readtime = sysdate where approvalcode = :mycode and approvaleuid = :myid",
                            new
                            {
                                mycode = approvalcode,
                                myid = euid
                            }, tran);
                    }
                    flag = _conn.Query<int>("select count(*) from APPROVAL_CCLIST where approvalcode = :mycode and cceuid = :myid and isread = 1", new { mycode = approvalcode, myid = euid }).FirstOrDefault();
                    if (flag > 0)
                    {
                        _conn.Execute("update APPROVAL_CCLIST set isread = 2,readtime = sysdate where approvalcode = :mycode and cceuid = :myid",
                       new
                       {
                           mycode = approvalcode,
                           myid = euid
                       }, tran);
                    }
                    tran.Commit();
                    return new { status = 1, msg = "保存成功！" };
                }
                catch (Exception ex)
                {
                    if (tran != null)
                    {
                        tran.Rollback();
                    }
                    log.Error(ex, "设置已读状态时发生异常。");
                    return new { status = -1, msg = "保存失败！" };
                }
            }
        }
        /// <summary>
        /// 获取未处理审批和抄送给我的数量
        /// </summary>
        /// <param name="orgid"></param>
        /// <param name="euid"></param>
        /// <returns></returns>
        public object GetCount(string orgid, string euid)
        {
            try
            {
                using (var _conn = OpenConnection())
                {
                    return new
                    {
                        unprocesscount = _conn.Query<int>("select count(*) from APPROVALER where ORGEUID = :mycode and approvaleuid = :myid and APPROVALRESULT = 0", new { mycode = orgid, myid = euid }).FirstOrDefault(),
                        copyunreadcount = _conn.Query<int>("select count(*) from APPROVAL_CCLIST where ORGEUID = :mycode and cceuid = :myid and isread = 1", new { mycode = orgid, myid = euid }).FirstOrDefault()
                    };
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "获取未处理审批和抄送给我的数量时发生异常。");
                return new { unprocesscount = 0, copyunreadcount = 0 };
            }
        }

        /// <summary>
        /// 发送通知
        /// </summary>
        /// <param name="receverid">接收者euid</param>
        /// <param name="sendername">发送者姓名</param>
        /// <param name="noticetitle">信息内容</param>
        /// <returns></returns>
        public object Send(string receverid, string sendername, string noticetitle)
        {
            try
            {
                var ConnectionString = ConfigurationManager.ConnectionStrings["BaseDbContext"].ConnectionString;
                var mediatype = ConfigurationManager.AppSettings["mediatype"];
                var times = DateTime.Now.Year.ToString() + "/" + DateTime.Now.Month.ToString() + "/" + DateTime.Now.Day.ToString() + " " + DateTime.Now.Hour.ToString() + ":" + (DateTime.Now.Minute.ToString().Length == 1 ? ("0" + DateTime.Now.Minute.ToString()) : DateTime.Now.Minute.ToString());
                using (var myconn = OpenConnection(ConnectionString))
                {
                    myconn.Execute("insert into SEND_INFO_TABLE (APP_ID,RECV_USER,INFO_TYPE,INFO_CONTENT) values ('WebAPI_Announce',:receverid,:mediatype,:infocontent)",
                        new
                        {
                            receverid = receverid,
                            mediatype = mediatype,
                            infocontent = JsonConvert.SerializeObject(new
                            {
                                Display = 1,
                                Title = sendername,
                                Content = noticetitle,
                                Time = times
                            })
                        });
                    return new { status = 1, msg = "保存成功！" };
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "发送通知时发生异常。");
                return new { status = -1, msg = "保存失败！" };
            }
        }
        /// <summary>
        /// 打开数据库链接
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        private IDbConnection OpenConnection(string connection = null)
        {
            IDbConnection _conn;
            if (string.IsNullOrEmpty(connection))
            {
                var conString = ConfigurationManager.ConnectionStrings["OracleDbContext"].ConnectionString;
                _conn = new OracleConnection(conString);
            }
            else {
                _conn = new OracleConnection(connection);
            }
            if (_conn.State != ConnectionState.Open)
            {
                _conn.Open();
            }
            return _conn;
        }
    }
}