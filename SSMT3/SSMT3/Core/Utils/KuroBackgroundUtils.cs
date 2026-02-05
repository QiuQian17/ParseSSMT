using Newtonsoft.Json.Linq;
using SharpCompress.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SSMT3.Core.Utils
{
    public class KuroBackgroundUtils
    {
        // 以后可能会变动, 可能需要长期维护
        private static string kuroLanuncherBackgroundRequestUrl = "https://prod-cn-alicdn-gamestarter.kurogame.com/launcher/10003_Y8xXrXk65DqFHEDgApn3cpK5lfczpFx5/G152/background/ucLkUF6LaT5NqcUzIoLQIUu3ALrdU2qL/zh-Hans.json?_t=1769515463";

        /// <summary>
        /// 获取鸣潮启动器背景URL
        /// </summary>
        /// <returns>如果背景URL获取成功则返回URL, 否则返回空字符串</returns>
        public static async Task<string> GetBackgroundUrlAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var result = JObject.Parse(await client.GetStringAsync(kuroLanuncherBackgroundRequestUrl));
                    if (result.ContainsKey("backgroundFile"))
                    {
                        return result["backgroundFile"].ToString();
                    }
                    else
                    {
                        return "";
                    }
                }
                catch (Exception ex)
                {
                    LOG.Error($"鸣潮启动器背景图URL获取失败: {ex.ToString()}");
                    // 如果请求的返回的压根不是字符串, 或者其它原因导致的报错, 那就真没招了, 直接返回空字符串
                    return "";
                }
            }
        }

        /// <summary>
        /// 下载鸣潮启动器背景
        /// </summary>
        /// <param name="url">背景URL</param>
        /// <param name="targetGame">目标游戏</param>
        /// <returns>如果下载成功就返回保存后的路径, 否则返回空字符串</returns>
        public static async Task<string> DownloadBackgroundAsync(string url, string targetGame)
        {
            string videoPath = url;
            string ext = Path.GetExtension(videoPath);
            string savePath = Path.Combine(PathManager.Path_GamesFolder, targetGame, "Background" + ext);

            try
            {
                using(HttpClient client = new HttpClient())
                {
                    using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();

                        using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                        using (FileStream fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await contentStream.CopyToAsync(fileStream);
                        }
                    }
                }

                return savePath;
            }
            catch (Exception ex)
            {
                LOG.Error($"视频下载失败: {ex}");
                return "";
            }
        }
    }
}
