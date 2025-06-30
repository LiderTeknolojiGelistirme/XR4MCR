using System;
using System.IO;
using System.Text;
using UnityEngine;
// StringBuilder için gerekli
// Dosya işlemleri için gerekli

// Exception handling için gerekli

namespace Managers
{
    /// <summary>
    /// Oyun çalışırken oluşan tüm Debug log'larını yakalar, biriktirir ve bir dosyaya yazdırır.
    /// Bu scripti sahnede boş bir GameObject'e eklemeniz yeterlidir.
    /// Singleton yapısı sayesinde sahne geçişlerinde de çalışmaya devam eder.
    /// </summary>
    public class RuntimeLogExporter : MonoBehaviour
    {
        // Singleton instance'ı. Diğer scriptlerden kolayca erişim için kullanılır.
        public static RuntimeLogExporter Instance { get; private set; }

        // Logları biriktirmek için StringBuilder kullanmak, normal string birleştirmeye göre çok daha performanslıdır.
        private readonly StringBuilder logBuilder = new StringBuilder();

        [Tooltip("Kaydedilecek dosyanın adı (uzantısız).")]
        [SerializeField] private string fileName = "RuntimeLogs";

        private void Awake()
        {
            // Singleton pattern'i uygula
            if (Instance == null)
            {
                Instance = this;
                // Bu objenin sahne yüklendiğinde yok olmamasını sağla
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                // Eğer zaten bir instance varsa, bu yenisini yok et
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            // Unity'nin log mesajı event'ine kendi metodumuzu bağlıyoruz.
            // Bir log oluştuğunda HandleLog metodu otomatik olarak çağrılacak.
            Application.logMessageReceived += HandleLog;
        }

        private void OnDisable()
        {
            // Script devre dışı bırakıldığında veya yok edildiğinde event aboneliğini iptal et.
            // Bu, hafıza sızıntılarını (memory leak) önlemek için önemlidir.
            Application.logMessageReceived -= HandleLog;
        }

        /// <summary>
        /// Unity'den bir log geldiğinde bu metod tetiklenir.
        /// </summary>
        /// <param name="logString">Log mesajının kendisi.</param>
        /// <param name="stackTrace">Hatanın oluştuğu kod satırını gösteren yığın izi.</param>
        /// <param name="type">Log'un tipi (Log, Warning, Error, Exception).</param>
        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            // Logları daha okunaklı bir formatta StringBuilder'a ekle
            logBuilder.AppendLine($"[{DateTime.Now:HH:mm:ss}] [{type}] - {logString}");
        
            // Hata ve istisna durumlarında stack trace'i de ekle
            if (type == LogType.Error || type == LogType.Exception)
            {
                logBuilder.AppendLine($"Stack Trace: {stackTrace}");
            }
        
            logBuilder.AppendLine(); // Loglar arasına boşluk koy
        }

        /// <summary>
        /// Biriktirilen tüm logları bir .txt dosyasına kaydeder.
        /// </summary>
        public void ExportLogsToFile()
        {
            // Application.persistentDataPath, build alınmış oyunda dosya kaydetmek için en güvenli yerdir.
            // Editörde: ProjeKlasoru/Assets
            // Windows Build: C:/Users/KullaniciAdi/AppData/LocalLow/SirketAdi/OyunAdi
            string filePath = Path.Combine(Application.persistentDataPath, fileName + ".txt");

            try
            {
                // StringBuilder'daki tüm metni dosyaya yaz.
                File.WriteAllText(filePath, logBuilder.ToString());
                
            
                // Kullanıcıya bilgi ver (bu log da dosyaya yazılacak)
                Debug.Log($"Loglar başarıyla şuraya kaydedildi: {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Logları dosyaya kaydederken bir hata oluştu: {e.Message}");
            }
        }
    }
}