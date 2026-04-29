using UnityEngine;
using System.Collections.Generic;
using System;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    [Header("按顺序放入房间预制体 1、2、3...")]
    public GameObject[] roomPrefabs;

    [Header("玩家")]
    public Transform player;

    [Header("失败传送位置（1号房固定点）")]
    public Transform firstRoomSpawnPoint;

    [Header("黑屏渐变时间")]
    public float fadeInTime = 1.5f;
    public float fadeOutTime = 1.5f;

    [Header("当前关卡")]
    public int currentLevel = 1;

    private List<GameObject> spawnedRoomList = new List<GameObject>();
    private Texture2D blackTex;
    private float alpha = 0f;
    private bool isFading = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 初始化黑屏贴图
        blackTex = new Texture2D(1, 1);
        blackTex.SetPixel(0, 0, Color.black);
        blackTex.Apply();
    }

    void Start()
    {
        SpawnRoomByLevel(1);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            GoNextLevel();
        }

        // 按R 触发带黑屏传送
        if (Input.GetKeyDown(KeyCode.R) && !isFading)
        {
            StartCoroutine(FadeAndTeleport());
        }
    }

    void SpawnRoomByLevel(int level)
    {
        if (level < 1 || level > roomPrefabs.Length) return;

        int idx = level - 1;
        GameObject newRoom = Instantiate(roomPrefabs[idx]);
        spawnedRoomList.Add(newRoom);
        currentLevel = level;
    }

    void GoNextLevel()
    {
        int next = currentLevel + 1;
        if (next > roomPrefabs.Length)
        {
            Debug.Log("已经是最后一关");
            return;
        }
        SpawnRoomByLevel(next);
    }

    // 黑屏渐入 -> 传送 -> 黑屏淡出
    System.Collections.IEnumerator FadeAndTeleport()
    {
        isFading = true;

        // 1. 渐入黑屏 1.5秒
        float t = 0;
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            alpha = Mathf.Lerp(0, 1, t / fadeInTime);
            yield return null;
        }
        alpha = 1;

        // 2. 清空房间 + 重建1号房
        foreach (var room in spawnedRoomList)
            Destroy(room);
        spawnedRoomList.Clear();
        SpawnRoomByLevel(1);

        // 3. 传送玩家到指定Pos
        if (player != null && firstRoomSpawnPoint != null)
        {
            player.position = firstRoomSpawnPoint.position;
            player.rotation = firstRoomSpawnPoint.rotation;
        }

        // 4. 渐出黑屏 1.5秒
        t = 0;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            alpha = Mathf.Lerp(1, 0, t / fadeOutTime);
            yield return null;
        }
        alpha = 0;

        isFading = false;
    }

    // 绘制全屏黑幕
    void OnGUI()
    {
        if (alpha <= 0) return;
        GUI.color = new Color(0, 0, 0, alpha);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTex);
    }
}