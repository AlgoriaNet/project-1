using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using utils;

public class PlayermaskManager : MonoBehaviour
{
    public void Start()
    {
        GameObject player1 = LoadPrefab.Load("ally/rangeally/r2");
        // player1.transform.SetParent(this.transform, true);
        // player1.transform.localPosition = new Vector3(0, -0.4870005f, 0); // 示例位置
        // player1.transform.localRotation = Quaternion.Euler(new Vector3(-90f, 0f, 0f)); // 默认旋转
        // player1.transform.localScale = Vector3.one; // 默认缩放
    }
    
}