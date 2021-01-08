using UnityEngine;

/// <summary>
/// Class for drawing a generated Crosshair on the screen.
/// </summary>
public class Crosshair : MonoBehaviour
{
    private Texture2D lineTex;
    private Texture2D dotTex;
    private GUIStyle lineStyle;
    private GUIStyle dotStyle;

    public bool drawCrosshair = true;
    public bool drawNorth     = true;
    public bool drawEast      = true;
    public bool drawSouth     = true;
    public bool drawWest      = true;
    public bool drawDot;
    private bool drawOutline => outline != 0;
    
    public Color color = Color.white;
    public Color dotColor = Color.white;
    private readonly Color outlineColor = Color.black;
    
    [Range(-250,250)] public int thickness = 1;
    [Range(-250,250)] public int size = 3;
    [Range(-250,250)] public int gap = 0;
    [Range(0, 3)]     public int outline;
    [Range(0,1)]      public float alpha = 1;
    [Range(0,1)]      public float outlineAlpha = 1;
    private const int defaultGap = 3;
    private int totalGap => defaultGap + gap;

    private Rect dotRect;
    private Rect northRect;
    private Rect eastRect;
    private Rect southRect;
    private Rect westRect;

    private int northY;
    private int eastX;
    private int southY;
    private int westX;

    private int centerX;
    private int centerY;

    private Outline[] outlines;
    
    private void Awake()
    {
        outlines = new Outline[5];
        
        lineTex = new Texture2D(1, 1);
        lineStyle = new GUIStyle {normal = {background = lineTex}};

        dotTex = new Texture2D(1, 1);
        dotStyle = new GUIStyle {normal = {background = dotTex}};

        centerX = Mathf.CeilToInt(Screen.width * .5f);
        centerY = Mathf.CeilToInt(Screen.height * .5f);

        CalculateValues();
    }

    private void OnValidate()
    {
        CalculateValues();
    }
     
     private void OnGUI ()
     {
         if (drawCrosshair)
         {
             if (drawDot)
             {
                 GUI.Box(dotRect, GUIContent.none, dotStyle);
                 if (drawOutline)
                    outlines[0]?.Draw();
             }

             if (drawNorth)
             {
                 GUI.Box(northRect, GUIContent.none, lineStyle);
                 if (drawOutline)
                    outlines[1]?.Draw();
             }

             if (drawEast)
             {
                 GUI.Box(eastRect, GUIContent.none, lineStyle);
                 if (drawOutline)
                     outlines[2]?.Draw();
             }

             if (drawSouth)
             {
                 GUI.Box(southRect, GUIContent.none, lineStyle);
                 if (drawOutline)
                     outlines[3]?.Draw();
             }

             if (drawWest)
             {
                 GUI.Box(westRect, GUIContent.none, lineStyle);
                 if (drawOutline)
                     outlines[4]?.Draw();
             }
         }
     }
     
