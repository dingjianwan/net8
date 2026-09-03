using SqlSugar;

namespace WebApplication1 {
    public class ApiModels
    {
        /// <summary>
        /// 标准状态返回接口
        /// </summary>
        public class ResultModel
        {
            /// <summary>
            /// 接口调用结果
            /// </summary>
            public bool success { get; set; }
            /// <summary>
            /// 接口反馈码
            /// </summary>
            public int errCode { get; set; }
            /// <summary>
            /// 错误说明
            /// </summary>
            public string errMsg { get; set; } = "";
        }

        #region     hosttoagv-实体模型
        /// <summary>
        /// 返回给hosttoagv
        /// </summary>
        public class ReturnResult
        {
            public int ResultCode { get; set; }
            public string ResultMsg { get; set; }
        }
        /// <summary>
        /// hosttoagv上报任务状态{"task_no":"TN2604210282","state":1,"forklift_no":1}
        /// </summary>
        public class AgvTaskState
        {
            public int state { get; set; }
            public string task_no { get; set; }
            public string forklift_no { get; set; }
            public string lock_no { get; set; }
            public string ext_data { get; set; }
        }


        /// <summary>
        /// hosttoagv上报车辆状态（参数在hosttoagv服务配置，可修改）
        /// </summary>
        public class AgvDeviceStatus
        {
            public string forkliftNo { get; set; } = "";
            public string errCode { get; set; } = "";
            public string errCode2 { get; set; } = "";
            public string faildCode { get; set; } = "";
            public string xPos { get; set; } = "";
            public string yPos { get; set; } = "";
            public string battery { get; set; } = "";
            public string detail { get; set; } = "";
            public string infoType { get; set; } = "";
            public string inMapRoute { get; set; } = "";
            public string CumInfo { get; set; } = "";
            public string agvCurrTaskInfo { get; set; } = "";
            /// <summary>
            /// 1----手动 ，2----半自动 ， 3----自动
            /// </summary>
            public string AutoMode { get; set; } = "";
            /// <summary>
            /// 0----未充电 ，1----充电状态
            /// </summary>
            public string chargedstate { get; set; } = "";
        }
        /// <summary>
        /// hosttoagv上报其它事件信息
        /// </summary>
        public class AgvEventInfo
        {
            public int Code { get; set; }
            public string CarID { get; set; }
            public string Param1 { get; set; }
            public string Param2 { get; set; }
            public string Param3 { get; set; }
        }
        #endregion

        #region RestAPI-RCS-实体模型

        public class RCSReturnResult
        {
            public int code { get; set; }
            public string msg { get; set; }
        }
        /// <summary>
        /// 订单状态上报接口数据模型
        /// </summary>
        public class OrderStatusModel
        {
            /// <summary>
            /// 订单ID
            /// </summary>
            public int orderID { get; set; }
            /// <summary>
            /// 订单名称
            /// </summary>
            public string orderName { get; set; }
            /// <summary>
            /// 订单状态
            /// 一个标准p2p点到点搬运，订单状态的变化过程：waiting、active、dispatched、source_finish、dest_finish、finish
	        ///waiting -新订单；
	        ///active -任务开始；
	        ///dispatched -调度派车；
	        ///source_finish -取货完成；
	        ///dest_finish -卸货完成；
	        ///finish -订单完成；
	        ///error -订单出错；
	        ///waiting_cancel -订单取消中；
	        ///cancel_finish -订单取消完成；
	        ///waiting_manually_finish -订单手动完成中；
	        ///manually_finish -订单手动完成；
            /// </summary>
            public string orderStatus { get; set; }
            /// <summary>
            /// 指派的AGV的ID列表
            /// </summary>
            public string agvIDList { get; set; }
            /// <summary>
            /// 订单优先级
            /// </summary>
            public int priority { get; set; }
            /// <summary>
            /// 当前目的地
            /// </summary>
            public string currentDes { get; set; }
            /// <summary>
            /// 当前指令
            /// </summary>
            public string currentCmd { get; set; }
            /// <summary>
            /// 错误码
            /// </summary>
            public int errorCode { get; set; }
            /// <summary>
            /// 订单的截至时间
            /// </summary>
            public string deadLine { get; set; }
            /// <summary>
            /// 订单的创建时间
            /// </summary>
            public string createdTime { get; set; }
            /// <summary>
            /// 额外信息1
            /// </summary>
            public string extraInfo1 { get; set; }
            /// <summary>
            /// 额外信息2
            /// </summary>
            public string extraInfo2 { get; set; }
            /// <summary>
            /// 状态变更时间
            /// </summary>
            public string StatusChangeTime { get; set; }
        }

        /// <summary>
        /// 安全交互接口数据模型
        /// </summary>
        public class SafetyInteractionModel
        {
            /// <summary>
            /// 机台，输送线安全交互时，请求取货/卸货的站台库位名称，例如work6、work8；
            /// 物流门安全交互时，请求通过的区域名称例如Door1、DoorYL1；
            /// 电梯的安全交互时，请求的设备名称，例如Lift1、LiftSMT
            /// </summary>
            public string device_name { get; set; }
            /// <summary>
            /// 设备类型，“Station”，“Area”，“Lift”
            /// </summary>
            public string device_type { get; set; }
            /// <summary>
            /// 请求码
            /// </summary>
            public string apply_code { get; set; }
            /// <summary>
            /// 目标楼层
            /// </summary>
            public int target_floor { get; set; }
            /// <summary>
            /// 订单id
            /// </summary>
            public int Order_id { get; set; }

        }


        #endregion

        /// <summary>
        /// HangChaAGV
        /// </summary>
        [SugarTable("dbo.HangChaAGV")]
        public class HangChaAGV : BaseModel
        {
            public string ext4 { get; set; }
            public string ext3 { get; set; }
            public string ext2 { get; set; }
            public string ext1 { get; set; }
            public DateTime createDate { get; set; }
            public string agvErrMsg { get; set; }
            public string faildCode { get; set; }
            public string ext5 { get; set; }
            public string errCode2 { get; set; }
            public string agvRunStatus { get; set; }
            public string agvCurrTaskInfo { get; set; }
            public string agvBattery { get; set; }
            public string agvYPos { get; set; }
            public string agvXPos { get; set; }
            public string agvNo { get; set; }
            public string agvErrCode { get; set; }
            public string ext6 { get; set; }
        }
    }
}
