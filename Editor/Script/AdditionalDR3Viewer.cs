using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Diagnostics;
using UnityEngine.Events;
using System.Reflection;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor;
using EditorAppUtil;
#endif
//using ADR3Util;


//データ保存
/*public class DR3VPEditorSetting : ScriptableObject
{
	public bool s_debug = false;
    public int s_SongKeywordIndex = 0;
    public int s_SongHardIndex = 0;
}*/


    public class DR3PStaticParameter : ScriptableSingleton<DR3PStaticParameter>
    {
	    public string editorstarttime;

        public string FixSongKeyword = "null";
        public string FixSongHard = "null";

        public string SongKeyword;
        public int SongHard;

        public float ChartStartSection = 0f;

        public bool ChartFix = false;

        public List<AdditionalDR3Viewer.SCList> sclist = new List<AdditionalDR3Viewer.SCList>();

        public bool isRealStart = false;
    }



public class AdditionalDR3Viewer : EditorWindow, IHasCustomMenu
{
    //データ保存
    //public static DR3VPEditorSetting setting;


    [MenuItem("DR3Viewer/WindowOpen")]
    static void MainMenuOpen()
    {

        var window = GetWindow<AdditionalDR3Viewer>();
        window.titleContent = new GUIContent("DR3Viewer");
        //Gamemanager = GameObject.Find("GameManager").GetComponent<TheGameManager>();

        //データ保存
        /*var path = "Assets/DR3ViewerPlus/SaveData/DR3VPEditorSetting.asset";
		setting = AssetDatabase.LoadAssetAtPath<DR3VPEditorSetting>(path);

        if(setting==null)
		{ // ロードしてnullだったら存在しないので生成
			setting = ScriptableObject.CreateInstance<DR3VPEditorSetting>();
			AssetDatabase.CreateAsset(setting, path);
		}*/
        
    }

    //ウィンドウの右クリックメニュー
    public void AddItemsToMenu( GenericMenu menu )
    {
        menu.AddItem(new GUIContent("ログファイルを開く"), false, () => OpenLog());
        menu.AddItem(new GUIContent("全てのログファイルを消去"), false, () => DaleteLog());
    }


    private Vector2 _scrollPosition = Vector2.zero;



    bool emergency = false;
    System.Exception error = new System.Exception();

    [SerializeField]
    bool debug, debugfold, FoldSongList, FoldAdjust, FoldUtility;
    bool debugquick;

    [SerializeField]
    bool ChartFix, isRealStart;

    [SerializeField] float ChartStartSection;

    [SerializeField]
    int SongKeywordIndex = 0;
    [SerializeField]
    string[] SongKeywords, SongKeywordsEX, SongHardNum;

    [SerializeField]
	int SongHardIndex = 0;

    static float bgmtime;

    static float setms = 0;

    bool FoldSCList;

    [SerializeField]
    float OffsetAdjust , OffsetAdjust_old;
    


    [SerializeField]
    int FPSLimit_now;

    /*
    int AspectIndex;
    Vector2 CustomAspect = new Vector2();
    */

    readonly string[] fpsmenu = new string[]
    {
        "AUTO",
        "20FPS",
        "30FPS",
        "60FPS",
        "90FPS",
        "120FPS",
        "144FPS",
        "165FPS",
        "240FPS"
    };
    readonly int[] fpsmenu_value = new int[]
    {
        -1,
        20,
        30,
        60,
        90,
        120,
        144,
        165,
        240
    };
    /*
    readonly string[] Aspectmenu = new string[]
    {
        "1600:900(16:9)",
        "1280:720(16:9)",
        "640,360(16:9)",
        "1400:1050(4:3)",
        "1024:768(4:3)",
        "800:600(4:3)",
        "custom"
    };
    readonly Vector2[] Aspectmenu_value = new Vector2[]
    {
        new Vector2(1600,900),
        new Vector2(1280,720),
        new Vector2(640,360),
        new Vector2(1400,1050),
        new Vector2(1024,768),
        new Vector2(800,600),
    };
    */
    


    float lastsctime = 0f;

    public class SCList
    {
        public int id;
        public float sc;
        public float sci;
        public float scms;
        public float interval;
    }

    [SerializeField] List<SCList> sclist;


    


    FieldInfo f_SongKeyword;
    FieldInfo f_SongHard;
    readonly FieldInfo f_CurrentSC;
    readonly FieldInfo f_BGMManager;
    readonly FieldInfo f_NoteOffset;
    FieldInfo f_drbfile;
    FieldInfo f_COMBO;
    FieldInfo f_PERFECT_J;
    readonly FieldInfo f_PERFECT;
    readonly FieldInfo f_GOOD;
    readonly FieldInfo f_MISS;
    FieldInfo f_isCreated;
    FieldInfo f_makenotestart;
    FieldInfo f_MAXCOMBO;
    
    GameObject gmcamera;

    MethodInfo fm_COMBOREFLASH;

    FieldInfo f_onpu;
    FieldInfo f_onpu_ms;

    static bool UpdateCheckRemoved, UpdateCheckOffset;




