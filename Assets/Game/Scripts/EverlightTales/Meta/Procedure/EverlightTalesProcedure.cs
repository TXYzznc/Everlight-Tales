using System;
using Everlight.Tales.Data;
using Everlight.Tales.Meta.Input;
using GameFramework;
using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityGameFramework.Runtime;

namespace Everlight.Tales.Procedure
{
    /// <summary>
    /// 业务启动 Procedure。框架在通用预加载完成后按 AppConfigs 的流程列表解析本类型并切换过来。
    /// 本类型只承担启动职责：登记业务入口、初始化项目输入模块、转入业务场景加载；
    /// MUST NOT 在此实现玩法、表加载、存档或 UI 逻辑。
    /// </summary>
    [Obfuz.ObfuzIgnore(Obfuz.ObfuzScope.TypeName | Obfuz.ObfuzScope.MethodName)]
    public sealed class EverlightTalesProcedure : ProcedureBase, IFrameworkStartupProcedure
    {
        /// <summary>业务入口场景名，相对场景目录。</summary>
        public const string EntrySceneName = "Home";

        private bool mStarted;

        /// <inheritdoc />
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            if (mStarted)
            {
                return;
            }

            mStarted = true;
            GFTrace.Info("EverlightTales", "Procedure.Enter");
            Log.Info("Everlight-Tales business startup: layers={0}", ProjectLayers.Count);

            InitializeInput();
            EnterEntryScene(procedureOwner);
        }

        private void InitializeInput()
        {
            InputModule.EnsureInitialized();
            ProjectInputDriver.EnsureRunning();
            GFTrace.Success(
                "EverlightTales",
                "Input.Initialized",
                null,
                GFTrace.Data("provider", InputModule.DescribeProvider()));
        }

        private void EnterEntryScene(IFsm<IProcedureManager> procedureOwner)
        {
            try
            {
                // ChangeSceneProcedure 的参数键是其内部常量（对其它程序集不可见），
                // 这里按框架既有约定使用同一字符串键，避免为了一个键名扩大可见性。
                procedureOwner.SetData<VarString>("SceneName", EntrySceneName);
                ChangeState<ChangeSceneProcedure>(procedureOwner);
            }
            catch (Exception exception)
            {
                Log.Error("Everlight-Tales failed to enter entry scene '{0}': {1}", EntrySceneName, exception);
                GFTrace.Failure("EverlightTales", "Scene.Enter.Failure", exception.ToString());
            }
        }
    }
}
