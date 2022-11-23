#define ENABLE_UNSAFE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.IO;
using System;
using System.Text.RegularExpressions;
using DR3ViewerPlusCustomMethod;
using TMPro;

[DisallowMultipleComponent, DefaultExecutionOrder(5)]
public class GamePlus : MonoBehaviour
{
    [HideInInspector]
    public string editorstarttime = null;

    [HideInInspector]
    public float ChartStartSection = 0f;

    [HideInInspector]
    public bool ChartFix = false;

    [HideInInspector]
    public bool mod_mode = false;

    [HideInInspector]
    public bool isRealStart = false;

    public string SongKeyword;
    public int SongHard;


    Image SpeedUEffect,SpeedDEffect;
    bool isAnimating;
    GamePlus.EffectType CurrentSCType;
    IEnumerator SCEffectCoroutine;




    



    TheGameManager gm;
    public TheGameManager gamemanager;


    FieldInfo f_sprSongImage;
    FieldInfo f_BGMManager;
    FieldInfo f_drbfile;
    AudioSource BGMManager;
    FieldInfo f_NoteOffset;
    FieldInfo f_ReadyTime;
    readonly FieldInfo f_CurrentSC;
    /*
    FieldInfo f_OnpuWeight;
    FieldInfo f_SpriteNotes;
    FieldInfo f_prefabEffect;
    */
    FieldInfo f_prefabOnpu;

    FieldInfo f_COMBO;
    FieldInfo f_PERFECT_J;
    FieldInfo f_isCreated;
    FieldInfo f_makenotestart;
    FieldInfo f_MAXCOMBO;
    MethodInfo fm_COMBOREFLASH;
    /*
    FieldInfo f_AudioMixer;
    AudioMixer audioMixer;
    FieldInfo f_EQCurve;
    AnimationCurve EQCurve;

    FieldInfo f_sourcematerial;

    public List<Material> materialplus;

    List<float> G_EQList = new List<float>();
    List<float> Y_EQList = new List<float>();

    float G_AudioMixerCenter = 1.0f, G_AudioMixerFreq = 1.0f, Y_AudioMixerCenter = 1.0f, Y_AudioMixerFreq = 1.0f;
    */


    GamePlus()
    {
        //TheGameManagerへのアクセス権
        var gm = typeof(TheGameManager);

        f_sprSongImage = gm.GetField("sprSongImage",BindingFlags.NonPublic | BindingFlags.Instance);
        //var drb = typeof(TheGameManager.DRBFile);
        f_BGMManager = gm.GetField("BGMManager",BindingFlags.NonPublic | BindingFlags.Instance);
        f_drbfile = gm.GetField("drbfile",BindingFlags.NonPublic | BindingFlags.Instance);
        f_NoteOffset = gm.GetField("NoteOffset",BindingFlags.NonPublic | BindingFlags.Instance);
        f_CurrentSC = gm.GetField("CurrentSC",BindingFlags.NonPublic | BindingFlags.Instance);
        //f_OnpuWeight = gm.GetField("OnpuWeight",BindingFlags.NonPublic | BindingFlags.Instance);
        //f_SpriteNotes = gm.GetField("SpriteNotes",BindingFlags.NonPublic | BindingFlags.Instance);
        //f_prefabEffect = gm.GetField("prefabEffect",BindingFlags.NonPublic | BindingFlags.Instance);
        f_prefabOnpu = gm.GetField("OnpuPrefab",BindingFlags.NonPublic | BindingFlags.Instance);
        //f_AudioMixer = gm.GetField("audioMixer",BindingFlags.NonPublic | BindingFlags.Instance);
        //f_EQCurve = gm.GetField("EQCurve",BindingFlags.NonPublic | BindingFlags.Instance);


        f_PERFECT_J = gm.GetField("PERFECT_J",BindingFlags.NonPublic | BindingFlags.Instance);
        f_COMBO = gm.GetField("COMBO",BindingFlags.NonPublic | BindingFlags.Instance);
        f_isCreated = gm.GetField("isCreated",BindingFlags.NonPublic | BindingFlags.Instance);
        f_makenotestart = gm.GetField("makenotestart",BindingFlags.NonPublic | BindingFlags.Instance);
        f_MAXCOMBO = gm.GetField("MAXCOMBO",BindingFlags.NonPublic | BindingFlags.Instance);

        fm_COMBOREFLASH = gm.GetMethod("COMBOREFLASH",BindingFlags.NonPublic | BindingFlags.Instance);



        //var conpu = typeof(TheOnpu);
        //f_sourcematerial = conpu.GetField("_material1",BindingFlags.NonPublic | BindingFlags.Instance);
    }

