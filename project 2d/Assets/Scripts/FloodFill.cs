﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FloodFill : MonoBehaviour
{
    Texture2D readble;
    Texture2D helper;
    public bool flag;
    public bool finished=false;
    Texture2D Bwimg;
    Texture2D Orig;
    int numPixels;
    int it;
    Dictionary<int, Vector2> areasCoord = new Dictionary<int, Vector2>();

    void Start()
    {
        if (GameObject.FindGameObjectWithTag("player").GetComponent<PaintByClick>().isTutorial())
        {
            readble = duplicateTexture(TextureToTexture2D(GameObject.FindGameObjectWithTag("paintable").GetComponent<Image>().mainTexture));
            Bwimg = duplicateTexture(TextureToTexture2D(GameObject.FindGameObjectWithTag("paintable").GetComponent<Image>().mainTexture));
            Orig = duplicateTexture(TextureToTexture2D(GameObject.FindGameObjectWithTag("painted").GetComponent<Image>().mainTexture));
        }
        else
        {
            StartCoroutine(LateStart(0.7f));
        }
    }

    IEnumerator LateStart(float waitTime)
    {
        
        yield return new WaitForSeconds(waitTime);
        Debug.Log("img size in flood fill width= " + GameObject.FindGameObjectWithTag("routeLevel").GetComponent<RouteLevel>().getBWImg().width + " ,height = " +GameObject.FindGameObjectWithTag("routeLevel").GetComponent<RouteLevel>().getBWImg().height);
        readble = duplicateTexture(GameObject.FindGameObjectWithTag("routeLevel").GetComponent<RouteLevel>().getBWImg());
        Debug.Log("after assignnnnnnnnn");
        Bwimg = duplicateTexture(GameObject.FindGameObjectWithTag("routeLevel").GetComponent<RouteLevel>().getBWImg());
        Orig = GameObject.FindGameObjectWithTag("routeLevel").GetComponent<RouteLevel>().getColorImg();
    }

    Texture2D duplicateTexture(Texture2D source)  {
        RenderTexture renderTex = RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readble = new Texture2D(source.width, source.height);
        readble.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readble.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readble;
    }

    private Texture2D TextureToTexture2D(Texture texture)
    {
        Texture2D texture2D = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 32);
        Graphics.Blit(texture, renderTexture);

        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();

        RenderTexture.active = currentRT;
        RenderTexture.ReleaseTemporary(renderTexture);
        return texture2D;
    }

    public void setVariables()
    {
        //GameObject.FindGameObjectWithTag("score").SetActive(true);
        GameObject.FindGameObjectWithTag("score").GetComponent<calculateScore>().setVariables(Orig, readble, areasCoord);
    }
    public Texture2D getCorrectPixelMouseClick(Vector2 dat,Color targetColor)
    {
        
        this.flag = false;
        Vector2 localCursor;
        var rect1 = GameObject.FindGameObjectWithTag("paintable").GetComponent<RectTransform>();
        var pos1 = dat;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect1, pos1,
            null, out localCursor))
        {
            Debug.Log("its nullll in local cursor");
            return null;
        }
        
        int xpos = (int)(localCursor.x);
        int ypos = (int)(localCursor.y);

        if (xpos < 0) xpos = xpos + (int)rect1.rect.width / 2;
        else xpos += (int)rect1.rect.width / 2;

        if (ypos > 0) ypos = ypos + (int)rect1.rect.height / 2;
        else ypos += (int)rect1.rect.height / 2;

        if (xpos < 0 || ypos < 0 || xpos > rect1.sizeDelta.x || ypos > rect1.sizeDelta.y)
        {
            Debug.Log("its nullll in border pos");
            return null;
        }
        int x = (int)(readble.width * (xpos / rect1.sizeDelta.x));
        int y = (int)(readble.height * (ypos / rect1.sizeDelta.y));

        if (Bwimg.GetPixel(x, y) == new Color(0f, 0f, 0f, 1f))
        {
            Debug.Log("its nullll in black color");
            return readble;
        }
        helper = new Texture2D(readble.width, readble.height);
        helper.SetPixels(readble.GetPixels());
        helper.Apply();
        floodFill4Stack(targetColor, x, y);
        finished = true;
        readble.Apply();
        gameObject.GetComponent<Image>().sprite = Sprite.Create(readble, new Rect(0.0f, 0.0f, readble.width, readble.height), new Vector2(0.5f, 0.5f), 100.0f); 
        return readble;
    }

    public static readonly int[] dx = { 0, 1, 0, -1 }; // relative neighbor x coordinates
    public static readonly int[] dy = { -1, 0, 1, 0 }; // relative neighbor y coordinates

    //4-way floodfill using our own stack routines
    void floodFill4Stack(Color targetColor, int x, int y)
    {
        
        if (!CheckValidity(x, y, targetColor))  return; //avoid infinite loop

        int iteration = 0;

        HashSet<Vector2> hs = new HashSet<Vector2>();
        hs.Add(new Vector2(x, y));
        Debug.Log("x, y = " + x + "," + y);
        while (hs.Count > 0 && !this.flag)
        {
            Vector2 point = hs.ElementAt(hs.Count - 1);
            hs.Remove(point);
            //StartCoroutine(setColor((int)point.x, (int)point.y, targetColor));
            readble.SetPixel((int)point.x, (int)point.y, targetColor);
            for (int i = 0; i < 4; i++)
            {
                int nx = (int)point.x + dx[i];
                int ny = (int)point.y + dy[i];
                if (CheckValidity(nx, ny, targetColor))
                {
                    hs.Add(new Vector2(nx, ny));
                }
                iteration++;
            }
            iteration++;
        }
        if (this.flag)
        {
            readble.SetPixels(helper.GetPixels());
            return;
        }
        if (!this.flag)
        {
            it += iteration;
            Debug.Log("it = " + it);
            if (!areasCoord.ContainsKey(iteration))
            {
                
                areasCoord.Add(iteration, new Vector2(x, y));  
            }
        }
        Debug.Log("iterations: " + iteration);
        foreach(KeyValuePair<int,Vector2> item in areasCoord){
            Debug.Log("area: " + item.ToString());
        }
    }

    bool CheckValidity(int x, int y, Color TargetColor)
    {
        if (colorCompare(readble.GetPixel(x, y) , TargetColor))
        {
            return false;
        }
        if (Bwimg.GetPixel(x, y) == new Color(0f, 0f, 0f, 1f))
        {
            return false;
        }
        if (x <= 0 || x >= readble.width - 1 || y <= 0 || y >= readble.height - 1)
        {
            this.flag = true;
            return false;
        }

        return true;
    }

    bool colorCompare(Color a, Color b)
    {
        if ((a.r + 0.01 > b.r && a.r - 0.01 < b.r) &&
            (a.g + 0.01 > b.g && a.g - 0.01 < b.g) &&
            (a.b + 0.01 > b.b && a.b - 0.01 < b.b)) return true;
        return false;

    }
}
