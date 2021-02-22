
namespace Contensive.Processor.Models.Domain {
    public class CollectionDataRecordModel {
        /// <summary>
        /// The content for this export. if recordGuid and recordName are empty, export all records
        /// </summary>
        public string contentName { get; set; }
        /// <summary>
        /// if not empty, the guid of the record to export
        /// </summary>
        public string recordGuid { get; set; }
        /// <summary>
        /// valid only if recordGuid is empty. if not empty, the record name to be exported
        /// </summary>
        public string recordName { get; set; }
    }
}
