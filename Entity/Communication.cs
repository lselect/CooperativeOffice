using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RestSharp;
using RestSharp.Authenticators;
using Newtonsoft.Json;
using System.Configuration;
using NLog;

namespace CooperativeOffice.Entity
{
    public class Communication
    {
        /// <summary>
        /// 日志实例
        /// </summary>
        protected Logger log = LogManager.GetCurrentClassLogger();
        private RestClient client;
        public Communication() {
            try
            {
                var smsString = ConfigurationManager.AppSettings["smsServiceUrl"];
                client = new RestClient(smsString);
            }
            catch (Exception ex)
            {
                log.Error(ex, "获取配置文件参数时发生异常。");
            }
        }
        /// <summary>
        /// 发送通知
        /// </summary>
        /// <param name="phonenumber">接收方电话号码</param>
        /// <param name="msg">消息</param>
        public void SendNew(string phonenumber,string msg) {
            //IRestRequest request = new RestRequest("",Method.GET);
        }
    }
}