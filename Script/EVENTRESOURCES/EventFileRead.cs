using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using System.Linq;
using System;
using System.IO;
using String;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;


[DisallowMultipleComponent]
public class EventFileRead : MonoBehaviour
{

    FieldInfo f_SongKeyword;
    FieldInfo f_SongHard;
    FieldInfo f_SpriteNotes;
    FieldInfo f_SpriteArror;
    FieldInfo f_SpriteHP;
    FieldInfo f_Camera;
    public string SongKeyword;
    public int SongHard;
    public Sprite[] default_s_note;
    public Sprite[] default_s_arror;
    public Sprite[] default_s_HP;
    public GameObject TheCamera;
    TheGameManager gm;
    public TheGameManager gamemanager;
    public HPManager hpmanager;
    public Transform tr_hpmanager;


    EventFileRead()
    {
        
        //TheGameManagerへのアクセス権
        var gm = typeof(TheGameManager);
        var hpm = typeof(HPManager);
        f_SongKeyword = gm.GetField("SongKeyword",BindingFlags.NonPublic | BindingFlags.Instance);
        f_SongHard = gm.GetField("SongHard",BindingFlags.NonPublic | BindingFlags.Instance);
        f_SpriteNotes = gm.GetField("SpriteNotes",BindingFlags.NonPublic | BindingFlags.Instance);
        f_SpriteArror = gm.GetField("SpriteArror",BindingFlags.NonPublic | BindingFlags.Instance);
        f_SpriteHP = hpm.GetField("spr",BindingFlags.NonPublic | BindingFlags.Instance);
        f_Camera = gm.GetField("TheCamera",BindingFlags.NonPublic | BindingFlags.Instance);
    }

    void InitObject()
    {
        if(!gamemanager)
        {
            
            var gmo = GameObject.Find("GameManager");
            var hpo = GameObject.Find("HPManager");
            tr_hpmanager = hpo.transform;
            gamemanager = gmo.GetComponent<TheGameManager>();
            hpmanager = hpo.GetComponent<HPManager>();
            SongKeyword = (string)f_SongKeyword.GetValue(gamemanager);
            SongHard = (int)f_SongHard.GetValue(gamemanager);
            default_s_note =(Sprite[])f_SpriteNotes.GetValue(gamemanager);
            default_s_arror =(Sprite[])f_SpriteArror.GetValue(gamemanager);
            default_s_HP = (Sprite[])f_SpriteHP.GetValue(hpmanager);
            TheCamera = (GameObject)f_Camera.GetValue(gamemanager);
            
        }

        loadimagespath = new List<string>();
        loadimages = new List<LoadImageIndex>();
    }




    Transform EventScriptCreated;
    Transform EventScriptCreatedUI;
    Transform EventScriptDummies;
    List<LyricsTimestampSort> LyricsSort;
    AnimationCurve LyricsTimestamp;
    Text LyricPath;
    int currentskinid = 0;
    bool Flag;
    public float offset = 0.0f;
    
    [SerializeField] AnimationCurve Check;
    [SerializeField] AnimationCurve Check0;
    [SerializeField] AnimationCurve Check1;
    [SerializeField] GameObject CheckO1;
    [SerializeField] Sprite[] TestSkinList;
    [SerializeField] Sprite[] TestArrorList;
    /*
    [SerializeField] AnimationCurve Checkc;
    [SerializeField] AnimationCurve Check1;
    [SerializeField] AnimationCurve Check2;
    [SerializeField] AnimationCurve Check3;
    [SerializeField] AnimationCurve Check4;*/


    commandflag CommandFlags = new commandflag();

    public class commandflag
    {
        public bool offset;
        public bool disabletapsound;
        public bool introduction;
        public bool endevent;

        public commandflag(bool f = false)
        {
            this.offset = f;
            this.disabletapsound = f;
            this.introduction = f;
            this.endevent = f;
        }
    }

    public class LoadImageIndex
    {
        public Texture2D image;
        public Sprite sprite;
    }

    public class Target
    {
        public string name;
        public GameObject path;
        public string namecode;
        public Animations anim = new Animations();
        public UsedFlag used = new UsedFlag();

        public bool createflag;
        public GameObject imagepath;
        public GameObject textpath;

        public Vector3 initialPos = new Vector3();
        public Vector3 initialRot = new Vector3();

        public bool usedummy;
        public GameObject dummypath;

    }

    public class UsedFlag
    {
        public bool pos;
        public bool rotation;
        public bool scale;
        public bool color;
        public bool text;
        public bool image;
        public bool radialblur;
        public bool hpm;
        public bool hpn;
    }
    
    public class TimeStamp
    {
        public float Time;
        public string Target;
        public string FunctionName;
        public List<string> values;
    }

    public class Animations
    {
        public AnimationTransform pos = new AnimationTransform();
        public AnimationCurve posdisable = new AnimationCurve();
        public AnimationTransform rotation = new AnimationTransform();
        public AnimationCurve rotdisable = new AnimationCurve();
        public AnimationTransform scale = new AnimationTransform();
        public AnimationColor color = new AnimationColor();

        public List<string> textlist = new List<string>();
        public AnimationCurve t_timestamp = new AnimationCurve();

        public AnimationCurve i_timestamp = new AnimationCurve();

        public AnimationCurve radialblur_timestamp = new AnimationCurve();

        public AnimationCurve hpm_timestamp = new AnimationCurve();
        public AnimationCurve hpn_timestamp = new AnimationCurve();

        public string poslastmode;
        public string rotlastmode;
        public string scalelastmode;
        public string colorlastmode;
        public string radialblurlastmode;
        public string hpmlastmode;
        public string hpnlastmode;

        public Animations()
        {
            this.posdisable.preWrapMode = WrapMode.Clamp;
            this.rotdisable.preWrapMode = WrapMode.Clamp;
        }
        
    }

    public class AnimationTransform
    {
        public AnimationCurve X = new AnimationCurve();
        public AnimationCurve Y = new AnimationCurve();
        public AnimationCurve Z = new AnimationCurve();

        public AnimationTransform()
        {
            this.X.preWrapMode = WrapMode.Clamp;
            this.Y.preWrapMode = WrapMode.Clamp;
            this.Z.preWrapMode = WrapMode.Clamp;
        }
    }

    public class AnimationColor
    {
        public AnimationCurve R = new AnimationCurve();
        public AnimationCurve G = new AnimationCurve();
        public AnimationCurve B = new AnimationCurve();
        public AnimationCurve A = new AnimationCurve();
    }

    public class LyricsTimestampSort
    {
        public float Time;
        public float EndTime;
        public string lyric;
        public LyricsTimestampSort(float t, float e, string l)
        {
            this.Time = t;
            this.EndTime = e;
            this.lyric = l;
        }
        
    }

    [System.Serializable]
    public class SkinFormat
    {
        public string DefaultPath;
        public List<NoteSkin> Note;
        public List<string> Hp;
        public bool HpGlow;
    }

    [System.Serializable]
    public class NoteSkin
    {
        public int Id;
        public string Texture = "*default";
        public string Arror = "*null";
        public float BorderT = 0f;
        public float BorderB = 0f;
        public float BorderL = 0f;
        public float BorderR = 0f;
    }


