using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEditor;
using TMPro;
using System.Text.RegularExpressions;

[DisallowMultipleComponent, DefaultExecutionOrder(8)]
public class TheOnpuPlus : MonoBehaviour
{
    /*[SerializeField]
    TMP_Text Info;
    [SerializeField]
    Transform tInfo;*/


    
    FieldInfo f_OnpuData;
    

    SpriteRenderer spriteRenderer;
    TheOnpu refonpu;
    public TheOnpu.OnpuData onpuData;
    TheGameManager gamemanager;

    TheOnpuPlus()
    {
        var conpu = typeof(TheOnpu);
        f_OnpuData = conpu.GetField("onpuData",BindingFlags.NonPublic | BindingFlags.Instance);
    }
    // Start is called before the first frame update

    public void onpuDataSet()
    {
        onpuData = (TheOnpu.OnpuData)f_OnpuData.GetValue(refonpu);
    }
    void Start()
    {
        refonpu = GetComponent<TheOnpu>();
        onpuData = (TheOnpu.OnpuData)f_OnpuData.GetValue(refonpu);
        spriteRenderer = GetComponent<SpriteRenderer>();

        if(CheckAlphaKind(onpuData.kind)) spriteRenderer.color = new Color(1.0f, 1.0f, 1.0f, 0.01f);
        /*if(Info)
        {
            if(onpuData.nsc != "1")
            Info.text = "<"+ onpuData.nsc + ">";
            else
            Info.text = "";

            Regex modergx = new Regex(@"^OFFSET=");
            if(modergx.IsMatch(onpuData.mode))
            {
                string[] modes = onpuData.mode.Split('=');
                tInfo.localPosition = new Vector3(tInfo.localPosition.x,float.Parse(modes[1]),tInfo.localPosition.z);
            }
        }*/
    }

    /*void Update()
    {
        
        if(onpuData.isnadnsc)
        {

                string[] nscs = onpuData.nsc.Split(';');
                int next =NextKey(refonpu.acNSC,(float)refonpu.gameManager.SHINDO);
                nscs[next] = "<color=#00ffff>" + nscs[next] + "</color>";
                if(next!=0) nscs[next-1] = "<color=#33ddff>" + nscs[next-1] + "</color>";
                
                if(Info)Info.text = "<"+string.Join(";", nscs)+">";
                

        }
    }*/

    bool CheckAlphaKind(int k)
    {
        if(k == 19) return true;
        if(k == 21) return true;
        if(k == 23) return true;
        return false;
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/
    int NextKey(AnimationCurve target, float t)
    {
        //target.keys
        for(int i=0; i<target.length;i++)
        {
            if(target.keys[i].time > t)
            {
                return i;
            }
            
        }
        return 0;
    }
}


#if !UNITY_EDITOR

[CustomEditor(typeof(TheOnpuPlus))]
public class TheOnpuPlusEditor : Editor
{

    bool islocalized;
    public override void OnInspectorGUI()
    {
        TheOnpuPlus onpu = target as TheOnpuPlus;
        


        if(onpu.onpuData != null)
        {

            islocalized = EditorGUILayout.Toggle("????????????????????????",islocalized);
            EditorGUILayout.IntField(islocalized ? "id":"ID",onpu.onpuData.id);
            EditorGUILayout.IntField("realid",onpu.onpuData.realid);
            EditorGUILayout.IntField(islocalized ? "kind":"??????",onpu.onpuData.kind);
            EditorGUILayout.FloatField(islocalized ? "ichi":"???????????????(??????)",onpu.onpuData.ichi);
            EditorGUILayout.FloatField(islocalized ? "ms":"???????????????(ms)",onpu.onpuData.ms);
            EditorGUILayout.FloatField(islocalized ? "dms":"????????????",onpu.onpuData.dms);
            EditorGUILayout.FloatField(islocalized ? "pos":"??????",onpu.onpuData.pos);
            EditorGUILayout.FloatField(islocalized ? "width":"???",onpu.onpuData.width);
            EditorGUILayout.TextField(islocalized ? "nsc":"????????????SC?????????",onpu.onpuData.nsc);
            EditorGUILayout.Toggle(islocalized ? "isnadnsc":"??????NSC?????????",onpu.onpuData.isnadnsc);
            EditorGUILayout.FloatField(islocalized ? "insc":"??????NSC",onpu.onpuData.insc);
            EditorGUILayout.FloatField("maxtime",onpu.onpuData.maxtime);
            EditorGUILayout.TextField(islocalized ? "mode":"??????????????????",onpu.onpuData.mode);

            EditorGUILayout.IntField(islocalized ? "parent":"????????????ID",onpu.onpuData.parent);
            EditorGUILayout.FloatField(islocalized ? "parent_ms":"????????????ms",onpu.onpuData.parent_ms);
            EditorGUILayout.FloatField(islocalized ? "parent_dms":"????????????dms",onpu.onpuData.parent_dms);
            EditorGUILayout.FloatField(islocalized ? "parent_pos":"????????????pos",onpu.onpuData.parent_pos);
            EditorGUILayout.FloatField(islocalized ? "parent_width":"????????????width",onpu.onpuData.parent_width);

            EditorGUILayout.FloatField(islocalized ? "center":"??????",onpu.onpuData.center);
            //onpu.onpuData. = EditorGUILayout.FloatField(islocalized ? "":"",onpu.onpuData.);
            EditorGUILayout.Toggle(islocalized ? "isNear":"????????????",onpu.onpuData.isNear);
            EditorGUILayout.Toggle(islocalized ? "isWaitForGD":"GD????????????",onpu.onpuData.isWaitForGD);
            EditorGUILayout.Toggle(islocalized ? "isWaitForPF":"PF????????????",onpu.onpuData.isWaitForPF);
            EditorGUILayout.FloatField(islocalized ? "WaitForSec":"??????????????????",onpu.onpuData.WaitForSec);
            
            
        }


    }
        
}
#endif