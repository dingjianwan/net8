using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication1 {
    /// <summary>
    /// 基础实体模型（对应数据库表结构）
    /// </summary>
    public class BaseModel {
        /// <summary>
        /// 标识
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public string S_ID { get; set; } = Guid.NewGuid().ToString("D");

        /// <summary>
        /// 状态
        /// </summary>
        public string S_STATE { get; set; } = "";

        /// <summary>
        /// 创建者标识
        /// </summary>
        public string S_CREATOR_ID { get; set; } = "sa";

        /// <summary>
        /// 原状态
        /// </summary>
        public string S_STATE_PRE { get; set; }

        /// <summary>
        /// 创建者名称
        /// </summary>
        public string S_CREATOR_NAME { get; set; } = "超级用户";

        /// <summary>
        /// 审核结果
        /// </summary>
        public int N_REVIEW_RESULT { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime T_CREATE { get; set; } = DateTime.Now;

        /// <summary>
        /// 审核意见
        /// </summary>
        public string S_REVIEW_COMMENT { get; set; }

        /// <summary>
        /// 创建部门标识
        /// </summary>
        public string S_DEPART_ID { get; set; }

        /// <summary>
        /// 创建部门名称
        /// </summary>
        public string S_DEPART_NAME { get; set; }

        /// <summary>
        /// 修改者标识
        /// </summary>
        public string S_MODIFIER_ID { get; set; }

        /// <summary>
        /// 修改者名称
        /// </summary>
        public string S_MODIFIER_NAME { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime T_MODIFY { get; set; } = DateTime.Now;

        /// <summary>
        /// 创建方法
        /// </summary>
        public int N_CREATEMETHOD { get; set; }

        /// <summary>
        /// 数据来源对象
        /// </summary>
        public string G_SOURCE_OBJ { get; set; }
    }

    #region 入库

    #region 入库单
    /// <summary>
    /// 入库单模型
    /// 表名：dbo.TN_Inbound_Order
    /// 继承 BaseModel 基础字段
    /// </summary>
    [SugarTable("dbo.TN_Inbound_Order")]
    public class InboundOrderModel : BaseModel {
        /// <summary>
        /// 入库单号
        /// </summary>
        public string S_NO { get; set; }

        /// <summary>
        /// 供应商编号
        /// </summary>
        public string S_SUPPLIER_NO { get; set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
        public string S_SUPPLIER_NAME { get; set; }

        /// <summary>
        /// 工厂标识
        /// </summary>
        public string S_FACTORY { get; set; }

        /// <summary>
        /// 仓库编码
        /// </summary>
        public string S_WH_CODE { get; set; }

        /// <summary>
        /// 库区编码
        /// </summary>
        public string S_AREA_CODE { get; set; }

        /// <summary>
        /// 来源类型
        /// </summary>
        public string S_BS_TYPE { get; set; }

        /// <summary>
        /// 来源单号
        /// </summary>
        public string S_BS_NO { get; set; }

        /// <summary>
        /// 业务状态 
        /// 0-未执行 1-组盘 2-组盘完成 3-入库完成
        /// </summary>
        public int N_B_STATE { get; set; }
        /// <summary>
        /// 状态名称
        /// </summary>
        public string S_B_STATE { get; set; }
        public string GetStateStr() {
            switch (N_B_STATE) {
                case 0:
                    return "未执行";
                case 1:
                    return "组盘";
                case 2:
                    return "组盘完成";
                case 3:
                    return "入库完成";
                default:
                    return "未知状态";
            }
        }
        /// <summary>
        /// 完工回报状态
        /// </summary>
        public int N_CR_STATE { get; set; }

        /// <summary>
        /// 完工回报时间
        /// </summary>
        public DateTime T_CR { get; set; }

        /// <summary>
        /// 完工回报错误
        /// </summary>
        public string S_CR_ERR { get; set; }

        /// <summary>
        /// 波次号
        /// </summary>
        public string S_WAVE_NO { get; set; }

        /// <summary>
        /// 创建方式
        /// </summary>
        public string S_CREATE_METHOD { get; set; }

        /// <summary>
        /// 工作台
        /// </summary>
        public string S_STATION_NO { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        public string S_OPERATOR_NAME { get; set; }

        /// <summary>
        /// 操作人账号
        /// </summary>
        public string S_OPERATOR { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string S_NOTE { get; set; }

        /// <summary>
        /// 业务类型
        /// </summary>
        public string S_OP_TYPE { get; set; }

        /// <summary>
        /// 商品种类
        /// </summary>
        public int N_GOOD_TYPE_NUM { get; set; }

        /// <summary>
        /// 总数
        /// </summary>
        public int N_TOTAL_QTY { get; set; }
    }

    /// <summary>
    /// 入库单明细
    /// </summary>
    [SugarTable("dbo.TN_Inbound_Detail")]
    public class InboundOrderDetailModel : BaseModel {
        /// <summary>
        /// 入库单号
        /// </summary>
        public string S_IO_NO { get; set; }

        /// <summary>
        /// 商品编码
        /// </summary>
        public string S_ITEM_CODE { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string S_ITEM_NAME { get; set; }

        /// <summary>
        /// 商品状态名称
        /// </summary>
        public string S_ITEM_STATE { get; set; }

        /// <summary>
        /// 批次号
        /// </summary>
        public string S_BATCH_NO { get; set; }

        /// <summary>
        /// 商品规格
        /// </summary>
        public string S_ITEM_SPEC { get; set; }

        /// <summary>
        /// 产品序列号
        /// </summary>
        public string S_SERIAL_NO { get; set; }

        /// <summary>
        /// 生产日期
        /// </summary>
        public string D_PRD_DATE { get; set; }

        /// <summary>
        /// 过期日期
        /// </summary>
        public string D_EXP_DATE { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string S_NOTE { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public decimal F_QTY { get; set; }
        public decimal F_ACC_E_QTY { get; set; }
        /// <summary>
        /// 计量单位
        /// </summary>
        public string S_UOM { get; set; }

        /// <summary>
        /// 累计关闭数量
        /// </summary>
        public float F_ACC_C_QTY { get; set; }

        /// <summary>
        /// 累计绑定数量
        /// </summary>
        public float F_ACC_B_QTY { get; set; }

        /// <summary>
        /// 业务状态 0-未执行 1-组盘 2-组盘完成 3-入库完成
        /// </summary>
        public int N_B_STATE { get; set; }
        /// <summary>
        /// 状态名称
        /// </summary>
        public string S_B_STATE { get; set; }

        /// <summary>
        /// 来源单号
        /// </summary>
        public string S_BS_NO { get; set; }

        /// <summary>
        /// 来源类型
        /// </summary>
        public string S_BS_TYPE { get; set; }

        /// <summary>
        /// 供应商编码
        /// </summary>
        public string S_SUPPLIER_NO { get; set; }

        /// <summary>
        /// 货主
        /// </summary>
        public string S_OWNER { get; set; }

        /// <summary>
        /// ERP仓库
        /// </summary>
        public string S_ERP_WH_CODE { get; set; }

        /// <summary>
        /// 单体重量
        /// </summary>
        public float F_WEIGHT { get; set; }

        /// <summary>
        /// 单个体积
        /// </summary>
        public float F_VOLUME { get; set; }

        /// <summary>
        /// 最小料格
        /// </summary>
        public string S_CELL_TYPE { get; set; }

        /// <summary>
        /// 累计入库数量
        /// </summary>
        public float F_ACC_I_QTY { get; set; }

        /// <summary>
        /// 打印数量
        /// </summary>
        public int N_PRINT_QTY { get; set; }

        /// <summary>
        /// 打印份数
        /// </summary>
        public int N_PRINT_NUM { get; set; }

        /// <summary>
        /// 来源单行号
        /// </summary>
        public int N_ROW_NO { get; set; }

    }

    #endregion

    #region 上架单
    /// <summary>
    /// 中能上架单
    /// </summary>
    [SugarTable("dbo.TN_ZN_OnOff")]
    public class ZNListingForm : BaseModel {
        /// <summary>
        /// 入库单号
        /// </summary>
        public string S_NO { get; set; }
        /// <summary>
        /// 上架单号
        /// </summary>
        public string S_ON_CODE { get; set; }
        /// <summary>
        /// 业务状态
        /// </summary>
        public string S_B_STATE { get; set; }
        /// <summary>
        /// 业务状态值
        /// </summary>
        public int N_B_STATE { get; set; }

    }

    /// <summary>
    /// 中能上架单明细
    /// </summary>
    [SugarTable("dbo.TN_ZN_OnOff_Detail")]
    public class ZNOnShelfDetail : BaseModel {
        /// <summary>
        /// 上架单号
        /// </summary>
        public string S_ON { get; set; }

        /// <summary>
        /// 商品编码
        /// </summary>
        public string S_ITEM_CODE { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string S_ITEM_NAME { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int F_QTY { get; set; }

        /// <summary>
        /// 来源单号
        /// </summary>
        public string S_BS_NO { get; set; }

        /// <summary>
        /// 入库单号
        /// </summary>
        public string S_IO_NO { get; set; }

        /// <summary>
        /// 批次号
        /// </summary>
        public string S_BATCH_NO { get; set; }

        /// <summary>
        /// 资料号
        /// </summary>
        public string S_DATE_NUMBER { get; set; }

        /// <summary>
        /// 清单编码
        /// </summary>
        public string S_LIST_NO { get; set; }

        /// <summary>
        /// 项目
        /// </summary>
        public string S_PROJECT { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string S_B_STATE { get; set; }

        /// <summary>
        /// 业务状态值
        /// </summary>
        public int N_B_STATE { get; set; }

        /// <summary>
        /// 累计需要绑定数量
        /// </summary>
        public int F_ACC_B_QTY { get; set; }
    }

    #endregion


    #endregion

    #region 出库

    #region 出库单
    /// <summary>
    /// 出库单
    /// </summary>
    [SugarTable("dbo.TN_Outbound_Order")]
    public class OutboundOrderModel : BaseModel {
        /// <summary>
        /// 出库单号
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public string S_NO { get; set; }

        /// <summary>
        /// 仓库编号（产品所在仓库）
        /// </summary>
        public string S_WH_CODE { get; set; }

        /// <summary>
        /// 库区编码
        /// </summary>
        public string S_AREA_CODE { get; set; }

        /// <summary>
        /// 业务状态 0–未配货 1–已配货 2–出库完成
        /// </summary>
        public int N_B_STATE { get; set; }
        /// <summary>
        /// 状态名称
        /// </summary>
        public string S_B_STATE { get; set; }
        public string GetStateStr() {
            switch (N_B_STATE) {
                case 0:
                    return "未执行";
                case 1:
                    return "配盘";
                case 2:
                    return "配盘完成";
                case 3:
                    return "出库完成";
                default:
                    return "未知状态";
            }
        }

        /// <summary>
        /// 来源类型
        /// </summary>
        public string S_BS_TYPE { get; set; }

        /// <summary>
        /// 来源单号
        /// </summary>
        public string S_BS_NO { get; set; }


        /// <summary>
        /// 工厂标识
        /// </summary>
        public string S_FACTORY { get; set; }

        /// <summary>
        /// 出库方向
        /// </summary>
        public string S_OUT_TO { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string S_ERR_MSG { get; set; }

        /// <summary>
        /// 完工回报状态
        /// </summary>
        public int N_CR_STATE { get; set; }

        /// <summary>
        /// 完工回报时间
        /// </summary>
        public DateTime T_CR { get; set; }

        /// <summary>
        /// 完工回报错误
        /// </summary>
        public string S_CR_ERR { get; set; }

        /// <summary>
        /// 创建方式
        /// </summary>
        public string S_CREATE_METHOD { get; set; }

        /// <summary>
        /// 错误前业务状态
        /// </summary>
        public int N_PRE_B_STATE { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string S_NOTE { get; set; }

        /// <summary>
        /// 业务类型
        /// </summary>
        public string S_OP_TYPE { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public int N_PRIORITY { get; set; }

        /// <summary>
        /// 出库类型
        /// </summary>
        public string S_OUT_TYPE { get; set; }


    }

    /// <summary>
    /// 出库单明细
    /// 表名：dbo.TN_Outbound_Order_Detail
    /// </summary>
    [SugarTable("dbo.TN_Outbound_Detail")]
    public class OutboundOrderDetailModel : BaseModel {
        /// <summary>
        /// 出库单号
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public string S_OO_NO { get; set; }

        /// <summary>
        /// 行号
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public int N_ROW_NO { get; set; }

        /// <summary>
        /// 业务状态 0-未配货 1-已配货 2出库完成
        /// </summary>
        public int N_B_STATE { get; set; }
        /// <summary>
        /// 状态名称
        /// </summary>
        public string S_B_STATE { get; set; }

        /// <summary>
        /// 商品编码
        /// </summary>
        public string S_ITEM_CODE { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string S_ITEM_NAME { get; set; }

        /// <summary>
        /// 批次号
        /// </summary>
        public string S_BATCH_NO { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public float F_QTY { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string S_UOM { get; set; }

        /// <summary>
        /// 累计配货数量
        /// </summary>
        public float F_ACC_D_QTY { get; set; }

        /// <summary>
        /// 累计出库数量
        /// </summary>
        public float F_ACC_O_QTY { get; set; }

        /// <summary>
        /// 累计关闭数量
        /// </summary>
        public float F_ACC_C_QTY { get; set; }

        /// <summary>
        /// 产品序列号
        /// </summary>
        public string S_SERIAL_NO { get; set; }

        /// <summary>
        /// 供应商编码
        /// </summary>
        public string S_SUPPLIER_NO { get; set; }


        /// <summary>
        /// 来源类型
        /// </summary>
        public string S_BS_TYPE { get; set; }

        /// <summary>
        /// 来源单号
        /// </summary>
        public string S_BS_NO { get; set; }

        /// <summary>
        /// 重量
        /// </summary>
        public float F_WEIGHT { get; set; }

        /// <summary>
        /// 体积
        /// </summary>
        public float F_VOLUME { get; set; }

        /// <summary>
        /// 货主
        /// </summary>
        public string S_OWNER { get; set; }


        /// <summary>
        /// 拣料箱编码
        /// </summary>
        public string S_PICK_BOX_CODE { get; set; }

    }

    #endregion

    #region 配盘单

    /// <summary>
    /// 配盘单实体类 ContainerDispatch
    /// </summary>
    [SugarTable("dbo.TN_Distribution_CNTR")] // 可按你实际表名修改
    public class DistCntr : BaseModel {
        #region 基本属性
        /// <summary>
        /// 配盘号
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public string S_DC_NO { get; set; }

        /// <summary>
        /// 仓库编码
        /// </summary>
        public string S_WH_CODE { get; set; }

        /// <summary>
        /// 库区编码
        /// </summary>
        public string S_AREA_CODE { get; set; }

        /// <summary>
        /// 货位编码
        /// </summary>
        public string S_LOC_CODE { get; set; }

        /// <summary>
        /// 业务状态
        /// </summary>
        public int N_B_STATE { get; set; }
        /// <summary>
        /// 业务状态名称
        /// 0-未执行 1-执行中 2-配盘完成 3-分拣完成 4-出库完成
        /// </summary>
        public string S_B_STATE { get; set; }
        public string GetStateStr() {
            string s_state = string.Empty;
            switch (N_B_STATE) {
                case 0:
                    s_state = "配盘完成";
                    break;
                case 1:
                    s_state = "下发中";
                    break;
                case 2:
                    s_state = "分拣中";
                    break;
                case 3:
                    s_state = "分拣完成";
                    break;
                case 4:
                    s_state = "出库完成";
                    break;

            }
            return s_state;
        }

        /// <summary>
        /// 料箱编码
        /// </summary>
        public string S_CNTR_CODE { get; set; }

        /// <summary>
        /// 出库口区域编码
        /// </summary>
        public string S_EXIT_AREA_CODE { get; set; }

        /// <summary>
        /// 出库口货位编码
        /// </summary>
        public string S_EXIT_LOC_CODE { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public int N_PRIORITY { get; set; }

        /// <summary>
        /// 作业编码
        /// </summary>
        public string S_OP_CODE { get; set; }

        /// <summary>
        /// 来源业务类型
        /// </summary>
        public string S_BS_TYPE { get; set; }

        /// <summary>
        /// 来源业务编码
        /// </summary>
        public string S_BS_NO { get; set; }

        /// <summary>
        /// 自动生成作业
        /// </summary>
        public string C_AUTO_OP { get; set; } = "N";

        /// <summary>
        /// 作业类型
        /// </summary>
        public string S_OP_TYPE { get; set; }

        /// <summary>
        /// 整拖出库
        /// </summary>
        public string C_WHOLE_OUT { get; set; }

        /// <summary>
        /// 错误前业务状态
        /// </summary>
        public int N_PRE_B_STATE { get; set; }

        /// <summary>
        /// 分拣站台
        /// </summary>
        public string S_STATION_NO { get; set; }

        /// <summary>
        /// 出库作业编码
        /// </summary>
        public string S_OUT_OP_NO { get; set; }

        /// <summary>
        /// 回库作业编码
        /// </summary>
        public string S_BACK_OP_NO { get; set; }

        /// <summary>
        /// 料箱出库作业定义
        /// </summary>
        public string S_OUT_OP_NAME { get; set; }

        /// <summary>
        /// 料箱回库作业定义
        /// </summary>
        public string S_BACK_OP_NAME { get; set; }

        /// <summary>
        /// 工厂标识
        /// </summary>
        public string S_FACTORY { get; set; }

        /// <summary>
        /// 来源系统
        /// </summary>
        public string S_SOURCE_SYS { get; set; }

        /// <summary>
        /// 操作者账号
        /// </summary>
        public string S_OPERATOR_ID { get; set; }

        /// <summary>
        /// 操作者姓名
        /// </summary>
        public string S_OPERATOR_NAME { get; set; }

        /// <summary>
        /// 完工回报状态
        /// </summary>
        public string S_CR_STATE { get; set; } = "N";


        /// <summary>
        /// 完工回报错误
        /// </summary>
        public string S_CR_ERR { get; set; }

        /// <summary>
        /// 播种墙格口
        /// </summary>
        public string S_PUT_WALL_NO { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string S_ERR_MSG { get; set; }

        #endregion
    }

    /// <summary>
    /// 配盘明细实体类
    /// </summary>
    [SugarTable("dbo.TN_Distribution_CNTR_Detail")]
    public class DistCntrDetail : BaseModel {
        #region 基本属性
        /// <summary>
        /// 配盘号
        /// </summary>
        public string S_DC_NO { get; set; }

        /// <summary>
        /// 业务状态 0--未执行 1–可执行分拣 2–完成分拣
        /// </summary>
        public int N_B_STATE { get; set; }
        /// <summary>
        /// 0-未执行 1-配盘中 2-配盘完成
        /// </summary>
        //public string S_B_STATE { get; set; } = "未执行";

        /// <summary>
        /// 料箱编码
        /// </summary>
        public string S_CNTR_CODE { get; set; }

        /// <summary>
        /// 货隔号
        /// </summary>
        public string S_CELL_NO { get; set; }

        /// <summary>
        /// 料格编码
        /// </summary>
        public string S_CELL_CODE { get; set; }

        /// <summary>
        /// 货主
        /// </summary>
        public string S_STORER { get; set; }

        /// <summary>
        /// 商品编码
        /// </summary>
        public string S_ITEM_CODE { get; set; }

        /// <summary>
        /// 商品状态名称
        /// </summary>
        public string S_ITEM_STATE { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string S_ITEM_NAME { get; set; }

        /// <summary>
        /// 批次号
        /// </summary>
        public string S_BATCH_NO { get; set; }

        /// <summary>
        /// 系列号
        /// </summary>
        public string S_SERIAL_NO { get; set; }

        /// <summary>
        /// 货主编码
        /// </summary>
        public string S_OWNER { get; set; }

        /// <summary>
        /// 供应商编码
        /// </summary>
        public string S_SUPPLIER_NO { get; set; }

        /// <summary>
        /// 配货数量
        /// </summary>
        public float F_QTY { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string S_UOM { get; set; }

        /// <summary>
        /// 来源业务类型
        /// </summary>
        public string S_BS_TYPE { get; set; }

        /// <summary>
        /// 来源业务编码
        /// </summary>
        public string S_BS_NO { get; set; }

        /// <summary>
        /// 业务来源行号
        /// </summary>
        public int N_BS_ROW_NO { get; set; }

        /// <summary>
        /// 累计拣货数量
        /// </summary>
        public float F_ACC_P_QTY { get; set; }

        /// <summary>
        /// 仓库编码
        /// </summary>
        public string S_WH_CODE { get; set; }

        /// <summary>
        /// 库区编码
        /// </summary>
        public string S_AREA_CODE { get; set; }

        /// <summary>
        /// 波次号
        /// </summary>
        public string S_WAVE_NO { get; set; }

        /// <summary>
        /// 分拣站台
        /// </summary>
        public string S_STATION_NO { get; set; }

        /// <summary>
        /// 站点出库任务号
        /// </summary>
        public string S_SOT_CODE { get; set; } = "";

        /// <summary>
        /// 货位编码 分拣出到站台的货位
        /// </summary>
        public string S_LOC_CODE { get; set; }

        /// <summary>
        /// 拣料箱编码
        /// </summary>
        public string S_PICK_BOX_CODE { get; set; }

        /// <summary>
        /// 库存料明细标识
        /// </summary>
        public string G_INV_DETAIL_ID { get; set; }

        /// <summary>
        /// 过期日期
        /// </summary>
        //public DateTime D_EXP_DATE { get; set; }

        /// <summary>
        /// 生产日期
        /// </summary>
        //public DateTime D_PRD_DATE { get; set; }

        /// <summary>
        /// 播种墙格口
        /// </summary>
        public string S_PUT_WALL_NO { get; set; }

        /// <summary>
        /// 匹配规则
        /// </summary>
        public string S_MATCH_RULE { get; set; }

        /// <summary>
        /// 入库批次
        /// </summary>
        public string S_WMS_BN { get; set; }

        /// <summary>
        /// 出库异常
        /// </summary>
        public int N_ANOMALY { get; set; }

        /// <summary>
        /// 异常原因名称
        /// </summary>
        public string S_ANOMALY { get; set; }

        /// <summary>
        /// 波次明细行号
        /// </summary>
        public int N_WAVE_ROW_NO { get; set; }

        #endregion
    }

    #endregion

    #endregion

    #region 作业，任务

    /// <summary>
    /// 作业表实体类
    /// </summary>
    [SugarTable("dbo.TN_Operation")]
    public class Operation : BaseModel {

        /// <summary>
        /// 作业编码
        /// </summary>
        public string S_CODE { get; set; }

        /// <summary>
        /// 作业类型
        /// </summary>
        public string S_TYPE { get; set; }

        /// <summary>
        /// 料箱编码
        /// </summary>
        public string S_CNTR_CODE { get; set; }

        /// <summary>
        /// 业务状态值 0未执行/1执行中/2完成/3错误
        /// </summary>
        public int N_B_STATE { get; set; }
        /// <summary>
        /// 状态名称
        /// </summary>
        public string S_B_STATE { get; set; } = "未执行";
        /// <summary>
        /// 0-未执行；1-执行中；2-完成；3-失败
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public string GetOpStateStr() {
            // 经典 switch case 写法
            switch (N_B_STATE) {
                case 0:
                    return "未执行";
                case 1:
                    return "执行中";
                case 2:
                    return "完成";
                case 3:
                    return "失败";
                default:
                    return "未知状态";
            }
        }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string S_ERR { get; set; }

        /// <summary>
        /// 扩展数据
        /// </summary>
        public string S_EXT_DATA { get; set; }


        /// <summary>
        /// 作业类型
        /// </summary>
        public int N_TYPE { get; set; }

        /// <summary>
        /// 作业定义名称
        /// </summary>
        public string S_OP_DEF_NAME { get; set; }

        /// <summary>
        /// 工厂标识
        /// </summary>
        public string S_FACTORY { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public int N_PRIORITY { get; set; }

        /// <summary>
        /// 作业定义编码
        /// </summary>
        public string S_OP_DEF_CODE { get; set; }

        /// <summary>
        /// 起点仓库
        /// </summary>
        public string S_START_WH { get; set; }

        /// <summary>
        /// 起点库区
        /// </summary>
        public string S_START_AREA { get; set; }

        /// <summary>
        /// 起点货位
        /// </summary>
        public string S_START_LOC { get; set; }

        /// <summary>
        /// 终点仓库
        /// </summary>
        public string S_END_WH { get; set; }

        /// <summary>
        /// 终点库区
        /// </summary>
        public string S_END_AREA { get; set; }

        /// <summary>
        /// 终点货位
        /// </summary>
        public string S_END_LOC { get; set; }

        /// <summary>
        /// 来源类型
        /// </summary>
        public string S_BS_TYPE { get; set; }

        /// <summary>
        /// 来源单号
        /// </summary>
        public string S_BS_NO { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string S_NOTE { get; set; }

        /// <summary>
        /// 上一个状态
        /// </summary>
        public int N_LASTE_B_STATE { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? T_START_TIME { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? T_END_TIME { get; set; }

        /// <summary>
        /// 是否需要分拣
        /// </summary>
        public string C_NEED_SORTING { get; set; }

        /// <summary>
        /// 启动失败次数
        /// </summary>
        public int N_FAIL_COUNT { get; set; }

        /// <summary>
        /// 最多启动失败次数
        /// </summary>
        public int N_FAIL_MAX { get; set; }

        /// <summary>
        /// 上一次失败时间
        /// </summary>
        public DateTime? T_LAST_FAIL { get; set; }

        /// <summary>
        /// 自动启动 Y-后台启动 N-不启动
        /// </summary>
        public string C_AUTO_START { get; set; }

        /// <summary>
        /// 失败后重启时间
        /// </summary>
        public int N_FAIL_RETRY_TIME { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public int N_TASK_STATE { get; set; }

        /// <summary>
        /// 启动时给容器加锁
        /// </summary>
        public string C_LOCK_CNTR { get; set; }

        /// <summary>
        /// 来源系统
        /// </summary>
        public string S_SOURCE_SYS { get; set; }

        /// <summary>
        /// 作业执行时间
        /// </summary>
        public float F_RUN_TIME { get; set; }

        /// <summary>
        /// 起始货位所在巷道
        /// </summary>
        public int N_START_AISLE { get; set; }

        /// <summary>
        /// 起始货位所在巷道编码
        /// </summary>
        public string S_START_AISLE_CODE { get; set; }

        /// <summary>
        /// 目的货位所在巷道
        /// </summary>
        public int N_END_AISLE { get; set; }

        /// <summary>
        /// 目的货位所在巷道编码
        /// </summary>
        public string S_END_AISLE_CODE { get; set; }

        /// <summary>
        /// 货品编码
        /// </summary>
        public string S_ITEM_CODE { get; set; }
        /// <summary>
        /// 总重量
        /// </summary>
        //public string S_TOTAL_WEIGHT { get; set; }

        /// <summary>
        /// 批次号
        /// </summary>
        public string S_BATCH_NO { get; set; }

        /// <summary>
        /// 携带容器业务类型
        /// </summary>
        public string S_CARRY_CB_CLS { get; set; }

        /// <summary>
        /// 携带容器流水号
        /// </summary>
        public string S_CARRY_CB_NO { get; set; }

        /// <summary>
        /// 站台号
        /// </summary>
        public string S_STATION_NO { get; set; }

        /// <summary>
        /// 作业人员
        /// </summary>
        public string S_OPERATOR { get; set; }

        /// <summary>
        /// 作业人员名称
        /// </summary>
        public string S_OPERATOR_NAME { get; set; }
        public string S_SJ_CODE { get; set; }
    }

    /*'N_START_AISLE' 无效。
列名 'S_START_AISLE_CODE' 无效。
列名 'N_END_AISLE' 无效。
列名 'S_END_AISLE_CODE' 无效。
列名 'S_EXT_INFO' 无效。
列名 'S_END_AREA_SET' 无效。*/
    /// <summary>
    /// 任务表
    /// </summary>
    [SugarTable("dbo.TN_Task")]
    public class WMSTask : BaseModel {
        /// <summary>
        /// 任务编码 S_CODE
        /// </summary>
        public string S_CODE { get; set; }
        /// <summary>
        /// 前置任务号
        /// </summary>
        public string S_PRE_TASK_NO { get; set; }
        /// <summary>
        /// 任务类型值 N_TYPE
        /// </summary>
        public int N_TYPE { get; set; }

        /// <summary>
        /// 任务类型 S_TYPE
        /// </summary>
        public string S_TYPE { get; set; }

        /// <summary>
        ///0 等待
        ///1已推送
        ///2执行中（hosttoagv上报1）
        ///3开始取货
        ///4取货完成
        ///5开始卸货
        ///6卸货完成
        ///7取消
        ///8完成（hosttoagv上报2）
        ///9强制完成
        /// </summary>
        public int N_B_STATE { get; set; }
        public string GetTaskStateStr() {
            string state = "";
            switch (N_B_STATE) {
                case 0: state = "等待"; break;
                case 1: state = "已推送"; break;
                case 2: state = "执行中"; break;
                case 3: state = "开始取货"; break;
                case 4: state = "取货完成"; break;
                case 5: state = "开始卸货"; break;
                case 6: state = "卸货完成"; break;
                case 7: state = "取消"; break;
                case 8: state = "完成"; break;
                case 9: state = "强制完成"; break;
                default: state = "未知状态"; break;
            }
            return state;

        }
        /// <summary>
        /// 任务状态 等待/已推送/执行中/完成/错误 S_B_STATE
        /// </summary>
        public string S_B_STATE { get; set; }

        /// <summary>
        /// 调度类型值 
        /// 0 ndc 1 gz 2 wcs
        /// </summary>
        public int N_SCHEDULE_TYPE { get; set; }

        /// <summary>
        /// 优先级 N_PRIORITY
        /// </summary>
        public int N_PRIORITY { get; set; }

        /// <summary>
        /// 顺序号 N_SORT_NO
        /// </summary>
        public int N_SORT_NO { get; set; }

        /// <summary>
        /// 作业编码 S_OP_CODE
        /// </summary>
        public string S_OP_CODE { get; set; }

        /// <summary>
        /// 工厂 S_FACTORY
        /// </summary>
        public string S_FACTORY { get; set; }

        /// <summary>
        /// 设备编号 S_EQ_NO
        /// </summary>
        public string S_EQ_NO { get; set; }

        /// <summary>
        /// 作业名称 S_OP_NAME
        /// </summary>
        public string S_OP_NAME { get; set; }

        /// <summary>
        /// 开始时间 T_START_TIME
        /// </summary>
        public DateTime T_START_TIME { get; set; }

        /// <summary>
        /// 结束时间 T_END_TIME
        /// </summary>
        public DateTime T_END_TIME { get; set; }

        /// <summary>
        /// 料箱编码 S_CNTR_CODE
        /// </summary>
        public string S_CNTR_CODE { get; set; }

        /// <summary>
        /// 调度类型 S_SCHEDULE_TYPE
        /// </summary>
        public string S_SCHEDULE_TYPE { get; set; }

        /// <summary>
        /// 设备任务号 大部分情况=S_CODE S_EQ_TASK_CODE
        /// </summary>
        public string S_EQ_TASK_CODE { get; set; }

        /// <summary>
        /// 起始仓库 S_START_WH
        /// </summary>
        public string S_START_WH { get; set; }

        /// <summary>
        /// 起始库区编号 S_START_AREA
        /// </summary>
        public string S_START_AREA { get; set; }

        /// <summary>
        /// 起点 S_START_LOC
        /// </summary>
        public string S_START_LOC { get; set; }

        /// <summary>
        /// 起始货位站点 S_START_SITE
        /// </summary>
        public string S_START_SITE { get; set; }

        /// <summary>
        /// 起始货位站点层 S_START_SITE_LAYER
        /// </summary>
        public int S_START_SITE_LAYER { get; set; }

        /// <summary>
        /// 目的仓库 S_END_WH
        /// </summary>
        public string S_END_WH { get; set; }

        /// <summary>
        /// 目的库区编号 S_END_AREA
        /// </summary>
        public string S_END_AREA { get; set; }

        /// <summary>
        /// 终点 S_END_LOC
        /// </summary>
        public string S_END_LOC { get; set; }

        /// <summary>
        /// 目的货位站点 S_END_SITE
        /// </summary>
        public string S_END_SITE { get; set; }

        /// <summary>
        /// 目的货位站点层 S_END_SITE_LAYER
        /// </summary>
        public int S_END_SITE_LAYER { get; set; }

        /// <summary>
        /// 备注 S_NOTE
        /// </summary>
        public string S_NOTE { get; set; }

        /// <summary>
        /// 错误信息 S_ERR
        /// </summary>
        public string S_ERR { get; set; }

        /// <summary>
        /// 错误码 N_ERR
        /// </summary>
        public int N_ERR { get; set; }

        /// <summary>
        /// 车间编码 S_WORKSHOP_NO
        /// </summary>
        public string S_WORKSHOP_NO { get; set; }

        /// <summary>
        /// 车间名称 S_WORKSHOP_NAME
        /// </summary>
        public string S_WORKSHOP_NAME { get; set; }

        /// <summary>
        /// 来源系统 S_SOURCE_SYS
        /// </summary>
        public string S_SOURCE_SYS { get; set; }

        /// <summary>
        /// 来源单号 S_BS_NO
        /// </summary>
        public string S_BS_NO { get; set; }

        /// <summary>
        /// 来源类型 S_BS_TYPE
        /// </summary>
        public string S_BS_TYPE { get; set; }

        /// <summary>
        /// 起始货位所在巷道 N_START_AISLE
        /// </summary>
        //public int N_START_AISLE { get; set; }

        /// <summary>
        /// 起始货位所在巷道编码 S_START_AISLE_CODE
        /// </summary>
        //public string S_START_AISLE_CODE { get; set; }

        /// <summary>
        /// 目的货位所在巷道 N_END_AISLE
        /// </summary>
        //public int N_END_AISLE { get; set; }

        /// <summary>
        /// 目的货位所在巷道编码 S_END_AISLE_CODE
        /// </summary>
        //public string S_END_AISLE_CODE { get; set; }

        /// <summary>
        /// 扩展数据 S_EXT_INFO
        /// </summary>
        //public string S_EXT_INFO { get; set; }

        /// <summary>
        /// 目标库区集 S_END_AREA_SET
        /// </summary>
        //public string S_END_AREA_SET { get; set; }
    }

    /// <summary>
    /// 任务动作
    /// </summary>
    [SugarTable("dbo.TN_Task_Action")]
    public class WmsTaskAction : BaseModel {
        /// <summary>
        /// 任务编码
        /// </summary>
        public string S_TASK_CODE { get; set; }
        /// <summary>
        /// 动作码
        /// </summary>
        public int N_ACTION_CODE { get; set; }
        /// <summary>
        /// 设备号
        /// </summary>
        public string S_EQ_CODE { get; set; }
        /// <summary>
        /// 设备类型
        /// </summary>
        public string S_EQ_TYPE { get; set; }
        /// <summary>
        /// 来源单号
        /// </summary>
        public string S_BS_NO { get; set; }
        /// <summary>
        /// 来源类型
        /// </summary>
        public string S_BS_TYPE { get; set; }
        /// <summary>
        /// 其它数据
        /// </summary>
        public string S_DATA { get; set; }

    }

    #endregion

    #region 货位和货位扩展 容器 物料实体表

    /// <summary>
    /// 货位表
    /// </summary>
    [SugarTable("dbo.TN_Location")]
    public class Location : BaseModel {
        [SugarColumn(IsPrimaryKey = true)]
        public string S_CODE { get; set; }
        public string S_AREA_CODE { get; set; }
        /// <summary>
        /// 仓库名-不可用
        /// </summary>
        public string S_WH_CODE { get; set; }
        public int N_CAPACITY { get; set; }
        public string S_AGV_SITE { get; set; }
        /// <summary>
        /// 0 货位 1 站点（不需要绑定解绑托盘）
        /// </summary>
        public int N_TYPE { get; set; }
        public int N_CURRENT_NUM { get; set; }
        public DateTime T_FULL_TIME { get; set; }
        /// <summary>
        /// 巷道
        /// </summary>
        //[SugarColumn(ColumnName = "N_AISLE")]
        public int N_ROADWAY { get; set; }
        /// <summary>
        /// 排
        /// </summary>
        public int N_ROW { get; set; }
        /// <summary>
        /// 排组号/巷道侧(建议后面换N_SIDE字段)
        /// </summary>
        public int N_ROW_GROUP { get; set; }
        /// <summary>
        /// 列
        /// </summary>
        public int N_COL { get; set; }
        /// <summary>
        /// 层
        /// </summary>
        public int N_LAYER { get; set; }
        /// <summary>
        /// 位置
        /// </summary>
        public int N_POS { get; set; }
        /// <summary>
        /// 0无/1入库锁/2出库锁/3其它锁
        /// </summary>
        public int N_LOCK_STATE { get; set; }
        public string GetLockStateStr() {
            string lockState = "";
            switch (N_LOCK_STATE) {
                case 0:
                    lockState = "无";
                    break;
                case 1:
                    lockState = "入库锁";
                    break;
                case 2:
                    lockState = "出库锁";
                    break;
                case 3:
                    lockState = "其它锁";
                    break;
                default:
                    lockState = "未知状态";
                    break;
            }
            return lockState;

        }
        /// <summary>
        /// 无/入库锁/出库锁/其它锁
        /// </summary>
        public string S_LOCK_STATE { get; set; } = "无";

        public string S_NOTE { get; set; }

        public string S_LOCK_OP { get; set; }
        /// <summary>
        /// Y
        /// </summary>
        public string C_ENABLE { get; set; } = "Y";

    }
    /// <summary>
    /// 货位扩展表
    /// </summary>
    [SugarTable("dbo.TN_Location_Ext")]
    public class LocationExt : BaseModel {
        /// <summary>
        /// 货位
        /// </summary>
        public string S_LOC_CODE { get; set; }
        /// <summary>
        /// agv站点
        /// </summary>
        public string S_AGV_SITE { get; set; }
        /// <summary>
        /// 标识
        /// </summary>
        public string S_EQ_TYPE { get; set; }
        public string N_EQ_TYPE { get; set; }
    }
    /*列名 'S_CTD_CODE' 无效。
列名 'S_SUBTYPE' 无效。
列名 'C_FORCED_FILL' 无效。
列名 'F_CNTR_UTIL' 无效。
列名 'C_DISTRIBUTION' 无效。
列名 'F_SCU_VALUE' 无效。
列名 'S_P_CNTR_CODE' 无效。
列名 'C_HAS_SUB_CNTR' 无效。
列名 'C_IN_STOCK' 无效。*/
    /// <summary>
    /// 容器实体类 Container
    /// </summary>
    [SugarTable("dbo.TN_Container")] // 可按你实际表名修改
    public class Container : BaseModel {
        #region 基本属性
        /// <summary>
        /// 容器编码
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public string S_CODE { get; set; }

        /// <summary>
        /// 容器定义编码
        /// </summary>
        //public string S_CTD_CODE { get; set; }

        /// <summary>
        /// 类型 Pallet / Normal / Cell_Box / Picking_Box
        /// </summary>
        public string S_TYPE { get; set; }

        /// <summary>
        /// 子类型
        /// </summary>
        //public string S_SUBTYPE { get; set; }

        /// <summary>
        /// 规格
        /// </summary>
        public string S_SPEC { get; set; }

        /// <summary>
        /// 自重
        /// </summary>
        public float F_WEIGHT { get; set; }

        /// <summary>
        /// 最大重量
        /// </summary>
        public float F_MAX_WEIGHT { get; set; }

        /// <summary>
        /// 是否可用
        /// </summary>
        public string C_ENABLE { get; set; } = "Y";

        /// <summary>
        /// 虚拟容器
        /// </summary>
        //public string C_IS_VIRTUAL { get; set; }

        /// <summary>
        /// 明细条数
        /// </summary>
        public int N_DETAIL_COUNT { get; set; }

        /// <summary>
        /// 容器业务状态
        /// </summary>
        public int N_B_STATE { get; set; }

        /// <summary>
        /// 锁定业务号
        /// </summary>
        public string S_LOCK_OP_CODE { get; set; }

        /// <summary>
        /// 锁状态
        /// </summary>
        public string S_LOCK_STATE { get; set; }

        /// <summary>
        /// 锁状态
        /// </summary>
        public int N_LOCK_STATE { get; set; }

        /// <summary>
        /// 货品重量
        /// </summary>
        public float F_GOOD_WEIGHT { get; set; }

        /// <summary>
        /// 最大体积
        /// </summary>
        public float F_MAX_VOLUME { get; set; }

        /// <summary>
        /// 货品体积
        /// </summary>
        public float F_GOOD_VOLUME { get; set; }

        /// <summary>
        /// 满框
        /// </summary>
        //public string C_FULL { get; set; }

        /// <summary>
        /// 空满状态
        /// </summary>
        public int N_EMPTY_FULL { get; set; }

        /// <summary>
        /// 最大料格数量
        /// </summary>
        public int N_MAX_CELL_NUM { get; set; }

        /// <summary>
        /// 空料格数量
        /// </summary>
        public int N_EMPTY_CELL_NUM { get; set; }

        /// <summary>
        /// 长
        /// </summary>
        public float F_LENGTH { get; set; }

        /// <summary>
        /// 宽
        /// </summary>
        public float F_WIDTH { get; set; }

        /// <summary>
        /// 高
        /// </summary>
        public float F_HEIGHT { get; set; }

        /// <summary>
        /// 容器位置
        /// </summary>
        public string S_POSITION { get; set; }

        /// <summary>
        /// 货品件数
        /// </summary>
        public int N_GOOD_NUM { get; set; }

        /// <summary>
        /// 预分配的料格数
        /// </summary>
        public int N_ALLOC_CELL_NUM { get; set; }

        /// <summary>
        /// 容器来源
        /// </summary>
        public string S_SOURCE { get; set; }

        /// <summary>
        /// 实际重量
        /// </summary>
        public float F_ACT_WEIGHT { get; set; }

        /// <summary>
        /// 强制置满
        /// </summary>
        //public string C_FORCED_FILL { get; set; }

        /// <summary>
        /// 容器类型
        /// </summary>
        public int N_TYPE { get; set; }

        /// <summary>
        /// 容器利用率
        /// </summary>
        //public float F_CNTR_UTIL { get; set; }

        /// <summary>
        /// 是否参与配盘
        /// </summary>
        //public string C_DISTRIBUTION { get; set; }

        /// <summary>
        /// 特定计数量
        /// </summary>
        //public float F_SCU_VALUE { get; set; }

        /// <summary>
        /// 父容器编码
        /// </summary>
        //public string S_P_CNTR_CODE { get; set; }

        /// <summary>
        /// 是否有子容器
        /// </summary>
        //public string C_HAS_SUB_CNTR { get; set; }

        /// <summary>
        /// 是否有货
        /// </summary>
        //public string C_IN_STOCK { get; set; }

        #endregion
    }


    /// <summary>
    /// 物料实体类
    /// </summary>
    [SugarTable("dbo.TN_Material")]
    public class Material : BaseModel {
        // ==================== 基本属性 ====================
        /// <summary>
        /// 体积
        /// </summary>
        public float F_VOLUME { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string S_UOM { get; set; }

        /// <summary>
        /// 来源
        /// </summary>
        public string S_FROM { get; set; }

        /// <summary>
        /// 长边
        /// </summary>
        public float F_LONG { get; set; }

        /// <summary>
        /// 中边
        /// </summary>
        public float F_MIDDLE { get; set; }

        /// <summary>
        /// 短边
        /// </summary>
        public float F_SHORT { get; set; }

        // ==================== 物料属性 ====================
        /// <summary>
        /// 商品号
        /// </summary>
        public string S_ITEM_CODE { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string S_ITEM_NAME { get; set; }

        /// <summary>
        /// 购制类型
        /// </summary>
        public string S_MP_TYPE { get; set; }

        /// <summary>
        /// 重量
        /// </summary>
        public double F_WEIGHT { get; set; }

        /// <summary>
        /// 材料
        /// </summary>
        public string S_MATERIAL { get; set; }

        /// <summary>
        /// 标识码
        /// </summary>
        public string S_IDCODE { get; set; }

        /// <summary>
        /// 系列码
        /// </summary>
        public string S_SERIES { get; set; }

        /// <summary>
        /// 是否库存件
        /// </summary>
        public string S_STORE { get; set; }

        /// <summary>
        /// 是否控制件
        /// </summary>
        public string S_CONTROL { get; set; }

        /// <summary>
        /// BOM类型
        /// </summary>
        public string S_BOM_TYPE { get; set; }

        /// <summary>
        /// BOM模值
        /// </summary>
        public int N_BOM_MOD { get; set; }

        /// <summary>
        /// 设计BOM更新时间
        /// </summary>
        public DateTime T_DBOM_UPDATE { get; set; }

        /// <summary>
        /// 工程BOM更新时间
        /// </summary>
        public DateTime T_EBOM_UPDATE { get; set; }

        /// <summary>
        /// 工艺BOM更新时间
        /// </summary>
        public DateTime T_PBOM_UPDATE { get; set; }

        // ==================== 工艺属性 ====================
        /// <summary>
        /// 多路线
        /// </summary>
        public bool C_MULTI_ROUTE { get; set; }

        /// <summary>
        /// 制造路线
        /// </summary>
        public string S_ROUTE_MAKE { get; set; }

        /// <summary>
        /// 装配路线
        /// </summary>
        public string S_ROUTE_ASSEMBLY { get; set; }

        /// <summary>
        /// 配送方式
        /// </summary>
        public string S_DMODE { get; set; }
    }
    #endregion

    #region 货位，容器，物料绑定中间表
    /// <summary>
    /// 货位容器表(这里的货位与托盘绑定)
    /// </summary>
    [SugarTable("dbo.TN_Loc_Container")]
    public class LocCntrRel : BaseModel {
        /// <summary>
        /// 货位编码
        /// </summary>
        public string S_LOC_CODE { get; set; }
        /// <summary>
        /// 容器编码
        /// </summary>

        [SugarColumn(IsPrimaryKey = true)]
        public string S_CNTR_CODE { get; set; }


        /// <summary>
        /// 托盘来源
        /// </summary>
        public string S_ACTION_SRC { get; set; }
        //public string S_TRAY_TYPE { get; set; }
        /// <summary>
        /// 绑定方式名称
        /// </summary>
        public string S_BINDING_METHOD { get; set; }
        /// <summary>
        /// 绑定方式
        /// </summary>
        public int N_BINDING_METHOD { get; set; }
        /// <summary>
        /// 绑定次序
        /// </summary>
        public int N_BIND_ORDER { get; set; }
    }

    #endregion

    /// <summary>
    /// 库存量表 TN_INV_Detail
    /// </summary>
    [SugarTable("dbo.TN_INV_Detail")]
    public class InvDetail : BaseModel {
        #region 基本属性
        /// <summary>
        /// 货品编码
        /// </summary>
        public string S_ITEM_CODE { get; set; }

        /// <summary>
        /// 货品状态 合格/待检/不合格/可疑
        /// </summary>
        public string S_ITEM_STATE { get; set; }

        /// <summary>
        /// 货主
        /// </summary>
        public string S_STORER { get; set; }

        /// <summary>
        /// 货品名称
        /// </summary>
        public string S_ITEM_NAME { get; set; }

        /// <summary>
        /// 货品规格
        /// </summary>
        public string S_ITEM_SPEC { get; set; }

        /// <summary>
        /// 批次号
        /// </summary>
        public string S_BATCH_NO { get; set; }

        /// <summary>
        /// 流水号 一品一号
        /// </summary>
        public string S_SERIAL_NO { get; set; }

        /// <summary>
        /// 物权
        /// </summary>
        public string S_OWNER { get; set; }

        /// <summary>
        /// 供应商编码
        /// </summary>
        public string S_SUPPLIER_NO { get; set; }

        /// <summary>
        /// 计量单位
        /// </summary>
        public string S_UOM { get; set; }

        /// <summary>
        /// 来源类型
        /// </summary>
        public string S_BS_TYPE { get; set; }

        /// <summary>
        /// 来源单号
        /// </summary>
        public string S_BS_NO { get; set; }

        /// <summary>
        /// 净重
        /// </summary>
        public float F_NET_WEIGHT { get; set; }

        /// <summary>
        /// 毛重
        /// </summary>
        public float F_GROSS_WEIGHT { get; set; }

        /// <summary>
        /// 重量单位
        /// </summary>
        public string S_WU { get; set; }

        /// <summary>
        /// 长度
        /// </summary>
        public float F_LENGTH { get; set; }

        /// <summary>
        /// 过期日期
        /// </summary>
        //public DateTime D_EXP_DATE { get; set; }

        /// <summary>
        /// 生产日期
        /// </summary>
       // public DateTime D_PRD_DATE { get; set; }

        /// <summary>
        /// 体积
        /// </summary>
        public float F_VOLUME { get; set; }

        /// <summary>
        /// 总体积（计算列）
        /// </summary>
        //public float F_SUM_VOLUME { get; set; }

        /// <summary>
        /// 重量
        /// </summary>
        public float F_WEIGHT { get; set; }

        /// <summary>
        /// 总重量（计算列）
        /// </summary>
        //public float F_SUM_WEIGHT { get; set; }

        /// <summary>
        /// 特别计数量
        /// </summary>
        public float F_SCU { get; set; }

        /// <summary>
        /// 总特别计数量（计算列）
        /// </summary>
        //public float F_SUM_SCU { get; set; }

        /// <summary>
        /// 入库时间
        /// </summary>
        public DateTime T_INBOUND_TIME { get; set; }

        /// <summary>
        /// 来源单行号
        /// </summary>
        public int N_BS_ROW_NO { get; set; }

        /// <summary>
        /// WMS入库批次
        /// </summary>
        public string S_WMS_BN { get; set; }

        /// <summary>
        /// 错误前货品状态
        /// </summary>
        //public string S_PER_ITEM_STATE { get; set; }
        #endregion

        #region 数量
        /// <summary>
        /// 数量
        /// </summary>
        public float F_QTY { get; set; }

        /// <summary>
        /// 分配量
        /// </summary>
        public float F_ALLOC_QTY { get; set; }

        /// <summary>
        /// 移动量
        /// </summary>
        public float F_QTY_MOVE { get; set; }

        /// <summary>
        /// 预分配量
        /// </summary>
        //public float F_PRE_ALLOC_QTY { get; set; }

        /// <summary>
        /// 冻结数量
        /// </summary>
        public float F_QTY_FREEZE { get; set; }

        /// <summary>
        /// 可用量（计算列） 计算属性，不能插入更新
        /// </summary>
        [SugarColumn(IsOnlyIgnoreInsert = true, IsOnlyIgnoreUpdate = true)]
        public float F_QTY_VALID { get; set; }
        #endregion

        #region 位置容器
        /// <summary>
        /// 料箱编码
        /// </summary>
        public string S_CNTR_CODE { get; set; }

        /// <summary>
        /// 箱格号
        /// </summary>
        public string S_CELL_NO { get; set; }

        /// <summary>
        /// 料格编码
        /// </summary>
        public string S_CELL_CODE { get; set; }

        /// <summary>
        /// 仓库编码
        /// </summary>
        public string S_WH_CODE { get; set; }

        /// <summary>
        /// 库区编码
        /// </summary>
        public string S_AREA_CODE { get; set; }

        /// <summary>
        /// 货位编码
        /// </summary>
        public string S_LOC_CODE { get; set; }

        /// <summary>
        /// 巷道
        /// </summary>
        public int N_AISLE { get; set; }

        /// <summary>
        /// 巷道编码
        /// </summary>
        public string S_AISLE_CODE { get; set; }
        #endregion
    }

    /// <summary>
    /// 库内作业容器货品明细
    /// </summary>
    [SugarTable("dbo.TN_IWP_CNTR_Detail")]
    public class InvContainerItemDetail : BaseModel {
        /// <summary>
        /// 库内作业容器流水号
        /// </summary>
        public string S_IWPC_NO { get; set; }

        /// <summary>
        /// 行号
        /// </summary>
        public int N_ROW_NO { get; set; }

        /// <summary>
        /// 盘点单号
        /// </summary>
        public string S_COUNT_NO { get; set; }

        /// <summary>
        /// 业务状态 0未盘点/ 1可盘点/2已盘点
        /// </summary>
        public int N_B_STATE { get; set; }

        /// <summary>
        /// 料箱编码
        /// </summary>
        public string S_CNTR_CODE { get; set; }

        /// <summary>
        /// 箱格号
        /// </summary>
        public string S_CELL_NO { get; set; }

        /// <summary>
        /// 料格编码
        /// </summary>
        public string S_CELL_CODE { get; set; }

        /// <summary>
        /// 商品系列号
        /// </summary>
        public string S_SERIAL_NO { get; set; }

        /// <summary>
        /// 商品编码
        /// </summary>
        public string S_ITEM_CODE { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string S_ITEM_NAME { get; set; }

        /// <summary>
        /// 商品规格
        /// </summary>
        public string S_ITEM_SPEC { get; set; }

        /// <summary>
        /// 商品状态
        /// </summary>
        public string S_ITEM_STATE { get; set; }

        /// <summary>
        /// 批号
        /// </summary>
        public string S_BATCH_NO { get; set; }

        /// <summary>
        /// 物权
        /// </summary>
        public string S_OWNER { get; set; }

        /// <summary>
        /// 供应商编码
        /// </summary>
        public string S_SUPPLIER_NO { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public float F_QTY { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string S_UOM { get; set; }

        /// <summary>
        /// 实际数量
        /// </summary>
        public float F_ACT_QTY { get; set; }

        /// <summary>
        /// 站点号 盘点站台
        /// </summary>
        public string S_STATION_NO { get; set; }

        /// <summary>
        /// 盘点差异标识 差异复盘时产生的容器盘点明细，需要说明是针对那条盘点差异生成的。不是必须有值，只是在盘点差异是复盘的时候会用到
        /// </summary>
        public string G_COUNT_DIFF_ID { get; set; }

        /// <summary>
        /// 批次编号
        /// </summary>
        public string S_WMS_BN { get; set; }

        /// <summary>
        /// 生产日期
        /// </summary>
        //public DateTime? D_PRD_DATE { get; set; }

        /// <summary>
        /// 有效日期
        /// </summary>
        //public DateTime? D_EXP_DATE { get; set; }

        /// <summary>
        /// 货主
        /// </summary>
        public string S_STORER { get; set; }

        /// <summary>
        /// 仓库
        /// </summary>
        public string S_WH_CODE { get; set; }

        /// <summary>
        /// 库区
        /// </summary>
        public string S_AREA_CODE { get; set; }

        /// <summary>
        /// 货位
        /// </summary>
        public string S_LOC_CODE { get; set; }

        /// <summary>
        /// 容器业务类型
        /// </summary>
        public int N_IWP_TYPE { get; set; }

        /// <summary>
        /// 作业顺序
        /// </summary>
        public int N_ORDER { get; set; }

        /// <summary>
        /// 库存量明细标识
        /// </summary>
        public string G_INV_DETAIL_ID { get; set; }
    }


}
