
using System;
using System.Data;
using Contensive.Processor.Controllers;
using Contensive.BaseClasses;
using System.Collections.Generic;

namespace Contensive.Processor {
    //
    //====================================================================================================
    //
    public class CPDbModelClass : Models.Db.DbModel {
        //
        //private CPBaseClass cp;
        //
        //====================================================================================================
        //
        //public CPDbModelClass( CPBaseClass cp ) {
        //    this.cp = cp;
        //}

        public override int id { get; set; }
        public override string name  { get; set; }
        public override string ccguid  { get; set; }
        public override bool active  { get; set; }
        public override int contentControlID  { get; set; }
        public override int createKey  { get; set; }
        public override DateTime dateAdded  { get; set; }
        public override int modifiedBy  { get; set; }
        public override DateTime modifiedDate  { get; set; }
        public override string sortOrder  { get; set; }

        public override T AddEmpty<T>(CPBaseClass cp) where T : Models.Db.DbModel {
            return Models.Db.DbModel.AddEmpty<T>(((CPClass)cp).core);
        }

        public override T Create<T>(CPBaseClass cp, int recordId) {
            throw new NotImplementedException();
        }

        public override T Create<T>(CPBaseClass cp, int recordId, ref List<string> callersCacheNameList) {
            throw new NotImplementedException();
        }

        public override T create<T>(CPBaseClass cp, string recordGuid) {
            throw new NotImplementedException();
        }

        public override T Create<T>(CPBaseClass cp, string recordGuid, ref List<string> callersCacheNameList) {
            throw new NotImplementedException();
        }

        public override T CreateByUniqueName<T>(CPBaseClass cp, string recordName) {
            throw new NotImplementedException();
        }

        public override T CreateByUniqueName<T>(CPBaseClass cp, string recordName, ref List<string> callersCacheNameList) {
            throw new NotImplementedException();
        }

        public override T CreateEmpty<T>(CPBaseClass cp) {
            throw new NotImplementedException();
        }

        public override List<T> CreateList<T>(CPBaseClass cp, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            throw new NotImplementedException();
        }

        public override List<T> CreateList<T>(CPBaseClass cp, string sqlCriteria, string sqlOrderBy) {
            throw new NotImplementedException();
        }

        public override List<T> CreateList<T>(CPBaseClass cp, string sqlCriteria) {
            throw new NotImplementedException();
        }

        public override List<T> CreateList<T>(CPBaseClass cp) {
            throw new NotImplementedException();
        }

        public override void Delete<T>(CPBaseClass cp, int recordId) {
            throw new NotImplementedException();
        }

        public override void Delete<T>(CPBaseClass cp, string guid) {
            throw new NotImplementedException();
        }

        public override void DeleteSelection<T>(CPBaseClass cp, string sqlCriteria) {
            throw new NotImplementedException();
        }

        public override string DerivedDataSourceName(Type derivedType) {
            throw new NotImplementedException();
        }

        public override bool DerivedNameFieldIsUnique(Type derivedType) {
            throw new NotImplementedException();
        }

        public override string DerivedTableName(Type derivedType) {
            throw new NotImplementedException();
        }

        public override int GetRecordId<T>(CPBaseClass cp, string guid) {
            throw new NotImplementedException();
        }

        public override string GetRecordName<T>(CPBaseClass cp, int recordId) {
            throw new NotImplementedException();
        }

        public override string GetRecordName<T>(CPBaseClass cp, string guid) {
            throw new NotImplementedException();
        }

        public override string GetSelectSql<T>(CPBaseClass cp, List<string> fieldList, string sqlCriteria, string sqlOrderBy) {
            throw new NotImplementedException();
        }

        public override string GetSelectSql<T>(CPBaseClass cp, List<string> fieldList, string criteria) {
            throw new NotImplementedException();
        }

        public override string GetSelectSql<T>(CPBaseClass cp, List<string> fieldList) {
            throw new NotImplementedException();
        }

        public override string GetSelectSql<T>(CPBaseClass cp) {
            throw new NotImplementedException();
        }

        public override string GetTableCacheKey<T>(CPBaseClass cp) {
            throw new NotImplementedException();
        }

        public override void InvalidateRecordCache<T>(CPBaseClass cp, int recordId) {
            throw new NotImplementedException();
        }

        public override void InvalidateTableCache<T>(CPBaseClass cp) {
            throw new NotImplementedException();
        }

        public override int Save(CPBaseClass cp, bool asyncSave = false) {
            throw new NotImplementedException();
        }
    }
}