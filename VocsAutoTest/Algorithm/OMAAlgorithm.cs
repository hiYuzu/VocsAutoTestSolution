using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace VocsAutoTest.Algorithm
{
    /// <summary>
    /// OMAAlgorithm 的摘要说明。
    /// </summary>
    public class OMAAlgorithm
    {
        public OMAAlgorithm()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }


        /// <summary>
        /// 插值顺序数组
        /// </summary>
        private static double[] xOrigin = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xOrigin">原始像素</param>
        /// <param name="yOrigin">原始光强</param>
        /// <returns>插值后的光强</returns>
        public static double[] SplineFunc(double[] yOrigin)
        {
            if (xOrigin == null)
            {
                xOrigin = new double[512];
                for (int i = 0; i < 512; i++)
                {
                    xOrigin[i] = i;
                }
            }

            int n = xOrigin.Length;
            double[] xInterp = new double[n * 4];
            double[] yInterp = new double[n * 4];
            double[] d = new double[n]; // d为方程组右值
            double[] mu = new double[n - 1];
            double[] lambda = new double[n - 1];
            double[] theta = new double[n];
            double[] M = new double[n];
            double[] h = new double[n - 1];

            for (int i = 0; i < n * 4; i++)
            {
                xInterp[i] = i * 0.25;
            }

            for (int i = 0; i < n - 1; i++)
            {
                h[i] = xOrigin[i + 1] - xOrigin[i];
            }

            // mu(j)*M(j-1)+2*M(j)+lambda(j)*M(j+1)=d(j)  (j=2,3...N-1)
            // mu是下对角元素；lambda是上对角元素，theta是对角元素，d是方程组的右值
            for (int i = 1; i < n - 1; i++)
            {
                mu[i - 1] = h[i - 1] / (h[i - 1] + h[i]);
                lambda[i] = h[i] / ((h[i - 1] + h[i]));
                d[i] = 6 * ((yOrigin[i + 1] - yOrigin[i]) / h[i] - (yOrigin[i] - yOrigin[i - 1]) / h[i - 1]) / (h[i - 1] + h[i]);
                theta[i] = 2;
            }

            mu[n - 1 - 1] = 0;
            lambda[0] = 0;
            d[0] = 0;
            d[n - 1] = 0;
            theta[0] = 1;
            theta[n - 1] = 1;

            // 追赶法求解方程组
            M = Chase(mu, theta, lambda, d);

            // 三次多项式表达式s(x)=M(j)*(x)
            // 求解插值后的值
            int k = 0;
            for (int i = 0; i < 4 * (n - 1); i++)
            {
                if ((xInterp[i] > xOrigin[k]) && (xInterp[i] > xOrigin[k + 1]))
                {
                    k = k + 1;
                }

                yInterp[i] = M[k] * Math.Pow((xOrigin[k + 1] - xInterp[i]), 3) / (6 * h[k]) + M[k + 1] * Math.Pow((xInterp[i] - xOrigin[k]), 3) / (6 * h[k]) + (yOrigin[k] - M[k] * h[k] * h[k] / 6) * (xOrigin[k + 1] - xInterp[i]) / h[k] + (yOrigin[k + 1] - M[k + 1] * h[k] * h[k] / 6) * (xInterp[i] - xOrigin[k]) / h[k];
            }

            for (int i = 4 * (n - 1); i <= 4 * (n - 1) + 3; i++)
            {
                yInterp[i] = M[n - 2] * Math.Pow((xOrigin[n - 1] - xInterp[i]), 3) / (6 * h[n - 2]) + M[n - 1] * Math.Pow((xInterp[i] - xOrigin[n - 2]), 3) / (6 * h[n - 2]) + (yOrigin[n - 2] - M[n - 2] * h[n - 2] * h[n - 2] / 6) * (xOrigin[n - 1] - xInterp[i]) / h[n - 2] + (yOrigin[n - 1] - M[n - 1] * h[n - 2] * h[n - 2] / 6) * (xInterp[i] - xOrigin[n - 2]) / h[n - 2];
            }

            return yInterp;
        }

        private static double[] Chase(double[] a, double[] b, double[] c, double[] d)
        {
            int n = b.Length;
            double[] r = new double[n];
            double[] y = new double[n];
            double[] x = new double[n];

            // 求二对角型方程组系数
            r[0] = c[0] / b[0];
            y[0] = d[0] / b[0];

            for (int i = 1; i < n - 1; i++)
            {
                r[i] = c[i] / (b[i] - r[i - 1] * a[i - 1]);
                y[i] = (d[i] - y[i - 1] * a[i - 1]) / (b[i] - r[i - 1] * a[i - 1]);
            }

            y[n - 1] = (d[n - 1] - y[n - 1 - 1] * a[n - 1 - 1]) / (b[n - 1] - r[n - 1 - 1] * a[n - 1 - 1]);

            // 求解未知量
            x[n - 1] = y[n - 1]; // x是待求的未知量
            for (int i = n - 1 - 1; i >= 0; i--)
            {
                x[i] = y[i] - r[i] * x[i + 1];
            }
            return x;
        }


        /// <summary>
        /// 光谱漂移调整
        /// </summary>
        /// <param name="rawspec"></param>
        /// <returns></returns>
        public static double[] AjustMove(double[] rawspec,int step)
        {
            if (step == 0)
            {
                return rawspec;
            }
            else
            {
                double []result = new double[rawspec.Length];
                if (step > 0)
                {
                    Array.Copy(rawspec, step, result, 0, rawspec.Length - step);
                    for (int i = rawspec.Length - step; i < rawspec.Length; i++)
                    {
                        result[i] = 0;
                    }
                }
                else
                {
                    Array.Copy(rawspec, 0, result, -step, rawspec.Length + step);
                    for (int i = 0; i < step; i++)
                    {
                        result[i] = 0;
                    }
                }

                return result;
            }
        }


    }
}
