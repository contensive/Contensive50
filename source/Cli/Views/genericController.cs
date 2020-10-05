
using System;

namespace Contensive.CLI {
    static class GenericController {
        //
        public static String promptForReply(String prompt, String currentValue, String defaultValue = "") {
            Console.Write(prompt + ": ");
            if (String.IsNullOrEmpty(currentValue)) currentValue = defaultValue;
            if (!String.IsNullOrEmpty(currentValue)) Console.Write("(" + currentValue + ")");
            String reply = Console.ReadLine();
            if (String.IsNullOrEmpty(reply)) reply = currentValue;
            return reply;
        }
    }
}
