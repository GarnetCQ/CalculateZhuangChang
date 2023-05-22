namespace CalculateZhuangChang
{
    internal class MathToolClass
    {
        /// <summary>
        /// 线性插值计算
        /// </summary>
        /// <param name="rangeNum">插值范围</param>
        /// <param name="valueNum">取值范围</param>
        /// <param name="interNum">插值</param>
        /// <returns>插值对应得取值</returns>
        public static double GetInter(List<double> r, List<double> v, double inter)
        {
            var index = r.BinarySearch(inter);

            if (index < 0)
            {
                var a = Math.Abs(index + 1);

                if (a == 0) 
                    return v[0];
                else if (a >= r.Count) 
                    return v[a - 1];
                else 
                    return (v[a] - v[a - 1]) / (r[a] - r[a - 1]) * (inter - r[a - 1]) + v[a - 1];
            }
            else 
                return v[index];
        }
    }
}
