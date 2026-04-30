using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GizmoHelper : SingletonMonoBehaviour<GizmoHelper>
{
    private static Dictionary<string, LineGizmos> rays = new();
    private static Dictionary<string, LineGizmos> boxes = new();

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnDrawGizmos()
    {
        foreach (var r in rays.Values)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(r._s, r._e);
        }

        foreach (var b in boxes.Values)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(b._s, b._e);
        }
    }

    public void SetLine(string key, Vector2 start, Vector2 end)
    {
        if (rays.ContainsKey(key))
            rays[key].SetValue(start, end);
        else
        {
            rays[key] = new LineGizmos(key, start, end);
            StartCoroutine(rays[key].StartTimer());
        }
    }

    public void SetBox(string key, Vector2 pos, Vector2 size, Color color)
    {
        if (boxes.ContainsKey(key))
            boxes[key].SetValue(pos, size);
        boxes[key] = new LineGizmos(key, pos, size);
    }

    private struct LineGizmos
    {
        public Vector2 _s;
        public Vector2 _e;
        private float timer;
        private float current_timer;
        private string _key;

        public LineGizmos(string key, Vector2 s, Vector2 e)
        {
            _key = key;
            timer = 3.0f;
            _s = s;
            _e = e;
            current_timer = 0.0f;
        }

        public void SetValue(Vector2 s, Vector2 e)
        {
            _s = s;
            _e = e;
            current_timer = 0.0f;
        }

        public IEnumerator StartTimer()
        {
            while (timer > current_timer)
            {
                current_timer += Time.deltaTime;
                yield return null;
            }

            rays.Remove(_key);
        }
    }
}