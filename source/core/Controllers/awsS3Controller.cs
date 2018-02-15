
//using System;
//using Amazon;
//using Amazon.S3;
//using Amazon.S3.Model;

//namespace Contensive.Core.Controllers {
//	public class awsS3Controller {
//        //
//        //====================================================================================================
//        /// <summary>
//        /// copy a file (object) up to s3
//        /// </summary>
//        /// <param name="srcLocalDosPathFilename"></param>
//        /// <param name="dstS3UnixPathFilename"></param>
//        /// <returns></returns>
//        public static bool copyLocalToS3(coreController core, fileController file, string srcLocalDosPathFilename, string dstS3UnixPathFilename) {
//            bool result = false;
//            try {
//                using (IAmazonS3 s3Client = new AmazonS3Client(core.serverConfig.awsAccessKey, core.serverConfig.awsSecretAccessKey, RegionEndpoint.USEast1)) {
//                    //
//                    // -- Setup request for putting an object in S3.
//                    PutObjectRequest request = new PutObjectRequest();
//                    request.BucketName = core.serverConfig.awsBucketName;
//                    request.Key = dstS3UnixPathFilename;
//                    request.FilePath = file.joinPath( file.rootLocalPath, srcLocalDosPathFilename);
//                    //
//                    // -- Make service call and get back the response.
//                    PutObjectResponse response = s3Client.PutObject(request);
//                    result = true;
//                }
//            } catch (Exception ex) {
//                core.handleException(ex);
//            }
//            return result;
//        }
//        //
//        //====================================================================================================
//        /// <summary>
//        /// copy a file (object) down from s3. The localDosPath must exist, and the file should NOT exist
//        /// </summary>
//        public static bool copyS3ToLocal( coreController core, fileController file, string srcS3UnixPathFilename, string dstLocalDosPath) {
//            bool result = false;
//            try {
//                using (IAmazonS3 s3Client = new AmazonS3Client(core.serverConfig.awsAccessKey, core.serverConfig.awsSecretAccessKey, RegionEndpoint.USEast1)) {
//                    GetObjectRequest request = new GetObjectRequest {
//                        BucketName = core.serverConfig.awsBucketName,
//                        Key = srcS3UnixPathFilename
//                    };
//                    using (GetObjectResponse response = s3Client.GetObject(request)) {
//                        var urlComponents = new genericController.urlComponentsClass();
//                        genericController.separateUrl(srcS3UnixPathFilename, urlComponents);
//                        string destPathFilename = file.joinPath( dstLocalDosPath , urlComponents.filename );
//                        //if (file.fileExists(destPathFilename)) {
//                        //    file.deleteFile(destPathFilename);
//                        //}
//                        response.WriteResponseStreamToFile(file.joinPath( file.rootLocalPath,  destPathFilename));
//                    }
//                }
//            } catch (Exception ex) {
//                core.handleException(ex);
//            }
//            return result;
//        }
//    }
//}