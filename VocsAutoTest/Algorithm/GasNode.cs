namespace VocsAutoTest.Algorithm
{
    class GasNode
    {
        public string name;
        public float weight;
        public int index;
        public GasNode(int index, string name, string weight)
        {
            this.index = index;
            this.name = name;
            this.weight = System.Single.Parse(weight);
        }
    }
}
