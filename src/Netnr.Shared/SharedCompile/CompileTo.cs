#if Full || Compile

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace Netnr.SharedCompile
{
    /// <summary>
    /// Using Roslyn in .NET Core
    /// 
    /// Install-Package Microsoft.CodeAnalysis
    /// Install-Package System.Runtime.Loader
    /// </summary>
    public class CompileTo
    {
        /// <summary>
        /// 编码
        /// </summary>
        public static Encoding encoding = Encoding.UTF8;

        /// <summary>
        /// 动态编译并执行
        /// </summary>
        /// <param name="code">代码</param>
        /// <param name="listUsing">引用，逗号分隔</param>
        /// <param name="className">类名</param>
        /// <param name="methodName">方法</param>
        public static string Executing(string code, IEnumerable<string> listUsing, string className = "DynamicCompile", string methodName = "Main")
        {
            string outContent = string.Empty;

            //设置输出
            var outFile = Path.GetTempFileName();
            Console.OutputEncoding = encoding;
            var sw = new StreamWriter(outFile, false, encoding)
            {
                AutoFlush = true
            };
            Console.SetOut(sw);


            var listUseObj = new List<Type>()
            {
                typeof(object),
                typeof(Enumerable),
                typeof(Console),
                typeof(DataSet),
                typeof(TypeConverter),
                MethodBase.GetCurrentMethod().DeclaringType
            };

            //引用对象
            var references = new List<MetadataReference>();
            listUseObj.ForEach(uo =>
            {
                references.Add(MetadataReference.CreateFromFile(uo.Assembly.Location));
            });

            //载入引用
            var sdkPath = Path.GetDirectoryName(listUseObj.First().Assembly.Location);
            var defaultUsing = "System.Runtime.dll,System.Collections.dll,netstandard.dll".Split(',').ToList();
            if (listUsing != null)
            {
                defaultUsing.AddRange(listUsing);
            }
            defaultUsing.ForEach(us =>
            {
                if (!string.IsNullOrWhiteSpace(us))
                {
                    var ffPath = Path.Combine(sdkPath, us);
                    if (!File.Exists(ffPath))
                    {
                        ffPath = Path.Combine(AppContext.BaseDirectory, us);
                        if (!File.Exists(ffPath))
                        {
                            ffPath = null;
                        }
                    }
                    if (ffPath != null && references.Any(x => x.Display != ffPath))
                    {
                        references.Add(MetadataReference.CreateFromFile(ffPath));
                    }
                }
            });

            //Console.WriteLine($"载入引用：{Environment.NewLine}{references.Select(x => Path.GetFileName(x.Display)).ToJson(true)}");

            string assemblyName = Path.GetRandomFileName();
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
            CSharpCompilation compilation = CSharpCompilation.Create(assemblyName, syntaxTrees: new[] { syntaxTree }, references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using var ms = new MemoryStream();
            EmitResult result = compilation.Emit(ms);

            // Compilation result
            if (!result.Success)
            {
                Console.WriteLine("编译失败");
                IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError ||
                    diagnostic.Severity == DiagnosticSeverity.Error);

                foreach (Diagnostic diagnostic in failures)
                {
                    Console.WriteLine($"{diagnostic.Id}：{diagnostic.GetMessage()}");
                }
            }
            else
            {
                ms.Seek(0, SeekOrigin.Begin);

                Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(ms);
                var type = assembly.GetType(className);
                var instance = assembly.CreateInstance(className);
                var meth = type.GetMember(methodName).First() as MethodInfo;
                meth.Invoke(instance, null);
            }

            //读取输出并重置
            sw.Close();
            outContent = File.ReadAllText(outFile, encoding);
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput())
            {
                AutoFlush = true
            });
            Console.OutputEncoding = encoding;

            return outContent;
        }
    }
}

#endif