/* ==================================================================================
 * 👑 BOSS MODE MANAGER (V8 - LOG INSPECTOR & STACK TRACE)
 * ==================================================================================
 * Author:        Muhammet Serhat Tatar (M.S.T.)
 * Description:   Complete Debug Console with TextMeshPro support.
 * Fixes:         Added "Click to Expand" feature for logs.
 * Displays full Stack Trace (Script Name:Line Number).
 * Selected log highlighting.
 * ==================================================================================
 */

#if DEVELOPMENT_BUILD || UNITY_EDITOR

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;
using System.Collections;

namespace Utilities.BossMode
{
    public class BossModeManager : MonoBehaviour
    {
        // --- SETTINGS ---
        private const int MAX_LOGS = 1000;
        private const float ICON_SIZE = 120f;
        private const float ICON_PADDING = 20f;

        // --- STATE ---
        private static BossModeManager _instance;
        private bool _isOpen = false;
        private int _currentTab = 0;
        private int _tapCount = 0;
        private float _lastTapTime = 0;
        private bool _uiDirty = false;

        // 🔍 SELECTION STATE (NEW)
        private int _selectedLogIndex = -1; // -1 means nothing selected

        // --- ALERT SYSTEM ---
        private enum AlertLevel { None, Warning, Error }
        private AlertLevel _currentAlert = AlertLevel.None;
        private Color _baseIconColor = new Color(0.1f, 0.1f, 0.1f, 0.4f);
        private Image _iconImageRef;

        // --- DATA ---
        private class LogEntry
        {
            public string Message;
            public string StackTrace;
            public LogType Type;
            public string Timestamp;
        }
        private List<LogEntry> _logs = new List<LogEntry>();

        // Filter States
        private bool _collapseLogs = true;
        private bool _showLogs = true;
        private bool _showWarnings = true;
        private bool _showErrors = true;

        private class BossItem
        {
            public string Name;
            public string Category;
            public MemberInfo Member;
            public object TargetInstance;
            public bool IsEconomy;
            public TMP_InputField InputRef;
        }
        private List<BossItem> _allItems = new List<BossItem>();

        // --- UI REFERENCES ---
        private GameObject _canvasObj;
        private GameObject _panelRoot;
        private Transform _filterArea;
        private Transform _contentArea;
        private ScrollRect _scrollRect;
        private GameObject _iconButtonObj;

        // --- INITIALIZATION ---

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInit()
        {
            if (_instance != null) return;
            GameObject go = new GameObject("[BOSS_MODE_SYSTEM]");
            _instance = go.AddComponent<BossModeManager>();
            DontDestroyOnLoad(go);
        }

        private void Awake()
        {
            _instance = this;
            Application.logMessageReceived += HandleLog;
            BuildUI();
            _panelRoot.SetActive(false);
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
        }

        // --- MAIN LOOP ---

        private void Update()
        {
            if (Time.time - _lastTapTime > 0.5f) _tapCount = 0;

            if (_uiDirty && _isOpen)
            {
                RefreshContent();
                _uiDirty = false;
            }

            if (_isOpen && _currentTab != 0) UpdateLiveValues();

            UpdateAlertIcon();
        }

        private void UpdateAlertIcon()
        {
            if (!_isOpen && _iconImageRef != null)
            {
                if (_currentAlert == AlertLevel.Error)
                    _iconImageRef.color = Color.Lerp(_baseIconColor, new Color(1f, 0f, 0f, 0.8f), Mathf.PingPong(Time.time * 2f, 1f));
                else if (_currentAlert == AlertLevel.Warning)
                    _iconImageRef.color = Color.Lerp(_baseIconColor, new Color(1f, 0.8f, 0f, 0.8f), Mathf.PingPong(Time.time * 2f, 1f));
                else
                    _iconImageRef.color = _baseIconColor;
            }
        }

        // --- LOG LOGIC ---

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error) _currentAlert = AlertLevel.Error;
            else if (type == LogType.Warning && _currentAlert != AlertLevel.Error) _currentAlert = AlertLevel.Warning;

            if (_logs.Count > MAX_LOGS) _logs.RemoveAt(0);

            _logs.Add(new LogEntry
            {
                Message = logString,
                StackTrace = stackTrace,
                Type = type,
                Timestamp = DateTime.Now.ToString("HH:mm:ss")
            });

