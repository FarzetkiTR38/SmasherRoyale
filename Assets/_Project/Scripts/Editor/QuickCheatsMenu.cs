using UnityEngine;
using UnityEditor;
using System.IO;
using RoyalSmash.Save;
using RoyalSmash.Lives;
using RoyalSmash.Economy;

namespace RoyalSmash.EditorTools
{
    public static class QuickCheatsMenu
    {
        [MenuItem("Tools/RoyalSmash/💖 Refill Lives (5 Can Ver)")]
        public static void RefillLives()
        {
            // 1. If in play mode and LivesManager is active
            if (Application.isPlaying && LivesManager.Instance != null)
            {
                LivesManager.Instance.RefillLives();
            }

            // 2. Direct save file modification (works in both Edit and Play Mode)
            string savePath = Path.Combine(Application.persistentDataPath, "player_save.json");
            string backupPath = Path.Combine(Application.persistentDataPath, "player_save_backup.json");

            PlayerSaveData data = null;
            if (File.Exists(savePath))
            {
                try
                {
                    data = JsonUtility.FromJson<PlayerSaveData>(File.ReadAllText(savePath));
                }
                catch { }
            }

            if (data == null) data = new PlayerSaveData();

            data.Lives = 5;
            data.WasInActiveLevel = false;
            data.LifeTimerStartTicks = 0;

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
            if (File.Exists(backupPath)) File.WriteAllText(backupPath, json);

            if (SaveManager.HasInstance && SaveManager.Instance.Data != null)
            {
                SaveManager.Instance.Data.Lives = 5;
                SaveManager.Instance.Data.WasInActiveLevel = false;
            }

            Debug.Log("[QuickCheats] ❤️ Canlar 5/5 olarak yenilendi! Kayıt dosyası güncellendi.");
            EditorUtility.DisplayDialog("Royal Smash", "❤️ Canların 5/5 olarak yenilendi! Artık oyunu BootScene'den veya menüden rahatça başlatabilirsin.", "Tamam");
        }

        [MenuItem("Tools/RoyalSmash/🪙 Give 10,000 Gold (+10k Altın)")]
        public static void GiveGold()
        {
            if (Application.isPlaying && GoldManager.Instance != null)
            {
                GoldManager.Instance.AddGold(10000, "CheatMenu");
            }

            string savePath = Path.Combine(Application.persistentDataPath, "player_save.json");
            string backupPath = Path.Combine(Application.persistentDataPath, "player_save_backup.json");

            PlayerSaveData data = null;
            if (File.Exists(savePath))
            {
                try { data = JsonUtility.FromJson<PlayerSaveData>(File.ReadAllText(savePath)); } catch { }
            }

            if (data == null) data = new PlayerSaveData();
            data.Gold += 10000;

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
            if (File.Exists(backupPath)) File.WriteAllText(backupPath, json);

            if (SaveManager.HasInstance && SaveManager.Instance.Data != null)
            {
                SaveManager.Instance.Data.Gold = data.Gold;
            }

            Debug.Log($"[QuickCheats] 🪙 10,000 Altın eklendi! Toplam: {data.Gold}");
            EditorUtility.DisplayDialog("Royal Smash", $"🪙 10,000 Altın eklendi!\nGüncel Toplam: {data.Gold}", "Tamam");
        }
    }
}
