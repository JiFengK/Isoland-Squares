﻿#region --------------------------文件信息--------------------------------------
/******************************************************************
** 文件名:	UIManager
** 版  权:	(C)  
** 创建人:  Unity喵
** 日  期:	
** 描  述: 	
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
*******************************************************************/
#endregion

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LoveYouForever
{
    public class UIManager : Single<UIManager>
    {
        /// <summary>
        /// UI层级
        /// </summary>
        public enum Layer
        {
            Top,
            Mid,
            Bot,
        }

        /// <summary>
        /// UI预设体路径
        /// </summary>
        private string uiPrefabPath = "Prefabs/";

        /// <summary>
        /// UI
        /// </summary>
        public Transform UI { get; }

        /// <summary>
        /// Canvas层
        /// </summary>
        public Transform UIRoot { get; }

        /// <summary>
        /// UI顶层
        /// </summary>
        public Transform UITop { get; }

        /// <summary>
        /// UI中层
        /// </summary>
        public Transform UIMid { get; }

        /// <summary>
        /// UI底层
        /// </summary>
        public Transform UIBot { get; }

        /// <summary>
        /// UI画布
        /// </summary>
        public Canvas RootCanvas { get; }

        /// <summary>
        /// Canvas层的RectTransform
        /// </summary>
        public RectTransform RootRectTransform { get; }

        /// <summary>
        /// 画布尺寸相关
        /// </summary>
        public CanvasScaler RootScaler { get; }

        /// <summary>
        /// 半屏宽
        /// </summary>
        public float HalfScreenW => Screen.width * 0.5f;

        /// <summary>
        /// 半屏高
        /// </summary>
        public float HalfScreenH => Screen.height * 0.5f;

        private Vector2 screenUI;
        
        /// <summary>
        /// 半屏UI位置
        /// </summary>
        public Vector2 ScreenUI => screenUI;

        /// <summary>
        /// 真机比率
        /// </summary>
        public float RealMachineRatio => (float)Screen.height / Screen.width;

        /// <summary>
        /// 预设比率
        /// </summary>
        public float PreaetRatio => RootScaler.referenceResolution.y / RootScaler.referenceResolution.x;

        /// <summary>
        /// UI面板管理表
        /// </summary>
        Dictionary<string, UIBase> uiPanelList;

        private UIManager()
        {
            uiPanelList = new Dictionary<string, UIBase>();
            UI = ResLoad.LoadPrefab(uiPrefabPath + "UI").transform;
            UIRoot = UI.Find("Canvas");
            UITop = UIRoot.Find("Top");
            UIMid = UIRoot.Find("Mid");
            UIBot = UIRoot.Find("Bot"); 
            RootCanvas = UIRoot.GetComponent<Canvas>();
            RootRectTransform = UIRoot.GetComponent<RectTransform>();
            RootScaler = UIRoot.GetComponent<CanvasScaler>();
            Object.DontDestroyOnLoad(UI.gameObject);
            ScreenToUI(new Vector3(Screen.width, Screen.height), out screenUI);
        }

        /// <summary>
        /// 屏幕坐标转UI坐标
        /// </summary>
        public void ScreenToUI(Vector3 v3, out Vector2 v2)
        {
            v2 = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(RootRectTransform, v3, RootCanvas.worldCamera, out v2);
        }

        /// <summary>
        /// 显示面板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="panelName"></param>
        /// <param name="prefabName"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public T ShowPanel<T>(string panelName, string prefabName, Layer layer = Layer.Mid)
            where T : UIBase
        {
            UIBase panel;
            if (!IsHavePanel(panelName))
            {
                GameObject obj = ResLoad.LoadPrefab(uiPrefabPath + prefabName);
                obj.name = panelName;
                //设置层级
                RectTransform tr = obj.transform as RectTransform;
                switch (layer)
                {
                    case Layer.Top: tr.SetParent(UITop); break;
                    case Layer.Mid: tr.SetParent(UIMid); break;
                    case Layer.Bot: tr.SetParent(UIBot); break;
                }
                tr.localPosition = Vector3.zero;
                tr.localScale = Vector3.one;
                tr.offsetMax = Vector2.zero;
                tr.offsetMin = Vector2.zero;
                panel = obj.AddComponent<T>();
                uiPanelList.Add(panelName, panel);
                panel.Init();
            }
            else
            {
                panel = uiPanelList[panelName];
                panel.transform.SetAsLastSibling();
            }
            panel.Show();
            return panel as T;
        }

        /// <summary>
        /// 动态加载UI预设体
        /// </summary>
        /// <param name="prefabName"></param>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        public GameObject LoadUIPrefab(string prefabName,Transform parent,string name = null)
        {
            GameObject obj = ResLoad.LoadPrefab(uiPrefabPath + prefabName);
            obj.name = name != String.Empty ? name : prefabName;
            RectTransform tr = obj.transform as RectTransform;
            tr.SetParent(parent);
            tr.localPosition = Vector3.zero;
            tr.localScale = Vector3.one;
            tr.offsetMax = Vector2.zero;
            tr.offsetMin = Vector2.zero;
            return obj;
        }

        /// <summary>
        /// 隐藏面板
        /// </summary>
        /// <param name="panelName"></param>
        public void HidePanel(string panelName)
        {
            if (IsHavePanel(panelName))
            {
                uiPanelList[panelName].Hide();
            }
        }

        /// <summary>
        /// 隐藏所有面板
        /// </summary>
        public void HideAllPanel()
        {
            Dictionary<string, UIBase>.Enumerator enumerator
                = uiPanelList.GetEnumerator();
            while (enumerator.MoveNext())
            {
                uiPanelList[enumerator.Current.Key].Hide();
            }
        }

        /// <summary>
        /// 移除面板
        /// </summary>
        /// <param name="panelName"></param>
        public void DestroyPanel(string panelName)
        {
            if (IsHavePanel(panelName))
            {
                uiPanelList[panelName].Destroy();
                uiPanelList.Remove(panelName);
            }
        }

        /// <summary>
        /// 移除所有面板
        /// </summary>
        public void DestroyAllPanel()
        {
            Dictionary<string, UIBase>.Enumerator enumerator = uiPanelList.GetEnumerator();
            while (enumerator.MoveNext())
            {
                uiPanelList[enumerator.Current.Key].Destroy();
            }
            uiPanelList.Clear();
        }

        /// <summary>
        /// 获取面板
        /// </summary>
        /// <param name="panelName"></param>
        /// <returns></returns>
        public T GetPanel<T>(string panelName)
            where T: UIBase
        {
            if (IsHavePanel(panelName))
                return uiPanelList[panelName] as T;
            return null;
        }

        /// <summary>
        /// 是否有对应名字的面板
        /// </summary>
        /// <param name="panelName"></param>
        /// <returns></returns>
        bool IsHavePanel(string panelName)
        {
            return uiPanelList.ContainsKey(panelName);
        }
    }
}