    AdditionalDR3Viewer()
    {
        //TheGameManagerへのアクセス権

        var gm = typeof(TheGameManager);
        var drb = typeof(TheGameManager.DRBFile);
        var onpu = typeof(TheOnpu);
        //drbfile.type = gm.GetNestedType("DRBFile");

        //var drbfile = Activator.CreateInstance(gm.GetNestedType("DRBFile"));
        //System.Type drbfiletype = System.Reflection.Assembly.Load("UnityEngine.dll").GetType("TheGameManager.DRBFile");
        


        //var drb = typeof(DRBFile);
        f_SongKeyword = gm.GetField("SongKeyword",BindingFlags.NonPublic | BindingFlags.Instance);
        f_SongHard = gm.GetField("SongHard",BindingFlags.NonPublic | BindingFlags.Instance);
        f_CurrentSC = gm.GetField("CurrentSC",BindingFlags.NonPublic | BindingFlags.Instance);
        f_BGMManager = gm.GetField("BGMManager",BindingFlags.NonPublic | BindingFlags.Instance);
        f_NoteOffset = gm.GetField("NoteOffset",BindingFlags.NonPublic | BindingFlags.Instance);
        f_PERFECT_J = gm.GetField("PERFECT_J",BindingFlags.NonPublic | BindingFlags.Instance);
        f_PERFECT = gm.GetField("PERFECT",BindingFlags.NonPublic | BindingFlags.Instance);
        f_GOOD = gm.GetField("GOOD",BindingFlags.NonPublic | BindingFlags.Instance);
        f_MISS = gm.GetField("MISS",BindingFlags.NonPublic | BindingFlags.Instance);
        f_COMBO = gm.GetField("COMBO",BindingFlags.NonPublic | BindingFlags.Instance);
        f_isCreated = gm.GetField("isCreated",BindingFlags.NonPublic | BindingFlags.Instance);
        f_makenotestart = gm.GetField("makenotestart",BindingFlags.NonPublic | BindingFlags.Instance);
        f_MAXCOMBO = gm.GetField("MAXCOMBO",BindingFlags.NonPublic | BindingFlags.Instance);

        fm_COMBOREFLASH = gm.GetMethod("COMBOREFLASH",BindingFlags.NonPublic | BindingFlags.Instance);


        f_drbfile = gm.GetField("drbfile",BindingFlags.NonPublic | BindingFlags.Instance);
        //f_onpu = drb.GetField("onpu",BindingFlags.NonPublic | BindingFlags.Instance);
        //f_onpu_ms = onpu.GetField("ms",BindingFlags.NonPublic | BindingFlags.Instance);


    }

    void RegacyObjectRemove()
    {
        //アップデート削除対象オブジェクト
        GameObject.DestroyImmediate(GameObject.Find("CameraDummy"));



        UpdateCheckRemoved = true;
    }

    

    void  OnInspectorUpdate()
    {
        if(!emergency)
        {
            if(!UpdateCheckRemoved) RegacyObjectRemove();
            UpdateObjectCheck();
            //最初にオブジェクト認識(初回のみ)
            InitObject();
            var instance = DR3PStaticParameter.instance;
            gameplus.editorstarttime = instance.editorstarttime;
            gameplus.ChartStartSection = instance.ChartStartSection;
            gameplus.ChartFix = instance.ChartFix;
            gameplus.isRealStart = instance.isRealStart;
            gameplus.SongKeyword = instance.SongKeyword;
            gameplus.SongHard = instance.SongHard;
            //再描画
            Repaint();
        }
        
    }

    //オブジェクト関係は最初に一回取るだけにしたいからグローバル化
    TheGameManager gamemanager;
    GamePlus gameplus;
    //DRBFile drbfile;
    GameObject NotesDown;
    GameObject NotesUp;
    AudioSource tmp_BGMManager;
    double AudioTime;
    float BGMSliderTime;


    void InitObject()
    {
        if(!gamemanager)
        {
            
            var gmo = GameObject.Find("GameManager");
            gamemanager = gmo.GetComponent<TheGameManager>();
            
        }
        if(!gameplus) gameplus = GameObject.Find("GameManagerPlus").GetComponent<GamePlus>();

        if(!NotesDown) NotesDown = GameObject.Find("NotesDown");
        if(!NotesUp) NotesUp = GameObject.Find("NotesUp");

        if(!gmcamera) gmcamera = GameObject.Find("Main Camera");

        
        


    }


    

