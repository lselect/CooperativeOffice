using System.Collections.Generic;

namespace CooperativeOffice.Models
{
    public class BorrowingModels
    {
        /// <summary>
        /// 企业编号
        /// </summary>
        public string orgeuid { get; set; }
        /// <summary>
        /// 请假人编号
        /// </summary>
        public string euid { get; set; }
        /// <summary>
        /// 请假人姓名
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
        /// 借款额
        /// </summary>
        public double loan { get; set; }
        /// <summary>
        /// 领款日期
        /// </summary>
        public string loanday { get; set; }
        /// <summary>
        /// 借款原因
        /// </summary>
        public string loanreson { get; set; }
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