using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CooperativeOffice.Models
{
    public class AwayModels
    {
        /// <summary>
        /// 企业编号
        /// </summary>
        public string orgeuid { get; set; }
        /// <summary>
        /// 外出人编号
        /// </summary>
        public string euid { get; set; }
        /// <summary>
        /// 外出人姓名
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
        /// 外出开始日期
        /// </summary>
        public string outstarttime { get; set; }
        /// <summary>
        /// 外出结束日期
        /// </summary>
        public string outendtime { get; set; }
        /// <summary>
        /// 外出时长
        /// </summary>
        public string outhours { get; set; }
        /// <summary>
        /// 外出事由
        /// </summary>
        public string outreason { get; set; }
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
    }
}