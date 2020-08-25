using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TestConsole {
    class Program {
        static void Main(string[] args) {
            using ( var cp = new Contensive.Processor.CPClass("app20200120100129")) {
                cp.CdnFiles.Save("testfile.txt", "some text");
                string text = cp.CdnFiles.Read("testfile.txt");
                //cp.Addon.Execute("fakename");
            }
        }
    }
}
