// dllmain.cpp : Defines the entry point for the DLL application.
#include "stdafx.h"

#define DEBUG
typedef std::basic_string<TCHAR> tstring;

typedef VOID* (__fastcall *tEncryptString)(VOID* unk, const char* input_str, const char* key);
tEncryptString orig_EncryptString;

typedef VOID* (__fastcall *tDecryptString)(char** output_buf, char* input_str, const char* key);
tDecryptString orig_DecryptString;

DWORD EncryptBase64Address;
DWORD DecryptBase64Address;

typedef int(*tEVP_EncryptInit_ex)(VOID *ctx, const VOID *type, VOID *impl, unsigned char *key, unsigned char *iv);
typedef int(*tEVP_EncryptUpdate)(VOID *ctx, unsigned char *out, int *outl, unsigned char *in, int inl);
typedef int(*tEVP_EncryptFinal_ex)(VOID *ctx, unsigned char *out, int *outl);

typedef int(*tEVP_DecryptInit_ex)(VOID *ctx, const VOID *type, VOID *impl, unsigned char *key, unsigned char *iv);
typedef int(*tEVP_DecryptUpdate)(VOID *ctx, unsigned char *out, int *outl, unsigned char *in, int inl);
typedef int(*tEVP_DecryptFinal_ex)(VOID *ctx, unsigned char *outm, int *outl);

tEVP_EncryptInit_ex orig_EVP_EncryptInit_ex;
tEVP_EncryptUpdate orig_EVP_EncryptUpdate;
tEVP_EncryptFinal_ex orig_EVP_EncryptFinal_ex;
tEVP_DecryptInit_ex orig_EVP_DecryptInit_ex;
tEVP_DecryptUpdate orig_EVP_DecryptUpdate;
tEVP_DecryptFinal_ex orig_EVP_DecryptFinal_ex;

#pragma region SigScan

bool Compare(const BYTE* pData, const BYTE* bMask, const char* szMask)
{
	for (; *szMask; ++szMask, ++pData, ++bMask)
		if (*szMask == 'x' && *pData != *bMask)   return 0;
	return (*szMask) == NULL;
}

DWORD Pattern(DWORD dwAddress, DWORD dwLen, BYTE *bMask, char * szMask)
{
	for (DWORD i = 0; i<dwLen; i++)
		if (Compare((BYTE*)(dwAddress + i), bMask, szMask))  return (DWORD)(dwAddress + i);
	return 0;
}

#pragma endregion

bool ScanFunc()
{
	DWORD baseAddress = (DWORD)GetModuleHandleA(NULL);
	cout << "Scanning DecryptBase64String." << endl;
	DecryptBase64Address = Pattern(baseAddress, baseAddress + 0xBFFFFF,
		(PBYTE)"\x55\x8B\xEC\x6A\xFF\x68\x00\x00\x00\x00\x64\xA1\x00\x00\x00\x00\x50\x81\xEC\x00\x00\x00\x00\xA1\x00\x00\x00\x00\x33\xC5\x89\x45\xEC\x53\x56\x57\x50\x8D\x45\xF4\x64\xA3\x00\x00\x00\x00\x89\x95\x00\x00\x00\x00\x8B\xF1", 
		"xxxxxx????xx????xxx????x????xxxxxxxxxxxxxx????xx????xx");

	cout << "Scanning EncryptBase64String." << endl;
	EncryptBase64Address = Pattern(baseAddress, baseAddress + 0xBFFFFF,
		(PBYTE)"\x55\x8B\xEC\x6A\xFF\x68\x00\x00\x00\x00\x64\xA1\x00\x00\x00\x00\x50\x81\xEC\x00\x00\x00\x00\xA1\x00\x00\x00\x00\x33\xC5\x89\x45\xF0\x53\x56\x57\x50\x8D\x45\xF4\x64\xA3\x00\x00\x00\x00\x8B\xF2\x89\xB5\x00\x00\x00\x00",
		"xxxxxx????xx????xxx????x????xxxxxxxxxxxxxx????xxxx????");

	cout << std::hex;
	cout << "Encrypt: " << EncryptBase64Address << endl;
	cout << "Decrypt: " << DecryptBase64Address << endl;
	cout << std::dec;
	return EncryptBase64Address != 0 && DecryptBase64Address != 0;
}

VOID* __fastcall m_EncryptString(VOID* unk, char* input_str, const char* key)
{
	//VOID* result = orig_EncryptString(unk, input_str, key);
	cout << "EncryptString called" << endl;
	cout << "Key detected: " << key << endl;
	cout << "Input string: " << endl;
	cout << input_str << endl;
	return unk;
}

VOID* __fastcall m_DecryptString(char** output_buf, char* input_str, const char* key)
{
	//VOID* result = orig_DecryptString(output_buf, input_str, key);
	cout << "DecryptString called" << endl;
	cout << "Key detected: " << key << endl;
	cout << "Input string: " << endl;
	cout << input_str << endl;
	cout << "Decrypted string: " << endl;
	cout << &output_buf << endl;
	return output_buf;
}

