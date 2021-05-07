using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointList : List<Vector2> { }

[RequireComponent(typeof(Camera))]
public class CameraPainting : MonoBehaviour
{
    //paiting materials
    public Material material;
    public float brashSize = 30;
    public Color brashColor = Color.white;

    // All rendered points on screen
    List<PointList> drawingPoints = new List<PointList>();

    //Used to eliminate duplicate points
    HashSet<Vector2> allPoints = new HashSet<Vector2>();

    //Used to recover discard points
    Stack<PointList> discardedPoints = new Stack<PointList>();

    bool drawingDirty = true;

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            var pos = Input.mousePosition;
            if(allPoints.Add(pos))
            {
                PointList pl;
                if (!drawingDirty)
                    pl = drawingPoints[drawingPoints.Count - 1];
                else
                {
                    pl = new PointList();
                    drawingPoints.Add(pl);
                }

                pl.Add(pos);
                drawingDirty = false;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            drawingDirty = true;
        }
    }

    private void OnRenderObject()
    {
        material.SetPass(0);
        material.SetColor("_Color", brashColor);

        var e = drawingPoints.GetEnumerator();
        while (e.MoveNext())
        {
            var current = e.Current;
            Vector2 lastPos = current[0];
            for (int i = 0; i < current.Count; i++)
            {
                var position = current[i];
                Draw(lastPos, position);
                lastPos = position;
            }
        }
    }

    void Draw(Vector2 lastPos, Vector2 currentPos)
    {
        float distance = Vector2.Distance(lastPos, currentPos);
        if (distance == 0)
        {
            GLGraw(currentPos);
            return;
        }

        for (int i = 0; i < distance; i++)
        {
            float delta = (float)i / distance;
            var point = currentPos - lastPos;
            GLGraw(lastPos + point * delta);
        }

        GLGraw(currentPos);

    }

    void GLGraw(Vector2 point)
    {
        GL.PushMatrix();
        GL.LoadOrtho();

        GL.Begin(GL.QUADS);

        GL.TexCoord3(0, 0, 0);
        GL.Vertex3((point.x - brashSize / 2) / Screen.width, (point.y - brashSize / 2) / Screen.height, 0);

        GL.TexCoord3(0, 1, 0);
        GL.Vertex3((point.x + brashSize / 2) / Screen.width, (point.y - brashSize / 2) / Screen.height, 0);

        GL.TexCoord3(1, 1, 0);
        GL.Vertex3((point.x + brashSize / 2) / Screen.width, (point.y + brashSize / 2) / Screen.height, 0);

        GL.TexCoord3(1, 0, 0);
        GL.Vertex3((point.x - brashSize / 2) / Screen.width, (point.y + brashSize / 2) / Screen.height, 0);
        GL.End();
        GL.PopMatrix();
    }

    public void Clear()
    {
        allPoints.Clear();
        drawingPoints.Clear();
        GL.PushMatrix();
        GL.Clear(true, true, new Color(1, 1, 1, 0));
        GL.PopMatrix();
    }

    public void Cancel()
    {
        if (drawingPoints.Count == 0)
            return;

        var lastPoints = drawingPoints[drawingPoints.Count - 1];
        discardedPoints.Push(lastPoints);
        drawingPoints.RemoveAt(drawingPoints.Count - 1);
       
        GL.PushMatrix();
        GL.Clear(true, true, new Color(1, 1, 1, 0));
        GL.PopMatrix();
    }

    public void Recover()
    {
        if (discardedPoints.Count == 0)
            return;

        var lastPoints = discardedPoints.Pop();
        drawingPoints.Add(lastPoints);

        GL.PushMatrix();
        GL.Clear(true, true, new Color(1, 1, 1, 0));
        GL.PopMatrix();
    }
}
