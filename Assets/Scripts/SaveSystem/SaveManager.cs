using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public sealed class SaveManager : MonoBehaviour
{
	public static SaveManager Instance { get; private set; }

	[SerializeField] string fileName = "save.dat";
	[SerializeField] float autosaveDebounceSeconds = 1.0f;

	public SaveGame Current { get; private set; }

	string SavePath => Path.Combine(Application.persistentDataPath, fileName);
	string TempPath => SavePath + ".tmp";
	string BackupPath => SavePath + ".bak";

	byte[] masterKey32;

	bool dirty;
	float nextSaveTime;

	void Awake()
	{
		if (Instance && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);

		masterKey32 = GetOrCreateMasterKey();
		Current = LoadOrCreate();
	}

	void Update()
	{
		if (!dirty) return;
		if (Time.unscaledTime < nextSaveTime) return;

		SaveNow();
	}

	public static void MarkDirty()
	{
		if (Instance == null) return;
		Instance.dirty = true;
		Instance.nextSaveTime = Time.unscaledTime + Instance.autosaveDebounceSeconds;
	}

	public void SaveNow()
	{
		if (Current == null) return;

		dirty = false;

		Current.version = SaveGame.CurrentVersion;

		string json = JsonUtility.ToJson(Current);
		byte[] plain = Encoding.UTF8.GetBytes(json);

		byte[] blob = SaveCrypto.Protect(plain, masterKey32);
		AtomicWrite(blob);
	}

	public static void ResetSave(bool deleteFiles = false)
	{
		if (Instance == null) return;
		Instance.ResetInternal(deleteFiles);
	}

	public void ResetInternal(bool deleteFiles)
	{
		dirty = false;
		nextSaveTime = 0f;

		if (deleteFiles)
		{
			TryDelete(SavePath);
			TryDelete(BackupPath);
			TryDelete(TempPath);
		}

		var fresh = new SaveGame();
		fresh.Initialize();
		fresh.version = SaveGame.CurrentVersion;

		Current = fresh;
		SaveNow();
	}

	void TryDelete(string path)
	{
		try
		{
			if (File.Exists(path))
				File.Delete(path);
		}
		catch { }
	}

	SaveGame LoadOrCreate()
	{
		if (TryLoad(SavePath, out var save) || TryLoad(BackupPath, out save))
		{
			if (save.version > SaveGame.CurrentVersion)
				save.version = SaveGame.CurrentVersion;

			save.Initialize();
			return save;
		}

		var fresh = new SaveGame();
		fresh.Initialize();
		return fresh;
	}

	bool TryLoad(string path, out SaveGame save)
	{
		save = null;

		try
		{
			if (!File.Exists(path))
				return false;

			byte[] blob = File.ReadAllBytes(path);
			byte[] plain = SaveCrypto.Unprotect(blob, masterKey32);

			string json = Encoding.UTF8.GetString(plain);
			save = JsonUtility.FromJson<SaveGame>(json);
			return save != null;
		}
		catch
		{
			return false;
		}
	}

	void AtomicWrite(byte[] bytes)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(SavePath));

		File.WriteAllBytes(TempPath, bytes);
		using (var fs = new FileStream(TempPath, FileMode.Open, FileAccess.Read, FileShare.Read))
			fs.Flush(true);

		if (File.Exists(SavePath))
		{
			if (File.Exists(BackupPath))
				File.Delete(BackupPath);

			File.Move(SavePath, BackupPath);
		}

		if (File.Exists(SavePath))
			File.Delete(SavePath);

		File.Move(TempPath, SavePath);
	}

	byte[] GetOrCreateMasterKey()
	{
		const string prefKey = "SAVE_MASTER_KEY_B64";
		string b64 = PlayerPrefs.GetString(prefKey, "");

		if (!string.IsNullOrEmpty(b64))
			return Convert.FromBase64String(b64);

		byte[] k = new byte[32];
		RandomNumberGenerator.Fill(k);

		PlayerPrefs.SetString(prefKey, Convert.ToBase64String(k));
		PlayerPrefs.Save();

		return k;
	}

	void OnApplicationPause(bool pause)
	{
		if (pause) SaveNow();
	}

	void OnApplicationQuit()
	{
		SaveNow();
	}
}
