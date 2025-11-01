using UnityEngine;

namespace tinygrox.DuckovMods.GiveMeInventoryFilter
{
    public class ModLogger
    {
        private static readonly System.Lazy<ModLogger> s_instance = new System.Lazy<ModLogger>(() => new ModLogger());
        public static ModLogger Instance => s_instance.Value;

        private ModLogger()
        {
#if DEBUG
            IsLoggingEnabled = true;
#else
            IsLoggingEnabled = false;
#endif
        }

        public enum Level
        {
            Info,
            Warning,
            Error,
            Debug
        }

        public bool IsLoggingEnabled { get; set; }
        public string DefaultModName { get; set; } = "tinygrox.DuckovMods";
        public static LogBuilder Log => new LogBuilder(Instance);

        internal void PerformLog(Level level, string message, string modName)
        {
            if (!IsLoggingEnabled)
            {
                // Debug.Log("[GiveMeInventoryFilter]No LoggingEnabled");
                return;
            }

            string prefix = $"[{modName ?? DefaultModName}]";

            switch (level)
            {
                case Level.Info:
                    Debug.Log($"{prefix} {message}");
                    break;
                case Level.Warning:
                    Debug.LogWarning($"{prefix} {message}");
                    break;
                case Level.Error:
                    Debug.LogError($"{prefix} {message}");
                    break;
                case Level.Debug:
#if DEBUG
                    Debug.Log($"{prefix}[DEBUG] {message}");
#endif
                    break;
            }
        }

        public class LogBuilder
        {
            private readonly ModLogger _logger;
            private string _modName;
            private bool _shouldLog = true; // 用于条件日志

            internal LogBuilder(ModLogger logger)
            {
                _logger = logger;
                _modName = logger.DefaultModName; // 继承默认 Mod 名称
            }

            public LogBuilder WithModName(string modName)
            {
                _modName = modName;
                return this;
            }

            public LogBuilder When(bool condition)
            {
                if (!condition)
                {
                    _shouldLog = false;
                }

                return this;
            }

            public LogBuilder Info(string message)
            {
                if (_shouldLog)
                {
                    _logger.PerformLog(Level.Info, message, _modName);
                }

                return this;
            }

            public LogBuilder Warning(string message)
            {
                if (_shouldLog)
                {
                    _logger.PerformLog(Level.Warning, message, _modName);
                }

                return this;
            }

            public LogBuilder Error(string message)
            {
                if (_shouldLog)
                {
                    _logger.PerformLog(Level.Error, message, _modName);
                }

                return this;
            }

            public LogBuilder Debug(string message)
            {
                if (_shouldLog)
                {
                    _logger.PerformLog(Level.Debug, message, _modName);
                }

                return this;
            }
        }
    }
}
