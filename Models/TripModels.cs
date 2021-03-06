﻿using System;
using System.Collections.Generic;

namespace CooperativeOffice.Models
{
    /// <summary>
    /// 出差数据模型
    /// </summary>
    public class TripModels
    {
        /// <summary>
        /// 企业编号
        /// </summary>
        public string orgeuid { get; set; }
        /// <summary>
        /// 出差人编号
        /// </summary>
        public string euid { get; set; }
        /// <summary>
        /// 出差人姓名
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// 审批编号
        /// </summary>
        public string approvalcode { get; set; }
        /// <summary>
        /// 出差天数
        /// </summary>
        public int tripdays { get; set; }
        /// <summary>
        /// 出差原因
        /// </summary>
        public string tripreason { get; set; }
        /// <summary>
        /// 审批人
        /// </summary>
        public List<ApprovalModels> approvalList { get; set; }
        /// <summary>
        /// 抄送人
        /// </summary>
        public List<CopyToModels> copyto { get; set; }
        /// <summary>
        /// 图片
        /// </summary>
        public List<ImageModels> imgs { get; set; }
        /// <summary>
        /// 明细列表
        /// </summary>
        public List<TripDetailModels> detail { get; set; }
    }
    /// <summary>
    /// 明细数据模型
    /// </summary>
    public class TripDetailModels
    {
        /// <summary>
        /// 企业编号
        /// </summary>
        public string orgeuid { get; set; }
        /// <summary>
        /// 审批编号
        /// </summary>
        public string approvalcode { get; set; }
        /// <summary>
        /// 行程序号
        /// </summary>
        public string tripseq { get; set; }
        /// <summary>
        /// 行程目的地
        /// </summary>
        public string tripsite { get; set; }
        /// <summary>
        /// 行程开始日期
        /// </summary>
        public string tripstarttime { get; set; }
        /// <summary>
        /// 行程结束日期
        /// </summary>
        public string tripendtime { get; set; }
    }
}