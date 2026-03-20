using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

/// <summary>
/// 플레이어 데이터 저장/로드 (AES 암호화)
/// </summary>
public static class SaveDataManager
{
    // 암호화 키 (실제 프로덕션에서는 더 안전한 방법 사용)
    // 32 bytes for AES-256
    private static readonly byte[] EncryptionKey = Encoding.UTF8.GetBytes("3DQuarterViewRPG_SecretKey_2024!");
    // 16 bytes for IV
    private static readonly byte[] EncryptionIV = Encoding.UTF8.GetBytes("RPG_InitVector!!");

    private const string SAVE_FILE_NAME = "player_data.sav";
    private const string BACKUP_FILE_NAME = "player_data_backup.sav";

    private static string SavePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
    private static string BackupPath => Path.Combine(Application.persistentDataPath, BACKUP_FILE_NAME);

    /// <summary>
    /// 플레이어 데이터 저장
    /// </summary>
    public static bool Save(PlayerSaveData data)
    {
        if (data == null) return false;

        try
        {
            // 기존 파일 백업
            if (File.Exists(SavePath))
            {
                File.Copy(SavePath, BackupPath, true);
            }

            data.UpdateSaveTime();

            // JSON 직렬화
            string json = JsonUtility.ToJson(data, false);

            // 체크섬 추가
            string checksum = ComputeChecksum(json);
            string dataWithChecksum = checksum + "|" + json;

            // AES 암호화
            byte[] encrypted = Encrypt(dataWithChecksum);

            // Base64로 인코딩하여 저장
            string encoded = Convert.ToBase64String(encrypted);
            File.WriteAllText(SavePath, encoded);

            Debug.Log($"[SaveData] Saved to {SavePath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveData] Save failed: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 플레이어 데이터 로드
    /// </summary>
    public static PlayerSaveData Load()
    {
        // 메인 파일 시도
        var data = LoadFromFile(SavePath);
        if (data != null) return data;

        // 백업 파일 시도
        Debug.LogWarning("[SaveData] Main file failed, trying backup...");
        data = LoadFromFile(BackupPath);
        if (data != null)
        {
            // 백업에서 복구 성공 시 메인 파일로 복사
            Save(data);
            return data;
        }

        // 새 데이터 생성
        Debug.Log("[SaveData] No save file found, creating new data");
        return new PlayerSaveData();
    }

    private static PlayerSaveData LoadFromFile(string path)
    {
        if (!File.Exists(path)) return null;

        try
        {
            // 파일 읽기
            string encoded = File.ReadAllText(path);

            // Base64 디코딩
            byte[] encrypted = Convert.FromBase64String(encoded);

            // AES 복호화
            string dataWithChecksum = Decrypt(encrypted);

            // 체크섬 검증
            int separatorIndex = dataWithChecksum.IndexOf('|');
            if (separatorIndex <= 0)
            {
                Debug.LogError("[SaveData] Invalid file format (no checksum)");
                return null;
            }

            string storedChecksum = dataWithChecksum.Substring(0, separatorIndex);
            string json = dataWithChecksum.Substring(separatorIndex + 1);

            string computedChecksum = ComputeChecksum(json);
            if (storedChecksum != computedChecksum)
            {
                Debug.LogError("[SaveData] Checksum mismatch - data may be corrupted or tampered");
                return null;
            }

            // JSON 역직렬화
            PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);

            Debug.Log($"[SaveData] Loaded from {path}");
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveData] Load failed from {path}: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// 저장 파일 존재 여부
    /// </summary>
    public static bool SaveExists()
    {
        return File.Exists(SavePath) || File.Exists(BackupPath);
    }

    /// <summary>
    /// 저장 파일 삭제 (주의!)
    /// </summary>
    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
        if (File.Exists(BackupPath))
            File.Delete(BackupPath);

        Debug.Log("[SaveData] Save files deleted");
    }

    #region Encryption

    private static byte[] Encrypt(string plainText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = EncryptionKey;
            aes.IV = EncryptionIV;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
                    cs.Write(inputBytes, 0, inputBytes.Length);
                    cs.FlushFinalBlock();
                }
                return ms.ToArray();
            }
        }
    }

    private static string Decrypt(byte[] cipherBytes)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = EncryptionKey;
            aes.IV = EncryptionIV;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (MemoryStream ms = new MemoryStream(cipherBytes))
            {
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader sr = new StreamReader(cs, Encoding.UTF8))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }
    }

    private static string ComputeChecksum(string data)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 8; i++) // 앞 8바이트만 사용
            {
                sb.Append(bytes[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }

    #endregion

    #region Debug/Editor

#if UNITY_EDITOR
    /// <summary>
    /// 에디터 전용: 저장 파일 경로 열기
    /// </summary>
    [UnityEditor.MenuItem("Tools/Save Data/Open Save Folder")]
    public static void OpenSaveFolder()
    {
        string path = Application.persistentDataPath;
        System.Diagnostics.Process.Start(path);
    }

    /// <summary>
    /// 에디터 전용: 저장 파일 삭제
    /// </summary>
    [UnityEditor.MenuItem("Tools/Save Data/Delete Save")]
    public static void DeleteSaveMenuItem()
    {
        if (UnityEditor.EditorUtility.DisplayDialog("Delete Save", "정말 저장 파일을 삭제하시겠습니까?", "삭제", "취소"))
        {
            DeleteSave();
        }
    }
#endif

    #endregion
}
