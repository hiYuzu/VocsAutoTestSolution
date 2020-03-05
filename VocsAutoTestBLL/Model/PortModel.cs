using System;

namespace VocsAutoTestBLL.Model
{
    public class PortModel
    {
        public string Port { get; set; }
        public string Baud { get; set; }
        public string Parity { get; set; }
        public string Data { get; set; }
        public string Stop { get; set; }

        public PortModel(string port, string baud, string parity, string data, string stop)
        {
            Port = port;
            Baud = baud;
            Parity = parity;
            Data = data;
            Stop = stop;
        }
    }
}
