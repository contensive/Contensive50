
using System;
using System.Data;
//
namespace Contensive.Addons.ExportSql {
    //
    public class ExportCsvClass : Contensive.BaseClasses.AddonBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// execute an sql command on a given datasource and save the result as csv in a cdn file
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(Contensive.BaseClasses.CPBaseClass cp) {
            try {
                using ( var db = cp.DbNew(cp.Doc.GetText("datasource"))) {
                    using (DataTable dt = db.ExecuteQuery(cp.Doc.GetText("sql"))) {
                        return dt.ToCsv();
                    }
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return "";
        }
    }
}
