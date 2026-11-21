using System.IO;
using UnityEngine;

// 저장 파일 읽고 쓰기
public static class SaveManager
{
    private static string PermanentPath => Application.persistentDataPath + "/permanent.json"; //Uniyt가 자체적으로 만들어주는 파일
    //그래서 경로가 C:\Users\user\AppData\LocalLow\TeamHero\The Heros Party\permanent.json 이곳으로 저장

    public static void SavePermanent(PermanentData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(PermanentPath, json);
    }

    // 저장 파일 없으면 null
    public static PermanentData LoadPermanent()
    {
        if (!File.Exists(PermanentPath)) return null;

        string json = File.ReadAllText(PermanentPath);
        return JsonUtility.FromJson<PermanentData>(json);
    }
}
