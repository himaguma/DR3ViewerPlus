using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShutter : MonoBehaviour
{

    [SerializeField]
    Vector2Int BlockCount = new Vector2Int(9,16);

    float[,] Scale;

    [SerializeField]
    Texture source;

    [SerializeField]
    Texture missing;

    Vector2 Blocksize = new Vector2();
    // Start is called before the first frame update
    void Start()
    {
        if(source == null)
        {
            source = missing;
        }

        Scale = new float[BlockCount.x,BlockCount.y];
	    for (int i = 0; i < BlockCount.x; i++)
	    {
		    for (int j = 0; j < BlockCount.y; j++)
		    {
    			Scale[i, j] = 0f;
		    }
	    }


        BlockInit();
        
        for (int k = 0; k < BlockCount.x; k++)
	    {
		    for (int l = 0; l < BlockCount.y; l++)
		    {			    
    			Scale[k, l] = 1.1f;	
		    }
	    }
        StartCoroutine(AnimBlock());
        
    }

    void BlockInit()
    {
        
        Blocksize = new Vector2((float)Screen.width / (float)BlockCount.x, (float)Screen.height / (float)BlockCount.y);
    
    }

    IEnumerator AnimBlock()
    {
        int time = 0;
        
        
        while (true)
	    {
		    for (int k = 0; k < BlockCount.x; k++)
		    {
    			for (int l = 0; l < BlockCount.y; l++)
			    {
    				if (k + l < time)
				    {
    					this.Scale[k, l] -= 0.1f;
					    if (this.Scale[k, l] <= 0f)
					    {
    						this.Scale[k, l] = 0f;
					    }
				    }
			    }
		    }
		    
		    time++;
		    if (time >= BlockCount.x + BlockCount.y + 11)
		    {
			    break;
		    }
		    yield return new WaitForSeconds(0.01f);
	    }
	    //this.flag = false;
	    yield return null;
	    yield break;
	    
    }
    void OnGUI()
    {
	    for (int i = 0; i < BlockCount.x; i++)
	    {
		    for (int j = 0; j < BlockCount.y; j++)
		    {
			    GUI.DrawTextureWithTexCoords(
                new Rect(Blocksize.x * (float)i + (1f - Scale[i, j]) * 0.5f * Blocksize.x, Blocksize.y * (float)j + (1f - Scale[i, j]) * 0.5f * Blocksize.y, Scale[i, j] * Blocksize.x, Scale[i, j] * Blocksize.y),
                source,
                new Rect(0.5f * (1f - Scale[i, j]), 0.5f * (1f - Scale[i, j]), Scale[i, j], Scale[i, j]));
		    }
	    }
    }
}
