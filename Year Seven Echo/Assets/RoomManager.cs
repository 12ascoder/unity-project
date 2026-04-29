using UnityEngine;
using System.Collections.Generic;

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

    // 按生成顺序记录所有房间
    private List<GameObject> spawnedRoomList = new List<GameObject>();

    private Texture2D blackTex;
    private float alpha = 0f;
    private bool isFading = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

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
        if (Input.GetKeyDown(KeyCode.F) && !isFading)
        {
            GoNextLevel();
        }

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

        // 核心规则：永远只保留【最新2个房间】，多余的最旧房间删掉
        // 每进下一关，先删最旧的，保证最多只留两间
        while (spawnedRoomList.Count >= 2)
        {
            GameObject oldRoom = spawnedRoomList[0];
            if (oldRoom != null) Destroy(oldRoom);
            spawnedRoomList.RemoveAt(0);
        }

        // 生成下一关
        SpawnRoomByLevel(next);
    }

    // 失败：黑屏渐入 → 清所有房间 → 重生1号 → 传送 → 渐出
    System.Collections.IEnumerator FadeAndTeleport()
    {
        isFading = true;

        float t = 0;
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            alpha = Mathf.Lerp(0, 1, t / fadeInTime);
            yield return null;
        }
        alpha = 1;

        // 销毁全部房间
        foreach (var room in spawnedRoomList)
        {
            if (room != null) Destroy(room);
        }
        spawnedRoomList.Clear();

        // 重新生成1号房
        currentLevel = 1;
        SpawnRoomByLevel(1);

        // 传送到指定点位
        if (player != null && firstRoomSpawnPoint != null)
        {
            player.position = firstRoomSpawnPoint.position;
            player.rotation = firstRoomSpawnPoint.rotation;
        }

        // 黑屏淡出
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

    void OnGUI()
    {
        if (alpha <= 0) return;
        GUI.color = new Color(0, 0, 0, alpha);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTex);
    }
}