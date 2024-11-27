using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AnimatedImage : MonoBehaviour
{
    public Image imageComponent;
    public Sprite[] sprites;  // 所有的帧图像
    private int currentFrame = 0;

    void Start()
    {
        StartCoroutine(PlayAnimation());
    }

    IEnumerator PlayAnimation()
    {
        while (true)
        {
            imageComponent.sprite = sprites[currentFrame];
            currentFrame = (currentFrame + 1) % sprites.Length;
            yield return new WaitForSeconds(0.1f);  // 设置帧速率
        }
    }
}