using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contensive.CLI {
    class GenericController {
        //
        public static String promptForReply(String prompt, String currentValue, String defaultValue = "") {
            String reply = "";
            Console.Write(prompt + ": ");
            if (String.IsNullOrEmpty(currentValue)) currentValue = defaultValue;
            if (!String.IsNullOrEmpty(currentValue)) Console.Write("(" + currentValue + ")");
            reply = Console.ReadLine();
            if (String.IsNullOrEmpty(reply)) reply = currentValue;
            return reply;
        }
    }
}