     private void CalculateValues()
     {
         if (lineTex != null)
         {
             // Set hair color
             Color clr = new Color(color.r, color.g, color.b, alpha);
             lineStyle.normal.background.SetPixel(0,0,clr);
             lineStyle.normal.background.Apply();
         }
         
         if (dotTex != null)
         {
             // Set dot color
             Color clr = new Color(dotColor.r, dotColor.g, dotColor.b, alpha);
             dotStyle.normal.background.SetPixel(0,0,clr);
             dotStyle.normal.background.Apply();
         }

         // calc offset due to thickness
         int offset = thickness == 1 ? 0 : Mathf.CeilToInt(thickness * .5f) - 1;
         int dotPosX = centerX - offset;
         int dotPosY = centerY - offset;
         
         // calc positions of the 'hairs'
         northY = dotPosY - totalGap - (size - 1);
         eastX = dotPosX - totalGap - (size - 1);
         southY = dotPosY + totalGap + (thickness - 1);
         westX = dotPosX + totalGap + (thickness - 1);

         // init the rectangles
         dotRect   = new Rect(dotPosX, dotPosY, thickness, thickness);
         northRect = new Rect(dotPosX, northY,  thickness, size);
         eastRect  = new Rect(eastX,   dotPosY, size,      thickness);
         southRect = new Rect(dotPosX, southY,  thickness, size);
         westRect  = new Rect(westX,   dotPosY, size,      thickness);

         if (outline != 0 && outlines != null)
         {
             Outline.outlineStyle.normal.background.SetPixel(0,0, new Color(0, 0, 0, outlineAlpha));
             Outline.outlineStyle.normal.background.Apply();
             
             if (drawDot)
                 outlines[0] = new Outline(dotRect, OutlineType.Dot, thickness, size, outline);
             if (drawNorth)
                 outlines[1] = new Outline(northRect, OutlineType.Vertical, thickness, size, outline);
             if (drawEast)
                 outlines[2] = new Outline(eastRect, OutlineType.Horizontal, thickness, size, outline);
             if (drawSouth)
                 outlines[3] = new Outline(southRect, OutlineType.Vertical, thickness, size, outline);
             if (drawWest)
                 outlines[4] = new Outline(westRect, OutlineType.Horizontal, thickness, size, outline);
         }
     }
}

public class Outline
{
    public static GUIStyle outlineStyle { get; }
    
    private Rect top;
    private Rect right;
    private Rect bot;
    private Rect left;
    private readonly int offset;
    private readonly int outlineWidth;

    static Outline()
    {
        Texture2D tex = new Texture2D(1,1);
        outlineStyle = new GUIStyle {normal = {background = tex}};
    }

    public Outline(Rect rect, OutlineType type, int chThickness, int chSize, int olThickness)
    {
        offset = olThickness - 1;
        outlineWidth = olThickness;
        
        switch (type)
        {
            case OutlineType.Dot:
                InitDot(rect, chThickness);
                break;
            case OutlineType.Horizontal:
                InitHorizontal(rect, chSize, chThickness);
                break;
            case OutlineType.Vertical:
                InitVertical(rect, chSize, chThickness);
                break;
        }
    }

    public void Draw()
    {
        GUI.Box(top, GUIContent.none, outlineStyle);
        GUI.Box(right, GUIContent.none, outlineStyle);
        GUI.Box(bot, GUIContent.none, outlineStyle);
        GUI.Box(left, GUIContent.none, outlineStyle);
    }
    
    private void InitDot(Rect rect, int thickness)
    {
        top   = new Rect(rect.x, rect.y - (1 + offset), thickness, outlineWidth);
        right = new Rect(rect.x + thickness, rect.y, outlineWidth, thickness);
        bot   = new Rect(rect.x, rect.y + thickness, thickness, outlineWidth);
        left  = new Rect(rect.x - (1 + offset), rect.y, outlineWidth, thickness);
    }

    private void InitHorizontal(Rect rect, int chSize, int chThickness)
    {
        top   = new Rect(rect.x, rect.y - (1 + offset), chSize, outlineWidth);
        right = new Rect(rect.x + chSize, rect.y, outlineWidth, chThickness);
        bot   = new Rect(rect.x, rect.y + chThickness, chSize, outlineWidth);
        left  = new Rect(rect.x - (1 + offset), rect.y, outlineWidth, chThickness);
    }
    
    private void InitVertical(Rect rect, int chSize, int chThickness)
    {
        top   = new Rect(rect.x, rect.y - (1 + offset), chThickness, outlineWidth);
        right = new Rect(rect.x + chThickness, rect.y, outlineWidth, chSize);
        bot   = new Rect(rect.x, rect.y + chSize, chThickness, outlineWidth);
        left  = new Rect(rect.x - (1 + offset), rect.y, outlineWidth, chSize);
    }
}

public enum OutlineType
{
    Dot,
    Vertical,
    Horizontal
}