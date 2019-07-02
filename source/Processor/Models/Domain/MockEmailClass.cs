
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Domain {
    [System.Serializable]
    public class MockEmailClass {
        public EmailController.EmailClass email;
        public string AttachmentFilename = string.Empty;
    }

}