    void OnGUI()
    {
       //データ保存
       //EditorGUI.BeginChangeCheck();
       
        
           
       if(!emergency)
       {
            var instance = DR3PStaticParameter.instance;
            //処理とか
            if(SongKeywordsEX != null)
            {
                
                instance.FixSongKeyword = (SongKeywordsEX.Length == 0 ? null : SongKeywordsEX[Mathf.Min(SongKeywordIndex,SongKeywordsEX.Length-1)]);
                if(SongKeywordsEX.Length != 0)instance.FixSongHard = (SongHardNum.Length == 0 ? null : SongHardNum[Mathf.Min(SongHardIndex,SongHardNum.Length)]);
            }

            
            QuickSongKeyword();

            
            sclist = new List<SCList>();
            instance.sclist = sclist;


            //譜面調整のオフセット再読み込み(ファイル切り替え後、Gamanagerの更新が済んでから)
                try
                {
                    
                    if(!UpdateCheckOffset && FoldSongList)
                    {
                        //UnityEngine.Debug.Log(SongKeywordsEX[SongKeywordIndex] + "." + SongHardNum[SongHardIndex]);
                        OffsetReader(true);
                    }
                }
                catch(TargetException)
                {
                    //TargetExceptionよくわからん...とりあえずスルーさせる(という名の問題の先送り)
                }
                catch(System.Exception e)
                {
                    UnityEngine.Debug.Log("<b><color=#ff0000ff>Error</color>:</b>ウィンドウ描画で不明なエラーが発生しました。");
                }
        

        
        
        


            //描画関係ここから
            using (var scroll = new GUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scroll.scrollPosition;
            

                

                //畳んでいても裏で動かす処理
                
                if((gamemanager != null))tmp_BGMManager = (AudioSource)f_BGMManager.GetValue(gamemanager);
                if(tmp_BGMManager)AudioTime = tmp_BGMManager.time * 1000.0 - 100.0 + (float)f_NoteOffset.GetValue(gamemanager);
                

                if(tmp_BGMManager && tmp_BGMManager.isPlaying && sclist.Count == 0) SCListInit();



                //var drbfile = (DRBFile)f_drbfile.GetValue(gamemanager);


                debugfold = EditorGUILayout.Foldout( debugfold,"Parameter" );

                if(debugfold)
                {
                    EditorGUI.indentLevel++;
                    //debugquick = EditorGUILayout.Toggle("更新頻度を増やす",debugquick);
                    EditorGUILayout.LabelField("楽曲時間 : " + ((gamemanager != null) ? gamemanager.SHINDO : 0) + "(ms)");

                    if(tmp_BGMManager) bgmtime = tmp_BGMManager.time;
                    EditorGUILayout.LabelField("再生時間 : " + bgmtime + "(s)");

                    EditorGUILayout.LabelField("再生時間(fixed) : " + AudioTime + "(ms)");

                    EditorGUI.indentLevel--;
                }
                

                if (GUILayout.Button("音ズレ修正"))
                    {
                        AudioFix(tmp_BGMManager, gamemanager.SHINDO, AudioTime);
                    }  

                if(debugfold)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("譜面位置 : " + ((gamemanager != null) ? gamemanager.DSHINDO : 0));
                    int NoteCount = ((NotesDown != null && NotesUp != null)?NotesDown.transform.childCount + NotesUp.transform.childCount : 0);
                    EditorGUILayout.LabelField("生成ノーツ数 : " + NoteCount);
                    EditorGUILayout.LabelField("SC : " + ((gamemanager != null) ? (float)f_CurrentSC.GetValue(gamemanager) : 0));
                    AnimationCurve HeightCurve = new AnimationCurve((gamemanager != null) ? gamemanager.HeightCurve.keys : null);
                    EditorGUILayout.CurveField("HeightCurve", HeightCurve);
                    AnimationCurve PositionCurve = new AnimationCurve((gamemanager != null) ? gamemanager.PositionCurve.keys : null);
                    EditorGUILayout.CurveField("PositionCurve", PositionCurve);
                    AnimationCurve SCCurve = new AnimationCurve((gamemanager != null) ? gamemanager.SCCurve.keys : null);
                    EditorGUILayout.CurveField("SCCurve", SCCurve);

                    EditorGUI.indentLevel--;
                }

                
                //bool ChartFixVCheck = EditorGUILayout.Toggle("譜面修正を行う",ChartFix);
                //if(ChartFixVCheck != ChartFix) instance.ChartFix = ChartFixVCheck;
                //ChartFix = ChartFixVCheck;
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    ChartFix = EditorGUILayout.Toggle("譜面修正を行う",ChartFix);
                    isRealStart = EditorGUILayout.Toggle("開始仕様再現",isRealStart);
                    if (check.changed)
                    {
                        instance.isRealStart = isRealStart;
                        instance.ChartFix = ChartFix;
                    }

                }

                //楽曲ファイルクイックセレクト
                FoldSongList = EditorGUILayout.Foldout( FoldSongList,"Song" );
		        if(FoldSongList)
		        {
                    EditorGUI.indentLevel++;
                    using (var check = new EditorGUI.ChangeCheckScope())//これ使えるからそのうち他のとこにも使って処理回数削減する。
                    {
			            SongKeywordIndex = EditorGUILayout.Popup("Song",SongKeywordIndex,SongKeywordsEX);
                        if(SongHardIndex >= SongHardNum.Length) SongHardIndex = 0;
                        SongHardIndex = EditorGUILayout.Popup("Hard",SongHardIndex,SongHardNum);
                        if (check.changed && FoldAdjust)
                        {
                            //UnityEngine.Debug.Log(SongKeywordsEX[SongKeywordIndex] + "." + SongHardNum[SongHardIndex]);
                            UpdateCheckOffset = false;
                        }
                    }
                    //oggファイルだけあって譜面が無い時は警告
                    if(SongHardNum.Length == 0)
                    EditorGUILayout.HelpBox("この楽曲ファイルには譜面ファイルが存在しません。",MessageType.Warning);

                    if(SongKeywordsEX != null && gamemanager != null)f_SongKeyword.SetValue(gamemanager,SongKeywordsEX[SongKeywordIndex]);

                    if(SongHardNum.Length != 0 && gamemanager != null)f_SongHard.SetValue(gamemanager,int.Parse(SongHardNum[SongHardIndex]));
                    EditorGUI.indentLevel--;
		        }

                //譜面簡易調整
                using (var check = new EditorGUI.ChangeCheckScope())
                {
            
                    FoldAdjust = EditorGUILayout.Foldout(FoldAdjust,"Chart Adjust");
                    if (check.changed && FoldAdjust)
                    {
                        UpdateCheckOffset = false;
                    }

                }
                if(FoldAdjust)
                {
                    EditorGUI.indentLevel++;
                    
                    OffsetAdjust = EditorGUILayout.FloatField("譜面側オフセット調整",OffsetAdjust);
                    if(OffsetAdjust != OffsetAdjust_old)
                    {
                        EditorGUILayout.HelpBox("「変更を反映」ボタンを押すまで譜面には反映されません。",MessageType.Warning);
                    }
                    if(GUILayout.Button("オフセットを再読み込み"))
                    {
                        OffsetReader();
                    }
                    if(GUILayout.Button("変更を反映"))
                    {
                        OffsetChanger(OffsetAdjust);
                        AssetDatabase.Refresh();
                    }


                    if(GUILayout.Button("ABNSC相対変換出力"))
                    {
                        if(ABNSCtConv())
                        {
                            AssetDatabase.Refresh();
                        }
                    }
                    EditorGUI.indentLevel--;

                }
            

                //EditorGUILayout.Space();
                GUILayout.Box("", GUILayout.Width(this.position.width -10), GUILayout.Height(1));
                //EditorGUILayout.Space();

                
                float chartstartchange = EditorGUILayout.FloatField("再生開始小節", ChartStartSection);
                if(ChartStartSection != chartstartchange) instance.ChartStartSection = chartstartchange;
                ChartStartSection = chartstartchange;


                if(GUILayout.Button("途中再生位置設定"))
                {
                    setms = tmp_BGMManager.time;
                    //tmp_BGMManager.time = 0f;
                    //  

                }
                if(setms != 0f)
                {
                    EditorGUILayout.LabelField("途中再生位置 : " + setms + "(s)");
                    if(GUILayout.Button("設定時間にジャンプ"))
                    {
                        GoBackChart(tmp_BGMManager);
                    }

                }
                if(tmp_BGMManager) tmp_BGMManager.pitch = EditorGUILayout.Slider( "Pitch",tmp_BGMManager.pitch,0.0f,3.0f);



                
                /*if(tmp_BGMManager)
                {
                    EditorGUILayout.LabelField("楽曲時間シークバー");
            
                    BGMSliderTime = tmp_BGMManager.time;
                    float SliderTimeChach = EditorGUILayout.Slider(BGMSliderTime,0.0f,tmp_BGMManager.clip? tmp_BGMManager.clip.length : 0.0f);

                    if(BGMSliderTime  < (tmp_BGMManager.clip? tmp_BGMManager.clip.length : 0.0f))
                    {
                        if((tmp_BGMManager.clip? (tmp_BGMManager.time != 0f) : false)) 
                        {
                            //if(SliderTimeChach != (float)BGMSliderTime) UnityEngine.Debug.Log("Changed:"+SliderTimeChach +","+(float)BGMSliderTime);
                            if(SliderTimeChach != BGMSliderTime)
                            {
                                MoveChart(tmp_BGMManager,SliderTimeChach);
                                tmp_BGMManager.time = SliderTimeChach;
                            }
                            
                        }
                    }
                    else tmp_BGMManager.time = 0f;
                    
                }*/


                if(GUILayout.Button("SCList"))
                {
                    //var SCWtype = typeof(AdditionalDR3Viewer.SCWindow);
                    SCWindow.SCWindowOpen();
                }

                if (GUILayout.Button("DRMaker起動"))   
                {
                    DRMCall();
                }

                //Utility
                /*FoldUtility = EditorGUILayout.Foldout( FoldUtility,"Utility" );
                if(FoldUtility)
                {
                    EditorGUI.indentLevel++;
                    AspectIndex = EditorGUILayout.Popup("アスペクト比",AspectIndex,Aspectmenu);
                    if(AspectIndex == Aspectmenu.Length-1)
                    CustomAspect = EditorGUILayout.Vector2Field("アスペクト比",CustomAspect);
                    if(CustomAspect.x < 512) CustomAspect.x = 512;
                    if(CustomAspect.y < 288) CustomAspect.y = 288;

                    if(GUILayout.Button("DanceRail3ウィンドウサイズ制御"))
                    {
                        if(AspectIndex == Aspectmenu.Length-1)
                        MiscUtility.ApplicationAspect(Mathf.RoundToInt(Mathf.Max(CustomAspect.x,512)),Mathf.RoundToInt(Mathf.Max(CustomAspect.y,288)));
                        else
                        MiscUtility.ApplicationAspect(Mathf.RoundToInt(Aspectmenu_value[AspectIndex].x),Mathf.RoundToInt(Aspectmenu_value[AspectIndex].y));

                    }
                    EditorGUI.indentLevel--;
                }*/

                //FPSLimit
                FPSLimit_now = EditorGUILayout.Popup("FPS制限",FPSLimit_now,fpsmenu);
                Application.targetFrameRate = fpsmenu_value[FPSLimit_now];


            }
        }

        //データ保存
       /*if (EditorGUI.EndChangeCheck())
       {
           setting.s_debug = debug;
           setting.s_SongKeywordIndex = SongKeywordIndex;
           setting.s_SongHardIndex = SongHardIndex;
           EditorUtility.SetDirty(setting);
       }*/
        
       
    }

    //static Type _this = typeof(AdditionalDR3Viewer);

    /*[InitializeOnLoadMethod]
    static void EditorGenericMethod()
    {
        //Update
        EditorApplication.update += () =>
        {
            //MethodInfo addDR3V_repaint;
            //var _this = typeof(AdditionalDR3Viewer);
            //addDR3V_repaint = _this.GetMethod("Repaint",BindingFlags.NonPublic | BindingFlags.Instance);
        
            //MethodInfo addDR3V_repaint = typeof(AdditionalDR3Viewer).GetMethod("Repaint", BindingFlags.NonPublic | BindingFlags.Instance);
            
            //static var _this = this
            if(debugquick) _this.Repaint();
            //addDR3V_repaint.Invoke(addDR3V_repaint.DeclaringType, null);

            
        };
    }*/

    //////ここからウィンドウ
    //*
    public class SCWindow : EditorWindow
    {
        static public void SCWindowOpen()
        {
            var window = GetWindow<SCWindow>("SCList",typeof(AdditionalDR3Viewer));
            //window.titleContent = new GUIContent("SCList");
        
        }
        private Vector2 _scrollPosition = Vector2.zero;
        void OnGUI()
        {
            //var adr3 = typeof(AdditionalDR3Viewer);
            var instance = DR3PStaticParameter.instance;
            var sclist = instance.sclist;
            
            using (var scroll = new GUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scroll.scrollPosition;
                if(sclist != null)
                {
                    GUIStyle style = new GUIStyle()
                    {
                        stretchWidth = false
                    };
            
                    using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                    {

                        using (new EditorGUILayout.HorizontalScope())
                        {

                            //EditorGUILayout.PrefixLabel ("PrefixLabel1");
                            GUILayout.Label("id",GUILayout.MaxWidth(75f));
                            GUILayout.Label("sc",GUILayout.MaxWidth(75f));
                            GUILayout.Label("sci",GUILayout.MaxWidth(75f));
                            GUILayout.Label("scms",GUILayout.MaxWidth(75f));
                            GUILayout.Label("interval",GUILayout.MaxWidth(75f));
                        }

                        foreach(SCList t in sclist)
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                EditorGUILayout.SelectableLabel(t.id.ToString(),GUILayout.MaxWidth(75f));
                                EditorGUILayout.SelectableLabel(t.sc.ToString(),GUILayout.MaxWidth(75f));
                                EditorGUILayout.SelectableLabel(t.sci.ToString(),GUILayout.MaxWidth(75f));
                                EditorGUILayout.SelectableLabel(t.scms.ToString(),GUILayout.MaxWidth(75f));
                                EditorGUILayout.SelectableLabel(t.interval.ToString(),GUILayout.MaxWidth(75f));
                            }
                        }
                
                    }

                    if(sclist.Count == 0)
                    {
                        GUILayout.Space(20);
                        EditorGUILayout.LabelField("再生中にも関わらずSCListが表示されない場合、");
                        EditorGUILayout.LabelField("DRViewerウィンドウを一度表示させてみてください。");
                    }
                }
            }
        }
        

    }
    //*/

    //////ここから個別メソッド

    void OffsetReader(bool queryEditorWindow = false)
    {
        UpdateCheckOffset = true;
        if(f_SongKeyword == null || f_SongHard == null)
        {
            var gm = typeof(TheGameManager);
            f_SongKeyword = gm.GetField("SongKeyword",BindingFlags.NonPublic | BindingFlags.Instance);
            f_SongHard = gm.GetField("SongHard",BindingFlags.NonPublic | BindingFlags.Instance);
        }
        
        string file;   
        if(queryEditorWindow) file = SongKeywordsEX[SongKeywordIndex] + "." + SongHardNum[SongHardIndex];
        else file = f_SongKeyword.GetValue(gamemanager) + "." + f_SongHard.GetValue(gamemanager);
        string path = Application.dataPath + "/Resources/SONGS/" + file + ".txt";
        //UnityEngine.Debug.Log("called:OffsetReader");

        if(!File.Exists(path))
        {
            UnityEngine.Debug.Log("<b><color=#ff0000ff>Error</color>:</b>該当譜面ファイルが見つかりませんでした。"+"("+ file +")");
            UpdateCheckOffset = true;
            return;
        }
        TextAsset textasset = new TextAsset();
        textasset = Resources.Load("SONGS/" + file, typeof(TextAsset)) as TextAsset;
        string TextLines = textasset.text;
        Regex c_offset = new Regex(@"^#OFFSET=[-0-9\.]*");
        if(!c_offset.IsMatch(TextLines))
        {
            UnityEngine.Debug.Log("<b><color=#ff0000ff>Error</color>:</b>OFFSETが認識できませんでした。");
            return;
        }
        float offset;
        try
        {
            offset = float.Parse(c_offset.Match(TextLines).Value.Replace("#OFFSET=", ""));
        }
        catch(FormatException)
        {
            UnityEngine.Debug.Log("<b><color=#ff0000ff>Error</color>:</b>OFFSETが数値として認識できませんでした。");
            return;
        }

        OffsetAdjust = offset;
        OffsetAdjust_old = offset;
        

        
    }

    void OffsetChanger(float offset_new)
    {
        string file = f_SongKeyword.GetValue(gamemanager) + "." + f_SongHard.GetValue(gamemanager);
        string path = Application.dataPath + "/Resources/SONGS/" + file + ".txt";
        if(!File.Exists(path))
        {
            UnityEngine.Debug.Log("<b><color=#ff0000ff>Error</color>:</b>該当譜面ファイルが見つかりませんでした。");
            return;
        }
        //FileUtil.CopyFileOrDirectory(path, Application.dataPath + "/Resources/BACKUP/" + file + ".txt");
        FileUtil.ReplaceFile(path, Application.dataPath + "/Resources/BACKUP/" + file + ".txt");

        TextAsset textasset = new TextAsset();
        textasset = Resources.Load("SONGS/" + file, typeof(TextAsset)) as TextAsset;
        string TextLines = textasset.text;

        Regex c_offset = new Regex(@"^#OFFSET=[-0-9\.]*");
        if(!c_offset.IsMatch(TextLines))
        {
            UnityEngine.Debug.Log("<b><color=#ff0000ff>Error</color>:</b>OFFSETが認識できませんでした。");
            return;
        }
        string replacetarget = c_offset.Match(TextLines).Value;
        string replacevalue = "#OFFSET=" + offset_new.ToString();
        TextLines = TextLines.Replace(replacetarget, replacevalue);

        OffsetAdjust_old = OffsetAdjust;



        File.WriteAllText(path, TextLines);
    }

    bool ABNSCtConv()
    {
        
        string file = f_SongKeyword.GetValue(gamemanager) + "." + f_SongHard.GetValue(gamemanager);
        string path = Application.dataPath + "/Resources/SONGS/" + file + ".txt";
        if(!File.Exists(path))
        {
            UnityEngine.Debug.Log("<b><color=#ff0000ff>Error</color>:</b>該当譜面ファイルが見つかりませんでした。");
            return false;
        }
        
        

        TextAsset textasset = new TextAsset();
        textasset = Resources.Load("SONGS/" + file, typeof(TextAsset)) as TextAsset;
        string TextLines = textasset.text;
        string[] s = TextLines.Split('\n');
        bool flag = false;


        Regex abnsc_check = new Regex(@"^([^>]*>){7}[^>]*<ABNSC>.*");

        for(int i = 0; i < s.Length; i++)
        {
            Match m = abnsc_check.Match(s[i]);
            if(m.Success)
            {
                flag = true;
                string ss = s[i].Replace("<", "");
                string[] sss = ss.Substring(0, ss.Length - 2).Split('>');
                if(sss[5].Contains(":"))
                {
                    string[] nscs = sss[5].Split(';');
                    for(int ii=0;ii< nscs.Length;ii++)
                    {
                        float ms = float.Parse(sss[2]) - float.Parse(nscs[ii].Split(':')[0]);

                        nscs[ii] = ms + ":" + nscs[ii].Split(':')[1];

                    }
                    string result = string.Join(";", nscs);
                    //UnityEngine.Debug.Log(result);
                    //UnityEngine.Debug.Log(s[i].Replace(sss[5],result));
                    s[i] = s[i].Replace(sss[5],result);
                    s[i] = s[i].Replace("<ABNSC>","");
                }

                
            }
        }
        //UnityEngine.Debug.Log(string.Join("\n", s));
        

    

        if(!flag)
        {
            UnityEngine.Debug.Log("<b><color=#ffff00ff>Warn</color>:</b>変換対象のノーツが見つかりませんでした");
            return false;
        }


        FileUtil.ReplaceFile(path, Application.dataPath + "/Resources/BACKUP/" + file + ".txt");



        File.WriteAllText(path, string.Join("\n", s));
        UnityEngine.Debug.Log("<b>Info:</b>ABNSCノーツを変換しました");
        return true;

    }
   

    void QuickSongKeyword()
    {

        //譜面ファイル切り替え
        //OGG
        string dir =Application.dataPath + "/Resources/SONGS";
        SongKeywords = Directory.GetFiles(dir, "*.ogg");
        SongKeywordsEX = SongKeywords;
        for(int i=0; i < SongKeywords.Length; i++)
        {
            string metapath = SongKeywords[i];
            SongKeywords[i] = Path.GetFileNameWithoutExtension(SongKeywords[i]);
            SongKeywordsEX[i] = Path.GetFileNameWithoutExtension(SongKeywords[i]);

            /*if(Directory.GetFiles(metapath, "*.meta") == null)
            SongKeywordsEX[i] = SongKeywordsEX[i] + "*";*/

        }
        //TXT
        string sfile = SongKeywords[(SongKeywordIndex >= SongKeywords.Length ? 0 : SongKeywordIndex)] + "*.txt";
        SongHardNum = Directory.GetFiles(dir, sfile);
        for(int i=0; i < SongHardNum.Length; i++)
        {
            string metapath = SongHardNum[i];
            SongHardNum[i] = Path.GetExtension(Path.GetFileNameWithoutExtension(SongHardNum[i]));
            SongHardNum[i] = SongHardNum[i].Replace(".", "");
        }

        var instance = DR3PStaticParameter.instance;
        
        int newSongKeywordIndex = Array.IndexOf(SongKeywordsEX, instance.FixSongKeyword);
        int newSongHardIndex = Array.IndexOf(SongHardNum, instance.FixSongHard);

        SongKeywordIndex = (newSongKeywordIndex < 0 ? 0 : newSongKeywordIndex);
        SongHardIndex = (newSongHardIndex < 0 ? 0 : newSongHardIndex);
        if(newSongHardIndex < 0)
        {
            if(SongHardNum.Length != 0 && gamemanager != null)f_SongHard.SetValue(gamemanager,int.Parse(SongHardNum[SongHardIndex]));
        }

        try
        {
            string setkeyword = SongKeywordsEX[Mathf.Clamp(SongKeywordIndex,0,SongKeywordsEX.Length-1)];
            int    sethard    = int.Parse(SongHardNum[Mathf.Clamp(SongHardIndex,0,SongHardNum.Length-1)]);
            if(instance.SongKeyword != setkeyword) instance.SongKeyword = setkeyword;
            if(instance.SongHard != sethard)instance.SongHard = sethard;
        }
        catch(Exception e)
        {
            UnityEngine.Debug.Log("SongKeywordsEX:"+SongKeywordsEX.Length);
            UnityEngine.Debug.Log("SongKeywordIndex:"+SongKeywordIndex);
            UnityEngine.Debug.Log("SongHardNum:"+SongHardNum.Length);
            UnityEngine.Debug.Log("SongHardIndex:"+SongHardIndex);
        }
        
    }

    void UpdateObjectCheck()
    {
        if(!gameplus)
        {
            bool flug1 = (bool)GameObject.Find("GameManagerPlus");
            if(!flug1)
            {
                GameObject gmp = new GameObject("GameManagerPlus");
                gmp.AddComponent<GamePlus>();
            }
        }
        
    }

    void AudioFix(AudioSource target, double now, double fix)
    {
        float fixs =(float)(fix -now)/1000f;

        target.time = target.time - fixs;
    }

    void MoveChart(AudioSource target,float settime)
    {
        target.Pause();
        float NoteOffset = (float)f_NoteOffset.GetValue(gamemanager);
        var drbfile = (TheGameManager.DRBFile)f_drbfile.GetValue(gamemanager);
        AudioSource BGMManager = (AudioSource)f_BGMManager.GetValue(gamemanager);
        List<bool> isCreated = (List<bool>)f_isCreated.GetValue(gamemanager);
        int COMBO = (int)f_COMBO.GetValue(gamemanager);
        int PERFECT_J = (int)f_PERFECT_J.GetValue(gamemanager);
        int makenotestart = (int)f_makenotestart.GetValue(gamemanager);
        int MAXCOMBO = (int)f_MAXCOMBO.GetValue(gamemanager);

        //float Time = BGMManager.time * 1000.0f - 100.0f + NoteOffset;
        double newSHINDO = settime * 1000.0 - 100.0 + NoteOffset;
        bool makestartflag = false;//(float)f_NoteOffset.GetValue(gamemanager)

        Transform[] NotesNotesUp, NotesNotesDown;

        for(int i = 0; i < drbfile.onpu.Count; i++)
        {
            //if(NoteDrawingTimeCheck(drbfile.onpu[i], newSHINDO, NoteOffset, gamemanager.SCCurve.Evaluate((float)newSHINDO), gamemanager.NoteSpeed, gamemanager.SHINDO))
            if((drbfile.onpu[i].ms > newSHINDO) && (drbfile.onpu[i].ms <= gamemanager.SHINDO) && isCreated[i])
            {
                UnityEngine.Debug.Log("replace:"+i);
                PERFECT_J--;
                COMBO--;
                if(!makestartflag)
                {
                    makenotestart = i;
                    makestartflag = true;
                    MAXCOMBO = i;
                }
                isCreated[i] = false;
            }
        }

        //NotesNotesUp = new Transform[NotesUp.transform.childCount];
        //NotesNotesDown = new Transform[NotesDown.transform.childCount];

        for(int i = 0; i < NotesUp.transform.childCount; i++)
        {
            GameObject note = NotesUp.transform.GetChild(i).gameObject;
            note.GetComponent<TheOnpuPlus>().onpuDataSet();
            if(!NoteDrawingTimeCheck(note.GetComponent<TheOnpuPlus>().onpuData, newSHINDO, NoteOffset, gamemanager.SCCurve.Evaluate((float)newSHINDO), gamemanager.NoteSpeed, gamemanager.SHINDO))
            {
                UnityEngine.Debug.Log("destroyed:id"+note.GetComponent<TheOnpuPlus>().onpuData.id);
                note.GetComponent<TheOnpuPlus>().enabled = false;
                Destroy(note);
            }
        }
        
        for(int i = 0; i < NotesDown.transform.childCount; i++)
        {
            GameObject note = NotesDown.transform.GetChild(i).gameObject;
            note.GetComponent<TheOnpuPlus>().onpuDataSet();
            if(!NoteDrawingTimeCheck(note.GetComponent<TheOnpuPlus>().onpuData, newSHINDO, NoteOffset, gamemanager.SCCurve.Evaluate((float)newSHINDO), gamemanager.NoteSpeed, gamemanager.SHINDO))
            {
                UnityEngine.Debug.Log("destroyed:id"+note.GetComponent<TheOnpuPlus>().onpuData.id);
                note.GetComponent<TheOnpuPlus>().enabled = false;
                Destroy(note);
            }
        }

            
            
        f_isCreated.SetValue(gamemanager,(List<bool>)isCreated);
        f_makenotestart.SetValue(gamemanager,makenotestart);
        f_MAXCOMBO.SetValue(gamemanager,MAXCOMBO);
        f_COMBO.SetValue(gamemanager,COMBO);
        f_PERFECT_J.SetValue(gamemanager,PERFECT_J);

        
        gamemanager.SHINDO = newSHINDO;
        target.UnPause();

    }

    bool NoteDrawingTimeCheck(TheOnpu.OnpuData onpu, double SHINDO, float offset, double DSHINDO, float NoteSpeed, double OLDSHINDO)
    {
        if(0.01f * ((isTail(onpu.kind) ? onpu.parent_dms : onpu.dms) - DSHINDO) * onpu.insc * NoteSpeed < 150.0f) return true;
        if(onpu.ms - SHINDO < 1000) return true;
        if(onpu.isnadnsc && onpu.ms - SHINDO < 10000.0f) return true;
        return false;
    }

    bool isTail(int k)
    {
        if (k == 4) return true;
        if (k == 6) return true;
        if (k == 7) return true;
        if (k == 8) return true;
        if (k == 11) return true;
        if (k == 12) return true;
        if (k == 17) return true;
        if (k == 18) return true;
        if (k == 19) return true;
        if (k == 20) return true;
        if (k == 21) return true;
        if (k == 22) return true;
        if (k == 23) return true;
        if (k == 24) return true;

        return false;
    }

    void GoBackChart(AudioSource target)
    {
        float NoteOffset = (float)f_NoteOffset.GetValue(gamemanager);
        var drbfile = (TheGameManager.DRBFile)f_drbfile.GetValue(gamemanager);
        AudioSource BGMManager = (AudioSource)f_BGMManager.GetValue(gamemanager);
        List<bool> isCreated = (List<bool>)f_isCreated.GetValue(gamemanager);
        int COMBO = (int)f_COMBO.GetValue(gamemanager);
        int PERFECT_J = (int)f_PERFECT_J.GetValue(gamemanager);
        int makenotestart = (int)f_makenotestart.GetValue(gamemanager);
        int MAXCOMBO = (int)f_MAXCOMBO.GetValue(gamemanager);

        float Time = BGMManager.time * 1000.0f - 100.0f + NoteOffset;
        bool flag = false;//(float)f_NoteOffset.GetValue(gamemanager);
        for(int i = 0; i < isCreated.Count; i++)
        {
            if((drbfile.onpu[i].ms > (setms * 1000.0f - 100.0f + NoteOffset)) && (drbfile.onpu[i].ms <= Time) && isCreated[i])
            {
                //Debug.Log("replace:"+i);
                PERFECT_J--;
                COMBO--;
                if(!flag)
                {
                makenotestart = i;
                flag = true;
                MAXCOMBO = i;
                }
                isCreated[i] = false;
                //COMBOREFLASH_LOW();
            }
            
            
        }
        f_isCreated.SetValue(gamemanager,(List<bool>)isCreated);
        f_makenotestart.SetValue(gamemanager,makenotestart);
        f_MAXCOMBO.SetValue(gamemanager,MAXCOMBO);
        f_COMBO.SetValue(gamemanager,COMBO);
        f_PERFECT_J.SetValue(gamemanager,PERFECT_J);

        BGMManager.time = setms;
        gamemanager.SHINDO = target.time * 1000.0 - 100.0 + (float)f_NoteOffset.GetValue(gamemanager);
        fm_COMBOREFLASH.Invoke(gamemanager, null);
    }

    void SCListInit()
    {
        var drbfile = (TheGameManager.DRBFile)f_drbfile.GetValue(gamemanager);
        
        
        
        if(drbfile == null) return;

        for(int i=0;i<drbfile.scns.Count;i++)
        {
            var sl = new SCList();
            float v = gamemanager.BPMCurve.Evaluate(drbfile.scns[i].sci);
            float j = v-lastsctime;
            sl.id = i;
            sl.sc = drbfile.scns[i].sc;
            sl.sci = drbfile.scns[i].sci;
            sl.scms = v;
            sl.interval = j;
            
            //Debug.Log("id:"+i+"  sc:"+drbfile.scns[i].sc + "   scms:"+v+"   sci:"+drbfile.scns[i].sci +"   ずれ:"+j);
            lastsctime = v;
            sclist.Add(sl);
        }
        //Debug.Log(sclist.Count);
    }


    void DRMCall()
    {
        string drmpath = Application.dataPath + "/Resources/DRmaker(3.13).exe";
        if(File.Exists(drmpath))
        {
            ProcessStartInfo proc = new ProcessStartInfo();
            proc.FileName = drmpath;
            proc.WorkingDirectory = Application.dataPath + "/Resources";
            Process.Start(proc);
        }
        else
        {
            UnityEngine.Debug.Log("<b><color=#ff0000ff>Error</color>:</b>DRMakerを認識できませんでした");
        }
    }

    


    [InitializeOnLoadMethod]
    static void EditorStartUp()
    {
        UpdateCheckRemoved = false;
        UpdateCheckOffset = false;
        var instance = DR3PStaticParameter.instance;

        EditorApplicationUtility.projectWasLoaded += () =>
        {
            //UnityEngine.Debug.Log( "projectWasLoaded" );
            instance.editorstarttime = DateTime.Now.ToString().Replace("/", "-").Replace(":", "-").Replace(" ", "_");
            //UnityEngine.Debug.Log(instance.editorstarttime);
        };



        
    }


    void WriteLog(string res)
    {
        var instance = DR3PStaticParameter.instance;

        string logpath = Application.dataPath + "/../Logs/DR3ViewerPlus/" + instance.editorstarttime + ".log";
        if(!Directory.Exists(Application.dataPath + "/../Logs/DR3ViewerPlus/")) Directory.CreateDirectory(Application.dataPath + "/../Logs/DR3ViewerPlus/");
        File.AppendAllText(logpath, res);
    }



    void OpenLog()
    {
        var instance = DR3PStaticParameter.instance;

        string logpath = Application.dataPath + "/../Logs/DR3ViewerPlus/" + instance.editorstarttime + ".log";
        if(!Directory.Exists(Application.dataPath + "/../Logs/DR3ViewerPlus/")) Directory.CreateDirectory(Application.dataPath + "/../Logs/DR3ViewerPlus/");
        if(!File.Exists(logpath))
        {
            UnityEngine.Debug.Log("<b>Info:</b>ログファイルが未生成です。");
            return;
        }
        UnityEngine.Application.OpenURL("file://" + logpath);
        
    }
    void DaleteLog()
    {
        if (EditorUtility.DisplayDialog("ログファイル削除の確認", "全てのログファイルを削除します。よろしいですか?", "Yes", "No"))
        {
            string logdirpath = Application.dataPath + "/../Logs/DR3ViewerPlus/";
            Regex filecheck = new Regex(@"[0-9]{4}-[0-9]{2}-[0-9]{2}_[0-9]{1,2}-[0-9]{1,2}-[0-9]{1,2}.log");

            DirectoryInfo dir = new DirectoryInfo(logdirpath);
            FileInfo[] info = dir.GetFiles("*.log");
            foreach(FileInfo f in info)
            {
                if(filecheck.IsMatch(f.Name))
                try
                {
                    File.Delete(logdirpath + f.Name);
                }
                catch(IOException)
                {
                    UnityEngine.Debug.Log("<b><color=#ff0000ff>Error</color>:</b>" + f.Name + "は使用中のため、削除できませんでした。");
                }
            }
            
        }
    }
}

