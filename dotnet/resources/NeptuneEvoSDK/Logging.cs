using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NeptuneEVO.SDK
{
    public class nLog
    {
        /// <summary>
        /// Инициализация системы логирования
        /// </summary>
        /// <param name="reference">Зависимость - Пространство вызова лога, своя пометка в консоли</param>
        /// <param name="canDebug">Включить или отключить вывод отладочных сообщений для всего пространства</param>
        public nLog(string _reference = null, bool _canDebug = false)
        {
            if (_reference == null) _reference = "Logger";
            Reference = _reference;
            CanDebug = _canDebug;
        }
        public string Reference { get; set; }
        public bool CanDebug { get; set; }

        /// <summary>
        /// Флаги (пометки) строк при выводе в консоль
        /// </summary>
        public enum Type
        {
            Info,
            Warn,
            Error,
            Success,
            Save
        };

        /// <summary>
        /// Вывести в консоль обычный текст с нужным флагом
        /// </summary>
        /// <param name="text">Выводимый текст</param>
        /// <param name="logType">Флаг. Указывает, как нужно пометить строку</param>
        public void Write(string text, Type logType = Type.Info)
        {
            try
            {
                Console.ResetColor();
                Console.Write($"{DateTime.Now.ToString("HH':'mm")} ");
                switch (logType)
                {
                    case Type.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("[Error] ");
                        break;
                    case Type.Warn:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("[Warn] ");
                        break;
                    case Type.Info:
                        Console.Write("[Info] ");
                        break;
                    case Type.Success:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("[Succ] ");
                        break;
                    case Type.Save:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("[Save] ");
                        break;
                    default:
                        return;
                }
                Console.ResetColor();
                Console.Write($"{Reference} > {text}\n");
            }
            catch (Exception e)
            {
                Console.ResetColor();
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Logger Error:\n" + e.ToString());
                Console.ResetColor();
            }
        }
        /// <summary>
        /// Вывести в консоль обычный текст с нужным флагом асинхронно
        /// </summary>
        /// <param name="text"></param>
        /// <param name="logType"></param>
        public Task WriteAsync(string text, Type logType = Type.Info)
        {
            try
            {
                Console.ResetColor();
                Console.Write($"{DateTime.Now.ToString("HH':'mm")} ");
                switch (logType)
                {
                    case Type.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("[Error] ");
                        break;
                    case Type.Warn:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("[Warn] ");
                        break;
                    case Type.Info:
                        Console.Write("[Info] ");
                        break;
                    case Type.Success:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("[Succ] ");
                        break;
                    case Type.Save:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("[Save] ");
                        break;
                    default:
                        return Task.CompletedTask;
                }
                Console.ResetColor();
                Console.Write($"{Reference} > {text}\n");
            }
            catch (Exception e)
            {
                Console.ResetColor();
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Logger Error:\n" + e.ToString());
                Console.ResetColor();
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Вывести в консоль отладочный текст с нужным флагом
        /// </summary>
        /// <param name="text">Выводимый текст</param>
        /// <param name="logType">Флаг. Указывает, как нужно пометить строку</param>
        public void Debug(string text, Type logType = Type.Info)
        {
            try
            {
                if (!CanDebug) return;
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{DateTime.Now.ToString("HH':'mm':'ss.fff")}");
                Console.ResetColor();
                Console.Write($" | ");
                switch (logType)
                {
                    case Type.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("Error");
                        break;
                    case Type.Warn:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(" Warn");
                        break;
                    case Type.Info:
                        Console.Write(" Info");
                        break;
                    case Type.Success:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(" Succ");
                        break;
                    default:
                        return;
                }
                Console.ResetColor();
                Console.Write($" | {Reference} | {text}\n");
            }
            catch (Exception e)
            {
                Console.ResetColor();
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Logger Error:\n" + e.ToString());
                Console.ResetColor();
            }
        }
        /// <summary>
        /// Вывести в консоль отладочный текст с нужным флагом асинхронно
        /// </summary>
        /// <param name="text">Выводимый текст</param>
        /// <param name="logType">Флаг. Указывает, как нужно пометить строку</param>
        public Task DebugAsync(string text, Type logType = Type.Info)
        {
            try
            {
                if (!CanDebug) return Task.CompletedTask; ;
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{DateTime.Now.ToString("HH':'mm':'ss.fff")}");
                Console.ResetColor();
                Console.Write($" | ");
                switch (logType)
                {
                    case Type.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("Error");
                        break;
                    case Type.Warn:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(" Warn");
                        break;
                    case Type.Info:
                        Console.Write(" Info");
                        break;
                    case Type.Success:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(" Succ");
                        break;
                    default:
                        return Task.CompletedTask;
                }
                Console.ResetColor();
                Console.Write($" | {Reference} | {text}\n");
            }
            catch (Exception e)
            {
                Console.ResetColor();
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Logger Error:\n" + e.ToString());
                Console.ResetColor();
            }
            return Task.CompletedTask;
        }
    }
}