    // Start is called before the first frame update
    void Start()
    {


        var gmo = GameObject.Find("GameManager");
        gamemanager = gmo.GetComponent<TheGameManager>();


        try
        {
            SpeedUEffect = GameObject.Find("BackgroundCanvas/SpeedChangeAnim/ImageSpeedUp").GetComponent<Image>();
            SpeedDEffect = GameObject.Find("BackgroundCanvas/SpeedChangeAnim/ImageSpeedDown").GetComponent<Image>();
        }
        catch{}

        //ジャケット読み込み
        Image sprSongImage = (Image)f_sprSongImage.GetValue(gamemanager);
        if(sprSongImage.sprite == null)
        {
            sprSongImage.sprite = DR3PCTMethod.PathToSpriteRead((string)SongKeyword+".png",Vector4.zero);
        }

        //BPMCurveにキーフレームが存在しないということはBPMCurveの初期化の前でエラーが発生しているということ(大体譜面読み込みの所)
        if(gamemanager.BPMCurve.length == 0)
        {
            //NullReferenceExceptionを防ぐためにGameManagerを切る
            gamemanager.enabled = false;
            //譜面のエラーを特定する
            if(ChartErrorCheck().Count == 0)
            Debug.Log("<b><color=#ff0000ff>Error</color>:</b>構文エラーが特定できませんでした。未定義の構文エラータイプ、もしくは外部要因によるエラーの可能性があります。");
            return;

        }
        if(ChartFix)NotePWFix();

        PositionCurveFix();

        try
        {
            IsnadNSCConvert();
        }
        catch(Exception e)
        {
            //譜面読み込みエラー処理は前にやってるのでここで処理は不要
        }

        BGMManager = (AudioSource)f_BGMManager.GetValue(gamemanager);
        #if UNITY_EDITOR
        if(ChartStartSection > 0 && (BGMManager.clip.length > ((float)(gamemanager.BPMCurve.Evaluate(ChartStartSection) - 2000f)/1000f)))
        {
            var drbfile = (TheGameManager.DRBFile)f_drbfile.GetValue(gamemanager);

            bool[] s_isCreated = new bool[drbfile.onpu.Count];
            int s_makenotestart = 0;

            for (int i = 0; i < drbfile.onpu.Count; i++)
            {
                if(drbfile.onpu[i].ichi < ChartStartSection)
                {
                    s_isCreated[i] = true;

                }
                else
                {
                    s_makenotestart = i;
                    break;
                }
            }
            f_makenotestart.SetValue(gamemanager,s_makenotestart);
            List<bool> send_isCreated = new List<bool>();
            send_isCreated.AddRange(s_isCreated);
            f_isCreated.SetValue(gamemanager,send_isCreated);
            f_COMBO.SetValue(gamemanager,s_makenotestart);
            f_MAXCOMBO.SetValue(gamemanager,s_makenotestart);
            f_PERFECT_J.SetValue(gamemanager,s_makenotestart);

            fm_COMBOREFLASH.Invoke(gamemanager, null);
            BGMManager.time = Mathf.Max((float)(gamemanager.BPMCurve.Evaluate(ChartStartSection) - 2000f)/1000f, 0f);
            gamemanager.SHINDO = BGMManager.time * 1000.0 - 100.0 + (float)f_NoteOffset.GetValue(gamemanager);
        }
        #endif

        if(ChartStartSection == 0.0f && isRealStart)
        {
            var gm = typeof(TheGameManager);
            f_ReadyTime = gm.GetField("ReadyTime",BindingFlags.NonPublic | BindingFlags.Instance);
            
            StartCoroutine(InitPlus((float)f_ReadyTime.GetValue(gamemanager),(float)f_NoteOffset.GetValue(gamemanager)));
        }

        /*int[] tmp_OnpuWeight = new int[24]
        {
            1,3,3,1,1,1,1,1,2,2,3,1,1,2,2,2,2,3,3,1,1,1,1,3
        };
        f_OnpuWeight.SetValue(gamemanager,tmp_OnpuWeight);

        Sprite[] SpriteRes = (Sprite[])f_SpriteNotes.GetValue(gamemanager);
        Sprite[] SpriteAdd = NoteSpriteRead();
        SpriteRes = SpriteRes.Concat(SpriteAdd).ToArray();
        f_SpriteNotes.SetValue(gamemanager,SpriteRes);


        GameObject[] EffectRes = (GameObject[])f_prefabEffect.GetValue(gamemanager);
        GameObject[] EffectAdd = new GameObject[4]
        {
            EffectRes[18],
            EffectRes[18],
            EffectRes[18],
            EffectRes[18]
        };
        EffectRes = EffectRes.Concat(EffectAdd).ToArray();
        f_prefabEffect.SetValue(gamemanager,EffectRes);


        //MeshPlusInit();


        */
        GameObject POnpu = (GameObject)f_prefabOnpu.GetValue(gamemanager);
        
        if(!POnpu.GetComponent<TheOnpuPlus>())POnpu.AddComponent<TheOnpuPlus>();
        /*

        EQCurve = (AnimationCurve)f_EQCurve.GetValue(gamemanager);

        AudioMixerRead();
        audioMixer = (AudioMixer)f_AudioMixer.GetValue(gamemanager);*/


    }

