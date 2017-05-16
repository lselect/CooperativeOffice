using System.Collections.Generic;

namespace CooperativeOffice.Models
{
    /// <summary>
    /// 报销费用类型数据模型
    /// </summary>
    public class ExpenseTypeModels
    {
        /// <summary>
        /// 编号
        /// </summary>
        public string code { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string name { get; set; }
    }
    /// <summary>
    /// 报销数据模型
    /// </summary>
    public class ExpenseModels
    {
        /// <summary>
        /// 企业编号
        /// </summary>
        public string orgeuid { get; set; }
        /// <summary>
        /// 报销人编号
        /// </summary>
        public string euid { get; set; }
        /// <summary>
        /// 报销人姓名
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// 申请编号
        /// </summary>
        public string approvalcode { get; set; }
        /// <summary>
        /// 总金额
        /// </summary>
        public double expensetotal { get; set; }
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
        public List<ExpenseDetailModels> detail { get; set; }
    }
    /// <summary>
    /// 报销明细数据模型
    /// </summary>
    public class ExpenseDetailModels
    {
        /// <summary>
        /// 企业编号
        /// </summary>
        public string orgeuid { get; set; }
        /// <summary>
        /// 申请编号
        /// </summary>
        public string approvalcode { get; set; }
        /// <summary>
        /// 费用序号
        /// </summary>

        public int expenseseq { get; set; }
        /// <summary>
        /// 费用类型
        /// </summary>
        public string expensetype { get; set; }
        /// <summary>
        /// 费用
        /// </summary>
        public double expense { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string expenseremark { get; set; }
    }
}