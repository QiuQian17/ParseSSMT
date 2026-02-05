using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SSMT3;
using SSMT3.Core.Games;
using SSMT3.InfoItemClass;

namespace SSMT3
{
    public partial class CoreFunctions
    {

        public static bool ExtractModel(List<DrawIBItem> DrawIBItemList)
        {

            bool RunResult = false;

            try
            {
                
                LOG.Info("FrameAnalysisFolderPath: " + PathManager.Path_LatestFrameAnalysisFolder);

                

                GameConfig gameConfig = new GameConfig();

                //if (gameConfig.GamePreset == GamePreset.GIMI ||
                //    gameConfig.GamePreset == GamePreset.HIMI ||
                //    gameConfig.GamePreset == GamePreset.SRMI ||
                //    gameConfig.GamePreset == GamePreset.ZZMI)
                //{
                //    LOG.Error(SSMTErrorInfo.NotSupportedGamePreset());
                //    return false;
                //}


                if (gameConfig.LogicName == LogicName.SRMI)
                {
                    //HSR重写渲染管线和Shader，很特殊
                    RunResult = SRMI.ExtractHSR32New(DrawIBItemList);
                }
                else if (gameConfig.LogicName == LogicName.WWMI)
                {
                    RunResult = WWMIV2.ExtractWWMI(DrawIBItemList);
                }
                else if (gameConfig.LogicName == LogicName.WuWa)
                {
                    RunResult = WWMIV2.ExtractWWMI(DrawIBItemList);
                }
                else if (gameConfig.LogicName == LogicName.AEMI)
                {
                    RunResult = AEMI.ExtractUnityVS(DrawIBItemList);
                }
                else if (gameConfig.LogicName == LogicName.YYSLS)
                {
                    RunResult = YYSLS.ExtractModel(DrawIBItemList);
                }
                else if (gameConfig.LogicName == LogicName.CTXMC)
                {
                    RunResult = IdentityV.ExtractCTX(DrawIBItemList);
                }
                else if (gameConfig.LogicName == LogicName.IdentityV2)
                {
                    RunResult = IdentityV2.ExtractModel(DrawIBItemList);
                }
                else if (gameConfig.LogicName == LogicName.HOK)
                {
                    RunResult = HOK.ExtractModel(DrawIBItemList);
                }
                else if (gameConfig.LogicName == LogicName.ZZMI)
                {
                    RunResult = ZZMI.ExtractModel(DrawIBItemList);
                }
                else if (gameConfig.LogicName == LogicName.UnityCPU)
                {
                    RunResult = UnityCPU.ExtractModel(DrawIBItemList);
                }
                else if (gameConfig.LogicName == LogicName.AILIMIT)
                {
                    RunResult = AILimit.ExtractUnityVS(DrawIBItemList);
                }
                else if (gameConfig.LogicName == LogicName.UnityCS)
                {
                    RunResult = UnityCS.ExtractUnityCS(DrawIBItemList);
                }
                else if (   gameConfig.LogicName == LogicName.UnityVS)
                {
                    RunResult = UnityVS.ExtractUnityVS(DrawIBItemList);
                }
                else if (gameConfig.LogicName == LogicName.NierR)
                {
                    RunResult = NierR.ExtractUnityVS(DrawIBItemList);
                }
                else if (gameConfig.LogicName == LogicName.GIMI)
                {
                    RunResult = GIMI.ExtractUnityVS(DrawIBItemList);
                }
                else if (gameConfig.LogicName == LogicName.HIMI)
                {
                    RunResult = HIMI.ExtractUnityVS(DrawIBItemList);
                }
                else if (gameConfig.LogicName == LogicName.SnowBreak)
                {
                    RunResult = SnowBreak.ExtractModel(DrawIBItemList);
                }
                else if (gameConfig.LogicName == LogicName.UnityCSM)
                {
                    RunResult = UnityCSM.Extract(DrawIBItemList);
                }
                else
                {
                    LOG.Error("未知的执行逻辑名称: " + gameConfig.LogicName + "\n请先前往主页的游戏设置中指定执行逻辑");
                    RunResult = false;
                }
            }
            catch (Exception ex)
            {
                LOG.Error(ex.ToString());
                RunResult = false;
            }

            return RunResult;
        }
    }
}
