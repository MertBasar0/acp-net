# Ekosistem Konumlandırması

> 🇬🇧 English version: [ECOSYSTEM.md](ECOSYSTEM.md)

Son inceleme: 2026-06-13 (NuGet metadata + proje README'leri).

Bu not, Acp.Net'in ACP (Agent Client Protocol) için diğer .NET paketlerinin
yanında nasıl konumlandığını kaydeder. Anlık bir taramadır, canlı bir akış
değil; sayılar zamanla değişir.

## .NET ACP manzarası

ACP-protokol paketleri için yapılan bir NuGet araması, küçük ama büyüyen bir
küme gösteriyor. Agent Client Protocol'ü uygulayanlar (Tencent Cloud ACP veya
doküman-otomasyon wrapper'ları gibi alakasız "ACP" ürünleri hariç):

| Paket | Sahip | Son sürüm | İlk yayın | Kapsam |
| --- | --- | --- | --- | --- |
| [`AgentClientProtocol`](https://www.nuget.org/packages/AgentClientProtocol) | nuskey8 | 0.1.5 | 2025-11 | Typed protokol/JSON-RPC SDK (client + agent) |
| [`dotacp.protocol` / `.agent` / `.client`](https://www.nuget.org/packages/dotacp.protocol) | timxx | 2026.5.10 | 2026-05 | Şema-üretimli protokol + JSON-RPC üçlüsü (StreamJsonRpc kullanır) |
| [`Acp.Sdk`](https://www.nuget.org/packages/Acp.Sdk) | acp-sdk | 0.1.0 | 2026-04 | Agent-kurma SDK'sı (erken, tek sürüm) |
| [`LibAcp`](https://www.nuget.org/packages/LibAcp) | sargeMonkey | 0.1.0 | 2026-05 | Çağıran stream'i üzerinden JSON-RPC 2.0 + ndjson taşıma (erken) |
| **`Acp.Net.Process` / `Acp.Net.Testing`** | bu proje | 0.1.0-alpha.2 | 2026-06 | Process/runtime/test katmanı (protokol şeması yok) |

## Temel gözlem

Diğer her paket **protokol/SDK katmanında** çalışıyor: ACP tiplerini modelliyor ve
JSON-RPC'yi *çağıranın sağladığı* bir stream üzerinden çalıştırıyor. Kendi
dokümanları da bunu açıkça söylüyor — `dotacp`, "process spawn, Windows-WSL
köprüsü, path eşleme, environment/PATH, executable preflight, stdio transcript,
run artifact" maddelerini **tüketiciye bırakılmış** olarak sıralıyor; `LibAcp`'in
hızlı başlangıcı ise çağırana "agent process'ini (`ProcessStartInfo` ile) sen
başlat, stdio redirect'ini sen bağla, stream'i sen kur" diyor.

Tüketiciye bırakılan bu glue kod, tam olarak Acp.Net'in sağladığı şey. Protokol
katmanı kalabalık; process/runtime katmanı boş.

## Örtüşme ve ayrışma

- **Örtüşme: minimum.** Acp.Net bilinçli olarak protokol tiplerini sahiplenmez
  (bkz. [decisions/ADR-0001](decisions/ADR-0001-incumbent-comparison-decision.md)),
  protokol paketleri de bilinçli olarak process/runtime'ı sahiplenmez. İki katman
  diktir.
- **Ayrışma: tüm değer.** Acp.Net'in yüzeyi — process başlatma, Windows/WSL
  köprüsü, path eşleme, environment/PATH şekillendirme, executable preflight,
  transcript kaydı, run artifact, hata sınıflandırması, shutdown policy ve sahte
  agent'larla process-sınırı testi — protokol paketlerinin kapsam-dışı bıraktığı
  şeyin ta kendisi.

## Konumlandırma açısından anlamı

1. **Acp.Net protokol-paketi-agnostiktir.** Agent'ın stdio stream'lerini üretir ve
   tüketicinin tercih ettiği protokol paketine teslim eder (`AgentClientProtocol`,
   `dotacp`, `LibAcp`, …). `Acp.Net.Process` hiçbirine bağımlılık almaz; sample'lar
   tesadüfen `AgentClientProtocol` kullanıyor. Bu, her protokol paketinin tüm
   kullanıcı havuzunu potansiyel tüketicimiz yapar.
2. **ADR-0001 "narrow" kararı tuttu.** O karardan bu yana protokol katmanı beş
   pakete çıktı, runtime katmanı ise tek-paketlik bir kategori olarak kaldı.
3. **Traction'da dürüstlük.** `AgentClientProtocol` (~2.3k indirme) ve `dotacp`
   (~850) benimsenmede önde; Acp.Net alpha. Avantaj, en çok indirilen olmak değil,
   kendi niş'inde tek paket olmak.

## İzleme listesi

- `dotacp` en kapsamlı komşu (şema-üretimli, geniş hedef framework, aktif). Bir
  gün process/runtime yardımcıları eklerse örtüşme sorusu yeniden açılır — her
  sürümde kontrol etmeye değer.
- `Acp.Sdk` kendini agent-kurma SDK'sı olarak tanımlıyor; runtime orkestrasyona
  doğru genişlerse yeniden değerlendir.
