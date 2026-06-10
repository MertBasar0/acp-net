# Spike 006: Real Gemini ACP Dogfooding

## Amac

Acp.Net Process runner'in fake agent disinda gercek bir ACP agent ile deger uretip uretmedigini test etmek.

Kullanilacak agent:

- Gemini CLI
- Komut: `gemini --acp`

Claude bu spike'ta kullanilmayacak.

## Kapsam

Sample:

`src/samples/acp-process-with-gemini/`

Akis:

1. `AcpProcessRunner` Gemini CLI'yi WSL uzerinden baslatir.
2. `AgentClientProtocol` ile `initialize` gonderilir.
3. `session/new` gonderilir.
4. Kisa bir prompt gonderilir.
5. Transcript kaydedilir.

## Kabul Kriterleri

Spike tamamlanmis sayilmasi icin:

1. Gemini process runner ile baslatilir.
2. `initialize` basarili olur veya hata transcript ile net gorulur.
3. Prompt basarili olursa cevap ve stop reason kaydedilir.
4. Prompt basarisiz olursa root cause transcript/stderr ile raporlanir.
5. Runner API eksikleri listelenir.

## Dikkat

Bu spike gercek Gemini kotasi kullanabilir. Bu nedenle default `dotnet test` icine eklenmemelidir.

## Ilk Bulgu

Ilk canli deneme `initialize` seviyesini gecti, fakat `session/new` icin gonderilen `cwd` Windows UNC path olarak kaldigi icin Gemini WSL icinde su hata ile durdu:

```text
Directory does not exist: /home/.../\\\\wsl.localhost\\Ubuntu\\...
```

Bu, Acp.Net.Process icin onemli bir API gereksinimi dogurdu:

- process baslatma path'i kadar ACP payload icindeki path'ler de agent runtime'a gore normalize edilmeli.

Bu nedenle `AcpProcessSession.ToAgentPath(...)` eklendi ve sample buna gore guncellendi.

## Sonuc

Dogfood basarili tamamlandi. Detayli sonuc:

`docs/spikes/006-real-gemini-acp-dogfooding-result.md`
