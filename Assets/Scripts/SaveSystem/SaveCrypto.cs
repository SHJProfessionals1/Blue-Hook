using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class SaveCrypto
{
	const int Magic = 0x53415645;
	const byte Format = 1;

	public static byte[] Protect(byte[] plain, byte[] masterKey32)
	{
		byte[] encKey = DeriveKey(masterKey32, "enc");
		byte[] macKey = DeriveKey(masterKey32, "mac");

		using var aes = Aes.Create();
		aes.Key = encKey;
		aes.Mode = CipherMode.CBC;
		aes.Padding = PaddingMode.PKCS7;
		aes.GenerateIV();

		byte[] cipher;
		using (var enc = aes.CreateEncryptor())
			cipher = enc.TransformFinalBlock(plain, 0, plain.Length);

		byte[] header = BuildHeader(aes.IV, cipher);
		byte[] mac = HmacSha256(macKey, header);

		using var ms = new MemoryStream();
		using var bw = new BinaryWriter(ms);
		bw.Write(header);
		bw.Write((ushort)mac.Length);
		bw.Write(mac);
		return ms.ToArray();
	}

	public static byte[] Unprotect(byte[] blob, byte[] masterKey32)
	{
		byte[] encKey = DeriveKey(masterKey32, "enc");
		byte[] macKey = DeriveKey(masterKey32, "mac");

		using var ms = new MemoryStream(blob);
		using var br = new BinaryReader(ms);

		if (br.ReadInt32() != Magic) throw new CryptographicException("Bad magic");
		if (br.ReadByte() != Format) throw new CryptographicException("Bad format");

		int ivLen = br.ReadUInt16();
		int cipherLen = br.ReadInt32();

		byte[] iv = br.ReadBytes(ivLen);
		byte[] cipher = br.ReadBytes(cipherLen);

		int macLen = br.ReadUInt16();
		byte[] mac = br.ReadBytes(macLen);

		byte[] header = BuildHeader(iv, cipher);
		byte[] expected = HmacSha256(macKey, header);

		if (!FixedTimeEquals(mac, expected))
			throw new CryptographicException("Tampered");

		using var aes = Aes.Create();
		aes.Key = encKey;
		aes.IV = iv;
		aes.Mode = CipherMode.CBC;
		aes.Padding = PaddingMode.PKCS7;

		using var dec = aes.CreateDecryptor();
		return dec.TransformFinalBlock(cipher, 0, cipher.Length);
	}

	static byte[] BuildHeader(byte[] iv, byte[] cipher)
	{
		using var ms = new MemoryStream();
		using var bw = new BinaryWriter(ms);
		bw.Write(Magic);
		bw.Write(Format);
		bw.Write((ushort)iv.Length);
		bw.Write(cipher.Length);
		bw.Write(iv);
		bw.Write(cipher);
		return ms.ToArray();
	}

	static byte[] DeriveKey(byte[] masterKey32, string purpose)
	{
		byte[] info = Encoding.UTF8.GetBytes(purpose);
		using var h = new HMACSHA256(masterKey32);
		return h.ComputeHash(info);
	}

	static byte[] HmacSha256(byte[] key, byte[] data)
	{
		using var h = new HMACSHA256(key);
		return h.ComputeHash(data);
	}

	static bool FixedTimeEquals(byte[] a, byte[] b)
	{
		if (a == null || b == null || a.Length != b.Length) return false;
		int diff = 0;
		for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
		return diff == 0;
	}
}
