﻿
using HandlebarsDotNet;

namespace Contensive.Processor.Controllers {
    /// <summary>
    /// Templating methods (Mustache, Stubble, Handlebars - need signed, and framework+core or standard)
    /// </summary>
    public static class TemplatingController {
        /// <summary>
        /// render template with dataset
        /// </summary>
        /// <param name="template"></param>
        /// <param name="dataSet"></param>
        /// <returns></returns>
        public static string renderStringToString(string template, object dataSet) {
            var templateCompiled = Handlebars.Compile(template);
            return templateCompiled(dataSet);
        }
    }
}