
using System;

namespace Contensive.CLI {
    static class GenericController {
        //
        public static string promptForReply(string prompt, string currentValue, string defaultValue = "") {
            Console.Write(prompt + ": ");
            if (string.IsNullOrEmpty(currentValue)) currentValue = defaultValue;
            if (!string.IsNullOrEmpty(currentValue)) Console.Write("(" + currentValue + ")");
            String reply = Console.ReadLine();
            if (String.IsNullOrEmpty(reply)) reply = currentValue;
            return reply;
        }
    }
}
