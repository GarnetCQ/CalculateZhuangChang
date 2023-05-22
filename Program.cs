// See https://aka.ms/new-console-template for more information
using CalculateZhuangChang;

// input 
double pile_length = 19; // 桩长
double pile_diameter = 1.5; // 直径
double drilling_heigth = 18.14; // 孔口标高
double ground_heigth = 17.62; // 地面标高
double cdth层垫土厚 = 0.2; // 沉淀土厚度
double k_2 = 2.5;
double gama = 18;
double bulk_density = 26; // 容重
double pile_top_counterforces = 2267; // 桩顶反力
var tuCeng = "粉砂";

var pile_circumference = Math.PI * pile_diameter; // 桩周长
var pileArea = Math.PI * pile_diameter * pile_diameter / 4; // 桩面积
var pileTopHeight = ground_heigth - cdth层垫土厚;  // 桩顶标高
var pileButtomHeigth = pileTopHeight - pile_length;  // 桩底标高 


// 地质参数
var diZhiRecords = new List<DiZhiRecord>()
{
    new("1-2 素填土",0,0),
    new("2-1 粉土",15,80),
    new("2-1a 粉质黏土",30,120),
    new("2-1b 粉质黏土",35,140),
    new("2-1c 粉质黏土",15,80),
    new("2-2 淤泥质粉质黏土",10,60),
    new("3-1 粉质黏土夹粉土",30,120),
    new("3-2 粉质黏土",50,180),
    new("4-1 含钙质结核黏土",65,260),
    new("4-1T 粉细砂夹粉土",40,140),
    new("5 含钙质结核黏土",70,350),
    new("6 中细砂",65,300),
    new("7 含钙质结核黏土",70,350),
    new("8 中砂",65,280),
    new("9 粉质黏土",70,300),
    new("10 中细砂",70,320),
    new("10-1 黏土",75,380),
    new("11 黏土",75,380),
    new("12 细砂",65,300),
    new("12-1 黏土",75,380),
    new("13 黏土",75,380),
    new("13-1 中砂",75,350),
};

// 土层信息
var tuCengRecords = new List<TuCengRecord>()
{
    new ("1-2 素填土",1),
    new ("3-2 粉质黏土",2.3),
    new ("4-1 含钙质结核黏土",8.3),
    new ("4-1T 粉细砂夹粉土",5.5),
    new ("5 含钙质结核黏土",4.8),
    new ("6 中细砂",7.1),
    new ("7 含钙质结核黏土",2.9),
    new ("8 中砂",11.6),
    new ("9 粉质黏土",3.2),
    new ("10 中细砂",8.3),
};

// 土层信息与地质信息绑定
tuCengRecords.ForEach(x=>x.GetDizhi(diZhiRecords));


// 增加钻孔
var zhuanKong = new ZhuankongRecord
{
    BridgeName = "泰山河",
    KongHao = "XG1",
    KongKou = drilling_heigth,
    TuCengInfo = tuCengRecords,
};

// 计算层顶，层顶
zhuanKong.CalulatHeight();


#region 单桩承载力

var qik_l = zhuanKong.TuCengInfo.Select(x => x.Qik)
                                .ToList();

// fa0_l
var index = zhuanKong.TuCengInfo.Select(x => x.CengDi)
                                .OrderBy(x => x)
                                .ToList()
                                .BinarySearch(pileButtomHeigth);

index = index >= 0 ? index : Math.Abs(index + 1);

var fa0 = zhuanKong.TuCengInfo.Select(x => x.Fa0)
                              .ElementAtOrDefault(index);


//修正顶标高
var soil_top_height_fix_list = zhuanKong.TuCengInfo.Select((x, i) => ground_heigth >= x.CengDing ? x.CengDing : ground_heigth)
                                                   .ToList();


var mclth摩檫力土厚 = soil_top_height_fix_list.Select((x, i) =>
{

    if (pileTopHeight <= zhuanKong.TuCengInfo[i].CengDi || pileButtomHeigth >= x)
        return 0.0;
    
    else if (pileTopHeight >= x && x >= pileButtomHeigth && pileButtomHeigth >= zhuanKong.TuCengInfo[i].CengDi)
        return x - pileButtomHeigth;
    
    else if (pileButtomHeigth <= zhuanKong.TuCengInfo[i].CengDi && x >= pileTopHeight && pileTopHeight >= zhuanKong.TuCengInfo[i].CengDi)
        return pileTopHeight - zhuanKong.TuCengInfo[i].CengDi;
    
    else
        return x - zhuanKong.TuCengInfo[i].CengDi;
}).ToList();


var zcmzl桩侧摩阻力 = mclth摩檫力土厚.Select((x, i) => x * pile_circumference * qik_l[i] / 2.0).ToList();

#endregion


#region 反力合计

double lamda = MathToolClass.GetInter(new List<double> { 20, 25 }, new List<double> { 0.65, 0.72 }, pile_length / pile_circumference);
double m0 = MathToolClass.GetInter(new List<double> { 0.1, 0.3, 0.6 }, new List<double> { 1, 0.7, 0.25 }, cdth层垫土厚 / pile_diameter);

var soilTopHeight = soil_top_height_fix_list.Max(); // 第一层土顶标高

double effectiveLength = pile_length, // 有效桩长
    freeLength = 0, // 桩自由长度
    soilDensityDiff = pileArea * pile_length * (bulk_density - gama),
    pileToSoilHeight = pileTopHeight - soilTopHeight;  // 桩顶到土顶的距离

// 桩比土层高
if (pileToSoilHeight > 0)
{
    effectiveLength -= pileToSoilHeight;
    soilDensityDiff -= pileArea * pileToSoilHeight * gama;
    freeLength = pileToSoilHeight;
}


// 持力土层
var tuCengValue = tuCeng switch
{
    "粉砂" => 1000.0,
    "细砂" => 1150,
    "中砂、粗砂、砾砂" => 1450,
    "碎石土" => 2750,
    _ => 0
};

var qr = 0.0;

if (true)
{
    qr = effectiveLength switch
    {
        <= 40 => m0 * lamda * (fa0 + k_2 * gama * (effectiveLength - 3)),
        _ => m0 * lamda * (fa0 + k_2 * gama * (40 - 3)),
    };
    qr = qr > tuCengValue ? tuCengValue : qr;
}



var zdzcl桩底支撑力 = qr * pileArea;

#endregion


var flhj反力合计 = pile_top_counterforces + soilDensityDiff;
var dzczl单桩承载力 = zcmzl桩侧摩阻力.Sum() + zdzcl桩底支撑力;

Console.WriteLine(flhj反力合计);
Console.WriteLine(dzczl单桩承载力);
