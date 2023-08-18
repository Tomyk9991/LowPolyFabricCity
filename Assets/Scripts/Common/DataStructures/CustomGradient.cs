using System.Collections.Generic;
using UnityEngine;

namespace Common.DataStructures
{
    public class CustomGradient
    {
        List<GradientColorKey> keys;

        public CustomGradient()
        {
            keys = new List<GradientColorKey>();
        }

        public int Count => keys.Count;

        public GradientColorKey this[int index]
        {
            get => keys[index];
            set
            {
                keys[index] = value;
                SortKeys();
            }
        }
        
        public void AddKey(GradientColorKey key)
        {
            keys.Add(key);
            SortKeys();
        }

        public void InsertKey(int index, float t, Color color)
            => InsertKey(index, new GradientColorKey(color, t));

        public void InsertKey(int index, GradientColorKey key)
        {
            keys.Insert(index, key);
            SortKeys();
        }

        public void RemoveKey(int index)
        {
            keys.RemoveAt(index);
            SortKeys();
        }

        public void RemoveInRange(float min, float max)
        {
            for (int i = keys.Count - 1; i >= 0; i--)
                if (keys[i].time >= min && keys[i].time <= max)
                    keys.RemoveAt(i);
            SortKeys();
        }

        public void Clear() => keys.Clear();

        void SortKeys() => keys.Sort((a, b) => a.time.CompareTo(b.time));

        (int l, int r) getNeighborKeys(float t)
        {
            var l = Count - 1;

            for (int i = 0; i <= l; i++)
            {
                if (keys[i].time >= t)
                {
                    if (i == 0) return (-1, i);
                    return (i - 1, i);
                }
            }

            return (l, -1);
        }

        public Color Evaluate(float t)
        {
            if (Count == 0) return new Color(0f, 0f, 0f, 0f);

            var neighbour = getNeighborKeys(t);

            if (neighbour.l < 0) return keys[neighbour.r].color;
            else if (neighbour.r < 0) return keys[neighbour.l].color;


            return keys[neighbour.l].color;
            // return Color.Lerp(
            //     keys[neighbour.l].color,
            //     keys[neighbour.r].color,
            //     Mathf.InverseLerp(keys[neighbour.l].time, keys[neighbour.r].time, t)
            // );
        }
    }
}