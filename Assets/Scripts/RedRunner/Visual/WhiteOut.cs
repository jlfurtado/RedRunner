using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RedRunner.Characters;

[RequireComponent(typeof(SpriteRenderer))]

public class WhiteOut : MonoBehaviour {

    private SpriteRenderer m_spriteRenderer;

    private void Start()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void PerformWhiteOut(Character character)
    {
        float cameraWidth = Camera.main.scaledPixelWidth;
        float cameraHeight = Camera.main.scaledPixelHeight;
        m_spriteRenderer.size = new Vector2(cameraWidth, cameraHeight);
        m_spriteRenderer.color = new Color(1, 1, 1, character.PercentFrozen);
    }
}
