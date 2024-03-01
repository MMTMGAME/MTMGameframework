using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class AutoRunDirection : MonoBehaviour
{
    [System.Serializable]
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
        SwitchLine
    }

    public Direction direction;
    [ShowIf("UseLineIndex")]
    public int lineIndex;

    bool UseLineIndex()
    {
        return direction == Direction.SwitchLine;
    }
}
