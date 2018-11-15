﻿
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
            string result = "";
            try {
                string sql = cp.Doc.GetText("sql");
                string datasource = cp.Doc.GetText("datasource");
                DataTable dt = cp.Db.ExecuteQuery(sql, datasource);
                result = dt.ToCsv();
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}