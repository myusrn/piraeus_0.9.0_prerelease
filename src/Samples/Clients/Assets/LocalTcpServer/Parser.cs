using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalTcpServer
{
    public class Parser
    {
        public static List<string> Switches;
        public static Dictionary<string,string> Parse(string[] args)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            int index = 0;
            while(index < args.Length)
            {
                if(Switches.Contains(args[index]))
                {
                    string parameter = args[index];
                    index++;
                    if(Switches.Contains(args[index]))
                    {
                        return null;
                    }
                    else
                    {
                        dict.Add(parameter, args[index]);
                    }
                }

                index++;
            }

            return dict;

        }
    }
}
