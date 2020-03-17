using System;
using System.Collections;
using System.IO;

namespace VocsAutoTest.Algorithm
{
    /// <summary>
    /// 光谱文件操作spectrum
    /// </summary>
    class SpectrumFile
    {
        private const int GAS_NUMBER = 4;//最多选择气体种类
        private const string HEAD_GAS = "GAS";
        private const string HEAD_FLOW = "FLOW";
        private const string HEAD_SPEC = "SPEC";
        //浓度矩阵,每行对应1次测量，每列对应1种气体
        private float[,] thicknessData;
        //光谱矩阵,每行对应1次测量，每列对应1个象素
        private float[,] riData;
        //GAS气体名称及量度
        private ArrayList gasNode = new ArrayList();

        //获得浓度和光谱矩阵数据
        public void GetConcAndRiData(out float[,] Conc, out float[,] Ri, string fileName)
        {
            TextReader textReader = null;
            Conc = null;
            Ri = null;

            try
            {
                FileInfo file = new FileInfo(fileName);
                textReader = file.OpenText();
                string line = null;
                //read GAS
                while ((line = textReader.ReadLine()) != null)
                {
                    if (line.Trim().Equals(HEAD_GAS))
                        break;
                }


                //读取气体
                int gasIndex = 0;
                while ((line = textReader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Equals(HEAD_FLOW))
                        break;
                    string[] gas = ParseLine(line);
                    if ((gas != null) && (gas.Length == 2))
                    {
                        gasNode.Add(new GasNode(gasIndex, gas[0], gas[1]));
                    }
                    if (++gasIndex >= GAS_NUMBER)
                        break;
                }

                ArrayList itemList = new ArrayList();

                while ((line = textReader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Equals(HEAD_SPEC))
                        break;
                    string[] lineData = ParseLine(line);
                    if ((lineData != null) && (lineData.Length > 2))
                    {
                        int[] flowData = new int[(lineData.Length - 1) / 2];
                        for (int i = 0; i < flowData.Length; i++)
                            flowData[i] = int.Parse(lineData[2 + i]);
                        float[] thicknessData = new float[(lineData.Length - 3) / 2];
                        for (int i = 0; i < thicknessData.Length; i++)
                            thicknessData[i] = float.Parse(lineData[thicknessData.Length + 3 + i]);
                        itemList.Add(new ItemNode(lineData[0], lineData[1].Equals("True"),
                            new DataNode(flowData, thicknessData, null)));
                    }
                }

                //读取编号行
                textReader.ReadLine();
                //光谱数据
                while ((line = textReader.ReadLine()) != null)
                {
                    line = line.Trim();
                    string[] lineData = ParseLine(line);

                    if (lineData.Length == itemList.Count)
                    {
                        for (int i = 0; i < itemList.Count; i++)
                        {
                            ((ItemNode)itemList[i]).AddSpecData(lineData[i]);
                        }
                    }
                    else
                    {
                        Console.WriteLine("光谱行数据格式错误!");
                        return;
                    }

                }

                for (int i = 0; i < itemList.Count; i++)
                {
                    ((ItemNode)itemList[i]).SetSpecData();
                }

                //光谱数组长度大于512时
                if (itemList.Count > 0 && ((ItemNode)itemList[0]).dataNode.riData.Length > 512)
                {
                    return;
                }
                thicknessData = new float[itemList.Count, ((ItemNode)itemList[0]).dataNode.thicknessData.Length];

                riData = new float[512, itemList.Count];

                int index = 0;
                //循环测量数据
                for (int i = 0; i < itemList.Count; i++)
                {
                    DataNode node = ((ItemNode)itemList[i]).dataNode;
                    float[] oneMeasureThicknessData = node.thicknessData;
                    for (int j = 0; j < oneMeasureThicknessData.Length; j++)
                    {
                        thicknessData[index, j] = oneMeasureThicknessData[j];
                    }
                    float[] oneMeasureRiData = node.riData;
                    for (int j = 0; j < oneMeasureRiData.Length; j++)
                    {
                        riData[j, index] = oneMeasureRiData[j];
                    }
                    index++;
                }
                Conc = thicknessData;
                Ri = riData;
            }
            catch (Exception e)
            {
                Console.WriteLine("读取光谱文件错误: " + e.Message);
            }
            finally
            {
                if (textReader != null)
                    textReader.Close();
            }
        }

        private string[] ParseLine(string line)
        {
            if (line == null)
                return new string[0];
            ArrayList list = new ArrayList();
            line = line.Trim();
            while (line.Length > 0)
            {
                int index = line.IndexOf('\t');
                if (index > 0)
                {
                    list.Add(line.Substring(0, index).Trim());
                    line = line.Substring(index + 1).Trim();
                }
                else
                {
                    list.Add(line);
                    break;
                }
            }

            string[] returnArray = new string[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                returnArray[i] = (string)list[i];
            }
            return returnArray;
        }
    }
}
