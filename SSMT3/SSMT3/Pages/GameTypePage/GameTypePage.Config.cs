using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSMT3
{
    public partial class GameTypePage
    {
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            SaveConfig();
            base.OnNavigatedFrom(e);
        }

        private void ReadConfig()
        {
            try
            {
                string configPath = PathManager.Path_GameTypePageConfig;
                if (!File.Exists(configPath))
                {
                    return;
                }

                string json = File.ReadAllText(configPath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return;
                }

                JObject settings = JObject.Parse(json);

                if (settings.ContainsKey("D3D11ElementList"))
                {
                    JArray elementArray = settings["D3D11ElementList"] as JArray;
                    if (elementArray != null)
                    {
                        List<D3D11Element> savedList = elementArray.ToObject<List<D3D11Element>>();
                        if (savedList != null)
                        {
                            D3D11ElementList.Clear();
                            foreach (D3D11Element element in savedList)
                            {
                                D3D11ElementList.Add(element);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            finally
            {
                if (D3D11ElementList.Count == 0)
                {
                    AddBlankD3D11ElementLine();
                }

                CalculateAndShowTotalStride();
            }
        }

        private void SaveConfig()
        {
            try
            {
                string configPath = PathManager.Path_GameTypePageConfig;
                string? directory = Path.GetDirectoryName(configPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                JObject settings = new JObject();
                JArray elementArray = new JArray();
                foreach (D3D11Element element in D3D11ElementList)
                {
                    if (element == null)
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(element.SemanticName))
                    {
                        continue;
                    }

                    elementArray.Add(JObject.FromObject(element));
                }

                settings["D3D11ElementList"] = elementArray;

                File.WriteAllText(configPath, settings.ToString());
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }
    }
}
