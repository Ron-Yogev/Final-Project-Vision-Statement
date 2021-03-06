﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Drawing;
using Color = UnityEngine.Color;
using System.Linq;
using Image = UnityEngine.UI.Image;
using UnityEngine.SceneManagement;

public class blackWhiteProcess : MonoBehaviour
{
    Texture2D grayImg;
    Texture2D passedImg;
    Texture2D helpImg;
    Texture2D helperFill;
    [SerializeField]
    Button okBtn = null;
    [SerializeField]
    Button afterBtn = null;
    [SerializeField]
    Slider slider = null;
    bool flag;
    List<List<Vector2>> areas = new List<List<Vector2>>();
    int pos = -1;

    // Start is called before the first frame update
    void Start()
    {
        afterBtn.gameObject.SetActive(false);
        passedImg = gameManager.getImage();
        passedImg = new Texture2D(passedImg.width, passedImg.height);
        passedImg.SetPixels(gameManager.getImage().GetPixels());
        grayImg = new Texture2D(passedImg.width, passedImg.height);
        helpImg = new Texture2D(passedImg.width, passedImg.height);
        grayImg.SetPixels(passedImg.GetPixels());
        ConvertToGrayscale();
        grayImg.Apply();
        gameObject.GetComponent<Image>().overrideSprite = Sprite.Create(grayImg, new Rect(0.0f, 0.0f, passedImg.width, passedImg.height), new Vector2(0.5f, 0.5f), 100.0f);

    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            this.flag = false;
            getCorrectPixelMouseClick(Input.mousePosition);
        }
    }

    public void hideOkButton()
    {
        helpImg.SetPixels(grayImg.GetPixels()); // for next step (extract areas)
        gameManager.setImageBW(helpImg);
        SceneManager.LoadScene("AreasChoosing");
        okBtn.gameObject.SetActive(false);
        slider.gameObject.SetActive(false);
        afterBtn.gameObject.SetActive(true);
    }

    public void afterAreasButton()
    {
        //Bitmap m = new Bitmap();
        for (int k = 0; k < areas.Count; k++)
        {
            float r = 0;
            float g = 0;
            float b= 0;
            for(int i=0; i<areas[k].Count; i++)
            {
                int x = (int)areas[k][i].x;
                int y = (int)areas[k][i].y;
                r += passedImg.GetPixel(x, y).r;
                g += passedImg.GetPixel(x, y).g;
                b += passedImg.GetPixel(x, y).b;
                if (i == areas[k].Count - 1)
                {
                    r /= areas[k].Count;
                    g /= areas[k].Count;
                    b /= areas[k].Count;
                    fillArea(new Color(r, g, b), k);
                }
            }           
        }
        passedImg.Apply();
        gameObject.GetComponent<Image>().overrideSprite = Sprite.Create(passedImg, new Rect(0.0f, 0.0f, passedImg.width, passedImg.height), new Vector2(0.5f, 0.5f), 100.0f);
    }

    public void fillArea(Color color, int k)
    {
        for(int i = 0; i < areas[k].Count; i++)
        {
            int x = (int)areas[k][i].x;
            int y = (int)areas[k][i].y;
            passedImg.SetPixel(x, y, color);
        }
    }

    public void changeTreshold(float value)
    {

        for (int i = 0; i < this.grayImg.width; i++)
        {
            for (int j = 0; j < this.grayImg.height; j++)
            {
                if (helpImg.GetPixel(i, j).r < value/255f)
                {
                    grayImg.SetPixel(i, j, Color.black);
                }
                else
                {
                    grayImg.SetPixel(i, j, Color.white);
                }
            }
        }
        grayImg.Apply();
        gameObject.GetComponent<Image>().overrideSprite = Sprite.Create(grayImg, new Rect(0.0f, 0.0f, passedImg.width, passedImg.height), new Vector2(0.5f, 0.5f), 100.0f);
    }

    void ConvertToGrayscale()
    {
        Color32[] pixels = grayImg.GetPixels32();
        for (int x = 0; x < grayImg.width; x++)
        {
            for (int y = 0; y < grayImg.height; y++)
            {
                Color32 pixel = pixels[x + y * grayImg.width];
                int p = ((256 * 256 + pixel.r) * 256 + pixel.b) * 256 + pixel.g;
                int b = p % 256;
                p = Mathf.FloorToInt(p / 256);
                int g = p % 256;
                p = Mathf.FloorToInt(p / 256);
                int r = p % 256;
                float l = (0.2126f * r / 255f) + 0.7152f * (g / 255f) + 0.0722f * (b / 255f);
                Color c = new Color(l, l, l, 1);
                grayImg.SetPixel(x, y, c);
            }
        }

        helpImg.SetPixels(grayImg.GetPixels()); // for next step (black white image)
        helpImg.Apply();
    }

    void getCorrectPixelMouseClick(Vector2 dat)
    {
        Vector2 localCursor;
        var rect1 = GetComponent<RectTransform>();
        var pos1 = dat;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect1, pos1,
            null, out localCursor))
            return;

        int xpos = (int)(localCursor.x);
        int ypos = (int)(localCursor.y);

        if (xpos < 0) xpos = xpos + (int)rect1.rect.width / 2;
        else xpos += (int)rect1.rect.width / 2;

        if (ypos > 0) ypos = ypos + (int)rect1.rect.height / 2;
        else ypos += (int)rect1.rect.height / 2;

        if (xpos < 0 || ypos < 0 || xpos > rect1.sizeDelta.x || ypos > rect1.sizeDelta.y) return;
        int x = (int)(grayImg.width * (xpos / rect1.sizeDelta.x));
        int y = (int)(grayImg.height * (ypos / rect1.sizeDelta.y));

        if (grayImg.GetPixel(x, y) == new Color(0f, 0f, 0f, 1f)) return;

        helperFill = new Texture2D(helpImg.width, helpImg.height);
        helperFill.SetPixels(helpImg.GetPixels());
        helperFill.Apply();

        floodFill4Stack(new Color(177 / 255f, 221 / 255f, 112 / 255f, 255 / 255f), x, y);

        helpImg.Apply();
        gameObject.GetComponent<Image>().overrideSprite = Sprite.Create(helpImg, new Rect(0.0f, 0.0f, helpImg.width, helpImg.height), new Vector2(0.5f, 0.5f), 100.0f);
        Debug.Log("areas number:" + areas.Count);


    }

    public static readonly int[] dx = { 0, 1, 0, -1 }; // relative neighbor x coordinates
    public static readonly int[] dy = { -1, 0, 1, 0 }; // relative neighbor y coordinates

    //4-way floodfill using our own stack routines
    void floodFill4Stack(Color targetColor, int x, int y)
    {
        if (!CheckValidity(new Vector2(x, y), targetColor)) return; //avoid infinite loop
        int iterations = 0;
        HashSet<Vector2> hs = new HashSet<Vector2>();
        hs.Add(new Vector2(x, y));
        areas.Add(new List<Vector2>());
        pos++;
        areas[pos].Add(new Vector2(x, y));

        while (hs.Count > 0 && !this.flag)
        {

            Vector2 point = hs.ElementAt(hs.Count - 1);
            hs.Remove(point);
            helpImg.SetPixel((int)point.x, (int)point.y, targetColor);
            areas[pos].Add(new Vector2((int)point.x, (int)point.y));
            for (int i = 0; i < 4; i++)
            {
                int nx = (int)point.x + dx[i];
                int ny = (int)point.y + dy[i];
                if (CheckValidity(new Vector2(nx, ny), targetColor))
                {
                    hs.Add(new Vector2(nx, ny));
                }
            }
            iterations++;
        }
        if (this.flag)
        {
            areas.RemoveAt(pos);
            pos--;
            helpImg.SetPixels(helperFill.GetPixels());
        }
        Debug.Log("iterations:" + iterations);
    }

    bool CheckValidity(Vector2 p, Color TargetColor)
    {
        if (helpImg.GetPixel((int)p.x, (int)p.y) == TargetColor)
        {
            return false;
        }
        if (helpImg.GetPixel((int)p.x, (int)p.y) == new Color(0f, 0f, 0f, 1f))
        {
            return false;
        }
        if (p.x <= 0 || p.x >= helpImg.width - 1 || p.y <= 0 || p.y >= helpImg.height - 1)
        {
            this.flag = true;
            return false;
        }

        return true;
    }
}