    public class SkinList
    {
        public Sprite[] notes;
        public Sprite[] arrors;
        public Sprite[] HP;

        public bool set_notes = false;
        public bool set_hp = false;
        public bool hpglow = false;
        public Material glowmat;
        
        public SkinList(Sprite[] n = null, Sprite[] a = null, Sprite[] h = null)
        {
            if(n != null)
            {
                this.notes = n;
                this.arrors = a;
                this.set_notes = true;
            }

            if(h != null)
            {
                this.HP = h;
                this.set_hp = true;

            }
            
        }

    }

    public List<string> skinfilepath = new List<string>();
    List<SkinList> skins = new List<SkinList>();
    bool skinused = false;
    AnimationCurve skinindex;



    [SerializeField] TheGameManager MasterScript;
    List<string> loadimagespath;
    List<LoadImageIndex> loadimages;

 

    List<TimeStamp> CommandList = new List<TimeStamp>();
    List<Target> TargetList = new List<Target>();

    // Start is called before the first frame update
    public void Start()
    {
        if(!MasterScript)MasterScript = GameObject.Find("GameManager").GetComponent<TheGameManager>();
        InitObject();

        GameObject createEVSC = new GameObject("EventScriptCreated");
        createEVSC.AddComponent<Canvas>();
        EventScriptCreated = createEVSC.transform;

        GameObject createEVSCUI = new GameObject("EventScriptCreatedUI");
        createEVSCUI.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        EventScriptCreatedUI = createEVSCUI.transform;


        GameObject createEVSD = new GameObject("EventScriptDummies");
        EventScriptDummies = createEVSD.transform;

        LyricsTimestamp = new AnimationCurve();
        EventReadIn();

        if(Flag)
        {
            //drbfile.bpms.Sort((a, b) => Mathf.RoundToInt(a.bpms * 1000.0f - b.bpms * 1000.0f));
            CommandList.Sort((a, b) => Mathf.RoundToInt(b.Time * 1000.0f - a.Time * 1000.0f));


            KeyframeSet();
        }



        if(Flag)
        {
            Check = new AnimationCurve();
            Check0 = new AnimationCurve();
            Check1 = new AnimationCurve();
        
            /*
            Checkc = new AnimationCurve();
            Check1 = new AnimationCurve();
            Check2 = new AnimationCurve();
            Check3 = new AnimationCurve();
            Check4 = new AnimationCurve();
            Check = skinindex;
            TestSkinList = skins[1].notes;
            TestArrorList = skins[1].arrors;/*
            */Check0 = TargetList[0].anim.hpm_timestamp;/*
            CheckO1 = TargetList[0].path;/*
            Checkc = TargetList[0].anim.posdisable;
            Check1 = TargetList[2].anim.color.R;
            Check2 = TargetList[2].anim.color.G;
            Check3 = TargetList[2].anim.color.B;
            Check4 = TargetList[2].anim.color.A;
            */
            //Check = TargetList[0].anim.pos.Y;
            //Check = LyricsTimestamp;
            //Check0 = TargetList[0].anim.posdisable;
            //Check1 = TargetList[0].anim.color.A;
            foreach(Target t in TargetList)
            {
                Debug.Log( "loadedtarget:" + t.name);
            }
        }



    }


