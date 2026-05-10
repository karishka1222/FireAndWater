using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Analytics;

// Глобальный менеджер аналитики.
// Кладётся один раз на GameObject в MainMenu, живёт всю игру (DontDestroyOnLoad).
// Потом из любого места: AnalyticsManager.LogEvent("level_start", new Dictionary<...>);
public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance { get; private set; }
    private bool initialized = false;

    void Awake()
    {
        // Singleton: только один экземпляр на всю игру
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
            AnalyticsService.Instance.StartDataCollection();
            initialized = true;
            Debug.Log("[Analytics] Инициализация завершена");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[Analytics] Ошибка инициализации: " + e.Message);
        }

        // Тестовое событие в Sentry — bug-tracker автоматически ловит все Debug.LogError
        // и unhandled exceptions, поэтому отдельный API-вызов не нужен.
        Debug.LogError("[FireAndWater] Sentry test event — игнорируйте, это проверка bug-tracker");
        Debug.Log("[Sentry] Тестовое сообщение отправлено через Debug.LogError");
    }

    // Статический метод для удобства — можно звать из любого скрипта
    public static void LogEvent(string eventName, Dictionary<string, object> parameters = null)
    {
        if (Instance == null || !Instance.initialized) return;

        try
        {
            var customEvent = new CustomEvent(eventName);
            if (parameters != null)
            {
                foreach (var kvp in parameters)
                    customEvent.Add(kvp.Key, kvp.Value);
            }
            AnalyticsService.Instance.RecordEvent(customEvent);
            Debug.Log("[Analytics] Событие: " + eventName);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[Analytics] Ошибка отправки " + eventName + ": " + e.Message);
        }
    }
}