            if (_isOpen && _currentTab == 0) _uiDirty = true;
        }

        private void ClearLogs()
        {
            _logs.Clear();
            _currentAlert = AlertLevel.None;
            _selectedLogIndex = -1; // Reset selection
            if (_iconImageRef != null) _iconImageRef.color = _baseIconColor;
            _uiDirty = true;
        }

        // --- AUTO DISCOVERY ---

        private void ScanScene()
        {
            _allItems.Clear();
            ScanStatics();
            MonoBehaviour[] sceneObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var mb in sceneObjects) { if (mb != null) ScanInstance(mb); }
        }

        private void ScanStatics()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (assembly.FullName.StartsWith("Unity") || assembly.FullName.StartsWith("System")) continue;
                foreach (var type in assembly.GetTypes()) ScanMembers(type, null, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            }
        }

        private void ScanInstance(object target)
        {
            ScanMembers(target.GetType(), target, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        private void ScanMembers(Type type, object target, BindingFlags flags)
        {
            foreach (var field in type.GetFields(flags))
            {
                var attr = field.GetCustomAttribute<BossControlAttribute>();
                if (attr != null) AddItem(attr, field, target);
            }
            foreach (var prop in type.GetProperties(flags))
            {
                var attr = prop.GetCustomAttribute<BossControlAttribute>();
                if (attr != null && prop.CanRead && prop.CanWrite) AddItem(attr, prop, target);
            }
            foreach (var method in type.GetMethods(flags))
            {
                var attr = method.GetCustomAttribute<BossControlAttribute>();
                if (attr != null) AddItem(attr, method, target);
            }
        }

        private void AddItem(BossControlAttribute attr, MemberInfo member, object target)
        {
            string displayName = attr.Name;
            if (target != null && target is UnityEngine.Object obj) displayName += $" <color=#888888>({obj.name})</color>";
            _allItems.Add(new BossItem { Name = displayName, Category = attr.Category, Member = member, TargetInstance = target, IsEconomy = attr.IsEconomy });
        }

        // --- LIVE VALUES ---

        private void UpdateLiveValues()
        {
            for (int i = 0; i < _allItems.Count; i++)
            {
                var item = _allItems[i];
                if (item.InputRef == null || !item.InputRef.gameObject.activeInHierarchy) continue;
                if (item.InputRef.isFocused) continue;
                if (item.TargetInstance as UnityEngine.Object == null && item.TargetInstance != null) continue;

                object val = GetValue(item);
                if (val != null) item.InputRef.text = val.ToString();
            }
        }

        private object GetValue(BossItem item)
        {
            if (item.TargetInstance == null && !IsStatic(item.Member)) return null;
            if (item.Member is FieldInfo f) return f.GetValue(item.TargetInstance);
            if (item.Member is PropertyInfo p) return p.GetValue(item.TargetInstance);
            return null;
        }

        private void SetValue(BossItem item, string input)
        {
            try
            {
                if (item.Member is FieldInfo f) f.SetValue(item.TargetInstance, Convert.ChangeType(input, f.FieldType));
                else if (item.Member is PropertyInfo p) p.SetValue(item.TargetInstance, Convert.ChangeType(input, p.PropertyType));
            }
            catch { }
        }

        private bool IsStatic(MemberInfo member)
        {
            if (member is FieldInfo f) return f.IsStatic;
            if (member is PropertyInfo p) return (p.GetGetMethod(true)?.IsStatic) ?? false;
            if (member is MethodInfo m) return m.IsStatic;
            return false;
        }

        // --- UI SYSTEM ---

        private void TogglePanel()
        {
            _isOpen = !_isOpen;
            _panelRoot.SetActive(_isOpen);
            if (_isOpen) { ScanScene(); _uiDirty = true; }
            else { _allItems.Clear(); _selectedLogIndex = -1; }
        }

        private void SwitchTab(int tabIndex)
        {
            if (tabIndex == -1) { TogglePanel(); return; }
            _currentTab = tabIndex;
            _selectedLogIndex = -1; // Reset selection on tab switch
            _uiDirty = true;
        }

        private void RefreshContent()
        {
            foreach (Transform child in _filterArea) Destroy(child.gameObject);
            foreach (Transform child in _contentArea) Destroy(child.gameObject);

            if (_currentTab == 0) DrawConsole();
            else if (_currentTab == 1) DrawInspector(false);
            else if (_currentTab == 2) DrawInspector(true);

            // AUTO SCROLL (Only if nothing selected, otherwise it's annoying)
            if (_currentTab == 0 && _selectedLogIndex == -1) StartCoroutine(ScrollToBottomFrame());
        }

        private IEnumerator ScrollToBottomFrame()
        {
            yield return null;
            if (_scrollRect != null) _scrollRect.verticalNormalizedPosition = 0f;
        }

        // --- UI BUILDING ---

        private void BuildUI()
        {
            _canvasObj = new GameObject("[BossCanvas]");
            Canvas cvs = _canvasObj.AddComponent<Canvas>();
            cvs.renderMode = RenderMode.ScreenSpaceOverlay;
            cvs.sortingOrder = 30000;

            CanvasScaler scaler = _canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            _canvasObj.AddComponent<GraphicRaycaster>();
            DontDestroyOnLoad(_canvasObj);

            // ICON
            _iconButtonObj = CreateImage("SmartIcon", _canvasObj.transform, _baseIconColor);
            _iconImageRef = _iconButtonObj.GetComponent<Image>();

            RectTransform iconRect = _iconButtonObj.GetComponent<RectTransform>();
            iconRect.anchorMin = Vector2.one; iconRect.anchorMax = Vector2.one;
            iconRect.pivot = Vector2.one;
            iconRect.anchoredPosition = new Vector2(-ICON_PADDING, -ICON_PADDING);
            iconRect.sizeDelta = new Vector2(ICON_SIZE, ICON_SIZE);

            CreateTMPText("Lbl", _iconButtonObj.transform, "DEV", 18, Color.white, TextAlignmentOptions.Center);
            _iconButtonObj.AddComponent<Button>().onClick.AddListener(() => {
                _tapCount++; _lastTapTime = Time.time;
                if (_tapCount >= 3) { TogglePanel(); _tapCount = 0; }
            });

            // PANEL ROOT
            _panelRoot = CreateImage("MainPanel", _canvasObj.transform, new Color(0.1f, 0.1f, 0.1f, 0.98f));
            StretchRect(_panelRoot.GetComponent<RectTransform>());

            // TOP BAR
            GameObject topBar = CreateImage("TopBar", _panelRoot.transform, new Color(0.15f, 0.15f, 0.15f));
            RectTransform topRect = topBar.GetComponent<RectTransform>();
            topRect.anchorMin = new Vector2(0, 1); topRect.anchorMax = new Vector2(1, 1);
            topRect.pivot = new Vector2(0.5f, 1);
            topRect.anchoredPosition = Vector2.zero;
            topRect.sizeDelta = new Vector2(0, 120);

            HorizontalLayoutGroup tabsGrp = topBar.AddComponent<HorizontalLayoutGroup>();
            tabsGrp.childControlWidth = true; tabsGrp.childForceExpandWidth = true;
            tabsGrp.padding = new RectOffset(5, 5, 5, 5); tabsGrp.spacing = 5;

            CreateTabButton(topBar.transform, "LOGS", 0);
            CreateTabButton(topBar.transform, "VARS", 1);
            CreateTabButton(topBar.transform, "ECON", 2);
            CreateTabButton(topBar.transform, "X", -1, new Color(0.8f, 0.2f, 0.2f), 0.3f);
            CreateTabButton(topBar.transform, "RELOAD", 99, new Color(0.2f, 0.5f, 0.8f), 0.5f);

            // FILTER AREA
            GameObject filterBarObj = CreateImage("FilterBar", _panelRoot.transform, new Color(0.12f, 0.12f, 0.12f));
            RectTransform filterRect = filterBarObj.GetComponent<RectTransform>();
            filterRect.anchorMin = new Vector2(0, 1); filterRect.anchorMax = new Vector2(1, 1);
            filterRect.pivot = new Vector2(0.5f, 1);
            filterRect.anchoredPosition = new Vector2(0, -120);
            filterRect.sizeDelta = new Vector2(0, 90);

            HorizontalLayoutGroup filterLayout = filterBarObj.AddComponent<HorizontalLayoutGroup>();
            filterLayout.childControlWidth = true; filterLayout.childForceExpandWidth = true;
            filterLayout.padding = new RectOffset(5, 5, 5, 5); filterLayout.spacing = 5;

            _filterArea = filterBarObj.transform;

            // CONTENT SCROLL
            GameObject contentScroll = new GameObject("ScrollView");
            contentScroll.transform.SetParent(_panelRoot.transform, false);
            RectTransform scrollRectTrans = contentScroll.AddComponent<RectTransform>();
            scrollRectTrans.anchorMin = Vector2.zero;
            scrollRectTrans.anchorMax = Vector2.one;
            scrollRectTrans.offsetMin = Vector2.zero;
            scrollRectTrans.offsetMax = new Vector2(0, -210);

            _scrollRect = contentScroll.AddComponent<ScrollRect>();
            _scrollRect.horizontal = false;
            _scrollRect.scrollSensitivity = 30f;

            GameObject viewport = CreateImage("Viewport", contentScroll.transform, Color.clear);
            viewport.AddComponent<RectMask2D>();
            StretchRect(viewport.GetComponent<RectTransform>());

            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            _contentArea = content.AddComponent<RectTransform>();
            StretchRect(_contentArea.GetComponent<RectTransform>());

            _contentArea.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);

            VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.childControlHeight = true; vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 2; vlg.padding = new RectOffset(10, 10, 10, 10);

            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _scrollRect.content = _contentArea.GetComponent<RectTransform>();
            _scrollRect.viewport = viewport.GetComponent<RectTransform>();
        }

        // --- DRAWING LOGIC ---

        private void DrawConsole()
        {
            CreateFilterButton(_filterArea, "LOG", _showLogs, () => { _showLogs = !_showLogs; _uiDirty = true; });
            CreateFilterButton(_filterArea, "WARN", _showWarnings, () => { _showWarnings = !_showWarnings; _uiDirty = true; });
            CreateFilterButton(_filterArea, "ERR", _showErrors, () => { _showErrors = !_showErrors; _uiDirty = true; });
            CreateFilterButton(_filterArea, _collapseLogs ? "EXPAND" : "COLLAPSE", _collapseLogs, () => { _collapseLogs = !_collapseLogs; _uiDirty = true; });
            CreateFilterButton(_filterArea, "CLEAR", false, () => { ClearLogs(); }, Color.red);

            // Filter Logs
            var logsToDisplay = _logs.Where(x =>
                (x.Type == LogType.Log && _showLogs) ||
                (x.Type == LogType.Warning && _showWarnings) ||
                ((x.Type == LogType.Error || x.Type == LogType.Exception) && _showErrors)
            ).ToList(); // Convert to List for indexing

            if (_collapseLogs)
            {
                var grouped = logsToDisplay
                    .GroupBy(x => x.Message)
                    .Select(g => new { Entry = g.First(), Count = g.Count() }).ToList();

                for (int i = 0; i < grouped.Count; i++)
                {
                    DrawLogItem(grouped[i].Entry, grouped[i].Count, i);
                }
            }
            else
            {
                for (int i = 0; i < logsToDisplay.Count; i++)
                {
                    DrawLogItem(logsToDisplay[i], 1, i);
                }
            }
        }

        private void DrawLogItem(LogEntry log, int count, int index)
        {
            Color color = Color.white;
            if (log.Type == LogType.Warning) color = Color.yellow;
            else if (log.Type == LogType.Error || log.Type == LogType.Exception) color = new Color(1f, 0.4f, 0.4f);

            bool isSelected = (_selectedLogIndex == index);

            // Background Highlight for selection
            Color bgColor = isSelected ? new Color(0.3f, 0.3f, 0.3f, 1f) : new Color(0, 0, 0, 0);

            // Content Construction
            string prefix = count > 1 ? $"<color=cyan>[{count}]</color> " : "";
            string time = $"<color=grey>[{log.Timestamp}]</color> ";
            string header = $"{prefix}{time}{log.Message}";

            // Only show Stack Trace if selected!
            string fullText = header;
            if (isSelected)
            {
                fullText += $"\n\n<size=24><color=#aaaaaa><b>--- STACK TRACE ---</b>\n{log.StackTrace}</color></size>";
            }

            // Create Row
            GameObject rowObj = CreateImage("LogLine", _contentArea, bgColor);
            Button btn = rowObj.AddComponent<Button>();

            // CLICK LISTENER: EXPAND/COLLAPSE
            int capturedIndex = index;
            btn.onClick.AddListener(() =>
            {
                if (_selectedLogIndex == capturedIndex) _selectedLogIndex = -1; // Deselect
                else _selectedLogIndex = capturedIndex; // Select
                _uiDirty = true; // Redraw
            });

            // Layout
            VerticalLayoutGroup vlg = rowObj.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(10, 10, 10, 10);

            // Text Component
            CreateTMPText("Txt", rowObj.transform, fullText, 26, color, TextAlignmentOptions.Left, false);

            // Auto-size height based on content
            ContentSizeFitter csf = rowObj.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private void DrawInspector(bool economyMode)
        {
            CreateTMPText("Label", _filterArea, economyMode ? "ECONOMY SETTINGS" : "VARIABLES & METHODS", 32, Color.cyan, TextAlignmentOptions.Center, true);

            var items = _allItems.Where(x => x.IsEconomy == economyMode).GroupBy(x => x.Category);
            if (!items.Any()) { CreateTMPTextRow("No items found. Add [BossControl].", Color.gray, 60, TextAlignmentOptions.Center); return; }

            foreach (var group in items)
            {
                CreateTMPTextRow($"--- {group.Key.ToUpper()} ---", Color.cyan, 60, TextAlignmentOptions.Center);
                foreach (var item in group)
                {
                    if (item.Member is MethodInfo method)
                        CreateButtonRow(item.Name, () => { method.Invoke(item.TargetInstance, null); _uiDirty = true; });
                    else
                        CreateVariableRow(item);
                }
                CreateSpacer();
            }
        }

        // --- TMP HELPER METHODS ---

        private void CreateTabButton(Transform parent, string text, int index, Color? overrideColor = null, float widthWeight = 1f)
        {
            Color btnColor = overrideColor ?? (_currentTab == index ? new Color(0.4f, 0.4f, 0.4f) : new Color(0.2f, 0.2f, 0.2f));
            if (index == 99)
            {
                GameObject rBtn = CreateImage("Btn", parent, btnColor);
                rBtn.AddComponent<Button>().onClick.AddListener(() => { ScanScene(); _uiDirty = true; });
                rBtn.AddComponent<LayoutElement>().flexibleWidth = widthWeight;
                CreateTMPText("Txt", rBtn.transform, text, 24, Color.white, TextAlignmentOptions.Center, true);
                return;
            }

            GameObject go = CreateImage("Btn", parent, btnColor);
            Button btn = go.AddComponent<Button>();
            btn.onClick.AddListener(() => SwitchTab(index));
            LayoutElement le = go.AddComponent<LayoutElement>();
            le.flexibleWidth = widthWeight;
            Color txtColor = (_currentTab == index || overrideColor.HasValue) ? Color.white : Color.gray;
            CreateTMPText("Txt", go.transform, text, 28, txtColor, TextAlignmentOptions.Center, true);
        }

        private void CreateFilterButton(Transform parent, string text, bool active, UnityEngine.Events.UnityAction onClick, Color? activeColor = null)
        {
            Color c = active ? (activeColor ?? new Color(0.3f, 0.6f, 0.3f)) : new Color(0.25f, 0.25f, 0.25f);
            if (!active && activeColor.HasValue) c = new Color(0.3f, 0.1f, 0.1f);

            GameObject go = CreateImage("Btn", parent, c);
            Button btn = go.AddComponent<Button>();
            btn.onClick.AddListener(onClick);
            CreateTMPText("Txt", go.transform, text, 22, active || activeColor.HasValue ? Color.white : Color.gray, TextAlignmentOptions.Center, true);
        }

        private void CreateVariableRow(BossItem item)
        {
            GameObject go = new GameObject("VarRow");
            go.transform.SetParent(_contentArea, false);
            go.AddComponent<LayoutElement>().minHeight = 90;
            HorizontalLayoutGroup hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 15; hlg.childControlWidth = true; hlg.childForceExpandWidth = false;

            GameObject lbl = new GameObject("Lbl"); lbl.transform.SetParent(go.transform, false);
            CreateTMPText("Txt", lbl.transform, item.Name + ":", 28, Color.white, TextAlignmentOptions.Left);
            LayoutElement le1 = lbl.AddComponent<LayoutElement>(); le1.flexibleWidth = 1; le1.minWidth = 200;

            GameObject inputBg = CreateImage("InputBg", go.transform, new Color(0.15f, 0.15f, 0.15f));
            LayoutElement le2 = inputBg.AddComponent<LayoutElement>(); le2.preferredWidth = 350; le2.flexibleWidth = 0;

            TMP_InputField input = CreateTMPInput(inputBg.transform);
            object val = GetValue(item);
            if (val != null) input.text = val.ToString();
            input.onEndEdit.AddListener((newValue) => SetValue(item, newValue));
            int i = _allItems.IndexOf(item); item.InputRef = input; _allItems[i] = item;
        }

        private TMP_InputField CreateTMPInput(Transform parent)
        {
            GameObject textArea = new GameObject("TextArea", typeof(RectTransform));
            textArea.transform.SetParent(parent, false);
            StretchRect(textArea.GetComponent<RectTransform>(), 5, 5, 10, 10);
            textArea.AddComponent<RectMask2D>();

            GameObject textObj = new GameObject("Text", typeof(RectTransform));
            textObj.transform.SetParent(textArea.transform, false);
            StretchRect(textObj.GetComponent<RectTransform>());

            TextMeshProUGUI textComp = textObj.AddComponent<TextMeshProUGUI>();
            textComp.fontSize = 28; textComp.color = Color.yellow; textComp.alignment = TextAlignmentOptions.Center;

            GameObject placeObj = new GameObject("Placeholder", typeof(RectTransform));
            placeObj.transform.SetParent(textArea.transform, false);
            StretchRect(placeObj.GetComponent<RectTransform>());

            TextMeshProUGUI placeComp = placeObj.AddComponent<TextMeshProUGUI>();
            placeComp.fontSize = 28; placeComp.color = new Color(1, 1, 1, 0.2f); placeComp.alignment = TextAlignmentOptions.Center;
            placeComp.text = "...";

            TMP_InputField input = parent.gameObject.AddComponent<TMP_InputField>();
            input.textViewport = textArea.GetComponent<RectTransform>();
            input.textComponent = textComp;
            input.placeholder = placeComp;
            input.fontAsset = GetTMPFont();
            return input;
        }

        private void CreateButtonRow(string label, UnityEngine.Events.UnityAction onClick)
        {
            GameObject go = CreateImage("Btn_" + label, _contentArea, new Color(0.2f, 0.4f, 0.6f));
            go.AddComponent<LayoutElement>().minHeight = 90;
            Button btn = go.AddComponent<Button>();
            btn.onClick.AddListener(onClick);
            CreateTMPText("Txt", go.transform, label.ToUpper() + " [RUN]", 30, Color.white, TextAlignmentOptions.Center, true);
        }

        private void CreateTMPTextRow(string content, Color color, int height = 50, TextAlignmentOptions align = TextAlignmentOptions.Left)
        {
            GameObject go = new GameObject("Line");
            go.transform.SetParent(_contentArea, false);
            go.AddComponent<LayoutElement>().minHeight = height;
            CreateTMPText("Txt", go.transform, content, 24, color, align);
        }

        private void CreateTMPText(string name, Transform parent, string content, float size, Color color, TextAlignmentOptions align, bool bold = false)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            StretchRect(go.AddComponent<RectTransform>());

            TextMeshProUGUI txt = go.AddComponent<TextMeshProUGUI>();
            txt.text = content;
            txt.fontSize = size;
            txt.color = color;
            txt.alignment = align;
            txt.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            txt.enableWordWrapping = true;
            txt.overflowMode = TextOverflowModes.Truncate;
            txt.font = GetTMPFont();
        }

        private TMP_FontAsset GetTMPFont()
        {
            return Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        }

        private void CreateSpacer()
        {
            GameObject spacer = new GameObject("Spacer");
            spacer.transform.SetParent(_contentArea, false);
            spacer.AddComponent<LayoutElement>().minHeight = 20;
        }

        private GameObject CreateImage(string name, Transform parent, Color color)
        {
            GameObject go = new GameObject(name);
            go.AddComponent<RectTransform>();
            go.transform.SetParent(parent, false);
            Image img = go.AddComponent<Image>();
            img.color = color;
            return go;
        }

        private void StretchRect(RectTransform rt, float top = 0, float bottom = 0, float left = 0, float right = 0)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom); rt.offsetMax = new Vector2(-right, -top);
        }
    }
}
#endif