int m_EVP_EncryptInit_ex(VOID *ctx, const VOID *type, VOID *impl, unsigned char *key, unsigned char *iv)
{
	Cout2VisualStudioDebugOutput c2v;
	cout << "Encrypt key: " << key << endl;
	return orig_EVP_EncryptInit_ex(ctx, type, impl, key, iv);
}
int m_EVP_EncryptUpdate(VOID *ctx, unsigned char *out, int *outl, unsigned char *in, int inl)
{
	Cout2VisualStudioDebugOutput c2v;
	return orig_EVP_EncryptUpdate(ctx, out, outl, in, inl);
}
int m_EVP_EncryptFinal_ex(VOID *ctx, unsigned char *out, int *outl)
{
	Cout2VisualStudioDebugOutput c2v;
	return orig_EVP_EncryptFinal_ex(ctx, out, outl);
}
int m_EVP_DecryptInit_ex(VOID *ctx, const VOID *type, VOID *impl, unsigned char *key, unsigned char *iv)
{
	Cout2VisualStudioDebugOutput c2v;
	cout << "Decrypt key: " << key << endl;
	return orig_EVP_DecryptInit_ex(ctx, type, impl, key, iv);
}
int m_EVP_DecryptUpdate(VOID *ctx, unsigned char *out, int *outl, unsigned char *in, int inl)
{
	Cout2VisualStudioDebugOutput c2v;
	return orig_EVP_DecryptUpdate(ctx, out, outl, in, inl);
}
int m_EVP_DecryptFinal_ex(VOID *ctx, unsigned char *outm, int *outl)
{
	Cout2VisualStudioDebugOutput c2v;
	return orig_EVP_DecryptFinal_ex(ctx, outm, outl);
}

void HookFunctions()
{
	cout << "Beginning hook" << endl;
	OutputDebugString(L"Beginning hook\n");
//#if defined(DEBUG)
//	if (AllocConsole()) {
//		freopen("CONIN$", "rb", stdin);
//		freopen("CONOUT$", "wb", stdout);
//		freopen("CONOUT$", "wb", stderr);
//
//		std::ios::sync_with_stdio();
//	}
//#endif
	if (MH_Initialize() != MH_OK)
	{
		return;
	}
	if (ScanFunc())
	{
		/*if (MH_CreateHook(reinterpret_cast<LPVOID>(EncryptBase64Address), m_EncryptString, reinterpret_cast<LPVOID*>(&orig_EncryptString)) != MH_OK)
		{
			return;
		}
		if (MH_CreateHook(reinterpret_cast<LPVOID>(DecryptBase64Address), m_DecryptString, reinterpret_cast<LPVOID*>(&orig_DecryptString)) != MH_OK)
		{
			return;
		}*/
		//orig_EncryptString = (tEncryptString) DetourFunction((PBYTE)EncryptBase64Address, (PBYTE)m_EncryptString);
		//orig_DecryptString = (tDecryptString) DetourFunction((PBYTE)DecryptBase64Address, (PBYTE)m_DecryptString);
	}
	if (MH_CreateHookApiEx(L"libeay32.dll", "EVP_DecryptInit_ex", m_EVP_DecryptInit_ex, reinterpret_cast<LPVOID*>(&orig_EVP_DecryptInit_ex), NULL) != MH_OK)
	{
		return;
	}
	if (MH_CreateHookApiEx(L"libeay32.dll", "EVP_EncryptInit_ex", m_EVP_EncryptInit_ex, reinterpret_cast<LPVOID*>(&orig_EVP_EncryptInit_ex), NULL) != MH_OK)
	{
		return;
	}
	if (MH_CreateHookApiEx(L"libeay32.dll", "EVP_EncryptFinal_ex", m_EVP_EncryptFinal_ex, reinterpret_cast<LPVOID*>(&orig_EVP_EncryptFinal_ex), NULL) != MH_OK)
	{
		return;
	}
	if (MH_CreateHookApiEx(L"libeay32.dll", "EVP_EncryptUpdate", m_EVP_EncryptUpdate, reinterpret_cast<LPVOID*>(&orig_EVP_EncryptUpdate), NULL) != MH_OK)
	{
		return;
	}
	if (MH_CreateHookApiEx(L"libeay32.dll", "EVP_DecryptFinal_ex", m_EVP_DecryptFinal_ex, reinterpret_cast<LPVOID*>(&orig_EVP_DecryptFinal_ex), NULL) != MH_OK)
	{
		return;
	}
	if (MH_CreateHookApiEx(L"libeay32.dll", "EVP_DecryptUpdate", m_EVP_DecryptUpdate, reinterpret_cast<LPVOID*>(&orig_EVP_DecryptUpdate), NULL) != MH_OK)
	{
		return;
	}

	if (MH_EnableHook(MH_ALL_HOOKS) != MH_OK)
	{
		cout << "Failure to enable hooks" << endl;
	}
}

void UnHookFunctions()
{
	//DetourRemove((PBYTE)orig_PutPacketQueue, (PBYTE)m_PutPacketQueue);
	//DetourRemove((PBYTE)orig_AgentInteract, (PBYTE)m_AgentInteract);
	FreeConsole();
}


BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
					 )
{
	Cout2VisualStudioDebugOutput c2v;
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
		OutputDebugString(L"DLL injection successful.\n");
		HookFunctions();
		break;
	case DLL_PROCESS_DETACH:
		OutputDebugString(L"DLL detaching from process, unhooking...\n");
		UnHookFunctions();
		break;
	}
	return TRUE;
}

