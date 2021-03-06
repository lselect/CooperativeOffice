﻿namespace CooperativeOffice.Models
{
    /// <summary>
    /// 审批人数据模型
    /// </summary>
    public class ApprovalModels
    {
        /// <summary>
        /// 审批人编号
        /// </summary>
        public string euid { get; set; }
        /// <summary>
        /// 审批人姓名
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 意见
        /// </summary>
        public string reason { get; set; }
        /// <summary>
        /// 审批结果
        /// </summary>
        public string result { get; set; }
    }
    /// <summary>
    /// 抄送人数据模型
    /// </summary>
    public class CopyToModels
    {
        /// <summary>
        /// 抄送人编号
        /// </summary>
        public string euid { get; set; }
        /// <summary>
        /// 抄送人姓名
        /// </summary>
        public string name { get; set; }
    }
}