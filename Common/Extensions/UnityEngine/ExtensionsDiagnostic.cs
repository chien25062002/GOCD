using System.Runtime.CompilerServices;
using GOCD.Framework.Diagnostics;
using UnityEngine;

namespace GOCD.Framework
{
    public static class ExtensionsDiagnostic
    {
        // Phương thức LogCaller
        public static void LogCaller(
            this Object context, // Tùy chọn thêm context để liên kết với object nào đó (gameobject hoặc component)
            [CallerLineNumber] int line = 0,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = ""
        )
        {
            // Log thông tin dòng, tên phương thức và file path
            GOCDDebug.Log($"{line} :: {memberName} :: {filePath}", context, Color.cyan);
        }
    }
}
