using System.Collections;

namespace VocsAutoTest.Algorithm
{
    /// <summary>
    /// 数据模型
    /// </summary>
    public class DataNode
    {
        //流量数据
        public int[] flowData;
        //浓度数据
        public float[] thicknessData;
        //光谱数据
        public float[] riData;

        public DataNode(int[] flowData, float[] thicknessData, float[] riData)
        {
            this.flowData = flowData;
            this.thicknessData = thicknessData;
            this.riData = riData;
        }
    }

    public class ItemNode
    {
        public string itemNumber;
        public bool itemChecked;
        public DataNode dataNode;
        private ArrayList tempSpecList;
        public ItemNode(string itemNumber, bool itemChecked, DataNode dataNode)
        {
            this.itemNumber = itemNumber;
            this.itemChecked = itemChecked;
            this.dataNode = dataNode;
            tempSpecList = new ArrayList();
        }

        public void AddSpecData(string specData)
        {
            tempSpecList.Add(float.Parse(specData));
        }

        public void SetSpecData()
        {
            dataNode.riData = new float[tempSpecList.Count];
            for (int i = 0; i < dataNode.riData.Length; i++)
                dataNode.riData[i] = (float)tempSpecList[i];
        }
    }

}
