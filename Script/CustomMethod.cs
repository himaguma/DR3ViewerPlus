using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;


namespace DR3ViewerPlusCustomMethod
{
    public class DR3PCTMethod
    {

    
        static public Sprite PathToSpriteRead(string path, Vector4 border)
        {
            string fullpath = Application.dataPath + "/Resources/IMAGES/" + path;

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
                //Debug.Log("image was not found : " + path);
                return null;
            }
            return sprite;
        }
    }
}