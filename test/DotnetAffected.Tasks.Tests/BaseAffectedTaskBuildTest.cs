using DotnetAffected.Testing.Utils;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotnetAffected.Tasks.Tests
{
    public abstract class BaseAffectedTaskBuildTest : BaseRepositoryTest
    {
        private static readonly Regex ProjectPathRegEx =  new Regex("^\\s*\\[AffectedProject](\\S.+)$", RegexOptions.Compiled);

        private readonly Process _buildProcess;
        private readonly List<string> _output = new List<string>();
        private readonly List<string> _errors = new List<string>();
        private readonly HashSet<string> _projects = new HashSet<string>();

        protected bool HasOutput => _output.Count > 0;
        protected bool HasErrors => _errors.Count > 0;
        protected bool HasProjects => _projects.Count > 0;
        protected bool ExitSuccess => ExitCode == 0;

        protected IEnumerable<string> Output => _output.AsEnumerable();
        protected IEnumerable<string> Errors => _errors.AsEnumerable();
        protected IEnumerable<string> Projects => _projects.AsEnumerable();
        protected int ExitCode { get; private set; } = -1;

        protected BaseAffectedTaskBuildTest()
        {
            _buildProcess = new Process();
            _buildProcess.EnableRaisingEvents = false;
            _buildProcess.StartInfo.RedirectStandardError = true;
            _buildProcess.StartInfo.RedirectStandardOutput = true;
            _buildProcess.StartInfo.FileName = "dotnet";
            _buildProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            _buildProcess.StartInfo.CreateNoWindow = true;
            _buildProcess.StartInfo.WorkingDirectory = Repository.Path;
        }
        
        protected override void Dispose(bool dispose)        {
            base.Dispose(dispose);
            _buildProcess.Close();
        }
        
        protected void ExecuteCommandAndCollectResults()
        {
            _output.Clear();
            _errors.Clear();
            _projects.Clear();

            _buildProcess.StartInfo.Arguments = $"msbuild -nodeReuse:false ci.props /t:DotnetAffectedCheck /p:DotnetAffectedNugetDir={Utils.DotnetAffectedNugetDir} /p:TargetFramework={Utils.TargetFramework}";

            _buildProcess.OutputDataReceived += (_, eventArgs) =>
            {
                if (string.IsNullOrWhiteSpace(eventArgs.Data))
                    return;

                _output.Add(eventArgs.Data);
                var match = ProjectPathRegEx.Match(eventArgs.Data);
                if (match.Success)
                {
                    var proj = match.Groups[1].Value;
                    if (!string.IsNullOrWhiteSpace(proj))
                        _projects.Add(proj);
                }
            };

            _buildProcess.ErrorDataReceived += (_, eventArgs) =>
            {
                if (!string.IsNullOrWhiteSpace(eventArgs.Data))
                    _errors.Add(eventArgs.Data);
            };

            _buildProcess.Start();
            _buildProcess.BeginOutputReadLine();
            _buildProcess.BeginErrorReadLine();
            _buildProcess.WaitForExit();

            ExitCode = _buildProcess.ExitCode;
            
            _buildProcess.CancelOutputRead();
            _buildProcess.CancelErrorRead();

        }
    }
}