    void EventReadIn()
    {   
        //ファイル読み込み
        string FilePath;
        string TextLines;

        //FilePath = Application.dataPath + "/../EVENTS/" + Menu.master_SongKeyword + "." + Menu.master_SongHard + ".dr3evt";
        FilePath = Application.dataPath + "/../EVENTS/" + SongKeyword + "." + SongHard + ".dr3evt";
        if (!File.Exists(FilePath))
        FilePath = Application.dataPath + "/../EVENTS/" + SongKeyword + ".dr3evt";
        if(!File.Exists(FilePath))
        {
            Debug.Log("event file was not found");
            return;
        }

        using (var fs = new StreamReader(FilePath, System.Text.Encoding.GetEncoding("UTF-8")))
        {
            TextLines = fs.ReadToEnd();
        }

        try
        {
            TextLines = RemoveSlash(TextLines);    //コメントの場所を取り除く
            //TextLines = ScriptString.RemoveSpace(TextLines);    //スペースがある場所を取り除く
            TextLines = ScriptString.RemoveTab(TextLines);      //タッブがある場所を取り除く
            //TextLines = ScriptString.RemoveEnter(TextLines);    //複数のエンターの場所を取り除く
            TextLines = TextLines.Replace("\n", "");
            string[] s = TextLines.Split(';');
    
    
            for (int i = 0; i < s.Length; i++)
            {
                //空き行を無視
                if (s[i] == "") continue;
    
                //余分なnull文字とかを削除
                s[i] = s[i].TrimStart();
    
                //TimeStamp
                if (s[i].IndexOf("[") >= 0)
                {
                    //Debug.Log("code(timestamp):"+s[i]);
                    s[i] = s[i].Replace("[","");//余分な文字を削除
                    s[i] = s[i].Replace("(","");
                    s[i] = s[i].Replace(")","");
                    string[] ss = s[i].Split(']');
                    
                    TimeStamp t = new TimeStamp();
                    t.values = new List<string>();
                    t.Time = float.Parse(ss[0]) + offset;
                    
                    string[] sss = ss[1].Split(',');
    
                    //特殊コマンド条件分岐 分割一つ目で@が先頭についていたらコマンドリストに追加しない
                    Regex regex_specialcommand = new Regex(@"^@");
                    if(regex_specialcommand.IsMatch(sss[0]))
                    {
                        //スキンチェンジ
                        if(sss[0] == "@skinchange") SkinReadIn(t.Time,sss[1]);
                        
                    }
                    else
                    {
                        t.Target = sss[0];
                        t.FunctionName = sss[1];
                        for(int j = 2; j < sss.Length; j++)
                        {
                            t.values.Add(sss[j]);
                        }
    
                        CommandList.Add(t);
                    }
    
                }
                //定義コマンド
                else
                {
                    //Debug.Log(s[i]);   
                    string[] ss = s[i].Split(' ');
    
                    //offset
                    if(ss[0] == "offset" && !CommandFlags.offset)
                    {
                        offset = float.Parse(ss[1]);
                        CommandFlags.offset = true;
                    }
                  
    
                    //disabletapsound
                    if(ss[0] == "disabletapsound" && !CommandFlags.disabletapsound)
                    {
                        CommandFlags.disabletapsound = true;
                    }
    
                    //introduction
                    if(ss[0] == "introduction" && !CommandFlags.introduction)
                    {
                        GameObject PrefabPath = (GameObject)Resources.Load ("EVENTRESOURCES/Prefabs/IntroductionChange");
                        Instantiate(PrefabPath,transform.position,Quaternion.identity,EventScriptCreated);
                        CommandFlags.introduction = true;
                    }
    
                    //endevent
                    if(ss[0] == "endevent")
                    {
                    
                    }
                    
    
                    //target
                    if(ss[0] == "target")
                    {
                        Target t = new Target();
                        int j = 0;
                        StringBuilder result = new StringBuilder();
                        result.Append(ss[1]);
                        for(j = 0; result.ToString().Substring(result.ToString().Length-1, 1) != "\"" ; j++)
                        {
                            result.Append(" ");
                            result.Append(ss[2+j]);
                            ss[1] =result.ToString();
                            
                        }
                        ss[1] = ss[1].Trim('\"');
                        t.name = ss[1];
                        t.namecode = ss[2+j];
                        if(UseDummyCheck(t.name)) t.usedummy = true;
    
                        t.path = GameObject.Find(t.name);
                        if(t.path == null) Debug.Log("[" +t.name + "]is not found");
                        else
                        {
                            t.initialPos = t.path.transform.position;
                            t.initialRot = t.path.transform.eulerAngles;
                        }
    
                        TargetList.Add(t);
    
                        //カメラの時は自動でダミーを生成
                        if(t.name == "Main Camera")
                        {
                            GameObject dcam = new GameObject("CameraDummy");
                            dcam.transform.SetParent(EventScriptDummies);
                            t.usedummy = true;
                            t.dummypath = dcam;
                        }
                    }
    
    
                    //parent : 一つ目に指定した名前のオブジェクトの親を二つ目のターゲットに設定する。制御対象にはしない。
                    if(ss[0] == "parent")
                    {
                        int j = 0;
                        StringBuilder result = new StringBuilder();
                        result.Append(ss[1]);
                        for(j = 0; result.ToString().Substring(result.ToString().Length-1, 1) != "\"" ; j++)
                        {
                            result.Append(" ");
                            result.Append(ss[2+j]);
                            ss[1] =result.ToString();
                            
                        }
                        ss[1] = ss[1].Trim('\"');
                        GameObject child = GameObject.Find(ss[1]);
                        //t.name = ss[1];
                        foreach(Target t in TargetList)
                        {
                            if(t.namecode == ss[2])
                            {
                                child.transform.parent = t.path.transform;
                            }
                        }
                        
                    }
    
                    //create~~~
                    /*
                    create：通常作成、スプライト、テキストが付属したオブジェクトを作成し、制御対象にする。
                    createonly：作成後、制御対象にはしない。
                    createnull：空のオブジェクトを作成、制御対象にする。(主にオブジェクトのグループ化に使用する)
                    createcamera：サブカメラの作成。制御対象にする。(未実装)
                    */
    
                    Regex regex_createkind = new Regex(@"^create.*");
                    //if(ss[0] == "create")
                    if(regex_createkind.IsMatch(ss[0]))
                    {
                        Target t = new Target();
                        
                        //オブジェクト名空白処理
                        StringBuilder result = new StringBuilder();
                        result.Append(ss[2]);
                        int j = 0;
                        for(j = 0; result.ToString().Substring(result.ToString().Length-1, 1) != "\"" ; j++)
                        {
                            result.Append(" ");
                            result.Append(ss[3+j]);
                            ss[2] =result.ToString();
                            
                        }
                        ss[1] = ss[1].Trim('\"');
                        ss[2] = ss[2].Trim('\"');
                        t.name = ss[2];
                        if(ss[0] != "createonly") t.namecode = ss[3+j];
    
                        //デフォルト位置、回転、サイズ設定
                        Vector3 defaultpos = new Vector3();
                        Vector3 defaultrot = new Vector3();
                        Vector3 defaultscale = new Vector3();
                        if(ss[0] == "createonly") defaultscale = Vector3.one;
    
                        if(ss.Length >= (7+j))defaultpos = new Vector3(float.Parse(ss[4+j]),float.Parse(ss[5+j]),float.Parse(ss[6+j]));
                        if(ss.Length >= (10+j))defaultrot = new Vector3(float.Parse(ss[7+j]),float.Parse(ss[8+j]),float.Parse(ss[9+j]));
                        if(ss.Length >= (13+j))defaultscale = new Vector3(float.Parse(ss[10+j]),float.Parse(ss[11+j]),float.Parse(ss[12+j]));
    
    
    
                        //画像ファイル読み込み
                        if(ss[0] != "createnull")
                        {
                            GameObject PrefabPath = (GameObject)Resources.Load ("EVENTRESOURCES/Prefabs/DummySplite");
                            GameObject instance =(GameObject)Instantiate(PrefabPath,transform.position,Quaternion.identity,EventScriptCreated);
                            instance.name = ss[2];
                            t.path = instance;
                            t.createflag = true;
                            t.imagepath = instance.transform.Find("Image").gameObject;
                            t.textpath = instance.transform.Find("Text").gameObject;
                            instance.transform.Find("Image").GetComponent<Image>().sprite = PathToSpriteRead(ss[1], Vector4.zero);
                            instance.transform.localPosition = defaultpos;
                            instance.transform.localEulerAngles = defaultrot;
                            instance.transform.localScale = defaultscale;
                        }
                        else//空のオブジェクトの作成(createnull)
                        {
                            GameObject instance2 = new GameObject(ss[2]);
                            t.path = instance2;
                            t.createflag = true;
                            instance2.transform.localPosition = defaultpos;
                            instance2.transform.localEulerAngles = defaultrot;
                            instance2.transform.localScale = defaultscale;
                        }
    
                        
                        
                        t.initialPos = t.path.transform.position;
                        t.initialRot = t.path.transform.eulerAngles;
                        
    
                        if(ss[0] != "createonly") TargetList.Add(t);
                    }
    
                    if(ss[0] == "lyric")
                    {
                        GameObject PrefabPath = new GameObject("TextLyric");
                        Transform LyricParent = GameObject.Find("CanvasNormal").transform;
                        GameObject Lyric = (GameObject)Instantiate(PrefabPath,new Vector3(0,250,0),Quaternion.identity,LyricParent);
    
                        var Rect = Lyric.AddComponent<RectTransform>();
                        Rect.anchoredPosition = new Vector2(0f,250f);
                        Rect.anchorMax = new Vector2(0.5f,0f);
                        Rect.anchorMin = new Vector2(0.5f,0f);
                        Rect.pivot = new Vector2(0.5f,0.5f);
                        Rect.sizeDelta = new Vector2(0f,0f);
                        Rect.localScale = new Vector2(0.6f,0.6f);
    
                        var text = Lyric.AddComponent<Text>();
                        text.horizontalOverflow = HorizontalWrapMode.Overflow;
                        text.verticalOverflow = VerticalWrapMode.Overflow;
                        text.alignment = TextAnchor.LowerCenter;
                        //text.Color = new Color();
                        text.fontSize = 100;
                        text.font = Resources.GetBuiltinResource (typeof(Font), "Arial.ttf") as Font;
    
                        var Shadow = Lyric.AddComponent<Shadow>();
    
                        Shadow.effectColor = new Color(0f,0f,0f,1f);
                        Shadow.effectDistance = new Vector2(8f,-8f);
    
    
                        LyricPath = Lyric.GetComponent<Text>();
    
                        int j = 0;
                        StringBuilder result = new StringBuilder();
                        result.Append(ss[1]);
                        for(j = 0; result.ToString().Substring(result.ToString().Length-1, 1) != "\"" ; j++)
                        {
                            result.Append(" ");
                            result.Append(ss[2+j]);
                            ss[1] =result.ToString();
                            
                        }
                        ss[1] = ss[1].Trim('\"');
                        LyricsReadIn(ss[1]);
                        
                        
                    }
                }
            }
    
            /*foreach(Target t in TargetList)
            {
                //Debug.Log("name:" + t.name + "  code:" + t.namecode );
                ListReset(t);
                
            }
            foreach(TimeStamp t in CommandList)
            {
                //Debug.Log("[" + t.Time + "]" + "  target:" + t.Target + "  function:" + t.FunctionName + "   Length:" + t.values.Count);
            }*/
    
            //return true;
            Flag = true;
        }
        catch(Exception e)
        {
            throw e;
            
        }
        
    }
    
    
    Sprite PathToSpriteRead(string path, Vector4 border, bool full = false)
    {
        string fullpath;
        if(full) fullpath = path;
        else fullpath = Application.dataPath + "/../EVENTS/" + path;

        //Debug.Log(fullpath);//画像パス出力
        
        Sprite sprite;
        if(System.IO.File.Exists(fullpath))
        {
            byte[] bytes = File.ReadAllBytes(fullpath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(bytes);
            Rect rect = new Rect(0f, 0f, texture.width, texture.height);
            //Debug.Log("READ L:"+border.x+" B:"+border.y+" R:"+border.z+" T:"+border.w);
            sprite = Sprite.Create(texture, rect, new Vector2(0.5f,0.5f),100.0f,0,SpriteMeshType.FullRect,border);
        }
        else
        {
            Debug.Log("image was not found : " + path);
            return null;
        }
        return sprite;
    }


    void SkinReadIn(float time, string path)
    {
        string skinjson;
        string f_path = Application.dataPath + "/../EVENTS/" + path + ".json";
        string filepathoffset;
        SkinFormat skin = new SkinFormat();

        if(!skinused)
        {
            skinindex = new AnimationCurve();
            Keyframe SkinIndexInit = new Keyframe(-10000f, 0.0f, float.PositiveInfinity,0f,0f,0f);
            skinindex.AddKey(SkinIndexInit);
            //初期値を先頭に
            skinfilepath.Add("*default");
            skins.Add(new SkinList(default_s_note,default_s_arror,default_s_HP));

        }
        int overlap = skinfilepath.IndexOf(path);
        if(overlap == -1)
        {
            if(!File.Exists(f_path))
            {
                Debug.Log("skin file was not found :" + path);
                return;
            }
            using (var fs = new StreamReader(f_path, System.Text.Encoding.GetEncoding("UTF-8")))
            {
                skinjson = fs.ReadToEnd();
            }
            skin.Note = new List<NoteSkin>();
            JsonUtility.FromJsonOverwrite(skinjson, skin);
            
            //ファイルパスオフセット
            if(!(skin.DefaultPath == ""))
            filepathoffset = Application.dataPath + "/../EVENTS/" + skin.DefaultPath;
            else
            filepathoffset = Application.dataPath + "/../EVENTS/";

            //スキン読み込み
            Sprite[] readinnotes = new Sprite[25];
            Sprite[] readinarror = new Sprite[7];
            Sprite[] readinhp = new Sprite[12];
            if(skin.Note.Count > 0)
            {
                skin.Note.Sort((a, b) => (a.Id - b.Id));
                int maxid = skin.Note[skin.Note.Count -1].Id;
                readinnotes = new Sprite[Mathf.Max(default_s_note.Length,maxid+1)];
                Array.Copy(default_s_note,0,readinnotes,0,default_s_note.Length);
                Array.Copy(default_s_arror,0,readinarror,0,default_s_arror.Length);
            }
            else
            {
                readinnotes = null;
                readinarror = null;
            }
            if(skin.Hp.Count > 0)
            {
                Array.Copy(default_s_HP,0,readinhp,0,default_s_HP.Length);
            }
            else
            {
                readinhp = null;
            }
            //Debug.Log("array0:" + readinarror[0].texture.name);
            
            if(skin.Note.Count > 0)
            {
                foreach(NoteSkin n in skin.Note)
                {
                    string noteimagepath;
                    string arrorimagepath = "";
                

                    Regex regex_prefix_default = new Regex(@"^\*");

                    //ノート本体
                    if(regex_prefix_default.IsMatch(n.Texture))
                    {
                        if(n.Texture == "*default")
                        {
                            noteimagepath = Application.dataPath + "/Sprites/Game/note" + n.Id.ToString() + ".png";
                            //Debug.Log("L:"+n.BorderL+" B:"+n.BorderB+" R:"+n.BorderR+" T:"+n.BorderT);
                            readinnotes[n.Id] = PathToSpriteRead(noteimagepath, new Vector4(n.BorderL,n.BorderB,n.BorderR,n.BorderT), true);
                        }
                        if(n.Texture == "*null")
                        {
                            readinnotes[n.Id] = Resources.Load<Sprite>("EVENTRESOURCES/Sprites/null");
                        }
                    }
                    else
                    {
                        noteimagepath = filepathoffset + n.Texture + ".png";
                        //Debug.Log("L:"+n.BorderL+" B:"+n.BorderB+" R:"+n.BorderR+" T:"+n.BorderT);
                        readinnotes[n.Id] = PathToSpriteRead(noteimagepath, new Vector4(n.BorderL,n.BorderB,n.BorderR,n.BorderT), true);
                    }



                    //アロー
                    if(IdToArror(n.Id) >= 0)
                    {
                        if(regex_prefix_default.IsMatch(n.Arror))
                        {
                            if(n.Arror == "*default")
                            {
                                arrorimagepath = Application.dataPath + "/Sprites/Game/arror" + n.Id.ToString("D2") + ".png";
                            }
                        }
                        else
                        arrorimagepath = filepathoffset + n.Arror + ".png";


                        if(n.Arror == "*null")
                        {   
                            readinarror[IdToArror(n.Id)] = Resources.Load<Sprite>("EVENTRESOURCES/Sprites/null");
                        }
                        else
                        readinarror[IdToArror(n.Id)] = PathToSpriteRead(arrorimagepath, Vector4.zero, true);
                    }
                }

            }
            //HP
            for(int i = 0;i<skin.Hp.Count;i++)
            {
                string hpimagepath;
                Regex regex_prefix_default = new Regex(@"^\*");

                if(regex_prefix_default.IsMatch(skin.Hp[i]))
                {
                    if(skin.Hp[i] == "*default")
                    {
                        hpimagepath = Application.dataPath + "/Sprites/Game/hpbar_" + IndextoID(i) + ".png";
                        //Debug.Log("L:"+n.BorderL+" B:"+n.BorderB+" R:"+n.BorderR+" T:"+n.BorderT);
                        readinhp[i] = PathToSpriteRead(hpimagepath, Vector4.zero,true);
                    }
                    if(skin.Hp[i] == "*null")
                    {
                        readinhp[i] = Resources.Load<Sprite>("EVENTRESOURCES/Sprites/null");
                    }
                }
                else
                {
                    hpimagepath = filepathoffset + skin.Hp[i] + ".png";
                    //Debug.Log("L:"+n.BorderL+" B:"+n.BorderB+" R:"+n.BorderR+" T:"+n.BorderT);
                    readinhp[i] = PathToSpriteRead(hpimagepath, Vector4.zero, true);
                }

            }
            skinfilepath.Add(path);
            skins.Add(new SkinList(readinnotes, readinarror,readinhp));
            overlap = skins.Count -1;

            skins[overlap].hpglow = skin.HpGlow;
            if(skin.HpGlow) skins[overlap].glowmat = Resources.Load<Material>("EVENTRESOURCES/Materials/LightSprite");
            

        }
        Keyframe c_SkinIndex = new Keyframe(KeyFrameRealTime(time), (float)overlap, float.PositiveInfinity,0f,0f,0f);
        skinindex.AddKey(c_SkinIndex);
        if(!skinused) skinused = true;
    
    

        
        
        

    }

    string IndextoID(int i)
    {
        if(i >= 0 && i < 10) return i.ToString();
        if(i == 10) return "%";
        if(i == 11) return "b";
        return "b";
    }

    int IdToArror(int k)
    {
        if(k==1) return 0;
        if(k==2) return 1;
        if(k==13) return 2;
        if(k==14) return 3;
        if(k==15) return 4;
        if(k==16) return 5;
        if(k==9) return 6;
        return -1;
    }
    

    void LyricsReadIn(string path)
    {
        //歌詞ファイル読み込み
        string FilePath;
        string LyricLines;
        LyricsTimestamp.preWrapMode = WrapMode.Clamp;
        LyricsSort = new List<LyricsTimestampSort>();
        FilePath = Application.dataPath + "/../EVENTS/" + path;
        using (var fs = new StreamReader(FilePath, System.Text.Encoding.GetEncoding("UTF-8")))
        {
            LyricLines = fs.ReadToEnd();
        }

        LyricLines = RemoveSlash(LyricLines);    //コメントの場所を取り除く
        //TextLines = ScriptString.RemoveSpace(LyricLines);    //スペースがある場所を取り除く
        LyricLines = ScriptString.RemoveTab(LyricLines);      //タッブがある場所を取り除く
        LyricLines = ScriptString.RemoveEnter(LyricLines);    //複数のエンターの場所を取り除く
        string[] s = LyricLines.Split('\n');
        for(int i = 0; i < s.Length; i++)
        {
            string[] ss = s[i].Split(']');
            if(ss.Length < 3) continue;
            ss[0] = ss[0].Trim('[');
            ss[1] = ss[1].Trim('[');
            //ss[0].Replace("[","");
            //ss[1].Replace("[","");
            ss[2] = ss[2].Replace("#","\n");
            LyricsSort.Add(new LyricsTimestampSort(float.Parse(ss[0]), float.Parse(ss[1]), ss[2]));

            //KeyFrameRealTime(float.Parse(ss[0]))
        }

        LyricsSort.Sort((a, b) => Mathf.RoundToInt(b.Time * 1000.0f - a.Time * 1000.0f));
        LyricsSort.Add(new LyricsTimestampSort(-1f,-1f,""));
        //LyricsSort.Insert(0,new LyricsTimestampSort(-1f,-1f,""));
        for(int i =0; i < LyricsSort.Count ; i++)
        {
            Keyframe LyricIndex = new Keyframe(KeyFrameRealTime(LyricsSort[i].Time), (float)(LyricsSort.Count - 1 - i), float.PositiveInfinity,0f,0f,0f);
            LyricsTimestamp.AddKey(LyricIndex);
            LyricIndex = new Keyframe(KeyFrameRealTime(LyricsSort[i].EndTime), 0f ,float.PositiveInfinity,0f,0f,0f);
            LyricsTimestamp.AddKey(LyricIndex);


        }
        LyricsSort.Reverse();

    }

    void ListReset(Target t)
    {
        t.initialPos = new Vector3();
        t.initialPos = t.path.transform.position;
        t.initialRot = new Vector3();
        t.initialRot = t.path.transform.eulerAngles;


        t.anim = new Animations();
        t.used = new UsedFlag();
        
        t.anim.pos = new AnimationTransform();
        t.anim.posdisable = new AnimationCurve();
        t.anim.posdisable.preWrapMode = WrapMode.Clamp;
        t.anim.rotation = new AnimationTransform();
        t.anim.rotdisable = new AnimationCurve();
        t.anim.rotdisable.preWrapMode = WrapMode.Clamp;
        t.anim.scale = new AnimationTransform();
        t.anim.color = new AnimationColor();

        //t.anim.pos.X = new AnimationCurve();
        t.anim.pos.X.preWrapMode = WrapMode.Clamp;
        t.anim.pos.Y = new AnimationCurve();
        t.anim.pos.Y.preWrapMode = WrapMode.Clamp;
        t.anim.pos.Z = new AnimationCurve();
        t.anim.pos.Z.preWrapMode = WrapMode.Clamp;
        t.anim.rotation.X = new AnimationCurve();
        t.anim.rotation.X.preWrapMode = WrapMode.Clamp;
        t.anim.rotation.Y = new AnimationCurve();        
        t.anim.rotation.Y.preWrapMode = WrapMode.Clamp;
        t.anim.rotation.Z = new AnimationCurve();
        t.anim.rotation.Z.preWrapMode = WrapMode.Clamp;
        t.anim.scale.X = new AnimationCurve();
        t.anim.scale.Y = new AnimationCurve();
        t.anim.scale.Z = new AnimationCurve();
        t.anim.color.R = new AnimationCurve();
        t.anim.color.G = new AnimationCurve();
        t.anim.color.B = new AnimationCurve();
        t.anim.color.A = new AnimationCurve();

        t.anim.t_timestamp = new AnimationCurve();
        t.anim.textlist = new List<string>();
        t.anim.i_timestamp = new AnimationCurve();
        t.anim.radialblur_timestamp = new AnimationCurve();
        t.anim.hpm_timestamp = new AnimationCurve();
        t.anim.hpn_timestamp = new AnimationCurve();

        
    }

    void KeyframeSet()
    {
        foreach(TimeStamp t in CommandList)
        {
            int i =TargetList.FindIndex(n => n.namecode == t.Target);
            //Debug.Log("code:"+t.Target+"   Listnum:"+i);

            if (t.FunctionName == "pos") AnimPos(KeyFrameRealTime(t.Time), i, t.values);
            if (t.FunctionName == "posReset") PosReset(KeyFrameRealTime(t.Time), i);
            if (t.FunctionName == "posLock") PosLock(KeyFrameRealTime(t.Time), i);
            if (t.FunctionName == "rot") AnimRot(KeyFrameRealTime(t.Time), i, t.values);
            if (t.FunctionName == "rotReset") RotReset(KeyFrameRealTime(t.Time), i);
            if (t.FunctionName == "scale") AnimScale(KeyFrameRealTime(t.Time), i, t.values);
            if (t.FunctionName == "color") AnimColor(KeyFrameRealTime(t.Time), i, t.values);
            if (t.FunctionName == "text") TextTimestamp(KeyFrameRealTime(t.Time), i, t.values);
            if (t.FunctionName == "image") ImageTimestamp(KeyFrameRealTime(t.Time), i, t.values);
            if (t.FunctionName == "radialblur") RadialBlurStrengh(KeyFrameRealTime(t.Time), i, t.values);
            if (t.FunctionName == "hpmax") AnimHPM(KeyFrameRealTime(t.Time), i, t.values);
            if (t.FunctionName == "hpnow") AnimHPN(KeyFrameRealTime(t.Time), i, t.values);

            
        }
    }

    float KeyFrameRealTime(float time)
    {
        return MasterScript.BPMCurve.Evaluate(time);
    }

    public class EasingReturn
    {
        public float InTan;
        public float OutTan;
        public float InWeight;
        public float OutWeight;
    }

    string EasingRegacy(string m)
    {
        if(m == "smoothOut") return "smoothOut_light";
        if(m == "smoothIn") return "smoothIn_light";
        if(m == "simple") return "linear";
        if(m == "smooth") return "smooth_light";
        return m;
    }

    public EasingReturn Easing(string mode, string lastmode)
    {
        if(lastmode == null) lastmode = "none";
        EasingReturn c = new EasingReturn();

        mode = EasingRegacy(mode);
        lastmode = EasingRegacy(lastmode);
        
        switch(mode)
        {
            case "set":
            default :
            c.InTan = float.PositiveInfinity;
            c.OutTan = 0f;
            c.InWeight = 0f;
            c.OutWeight = EaseSub(lastmode, "OutWeight");
            break;

            case "linear":
            c.InTan = 0f;
            c.OutTan = 0f;
            c.InWeight = 0f;
            c.OutWeight = EaseSub(lastmode, "OutWeight");
            break;

            case "smooth_light":
            c.InTan = 0f;
            c.OutTan = 0f;
            c.InWeight = 0.5f;
            c.OutWeight = EaseSub(lastmode, "OutWeight");
            break;

            case "smooth_hard":
            c.InTan = 0f;
            c.OutTan = 0f;
            c.InWeight = 1f;
            c.OutWeight = EaseSub(lastmode, "OutWeight");
            break;

            case "smoothOut_light":
            c.InTan = 0f;
            c.OutTan = 0f;
            c.InWeight = 1f;
            c.OutWeight = EaseSub(lastmode, "OutWeight");
            break;

            case "smoothIn_light":
            c.InTan = 0f;
            c.OutTan = 0f;
            c.InWeight = 0f;
            c.OutWeight = EaseSub(lastmode, "OutWeight");
            break;

        }
        return c;  
    }

    float EaseSub(string lastmode, string returnstate)
    {
        float returnvalue = 0f;
        switch(lastmode)
            {
                case "linear":
                if(returnstate == "OutWeight") returnvalue = 0f;
                break;

                case "smooth_light":
                if(returnstate == "OutWeight") returnvalue = 0.5f;
                break;

                case "smooth_hard":
                if(returnstate == "OutWeight") returnvalue = 1f;
                break;

                case "smoothOut_light":
                if(returnstate == "OutWeight") returnvalue = 0f;
                break;

                case "smoothIn_light":
                if(returnstate == "OutWeight") returnvalue = 1f;
                break;
            }
            return returnvalue;
    }

    void AnimPos(float t, int i, List<string> values)//pos(x_float,y_float,z_float,type_string)
    {

        EasingReturn ease = Easing(values[3], TargetList[i].anim.poslastmode);

        Keyframe Xkey = new Keyframe(t,float.Parse(values[0]),ease.InTan,ease.OutTan,ease.InWeight,ease.OutWeight);
        Keyframe Ykey = new Keyframe(t,float.Parse(values[1]),ease.InTan,ease.OutTan,ease.InWeight,ease.OutWeight);
        Keyframe Zkey = new Keyframe(t,float.Parse(values[2]),ease.InTan,ease.OutTan,ease.InWeight,ease.OutWeight);
        Keyframe Posflag = new Keyframe(t,0f,float.PositiveInfinity,0f,0f,0f);

        
        TargetList[i].anim.pos.X.AddKey(Xkey);
        TargetList[i].anim.pos.Y.AddKey(Ykey);
        TargetList[i].anim.pos.Z.AddKey(Zkey);
        
        TargetList[i].anim.posdisable.AddKey(Posflag);
        

        TargetList[i].anim.poslastmode = values[3];
        if(!TargetList[i].used.pos) TargetList[i].used.pos = true;
        
    }

    void PosReset(float t, int i)//x:0,y:9,z:-7
    {
        EasingReturn ease = Easing("set", TargetList[i].anim.poslastmode);

        Keyframe Xkey = new Keyframe(t,TargetList[i].initialPos.x,ease.InTan,ease.OutTan,ease.InWeight,ease.OutWeight);
        Keyframe Ykey = new Keyframe(t,TargetList[i].initialPos.y,ease.InTan,ease.OutTan,ease.InWeight,ease.OutWeight);
        Keyframe Zkey = new Keyframe(t,TargetList[i].initialPos.z,ease.InTan,ease.OutTan,ease.InWeight,ease.OutWeight);
        Keyframe Posflag = new Keyframe(t,1f,float.PositiveInfinity,0f,0f,0f);

        TargetList[i].anim.pos.X.AddKey(Xkey);
        TargetList[i].anim.pos.Y.AddKey(Ykey);
        TargetList[i].anim.pos.Z.AddKey(Zkey);
        
        TargetList[i].anim.posdisable.AddKey(Posflag);

        TargetList[i].anim.poslastmode = "set";

    }

    void PosLock(float t, int i)
    {
        Keyframe Posflag = new Keyframe(t,0f,float.PositiveInfinity,0f,0f,0f);
        TargetList[i].anim.posdisable.AddKey(Posflag);

        TargetList[i].anim.poslastmode = "set";

    }

    void AnimRot(float t, int i, List<string> values)//rot(x_float,y_float,z_float,type_string)
    {
        EasingReturn ease = Easing(values[3], TargetList[i].anim.rotlastmode);

        Keyframe Xkey = new Keyframe(t,float.Parse(values[0]),ease.InTan,ease.OutTan,ease.InWeight,ease.OutWeight);
        Keyframe Ykey = new Keyframe(t,float.Parse(values[1]),ease.InTan,ease.OutTan,ease.InWeight,ease.OutWeight);
        Keyframe Zkey = new Keyframe(t,float.Parse(values[2]),ease.InTan,ease.OutTan,ease.InWeight,ease.OutWeight);
        Keyframe Rotflag = new Keyframe(t,0f,float.PositiveInfinity,0f,0f,0f);

        
        TargetList[i].anim.rotation.X.AddKey(Xkey);
        TargetList[i].anim.rotation.Y.AddKey(Ykey);
        TargetList[i].anim.rotation.Z.AddKey(Zkey);
        
        TargetList[i].anim.rotdisable.AddKey(Rotflag);
        

        TargetList[i].anim.rotlastmode = values[3];
        if(!TargetList[i].used.rotation) TargetList[i].used.rotation = true;
    }

     void RotReset(float t, int i)//x:30,y:0,z:0
    {
        EasingReturn ease = Easing("set", TargetList[i].anim.rotlastmode);

        Keyframe Xkey = new Keyframe(t,TargetList[i].initialRot.x,ease.InTan,ease.OutTan,ease.InWeight,ease.OutWeight);
        Keyframe Ykey = new Keyframe(t,TargetList[i].initialRot.y,ease.InTan,ease.OutTan,ease.InWeight,ease.OutWeight);
        Keyframe Zkey = new Keyframe(t,TargetList[i].initialRot.z,ease.InTan,ease.OutTan,ease.InWeight,ease.OutWeight);
        Keyframe Rotflag = new Keyframe(t,1f,float.PositiveInfinity,0f,0f,0f);

        TargetList[i].anim.rotation.X.AddKey(Xkey);
        TargetList[i].anim.rotation.Y.AddKey(Ykey);
        TargetList[i].anim.rotation.Z.AddKey(Zkey);
        
        TargetList[i].anim.rotdisable.AddKey(Rotflag);

        TargetList[i].anim.rotlastmode = "set";

    }

    void AnimScale(float t, int i, List<string> values)//scale(x_float,y_float,z_float,type_string)
    {
        EasingReturn ease = Easing(values[3], TargetList[i].anim.scalelastmode);

        Keyframe Xkey = new Keyframe(t,float.Parse(values[0]),ease.InTan,ease.OutTan,ease.InWeight,ease.OutWeight);
        Keyframe Ykey = new Keyframe(t,float.Parse(values[1]),ease.InTan,ease.OutTan,ease.InWeight,ease.OutWeight);
        Keyframe Zkey = new Keyframe(t,float.Parse(values[2]),ease.InTan,ease.OutTan,ease.InWeight,ease.OutWeight);

        TargetList[i].anim.scale.X.AddKey(Xkey);
        TargetList[i].anim.scale.Y.AddKey(Ykey);
        TargetList[i].anim.scale.Z.AddKey(Zkey);
        
        TargetList[i].anim.scalelastmode = values[3];
        if(!TargetList[i].used.scale) TargetList[i].used.scale = true;

    }
    
    void AnimColor(float t, int i, List<string> values)//color(r_float,g_float,b_float,a_float,type_string)
    {
        EasingReturn ease = Easing(values[4], TargetList[i].anim.colorlastmode);

        Keyframe Rkey = new Keyframe(t,float.Parse(values[0]),ease.InTan,ease.OutTan,ease.InWeight,ease.OutWeight);
        Keyframe Gkey = new Keyframe(t,float.Parse(values[1]),ease.InTan,ease.OutTan,ease.InWeight,ease.OutWeight);
        Keyframe Bkey = new Keyframe(t,float.Parse(values[2]),ease.InTan,ease.OutTan,ease.InWeight,ease.OutWeight);
        Keyframe Akey = new Keyframe(t,float.Parse(values[3]),ease.InTan,ease.OutTan,ease.InWeight,ease.OutWeight);

        TargetList[i].anim.color.R.AddKey(Rkey);
        TargetList[i].anim.color.G.AddKey(Gkey);
        TargetList[i].anim.color.B.AddKey(Bkey);
        TargetList[i].anim.color.A.AddKey(Akey);
        
        TargetList[i].anim.colorlastmode = values[4];
        if(!TargetList[i].used.color) TargetList[i].used.color = true;
    }

    void TextTimestamp(float t, int i, List<string> values)
    {
        
        if(!TargetList[i].used.text)
        {
            Keyframe TextIndexInit = new Keyframe(-10000f, -1.0f, float.PositiveInfinity,0f,0f,0f);
            TargetList[i].anim.t_timestamp.AddKey(TextIndexInit);
        }
        Keyframe TextIndex = new Keyframe(t, (float)TargetList[i].anim.textlist.Count, float.PositiveInfinity,0f,0f,0f);
        TargetList[i].anim.t_timestamp.AddKey(TextIndex);
        TargetList[i].anim.textlist.Add(IndentionConvert(values[0]));
        if(!TargetList[i].used.text) TargetList[i].used.text = true;

    }

    string IndentionConvert(string s)
    {
        s = s.Replace("\\n","\n");
        return s;

    }

    void ImageTimestamp(float t, int i, List<string> values)
    {
        if(!TargetList[i].used.image)
        {
            Keyframe ImageIndexInit = new Keyframe(-10000f, -1.0f, float.PositiveInfinity,0f,0f,0f);
            TargetList[i].anim.i_timestamp.AddKey(ImageIndexInit);
        }
        //string imagepath = Application.dataPath + "/../EVENTS/" + values[0];
        //Debug.Log("loadimagepath : " + imagepath);
        int overlap = loadimagespath.IndexOf(values[0]);
        //Debug.Log("overlap : " + overlap);
        if(overlap == -1)
        {
            LoadImageIndex setimage = new LoadImageIndex();
            setimage.sprite = PathToSpriteRead(values[0],Vector4.zero);
            if(setimage.sprite == null) return;
            
            loadimages.Add(setimage);
            loadimagespath.Add(values[0]);
            overlap = loadimages.Count -1;
            
        }
        //Debug.Log("last : " + (overlap == -1 ? (float)loadimages.Count : overlap).ToString());

        Keyframe ImageIndex = new Keyframe(t, (float)overlap, float.PositiveInfinity,0f,0f,0f);
        TargetList[i].anim.i_timestamp.AddKey(ImageIndex);
        if(!TargetList[i].used.image) TargetList[i].used.image = true;

    }

    void AnimHPM(float t, int i, List<string> values)//hpmax(value_float,type_string)
    {
        EasingReturn ease = Easing(values[1], TargetList[i].anim.hpmlastmode);

        Keyframe HPkey = new Keyframe(t,Mathf.Clamp(float.Parse(values[0]),0.0f,100.0f), ease.InTan,ease.OutTan,ease.InWeight,ease.OutWeight);

        TargetList[i].anim.hpm_timestamp.AddKey(HPkey);
        
        TargetList[i].anim.hpmlastmode = values[1];
        if(!TargetList[i].used.hpm) TargetList[i].used.hpm = true;
    }
    void AnimHPN(float t, int i, List<string> values)//hpnow(value_float,type_string)
    {
        EasingReturn ease = Easing(values[1], TargetList[i].anim.hpnlastmode);

        Keyframe HPkey = new Keyframe(t,Mathf.Clamp(float.Parse(values[0]),0.0f,100.0f), ease.InTan,ease.OutTan,ease.InWeight,ease.OutWeight);

        TargetList[i].anim.hpn_timestamp.AddKey(HPkey);
        
        TargetList[i].anim.hpnlastmode = values[1];
        if(!TargetList[i].used.hpn) TargetList[i].used.hpn = true;
    }

    void RadialBlurStrengh(float t, int i, List<string> values)//radialblur(strengh_float,type_string)
    {
        EasingReturn ease = Easing(values[1], TargetList[i].anim.radialblurlastmode);

        Keyframe Strenghkey = new Keyframe(t,float.Parse(values[0]),ease.InTan,ease.OutTan,ease.InWeight,ease.OutWeight);

        TargetList[i].anim.radialblur_timestamp.AddKey(Strenghkey);
        
        TargetList[i].anim.radialblurlastmode = values[1];
        if(!TargetList[i].used.radialblur) TargetList[i].used.radialblur = true;
    }

    public bool UseDummyCheck(string name)
    {
        if(name == "Main Camera") return true;
        return false;
    }






    void SkinUpdate()
    {//((0.01f * (drbfile.onpu[i].ms) - MasterScript.SHINDO) * MasterScript.NoteSpeed < 150.0f)
        int nownoteskin = (int)skinindex.Evaluate((float)MasterScript.SHINDO + (15000f * (1f / MasterScript.NoteSpeed)));
        int nowskin = (int)skinindex.Evaluate((float)MasterScript.SHINDO);
        if(!(currentskinid == nowskin && currentskinid == nownoteskin))
        {
            if(skins[nownoteskin].set_notes)
            {
                f_SpriteNotes.SetValue(gamemanager,skins[nownoteskin].notes);
                f_SpriteArror.SetValue(gamemanager,skins[nownoteskin].arrors);
            }
            if(skins[nowskin].set_hp)
            {
                f_SpriteHP.SetValue(hpmanager,skins[nowskin].HP);
                if(skins[nowskin].hpglow)
                {
                    foreach (Transform hps in tr_hpmanager)
                    {
                        hps.gameObject.GetComponent<SpriteRenderer>().material = skins[nownoteskin].glowmat;
                    }

                }
                    
            }
            currentskinid = nowskin;
        }
    }



    bool AnimationBool_AND(float num1, float num2)
    {
        return (num1 == 1 && num2 == 1);
    }


        // Update is called once per frame
    void Update()
    {
        if(Flag)
        {
            if(LyricsTimestamp.length > 0)
            LyricPath.text = LyricsSort[(int)LyricsTimestamp.Evaluate((float)MasterScript.SHINDO)].lyric;
            if(skinused) SkinUpdate();

            foreach(Target t in TargetList)
            {

                if(t.usedummy)
                {
                    if(t.name == "Main Camera")
                    {
                        if(TheCamera == t.path && !AnimationBool_AND(t.anim.posdisable.Evaluate((float)MasterScript.SHINDO), t.anim.rotdisable.Evaluate((float)MasterScript.SHINDO)))
                        {
                            TheCamera = t.dummypath;
                        }
                        if(TheCamera == t.dummypath && AnimationBool_AND(t.anim.posdisable.Evaluate((float)MasterScript.SHINDO), t.anim.rotdisable.Evaluate((float)MasterScript.SHINDO)))
                        {
                            TheCamera = t.path;
                        }
                    }
                }
                Vector3 currentpos;
                Vector3 currentrot;


                if(t.usedummy && t.anim.posdisable.Evaluate((float)MasterScript.SHINDO) == 1)
                    currentpos = new Vector3
                    ( 
                        t.dummypath.transform.localPosition.x, 
                        t.dummypath.transform.localPosition.y,
                        t.dummypath.transform.localPosition.z
                    );
                else
                    currentpos = new Vector3
                    ( 
                        t.anim.pos.X.Evaluate((float)MasterScript.SHINDO), 
                        t.anim.pos.Y.Evaluate((float)MasterScript.SHINDO),
                        t.anim.pos.Z.Evaluate((float)MasterScript.SHINDO)
                    );

                if(t.usedummy && t.anim.rotdisable.Evaluate((float)MasterScript.SHINDO) == 1)
                    currentrot = new Vector3
                    (
                        t.dummypath.transform.localEulerAngles.x, 
                        t.dummypath.transform.localEulerAngles.y,
                        t.dummypath.transform.localEulerAngles.z
                    );
                else
                    currentrot = new Vector3
                    (
                        t.anim.rotation.X.Evaluate((float)MasterScript.SHINDO), 
                        t.anim.rotation.Y.Evaluate((float)MasterScript.SHINDO),
                        t.anim.rotation.Z.Evaluate((float)MasterScript.SHINDO)   
                    );

                Vector3 currentscale = new Vector3
                (
                    t.anim.scale.X.Evaluate((float)MasterScript.SHINDO), 
                    t.anim.scale.Y.Evaluate((float)MasterScript.SHINDO),
                    t.anim.scale.Z.Evaluate((float)MasterScript.SHINDO)
                );

                Color currentcolor = new Color
                (
                    t.anim.color.R.Evaluate((float)MasterScript.SHINDO), 
                    t.anim.color.G.Evaluate((float)MasterScript.SHINDO),
                    t.anim.color.B.Evaluate((float)MasterScript.SHINDO),
                    t.anim.color.A.Evaluate((float)MasterScript.SHINDO)
                );

                int currentimage = Mathf.RoundToInt(t.anim.i_timestamp.Evaluate((float)MasterScript.SHINDO));
                int currenttext = Mathf.RoundToInt(t.anim.t_timestamp.Evaluate((float)MasterScript.SHINDO));

                if(t.used.pos)t.path.transform.localPosition = currentpos;
                //Debug.Log(t.name + ":" + currentpos.x);
                if(t.used.rotation)t.path.transform.localEulerAngles = currentrot;
                if(t.used.scale)t.path.transform.localScale = currentscale;


                if(t.used.color)
                {
                    if(t.createflag)
                    {
                        t.imagepath.GetComponent<Image>().color = currentcolor;
                        t.textpath.GetComponent<Text>().color = currentcolor;
                    }
                    else
                    {
                        if(t.path.GetComponent<Image>())t.path.GetComponent<Image>().color = currentcolor;
                        if(t.path.GetComponent<Text>())t.path.GetComponent<Text>().color = currentcolor;
                        if(t.path.GetComponent<SpriteRenderer>())t.path.GetComponent<SpriteRenderer>().color = currentcolor;
                    }
                }

                if(t.used.text)
                {
                    if(t.createflag)
                    {
                        if(currenttext != -1.0f)t.textpath.GetComponent<Text>().text = t.anim.textlist[currenttext];
                    }
                    else
                    {
                        if(t.path.GetComponent<Text>() && currenttext != -1.0f)t.path.GetComponent<Text>().text = t.anim.textlist[currenttext];
                    }

                }

                if(t.used.image)
                {
                    if(t.createflag) 
                    {
                        if(currentimage != -1.0f) t.imagepath.GetComponent<Image>().sprite = loadimages[currentimage].sprite;
                    }
                    else
                    {
                        if(t.path.GetComponent<Image>() && currentimage != -1.0f)t.path.GetComponent<Image>().sprite = loadimages[currentimage].sprite;
                        if(t.path.GetComponent<SpriteRenderer>() && currentimage != -1.0f)t.path.GetComponent<SpriteRenderer>().sprite = loadimages[currentimage].sprite;
                    }
                }
                if(t.used.radialblur)
                {
                    t.path.GetComponent<Cam_Radial_Blur>()._strength = t.anim.radialblur_timestamp.Evaluate((float)MasterScript.SHINDO);
                }
                if(t.name == "HPManager")
                {
                    if(t.used.hpm)
                        hpmanager.HPMAX = t.anim.hpm_timestamp.Evaluate((float)MasterScript.SHINDO);
                        
                    if(t.used.hpn)
                        hpmanager.HPNOW = t.anim.hpn_timestamp.Evaluate((float)MasterScript.SHINDO);

                }
            }
        }
    }

    public bool Disableflag(string name , string param)
    {
        //1:外部から使用可能 0:イベントアニメーションが優先
        int i =TargetList.FindIndex(n => n.name == name);
        if(param == "pos" && i >= 0)
        {
            if(TargetList[i].anim.posdisable.Evaluate((float)MasterScript.SHINDO) > 0) return true;
            else return false;
        }
        if(param == "rot" && i >= 0)
        {
            if( TargetList[i].anim.rotdisable.Evaluate((float)MasterScript.SHINDO) > 0) return true;
            else return false;
        }
        return true;
    }

    public string RemoveSlash(string str)
    {
        string s = str;
        for(;s.IndexOf("//") >=0;)
        {
            int start = s.IndexOf("//");
            s = s.Remove(start,(s.IndexOf("\n",s.IndexOf("//"))+1-start));
        }
        return s;
    }
}
