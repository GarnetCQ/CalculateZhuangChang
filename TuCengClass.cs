using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculateZhuangChang
{

    /// <summary>
    /// 地质参数
    /// </summary>
    /// <param name="Name">土层名称</param>
    /// <param name="Qik">qik</param>
    /// <param name="Fa0">fa0</param>
    public record DiZhiRecord(string Name = "", double Qik = 0, double Fa0 = 0);


    /// <summary>
    /// 土层信息
    /// </summary>
    /// <param name="CengDi">层底标高</param>
    /// <param name="CengDing">层顶标高</param>
    public record TuCengRecord(string Name,double CengHou =0)
    {
        public double CengDing { get; set; } = 0;
        public double CengDi { get => CengDing - CengHou; }
        public double Qik { get; set; } = 0;
        public double Fa0 { get; set; } = 0;

        /// <summary>
        /// 获取地质参数
        /// </summary>
        /// <param name="diZhiRecords">地质信息</param>
        public void GetDizhi(List<DiZhiRecord> diZhiRecords)
        {
            diZhiRecords.ForEach(y =>
            {
                if (Name == y.Name)
                {
                    Fa0 = y.Fa0;
                    Qik = y.Qik;
                }
            });
        }
    }

    /// <summary>
    /// 钻孔信息
    /// </summary>
    public record ZhuankongRecord()
    {
        /// <summary>
        /// 孔号
        /// </summary>
        public string? KongHao { get; set; }

        /// <summary>
        /// 空口标高
        /// </summary>
        public double KongKou { get; set; }

        /// <summary>
        /// 桥名
        /// </summary>
        public string? BridgeName { get; set; } = "";

        /// <summary>
        /// 土层信息
        /// </summary>
        public List<TuCengRecord>? TuCengInfo { get; set; }

        /// <summary>
        /// 计算层底桩底标高
        /// </summary>
        public void CalulatHeight()
        {

            this.TuCengInfo = TuCengInfo?.Select((x, i) =>
            {
                x.CengDing = i == 0 ? KongKou : TuCengInfo[i - 1].CengDi;
                return x;
            }).ToList();
        }
    }
}
