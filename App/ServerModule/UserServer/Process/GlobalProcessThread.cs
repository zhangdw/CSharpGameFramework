﻿using System;
using System.Collections.Generic;
using GameFramework;
using GameFrameworkData;
using GameFrameworkMessage;
using System.Text;

namespace GameFramework
{
    enum GlobalParamEnum
    {
        LoginLotteryLastResetTime = 1,
        ConsumeId = 3,
        ConsumeResetTime = 4
    }
    /// <summary>
    /// 全局数据（非玩家拥有的数据）处理线程，在这里处理邮件、远征、帮会等数据。
    /// </summary>
    /// <remarks>
    /// 其它线程不应直接调用此类方法，应通过QueueAction发起调用。
    /// </remarks>
    internal sealed class GlobalProcessThread : MyServerThread
    {
        //=========================================================================================
        //全局数据初始化方法，由其它线程同步调用。
        //=========================================================================================
        internal bool LastSaveFinished
        {
            get { return m_LastSaveFinished; }
        }
        internal ulong GenerateUserGuid()
        {
            return m_GuidSystem.GenerateUserGuid();
        }
        internal ulong GenerateItemGuid()
        {
            return m_GuidSystem.GenerateItemGuid();
        }
        internal ulong GenerateMailGuid()
        {
            return m_GuidSystem.GenerateMailGuid();
        }
        internal ulong GenerateMemberGuid()
        {
            return m_GuidSystem.GenerateMemberGuid();
        }
        internal void InitGuidData(List<GuidInfo> guidList)
        {
            m_GuidSystem.InitGuidData(guidList);
        }
        //=========================================================================================
        //同步调用方法部分，其它线程可直接调用(需要考虑多线程安全)。
        //=========================================================================================

        //=========================================================================================
        //异步调用方法部分，其它线程通过QueueAction调用。
        //=========================================================================================

        protected override void OnStart()
        {
            TickSleepTime = 10;
            ActionNumPerTick = 10240;

            m_GuidCounter.SaveInterval = 121000;
        }
        protected override void OnTick()
        {
            long curTime = TimeUtility.GetLocalMilliseconds();
            if (m_LastTickTime != 0) {
                long elapsedTickTime = curTime - m_LastTickTime;
                if (elapsedTickTime > c_WarningTickTime) {
                    LogSys.Log(LOG_TYPE.MONITOR, "GlobalProcessThread Tick:{0}", elapsedTickTime);
                }
            }
            m_LastTickTime = curTime;
            if (m_LastLogTime + 60000 < curTime) {
                m_LastLogTime = curTime;

                DebugPoolCount((string msg) => {
                    LogSys.Log(LOG_TYPE.INFO, "GlobalProcessThread.ActionQueue {0}", msg);
                });
                LogSys.Log(LOG_TYPE.MONITOR, "GlobalProcessThread.ActionQueue Current Action {0}", this.CurActionNum);
            }
            //逻辑Tick
            UserServer.Instance.UserProcessScheduler.NicknameSystem.Tick();
            //全局数据存储
            long saveStartTime = 0;
            var dsThread = UserServer.Instance.DataCacheThread;
            if (dsThread.DataStoreAvailable) {
                if (m_GuidCounter.IsTimeToSave(curTime)) {
                    saveStartTime = TimeUtility.GetLocalMilliseconds();
                    dsThread.DSGSaveGuid(m_GuidSystem.GuidList, m_GuidCounter.NextSaveCount);
                    LogSys.Log(LOG_TYPE.DEBUG, "GlobalDataSave Guid SaveCount:{0}, Time:{1}", m_GuidCounter.NextSaveCount, TimeUtility.GetLocalMilliseconds() - saveStartTime);
                    m_GuidCounter.IncreaseNextSaveCount();
                }
                if (m_IsLastSave && IsLastSaveAllDone()) {
                    if (m_LastSaveFinished == false) {
                        //全局数据（Guid、战神赛排行榜，邮件，竞技场排行榜,军团,战力排行榜,抢矿玩家池）存储完成 
                        LogSys.Log(LOG_TYPE.MONITOR, "DoLastSaveGlobalData Step_2: GlobalData last save done");
                        m_IsLastSave = false;
                        m_LastSaveFinished = true;
                    }
                }
            }
        }
        protected override void OnQuit()
        {
        }
        private bool IsLastSaveAllDone()
        {
            bool isDone = m_GuidCounter.CurrentSaveCount == DataCacheThread.UltimateSaveCount;
            return isDone;
        }
        private bool CheckLastSaveDone(List<int> saveCountList)
        {
            foreach (int saveCount in saveCountList) {
                if (saveCount != DataCacheThread.UltimateSaveCount) {
                    return false;
                }
            }
            return true;
        }
        internal void DoLastSaveGlobalData()
        {
            LogSys.Log(LOG_TYPE.MONITOR, "DoLastSaveGlobalData Step_1: Start to save GlobalData.");
            m_LastSaveFinished = false;
            m_GuidCounter.SetUltimateNextSaveCount();

            var dsThread = UserServer.Instance.DataCacheThread;
            if (dsThread.DataStoreAvailable) {
                dsThread.DSGSaveGuid(m_GuidSystem.GuidList, m_GuidCounter.NextSaveCount);
            }
            m_GuidCounter.CurrentSaveCount = DataCacheThread.UltimateSaveCount;
            m_IsLastSave = true;
        }
        
        private GuidSystem m_GuidSystem = new GuidSystem();
        GlobalSaveCounter m_GuidCounter = new GlobalSaveCounter();            //Guid数据存储参数

        private const long c_WarningTickTime = 1000;
        private long m_LastTickTime = 0;
        private long m_LastLogTime = 0;
        private bool m_LastSaveFinished = false;      //最后一次数据存储完成状态
        private bool m_IsLastSave = false;            //是否在执行最后一次存储操作

        private const long c_TickInterval = 10000;
    }
}
