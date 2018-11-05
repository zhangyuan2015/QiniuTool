using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Qiniu.Http;
using Qiniu.Storage;
using Qiniu.Util;

namespace QiniuTool.Controllers
{
    [Route("api/Upload")]
    public class UploadController : Controller
    {
        [HttpPost]
        [Route("File")]
        public async Task<IActionResult> FileAction(List<IFormFile> files)
        {
            long size = files.Sum(f => f.Length);
            List<HttpResult> resList=new List<HttpResult>();
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var fileName = file.FileName;
                    //时间戳 +5分钟
                    long timeStamp= (DateTime.Now.AddMinutes(5).ToUniversalTime().Ticks - 621355968000000000) / 10000000;
                    //获取uploadToken
                    var tokenParamObj = new { scope = Const.BucketName + ":" + fileName, deadline = timeStamp };
                    string tokenParamJson = JsonConvert.SerializeObject(tokenParamObj);
                    string token = Auth.CreateUploadToken(new Mac(Const.AccessKey, Const.SecretKey), tokenParamJson);

                    string key = fileName;
                    HttpResult result = new UploadManager(new Config ()).UploadStream(file.OpenReadStream(), key, token, new PutExtra { });
                    resList.Add(result);
                }
            }

            // process uploaded files
            // Don't rely on or trust the FileName property without validation.

            return Ok(resList);
        }
    }
}