using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contensive.CLI {
    class cliController
    {
        //
        public static String promptForReply(String prompt, String defaultValue)
        {
            String reply = "";
            Console.Write( prompt + ": ");
            if (!String.IsNullOrEmpty(defaultValue)) Console.Write("(" + defaultValue + ")");
            reply = Console.ReadLine();
            if (String.IsNullOrEmpty(reply)) reply = defaultValue;
            return reply;
        }
    }
}
