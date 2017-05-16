namespace CooperativeOffice.Models
{
    /// <summary>
    /// 处理审批数据模型
    /// </summary>
    public class ApprovalProcessModels
    {
        /// <summary>
        /// 审批编号
        /// </summary>
        public string approvalcode { get; set; }
        /// <summary>
        /// 审批人EUID
        /// </summary>
        public string approvaleuid { get; set; }
        /// <summary>
        /// 审批意见
        /// </summary>
        public string opinion { get; set; }
        /// <summary>
        /// 审批结果
        /// </summary>
        public int result { get; set; }
    }
}