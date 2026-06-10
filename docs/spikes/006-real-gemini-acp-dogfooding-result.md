# Spike 006 Sonuc Raporu: Real Gemini ACP Dogfooding

Tarih: 2026-06-07

## Ozet

Spike basarili.

Claude kullanilmadi. Gercek agent olarak sadece Gemini CLI kullanildi.

Gemini CLI ACP mode, `AcpProcessRunner` ile WSL uzerinden baslatildi ve `AgentClientProtocol` C# client'i ile su akislar calisti:

- `initialize`
- `session/new`
- `session/prompt`
- streaming `session/update`
- graceful process exit

## Kullanilan Agent

Gemini CLI:

```text
/home/mertb/.nvm/versions/node/v22.22.2/bin/gemini
```

Surum:

```text
0.45.2
```

ACP bayragi:

```text
--acp
```

Not: `--experimental-acp` hala var ama deprecated; `--acp` kullanildi.

## Uretilen Sample

Kod:

`src/samples/acp-process-with-gemini/`

Ana dosya:

`src/samples/acp-process-with-gemini/Program.cs`

Komut:

```bash
dotnet run --project '\\wsl.localhost\Ubuntu\home\mertb\.openclaw\workspace\acp-net-training-factory\src\samples\acp-process-with-gemini\acp-process-with-gemini.csproj'
```

## Ilk Deneme Bulgusu

Ilk denemede `initialize` basarili oldu, fakat `session/new` icin gonderilen `cwd` Windows UNC path olarak kaldigi icin Gemini WSL icinde hata verdi:

```text
Directory does not exist: /home/.../\\\\wsl.localhost\\Ubuntu\\...
```

Bu dogfood, Acp.Net.Process icin gercek bir API eksigini ortaya cikardi:

- Process path mapping yetmez.
- ACP payload icindeki path alanlari da agent runtime'a gore normalize edilmeli.

Bu nedenle `AcpProcessSession.ToAgentPath(string path)` eklendi ve sample/test kodu bu API'yi kullanacak sekilde guncellendi.

## Basarili Deneme

Ikinci deneme basarili oldu.

Cikti:

```text
protocol=1
usesWsl=True
session=7e03970c-6ce6-4380-b9e0-eb667b0e627d
agentCwd=/home/mertb/.openclaw/workspace/acp-net-training-factory/src/samples/acp-process-with-gemini
ACP-DOGFOOD-OK
stopReason=EndTurn
chunks=1
```

Transcript ozeti:

```text
process.starting: wsl.exe --cd /home/.../acp-process-with-gemini -- /home/mertb/.nvm/versions/node/v22.22.2/bin/gemini --acp --skip-trust --approval-mode plan
initialize -> protocolVersion=1, agentInfo=gemini-cli 0.45.2
session/new -> modes returned, currentModeId=plan
session/prompt -> agent_message_chunk "ACP-DOGFOOD-OK"
prompt result -> stopReason=end_turn
process.graceful_exit -> ExitCode=0
```

Gemini quota metadata:

```text
model=gemini-3.1-pro-preview
input_tokens=10571
output_tokens=7
```

## Ek Bulgular

Gemini stderr'da su uyari goruldu:

```text
Ripgrep is not available. Falling back to GrepTool.
```

Bu, `wsl.exe -- <command>` ile baslatilan non-login ortamda PATH farki olabilecegini gosteriyor. Acp.Net.Process icin sonraki urunlestirme adiminda environment/PATH kontrolu veya shell/profile stratejisi ele alinmali.

## Karar Etkisi

Bu spike Acp.Net.Process hattinin urun degerini guclendirdi.

Fake agent disinda gercek bir ACP agent ile:

- process lifecycle calisti
- WSL bridge calisti
- transcript gercek hata ayiklamada ise yaradi
- API eksigi dogfood ile yakalandi ve giderildi

Bir sonraki adim:

1. `ToAgentPath` icin unit test eklemek.
2. Environment/PATH stratejisini tasarlamak.
3. Gemini sample'i opt-in dogfood olarak dokumante etmek.
4. Alpha checklist'e bu dogfood sonucunu eklemek.
