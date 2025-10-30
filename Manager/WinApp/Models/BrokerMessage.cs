using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class BrokerMessage
    {
        public string Name { get; set; }
        public string Time { get; set; }
        public string Content { get; set; }
        public string Type { get; set; }
        
        public string Color
        {
            get
            {
                switch (Type)
                {
                    case "warning": return "#C60";
                    case "danger": return "#C00";
                    case "success": return "#0C0";
                }
                return "#333";
            }
        }

        public BrokerMessage(string type, string name)
        {
            Type = type;
            Time = DateTime.Now.ToString("HH:mm:ss");
            Name = name;
        }

        public BrokerMessage SetContent(object content)
        {
            Content = content?.ToString();
            return this;
        }
    }
}