    void PositionCurveFix()
    {
        for(int i= gamemanager.PositionCurve.length -1 ;i >= 0 ;i--)
        {
            gamemanager.PositionCurve.keys[i].time += 100f;
        }
    }

    
    IEnumerator InitPlus(float ReadyTime, float NoteOffset)
    {
        float timer = ReadyTime;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            gamemanager.DSHINDO = -timer * 1000.0 / (BGMManager.pitch > 0 ? BGMManager.pitch : 0.01) - 100.0 + NoteOffset;
            yield return null;
        }
    }

    


    public class ChartErrorInfo
    {
        public bool hidelp;
        public int line;
        public int pos;
        public string info;
        public int subline;
        public int subpos;

        public ChartErrorInfo(int l, int p, string i , int sl=0, int sp=0)
        {
            this.hidelp = false;
            this.line = l;
            this.pos = p;
            this.info = i;
            this.hidelp = false;
            this.subline = sl;
            this.subpos = sp;
        }

        public ChartErrorInfo(bool h, string i, int l=0)
        {
            this.line = l;
            this.pos = 0;
            this.info = i;
            this.hidelp = h;
        }

    }
    public class ParentCheckInfo
    {
        public int line;
        public int pos;
        public int parentid;

        public ParentCheckInfo( int pid, int l, int p)
        {
            this.line = l;
            this.pos = p;
            this.parentid = pid;
        }
    }
    List<ChartErrorInfo> ChartErrorCheck()
    {
        List<ChartErrorInfo> Lineinfo = new List<ChartErrorInfo>();


        TextAsset textasset = new TextAsset();
        textasset = Resources.Load("SONGS/" + (string)SongKeyword + "." + SongHard, typeof(TextAsset)) as TextAsset;
        string TextLines = textasset.text;

        string[] s = TextLines.Split('\n');


        bool inoffset = false;
        bool inbeat = false;
        bool inbpm = false;
        bool insc = false;
        int bpm_count = -1;
        int sc_count = -1;
        int currentbpmn = 0;
        int currentscn = 0;


        //フォーマットエラーは基本的に全部の行で間違ってることが多いから一括検知
        List<int> formaterrorLine = new List<int>();

        //TailMiss検知用インデックス
        List<ParentCheckInfo> parentcheck = new List<ParentCheckInfo>();
        List<int> idindex = new List<int>();

        for (int i = 0; i < s.Length; i++)
        {
            //最後の行かつ何か入っているとバグる
            if(i == s.Length -1 && s[i] != "") Lineinfo.Add(new ChartErrorInfo((i+1), s[i].Length + 1 ,"<b><color=#ff0000ff>Error</color>:</b>ノーツデータの終端には改行が一つ必要です。"));
            //空白行だったら飛ばす
            if(s[i] == "") continue;

            //LFフォーマットチェック
            Regex formatcheck = new Regex(@"\r$");
            if(!formatcheck.IsMatch(s[i])) formaterrorLine.Add(i);

            
            Regex commandcheck = new Regex(@"^#");
            
            if(commandcheck.IsMatch(s[i]))//制御コマンドチェック
            {
                string[] ss = s[i].Split('=');
                if(ss.Length != 2)
                {
                    Lineinfo.Add(new ChartErrorInfo((i+1),1 , "<b><color=#ff0000ff>Error</color>:</b>制御コマンドは　#NAME=VALUE; の形で記述してください"));
                    continue;
                }
                Regex commandendcheck = new Regex(@"[0-9]\r$");
                Regex commandendcheck_s = new Regex(@"[^0-9]\r$");
                if(commandendcheck.IsMatch(s[i])) Lineinfo.Add(new ChartErrorInfo((i+1),commandendcheck.Match(s[i]).Index + 2, "<b><color=#ff0000ff>Error</color>:</b>制御コマンドの終端には;が一つ必要です。"));
                if(commandendcheck_s.IsMatch(s[i])) ss[1] = ss[1].Substring(0 , ss[1].Length - 2 );

                Regex c_offset = new Regex(@"^#\s*OFFSET\s*");
                Regex c_beat = new Regex(@"^#\s*BEAT\s*");
                Regex c_bpm = new Regex(@"^#\s*BPM_NUMBER\s*");
                Regex c_sc = new Regex(@"^#\s*SCN\s*");

                Regex c_bpmv = new Regex(@"^#\s*BPM[ S]\s*");
                Regex c_scv = new Regex(@"^#\s*SC[ I]\s*");

                Regex uintcheck = new Regex(@"[^0-9]");
                Regex floatcheck = new Regex(@"[^0-9\.\-]");
                Regex dfloatcheck = new Regex(@"\.[^<>]*\.");
                Regex dnegativecheck = new Regex(@"\-[^<>]*?\-");

                Regex spacecheck = new Regex(@"[ \t\f]");

                if(c_offset.IsMatch(s[i]) || c_beat.IsMatch(s[i]))
                {
                    MatchCollection spaces = spacecheck.Matches(ss[0]);
                    Match floaterror = floatcheck.Match(ss[1]);
                    Match dnegaerror = dnegativecheck.Match(ss[1]);
                    foreach (Match sss in spaces)Lineinfo.Add(new ChartErrorInfo((i+1),(sss.Index + 1) , "<b><color=#ff0000ff>Error</color>:</b>空白文字は許可されていません。"));
                    if(floaterror.Success) Lineinfo.Add(new ChartErrorInfo((i+1),(ss[0].Length + floaterror.Index + 2) , "<b><color=#ff0000ff>Error</color>:</b>小数のみ入力することができます。"));
                    if(dnegaerror.Success) Lineinfo.Add(new ChartErrorInfo((i+1),(ss[0].Length + dnegaerror.Index + dnegaerror.Length + 1) , "<b><color=#ff0000ff>Error</color>:</b>一つのパラメーターに複数のマイナス符号が存在しています。"));
                
                    //if(spaces.Count == 0 && !floaterror.Success && !dnegaerror.Success && c_offset.IsMatch(s[i])) inoffset = true;
                    if(c_offset.IsMatch(s[i])) inoffset = true;
                    if(c_beat.IsMatch(s[i])) inbeat = true;
                }
                if(c_bpm.IsMatch(s[i]) || c_sc.IsMatch(s[i]))
                {
                    MatchCollection spaces = spacecheck.Matches(ss[0]);
                    Match interror = uintcheck.Match(ss[1]);
                    foreach (Match sss in spaces)Lineinfo.Add(new ChartErrorInfo((i+1),(sss.Index + 1) , "<b><color=#ffff00ff>Warn</color>:</b>空白文字は許可されていません。(読み込みは可能です)"));
                    if(interror.Success) Lineinfo.Add(new ChartErrorInfo((i+1),(ss[0].Length + interror.Index + 2) , "<b><color=#ffff00ff>Warn</color>:</b>正の整数値のみ入力することができます。(読み込みは可能です)"));

                    if(c_bpm.IsMatch(s[i]))
                    {
                        inbpm = true;
                        if(!interror.Success) bpm_count = int.Parse(ss[1]);
                    }
                    if(c_sc.IsMatch(s[i]))
                    {
                        insc = true;
                        if(!interror.Success) sc_count = int.Parse(ss[1]);
                    }

                }
                if(c_bpmv.IsMatch(s[i]) || c_scv.IsMatch(s[i]))
                {
                    MatchCollection spaces = spacecheck.Matches(ss[0]);
                    Regex notcheckspace = new Regex(@"(PM +\[[0-9]*\])|(SC +\[[0-9]*\])");
                    Regex bpmscnbox = new Regex(@"\[.*\]");
                    Match c_notcheckspace = notcheckspace.Match(ss[0]);

                    Regex bpmscnoffset = new Regex(@"^.*?\[");
                    if(bpmscnbox.IsMatch(ss[0]))
                    {
                        foreach (Match sss in spaces)
                        {
                            if(sss.Index != c_notcheckspace.Index + 2) Lineinfo.Add(new ChartErrorInfo((i+1),(sss.Index + 1) , "<b><color=#ff0000ff>Error</color>:</b>空白文字は許可されていません。"));
                        }
                    }
                    else
                    {
                        Regex numboxoffset = new Regex(@"(BPM[ S])|(SC[ I])");
                        Match c_numboxoffset = numboxoffset.Match(ss[0]);
                        Lineinfo.Add(new ChartErrorInfo((i+1),(c_numboxoffset.Index + c_numboxoffset.Length + 1) , "<b><color=#ff0000ff>Error</color>:</b>[]で整数値を囲む必要があります。"));
                    }
                }
                


            }
            else //if(notecheck.IsMatch(s[i]))//ノーツデータチェック
            {
                //<>チェック
                bool check1 = false;
                Regex datasplit_error = new Regex(@"<[^<>]*?<");
                Regex datasplit_warn = new Regex(@"(>[^<>]*?>)|(^[^<]*?>)");
                Regex datasprit_warnb = new Regex(@"<[^>]*?$");
                Regex datasprit_invalid = new Regex(@">[^<>]+?<");
                MatchCollection matches1 = datasplit_error.Matches(s[i]);
                MatchCollection matches2 = datasplit_warn.Matches(s[i]);
                Match matches3 = datasprit_warnb.Match(s[i]);
                MatchCollection matches4 = datasprit_invalid.Matches(s[i]);
                foreach (Match ss in matches1)
                {
                    Lineinfo.Add(new ChartErrorInfo((i+1),(ss.Index + ss.Length) , "<b><color=#ff0000ff>Error</color>:</b>>が抜けています。"));
                    check1 = true;
                }
                foreach (Match ss in matches2)
                {
                    Lineinfo.Add(new ChartErrorInfo((i+1),(ss.Index + 1) , "<b><color=#ffff00ff>Warn</color>:</b><が抜けています。(読み込みは可能です)"));
                }
                
                if(matches3.Success)
                {
                    Lineinfo.Add(new ChartErrorInfo((i+1),(matches3.Index + matches3.Length) , "<b><color=#ffff00ff>Warn</color>:</b>>が抜けています。(読み込みは可能です)"));
                }
                foreach (Match ss in matches4)
                {
                    Lineinfo.Add(new ChartErrorInfo((i+1),(ss.Index + 1) , "<b><color=#ff0000ff>Error</color>:</b>データ外に値が存在します"));
                    check1 = true;
                }
                if(!check1)//データ内容フォーマットチェック
                {
                    string[] ss = s[i].Replace("<", "").Split('>');
                    if(ss.Length < 8) Lineinfo.Add(new ChartErrorInfo((i+1),s[i].Length , "<b><color=#ff0000ff>Error</color>:</b>パラメーターが"+ (8 - ss.Length) + "つ不足しています。"));
                    if(ss.Length > 9 && !mod_mode) Lineinfo.Add(new ChartErrorInfo((i+1),new Regex(RegexposIndex(8)).Match(s[i]).Index + new Regex(RegexposIndex(8)).Match(s[i]).Length, "<b><color=#ffff00ff>Warn</color>:</b>パラメーターが"+ (ss.Length - 9) + "つ余分に存在しています。(読み込みは可能です)"));
                    Regex intcheck = new Regex(@"[^0-9\-]");
                    Regex floatcheck = new Regex(@"[^0-9\.\-]");
                    Regex dfloatcheck = new Regex(@"\.[^<>]*\.");
                    Regex dnegativecheck = new Regex(@"\-[^<>]*?\-");
                    Regex insccheck = new Regex(@"[^0-9\-:;\.]");
                    
                    for(int ii=0;ii < ss.Length -1; ii++)//数値パラメーターフォーマットチェック
                    {
                        bool parenterror = ss.Length < 7;
                        if(ii == 0 || ii == 1 || ii == 6)
                        {
                            if(intcheck.IsMatch(ss[ii]))
                            {
                                Lineinfo.Add(new ChartErrorInfo((i+1),(new Regex(RegexposIndex(ii)).Match(s[i]).Index + new Regex(RegexposIndex(ii)).Match(s[i]).Length + intcheck.Match(ss[ii]).Index)+1 , "<b><color=#ff0000ff>Error</color>:</b>整数値のみ入力することができます。"));
                                if(ii == 1 || ii == 6) parenterror = true;
                            }
                            else 
                            {
                                if(ii == 0) idindex.Add(int.Parse(ss[ii]));
                                if(ii == 1)
                                {
                                    if((int.Parse(ss[ii]) < 0) || ((int.Parse(ss[ii]) == 0 || int.Parse(ss[ii]) > 24) && !mod_mode)) Lineinfo.Add(new ChartErrorInfo((i+1),(new Regex(RegexposIndex(ii)).Match(s[i]).Index + new Regex(RegexposIndex(ii)).Match(s[i]).Length)+1 , "<b><color=#ff0000ff>Error</color>:</b>未定義の種別番号[" + int.Parse(ss[ii]) + "]が指定されています"));
                                }
                            }
                            
                        }
                        if((ii == 2 || ii == 3 || ii == 4))
                        {
                            if(floatcheck.IsMatch(ss[ii])) Lineinfo.Add(new ChartErrorInfo((i+1),(new Regex(RegexposIndex(ii)).Match(s[i]).Index + new Regex(RegexposIndex(ii)).Match(s[i]).Length + floatcheck.Match(ss[ii]).Index)+1 , "<b><color=#ff0000ff>Error</color>:</b>小数のみ入力することができます。"));
                            if(dfloatcheck.IsMatch(ss[ii])) Lineinfo.Add(new ChartErrorInfo((i+1),(new Regex(RegexposIndex(ii)).Match(s[i]).Index + new Regex(RegexposIndex(ii)).Match(s[i]).Length + dfloatcheck.Match(ss[ii]).Index + dfloatcheck.Match(ss[ii]).Length) , "<b><color=#ff0000ff>Error</color>:</b>一つのパラメーターに複数の小数点が存在しています。"));
                            if(dnegativecheck.IsMatch(ss[ii])) Lineinfo.Add(new ChartErrorInfo((i+1),(new Regex(RegexposIndex(ii)).Match(s[i]).Index + new Regex(RegexposIndex(ii)).Match(s[i]).Length + dnegativecheck.Match(ss[ii]).Index + dnegativecheck.Match(ss[ii]).Length) , "<b><color=#ff0000ff>Error</color>:</b>一つのパラメーターに複数のマイナス符号が存在しています。"));
                        }
                        if(ii == 5) 
                        {
                            if(insccheck.IsMatch(ss[ii])) Lineinfo.Add(new ChartErrorInfo((i+1),(new Regex(RegexposIndex(ii)).Match(s[i]).Index + new Regex(RegexposIndex(ii)).Match(s[i]).Length + insccheck.Match(ss[ii]).Index)+1 , "<b><color=#ff0000ff>Error</color>:</b>小数、および[;]、[:]のみ入力することができます。"));
                            else
                            {
                                //構文エラーチェック定義
                                Regex inscsplitcheck_1 = new Regex(@"(;:)|(:;)|(;[^:]*?;)|(:[^;]*?:)|(;[0-9\.\-]*?:*$)");
                                Regex inscsplitcheck_f = new Regex(@"^:*[0-9\.\-]*?;");
                                Regex dfloatcheck_insc = new Regex(@"\.[^<>:;]*\.");
                                Regex dnegativecheck_insc = new Regex(@"\-[^<>:;]*?\-");

                                MatchCollection inscsp1_list = inscsplitcheck_1.Matches(ss[ii]);
                                MatchCollection inscspf_list = inscsplitcheck_f.Matches(ss[ii]);
                                MatchCollection inscdfl_list = dfloatcheck_insc.Matches(ss[ii]);
                                MatchCollection inscdng_list = dnegativecheck_insc.Matches(ss[ii]);
                                foreach (Match sss in inscsp1_list)
                                {
                                    Lineinfo.Add(new ChartErrorInfo((i+1),(new Regex(RegexposIndex(ii)).Match(s[i]).Index + new Regex(RegexposIndex(ii)).Match(s[i]).Length + sss.Index + 2) , "<b><color=#ff0000ff>Error</color>:</b>NSC記述構文が間違っています。"));
                                }
                                foreach (Match sss in inscspf_list)
                                {
                                    Lineinfo.Add(new ChartErrorInfo((i+1),(new Regex(RegexposIndex(ii)).Match(s[i]).Index + new Regex(RegexposIndex(ii)).Match(s[i]).Length + sss.Index + 1) , "<b><color=#ff0000ff>Error</color>:</b>NSC記述構文が間違っています。"));
                                }
                                foreach (Match sss in inscdfl_list)
                                {
                                    Lineinfo.Add(new ChartErrorInfo((i+1),(new Regex(RegexposIndex(ii)).Match(s[i]).Index + new Regex(RegexposIndex(ii)).Match(s[i]).Length + sss.Index + sss.Length) , "<b><color=#ff0000ff>Error</color>:</b>一つのパラメーターに複数の小数点が存在しています。"));
                                }
                                foreach (Match sss in inscdng_list)
                                {
                                    Lineinfo.Add(new ChartErrorInfo((i+1),(new Regex(RegexposIndex(ii)).Match(s[i]).Index + new Regex(RegexposIndex(ii)).Match(s[i]).Length + sss.Index + sss.Length) , "<b><color=#ff0000ff>Error</color>:</b>一つのパラメーターに複数のマイナス符号が存在しています。"));
                                }
                            }

                        }
                        
                        if(ii == 6 && !parenterror && isTail2(int.Parse(ss[1])))//parent検知登録
                        {
                            parentcheck.Add(new ParentCheckInfo(int.Parse(ss[ii]), (i+1), (new Regex(RegexposIndex(ii)).Match(s[i]).Index + new Regex(RegexposIndex(ii)).Match(s[i]).Length + 1)));
                        }
                        

                        
                    }
                    
                    

                    
                }

            }
            /*else//未定義状態(error)
            {

            }*/

            
        }

        //TailMiss検知
        for(int i=0; i < parentcheck.Count; i++)
        {
            int parentres = idindex.IndexOf(parentcheck[i].parentid);
            if(parentres < 0) Lineinfo.Add(new ChartErrorInfo(parentcheck[i].line, parentcheck[i].pos, "<b><color=#ff0000ff>Error</color>:</b>ID:" + parentcheck[i].parentid + "の親ノーツは存在しません。"));
        }

        if(formaterrorLine.Count > 0) Lineinfo.Add(new ChartErrorInfo(true,"<b><color=#ff0000ff>Error</color>:</b>対応していない改行コードが含まれています。LFからCRLFへ変換してください。"));

        for (int i = 0; i < Lineinfo.Count; i++)
        {
            if(!Lineinfo[i].hidelp)
            Debug.Log(Lineinfo[i].info + "("+Lineinfo[i].line + ":" + Lineinfo[i].pos + ")");
            else
            Debug.Log(Lineinfo[i].info);
        }

        return Lineinfo;
    }

    string RegexposIndex(int n)
    {
        string ret = "^";
        for(int i=0; i<n; i++) ret += ".*?>";
        ret += "<*";
        return ret;
    }

    void IsnadNSCConvert()
    {
        var drbfile = (TheGameManager.DRBFile)f_drbfile.GetValue(gamemanager);
        for (int i = 0; i < drbfile.onpu.Count; i++)
        {
            if(drbfile.onpu[i].nsc.Contains(":") && (drbfile.onpu[i].mode == "ABNSC"))
            {

                string[] nscs = drbfile.onpu[i].nsc.Split(';');
                for(int ii=0;ii< nscs.Length;ii++)
                {
                    float ms = drbfile.onpu[i].ichi - float.Parse(nscs[ii].Split(':')[0]);

                    nscs[ii] = ms + ":" + nscs[ii].Split(':')[1];

                }
                drbfile.onpu[i].nsc = string.Join(";", nscs);

            }
        }
    }

    void IsnadNSCConvertExport()
    {

    }



    void NotePWFix()
    {
        bool fix_flug;

        int fixsource_id;
        float fixsource_pos;
        float fixsource_width;


        string sendlog_result = null;


        var drbfile = (TheGameManager.DRBFile)f_drbfile.GetValue(gamemanager);

        for (int i = 0; i < drbfile.onpu.Count; i++)
        {
            fixsource_id = 0;
            fixsource_pos = 0f;
            fixsource_width = 0f;
            fix_flug = false;

            if(drbfile.onpu[i].pos < -16f || drbfile.onpu[i].width > 48f)
            {
                fixsource_id = drbfile.onpu[i].id;
                fixsource_pos = drbfile.onpu[i].pos;
                fixsource_width = drbfile.onpu[i].width;
                fix_flug = true;
            }
            //修正本体
            drbfile.onpu[i].pos = Mathf.Max(-16f, drbfile.onpu[i].pos);
            drbfile.onpu[i].width = Mathf.Min(48f, drbfile.onpu[i].width);
            if(drbfile.onpu[i].pos + drbfile.onpu[i].width > 32f)
            {
                if(!fix_flug)
                {
                    fixsource_id = drbfile.onpu[i].id;
                    fixsource_pos = drbfile.onpu[i].pos;
                    fixsource_width = drbfile.onpu[i].width;
                }


                drbfile.onpu[i].pos = 32f - drbfile.onpu[i].width;
                fix_flug =true;
            }

            if(fix_flug)
            sendlog_result += ("id : " + fixsource_id  + ",   pos : " + fixsource_pos + " -> " + drbfile.onpu[i].pos + ",   width : " + fixsource_width + " -> " + drbfile.onpu[i].width + "\r\n");

        }
        if(sendlog_result != null)
        {

            float[] HgtList = new float[1000];
            for (int i = 0; i < drbfile.onpu.Count; i++)
            {
                //parent位置再計算
                if (isTail2(drbfile.onpu[i].kind))
                {
                    drbfile.onpu[i].parent_pos = drbfile.onpu[drbfile.onpu[i].parent].pos;
                    drbfile.onpu[i].parent_width = drbfile.onpu[drbfile.onpu[i].parent].width;
                }
            }
            //カメラ高さ再計算
            if (gamemanager.PositionCurve.length <= 1)
            {
                gamemanager.HeightCurve = new AnimationCurve();
                
                //カメラ高さ調整
                for (int i = 0; i < drbfile.onpu.Count; i++)
                {
                    if (drbfile.onpu[i].pos < 0)
                    {
                        int s = (int)(drbfile.onpu[i].ms / 1000.0f);
                        if (s + 0 >= 0 && s + 0 < HgtList.Length) HgtList[s + 0] = Mathf.Max(drbfile.onpu[i].pos / (-16.0f), HgtList[s + 0]);
                        if (s + 1 >= 0 && s + 1 < HgtList.Length) HgtList[s + 1] = Mathf.Max(drbfile.onpu[i].pos / (-16.0f), HgtList[s + 1]);

                        if (drbfile.onpu[i].pos < -8)
                        {
                            if (s - 1 >= 0 && s - 1 < HgtList.Length) HgtList[s - 1] = Mathf.Max(drbfile.onpu[i].pos / (-32.0f), HgtList[s - 1]);
                            if (s + 2 >= 0 && s + 2 < HgtList.Length) HgtList[s + 2] = Mathf.Max(drbfile.onpu[i].pos / (-32.0f), HgtList[s + 2]);
                        }
                    }
                    if (drbfile.onpu[i].pos + drbfile.onpu[i].width > 16)
                    {
                        int s = (int)(drbfile.onpu[i].ms / 1000.0f);
                        if (s + 0 >= 0 && s + 0 < HgtList.Length) HgtList[s + 0] = Mathf.Max((drbfile.onpu[i].pos + drbfile.onpu[i].width - 16.0f) / 16.0f, HgtList[s + 0]);
                        if (s + 1 >= 0 && s + 1 < HgtList.Length) HgtList[s + 1] = Mathf.Max((drbfile.onpu[i].pos + drbfile.onpu[i].width - 16.0f) / 16.0f, HgtList[s + 1]);

                        if (drbfile.onpu[i].pos > 24)
                        {
                            if (s - 1 >= 0 && s - 1 < HgtList.Length) HgtList[s - 1] = Mathf.Max((drbfile.onpu[i].pos + drbfile.onpu[i].width - 16.0f) / 32.0f, HgtList[s - 1]);
                            if (s + 2 >= 0 && s + 2 < HgtList.Length) HgtList[s + 2] = Mathf.Max((drbfile.onpu[i].pos + drbfile.onpu[i].width - 16.0f) / 32.0f, HgtList[s + 2]);
                        }
                    }
                }
            }
            //高さカーブ生成
            for (int i = 0; i < 1000; i++)
            {
                gamemanager.HeightCurve.AddKey(i, HgtList[i]);
            }
        }

        if(sendlog_result != null)
        {
            sendlog_result = ("[" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "]" + " Notes Fixed(" + SongKeyword + "." + SongHard + ")\r\n" + sendlog_result +"\r\n\r\n");
            SendLog(sendlog_result);

            string logpath = Application.dataPath + "/../Logs/DR3ViewerPlus/" + editorstarttime + ".log";
            Debug.Log("<b><color=#ffff00ff>Warn</color>:</b>ノーツの位置に修正箇所があります。詳しくは<color=#0000ffff>Logs/DR3ViewerPlus/" + editorstarttime + ".log</color>を参照してください。(DR3Viewerウィンドウのコンテキストメニューからログファイルを開くことができます)");
        }



    }

    bool isTail2(int k)
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

    void SendLog(string res)
    {
        string logpath = Application.dataPath + "/../Logs/DR3ViewerPlus/" + editorstarttime + ".log";
        if(!Directory.Exists(Application.dataPath + "/../Logs/DR3ViewerPlus/")) Directory.CreateDirectory(Application.dataPath + "/../Logs/DR3ViewerPlus/");
        File.AppendAllText(logpath, res);
    }

    // Update is called once per frame
    void Update()
    {

        
        if(SpeedUEffect && SpeedUEffect)
        {
            if((float)f_CurrentSC.GetValue(gamemanager) > 1.05)//UP
            {
                if((CurrentSCType != GamePlus.EffectType.SpeedUp) && !isAnimating)
                {
                    if(CurrentSCType != GamePlus.EffectType.Nomal)
                    {
                        CurrentSCType = GamePlus.EffectType.Nomal;
                        SCEffectCoroutine = SpeedEffectChange(GamePlus.EffectType.Nomal);
                        StartCoroutine(SCEffectCoroutine);
                    }
                    else
                    {
                        //Debug.Log("call_u");
                        CurrentSCType = GamePlus.EffectType.SpeedUp;
                        SCEffectCoroutine = SpeedEffectChange(GamePlus.EffectType.SpeedUp);
                        StartCoroutine(SCEffectCoroutine);
                    }
                }
                
            }
            else if((float)f_CurrentSC.GetValue(gamemanager) < 0.95)//DOWN
            {
                if((CurrentSCType != GamePlus.EffectType.SpeedDown) && !isAnimating)
                {
                    if(CurrentSCType != GamePlus.EffectType.Nomal)
                    {
                        CurrentSCType = GamePlus.EffectType.Nomal;
                        SCEffectCoroutine = SpeedEffectChange(GamePlus.EffectType.Nomal);
                        StartCoroutine(SCEffectCoroutine);
                    }
                    else
                    {
                        //Debug.Log("call_d");
                        CurrentSCType = GamePlus.EffectType.SpeedDown;
                        SCEffectCoroutine = SpeedEffectChange(GamePlus.EffectType.SpeedDown);
                        StartCoroutine(SCEffectCoroutine);
                    }
                }
                
            }
            else//NOMAL
            {
                if((CurrentSCType != GamePlus.EffectType.Nomal) && !isAnimating)
                {
                    //Debug.Log("call_n");
                    CurrentSCType = GamePlus.EffectType.Nomal;
                    SCEffectCoroutine = SpeedEffectChange(GamePlus.EffectType.Nomal);
                    StartCoroutine(SCEffectCoroutine);
                }
                
            }


        }
    
        //BGMManager = (AudioSource)f_BGMManager.GetValue(gamemanager);
        
        /*if (BGMManager.isPlaying)
        {
            if (gamemanager.GameEffectParamEQLevel >= 1)
            {
                //緑ノートエフェクト
                if (G_EQList.Count <= 0)
                {
                    G_AudioMixerFreq -= (G_AudioMixerFreq - 1.0f) * 20 * Time.deltaTime;
                    G_AudioMixerCenter -= (G_AudioMixerCenter + 0.5f) * 20 * Time.deltaTime;
                }
                else
                {
                    float G_adv = G_EQList.Average();

                    G_AudioMixerFreq -= (G_AudioMixerFreq - (1.0f + 0.15f * gamemanager.GameEffectParamEQLevel)) * 20.0f * Time.deltaTime;
                    G_AudioMixerCenter -= (G_AudioMixerCenter - G_adv) * 20.0f * Time.deltaTime;
                }
                audioMixer.SetFloat("G_Center", EQCurve.Evaluate(G_AudioMixerCenter));
                audioMixer.SetFloat("G_Freq", G_AudioMixerFreq);

                G_EQList.Clear();


                //黄色ノートエフェクト
                if (Y_EQList.Count <= 0)
                {
                    Y_AudioMixerFreq -= (Y_AudioMixerFreq - 1.0f) * 20 * Time.deltaTime;
                    Y_AudioMixerCenter -= (Y_AudioMixerCenter - 1.5f) * 20 * Time.deltaTime;
                }
                else
                {
                    float Y_adv = Y_EQList.Average();

                    Y_AudioMixerFreq -= (Y_AudioMixerFreq - (1.0f + 0.15f * gamemanager.GameEffectParamEQLevel)) * 20.0f * Time.deltaTime;
                    Y_AudioMixerCenter -= (Y_AudioMixerCenter - Y_adv) * 20.0f * Time.deltaTime;
                }
                audioMixer.SetFloat("Y_Center", EQCurve.Evaluate(Y_AudioMixerCenter));
                audioMixer.SetFloat("Y_Freq", Y_AudioMixerFreq);

                Y_EQList.Clear();
            }
        }*/
        
        if(Input.GetKeyDown(KeyCode.P))//一時停止、再開
        {
            var tmp_BGMManager = (AudioSource)f_BGMManager.GetValue(gamemanager);

            if(tmp_BGMManager.isPlaying)
            {
                tmp_BGMManager.Pause();
            }
            else
            {
                tmp_BGMManager.UnPause();
            }
                                
            f_BGMManager.SetValue(gamemanager,tmp_BGMManager);

        }

    }


    enum EffectType
    {
        Nomal,
        SpeedDown,
        SpeedUp
        
    }

    IEnumerator SpeedEffectChange(GamePlus.EffectType type)
    {
        isAnimating = true;
        float duration = 0.1f;
        float time = 0.0f;

        float baseU = SpeedUEffect.color.a;
        float baseD = SpeedDEffect.color.a;

        float targetU;
        float targetD;

        switch (type)
        {
            case GamePlus.EffectType.Nomal:
            targetU = 0.0f;
            targetD = 0.0f;
            break;

            case GamePlus.EffectType.SpeedDown:
            targetU = 0.0f;
            targetD = 0.12f;
            break;

            case GamePlus.EffectType.SpeedUp:
            targetU = 0.12f;
            targetD = 0.0f;
            break;

            default:
            yield break;
        }

        while(true)
        {
            time += Time.deltaTime;
            if(time < duration)
            {
                SpeedUEffect.color = new Color(SpeedUEffect.color.r, SpeedUEffect.color.g, SpeedUEffect.color.b, Mathf.Clamp((baseU + ((targetU - baseU)* time / duration)),Mathf.Min(baseU,targetU),Mathf.Max(baseU,targetU)));
                SpeedDEffect.color = new Color(SpeedDEffect.color.r, SpeedDEffect.color.g, SpeedDEffect.color.b, Mathf.Clamp((baseD + ((targetD - baseD)* time / duration)),Mathf.Min(baseD,targetD),Mathf.Max(baseD,targetD)));
            }
            
            if(time >= duration)
            {
                if(SpeedUEffect.color.a != targetU) SpeedUEffect.color = new Color(SpeedUEffect.color.r, SpeedUEffect.color.g, SpeedUEffect.color.b,targetU);
                if(SpeedDEffect.color.a != targetD) SpeedDEffect.color = new Color(SpeedDEffect.color.r, SpeedDEffect.color.g, SpeedDEffect.color.b,targetD);
                //Debug.Log("break");
                isAnimating = false;
                yield break;
            }

            yield return null;
        }
        
    }

    /*Sprite[] NoteSpriteRead()
    {
        string mpath = Application.dataPath + "/DR3ViewerPlus/Sprites";
        string[] filenotes = Directory.GetFiles(mpath, "note*.png");
        Sprite[] result = new Sprite[4];
        for(int i =0; i < filenotes.Length; i++)
        {
            //Debug.Log(filenotes[i]);
            byte[] bytes = File.ReadAllBytes(filenotes[i]);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(bytes);
            Rect rect = new Rect(0f, 0f, 100.0f, 100.0f);
            result[i] = Sprite.Create(texture, rect, new Vector2(0.5f,0.5f),100.0f,0,SpriteMeshType.FullRect, new Vector4(49.0f,0.0f,49.0f,0.0f));
            
        }
        return result;
    }

    void MeshPlusInit()
    {
        
        materialplus = new List<Material>();
        
        var ponpu = (GameObject)f_prefabOnpu.GetValue(gamemanager);
        var theonpu =  ponpu.GetComponent<TheOnpu>();
        

        
        //var ponpu = (GameObject)f_prefabOnpu.GetValue(gamemanager);

        string mpath = Application.dataPath + "/DR3ViewerPlus/Sprites";
        string[] filemats = Directory.GetFiles(mpath, "longnote*.png");
        for(int i =0; i < filemats.Length; i++)
        {
            
            byte[] bytes = File.ReadAllBytes(filemats[i]);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(bytes);

            var source = (Material)f_sourcematerial.GetValue(theonpu);
            Material newmat = new Material(source);
            newmat.SetTexture("_MainTex",texture);
            materialplus.Add(newmat);
            
        }

    }

    void AudioMixerRead()
    {
        BGMManager = (AudioSource)f_BGMManager.GetValue(gamemanager);
        AudioMixer readaudioMixer = Resources.Load("addGY_AudioMixer") as AudioMixer;
        AudioMixerGroup[] readaudioMixerGroup = readaudioMixer.FindMatchingGroups("Master");
        f_AudioMixer.SetValue(gamemanager,readaudioMixer);
        BGMManager.outputAudioMixerGroup = readaudioMixerGroup[0];
    }



    void LateUpdate()
    {

    }
    








    #if ENABLE_UNSAFE
    static void ExchangeFunctionPointer(MethodInfo method0, MethodInfo method1,int mode)
    {// mode 1:swap mode 2:replace
        unsafe
        {
            var functionPointer0 = method0.MethodHandle.Value.ToPointer();
            var functionPointer1 = method1.MethodHandle.Value.ToPointer();
            switch(mode)
            {
                case 1:
                var tmpPointer = *((int*)new IntPtr(((int*)functionPointer0 + 1)).ToPointer());
                *((int*)new IntPtr(((int*)functionPointer0 + 1)).ToPointer()) = *((int*)new IntPtr(((int*)functionPointer1 + 1)).ToPointer());
                *((int*)new IntPtr(((int*)functionPointer1 + 1)).ToPointer()) = tmpPointer;
                break;
                case 2:
                *((int*)new IntPtr(((int*)functionPointer0 + 1)).ToPointer()) = *((int*)new IntPtr(((int*)functionPointer1 + 1)).ToPointer());
                break;
                default:
                break;
            }
        }
    }

    void Awake()
    {
        var gmo = GameObject.Find("GameManager");
        var ponpu = (GameObject)f_prefabOnpu.GetValue(gmo.GetComponent<TheGameManager>());
        MethodInfo onpMethod = ponpu.GetComponent<TheOnpu>().GetType().GetMethod("IsTail", BindingFlags.NonPublic | BindingFlags.Instance);
        MethodInfo masMethod = gmo.GetComponent<TheGameManager>().GetType().GetMethod("isTail", BindingFlags.NonPublic | BindingFlags.Instance);
        MethodInfo resMethod = this.GetType().GetMethod("isTailplus", BindingFlags.NonPublic | BindingFlags.Instance);
        ExchangeFunctionPointer(onpMethod, resMethod,2);
        ExchangeFunctionPointer(masMethod, resMethod,2);
        
    }

    bool isTailplus(int k)
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

        return false;
    }
    #endif

    public void AddGEQ(float f)
    {
        G_EQList.Add(f);
    }

    public void AddYEQ(float f)
    {
        Y_EQList.Add(f);
    }*/
}

