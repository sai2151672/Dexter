using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Speech.Recognition;
using System.Windows;
using System.Windows.Input;

namespace Dexter
{
    public partial class MainWindow : Window
    {
        private SpeechRecognitionEngine? _speechRecognizer;
        private bool _speechRecognitionReady;
        private bool _isAwake;
        private readonly DexterVoiceService _voiceService = new();
        private DexterConfig _config = new();
        private readonly Dictionary<string, AppConfig> _aliasToAppMap = new(StringComparer.OrdinalIgnoreCase);

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                LoadConfigFromDiskIntoState();
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Config load failed: {ex.Message}";
            }

            InitializeSpeechRecognition();
            StartBackgroundListening();

            _isAwake = false;
            Hide();
            StatusTextBlock.Text = "Dexter is waiting for 'good morning dexter', 'good evening dexter', or 'hello dexter'.";

            _ = _voiceService.InitializeAsync();
        }

        private void LoadConfigFromDiskIntoState()
        {
            _config = ConfigService.LoadConfig();
            BuildAliasMap();
        }

        private void BuildAliasMap()
        {
            _aliasToAppMap.Clear();

            foreach (var app in _config.Apps)
            {
                if (app.Aliases == null)
                    continue;

                foreach (var alias in app.Aliases)
                {
                    if (!string.IsNullOrWhiteSpace(alias))
                    {
                        _aliasToAppMap[alias.Trim().ToLowerInvariant()] = app;
                    }
                }
            }
        }

        private void InitializeSpeechRecognition()
        {
            try
            {
                _speechRecognizer = new SpeechRecognitionEngine(new CultureInfo("en-US"));

                var choices = new Choices();
                choices.Add("good morning dexter");
                choices.Add("good evening dexter");
                choices.Add("goodnight dexter");
                choices.Add("that is it");
                choices.Add("that's it");
                choices.Add("hello dexter");
                choices.Add("bye dexter");

                string[] verbs = { "open", "launch", "start", "close" };

                foreach (var verb in verbs)
                {
                    foreach (var alias in _aliasToAppMap.Keys)
                    {
                        choices.Add($"{verb} {alias}");
                    }
                }

                var grammarBuilder = new GrammarBuilder
                {
                    Culture = new CultureInfo("en-US")
                };
                grammarBuilder.Append(choices);

                var exactGrammar = new Grammar(grammarBuilder)
                {
                    Name = "DexterExactGrammar"
                };

                _speechRecognizer.LoadGrammar(exactGrammar);

                var dictationGrammar = new DictationGrammar
                {
                    Name = "DexterDictationGrammar"
                };

                _speechRecognizer.LoadGrammar(dictationGrammar);

                _speechRecognizer.SpeechRecognized += SpeechRecognizer_SpeechRecognized;
                _speechRecognizer.SpeechRecognitionRejected += SpeechRecognizer_SpeechRecognitionRejected;
                _speechRecognizer.SetInputToDefaultAudioDevice();

                _speechRecognitionReady = true;
            }
            catch (Exception ex)
            {
                _speechRecognitionReady = false;
                StatusTextBlock.Text = $"Speech recognition setup failed: {ex.Message}";
            }
        }

        private void StartBackgroundListening()
        {
            if (!_speechRecognitionReady || _speechRecognizer == null)
                return;

            try
            {
                _speechRecognizer.RecognizeAsync(RecognizeMode.Multiple);
            }
            catch (InvalidOperationException)
            {
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Failed to start background listening: {ex.Message}";
            }
        }

        public void RestartSpeechRecognition()
        {
            try
            {
                if (_speechRecognizer != null)
                {
                    _speechRecognizer.SpeechRecognized -= SpeechRecognizer_SpeechRecognized;
                    _speechRecognizer.SpeechRecognitionRejected -= SpeechRecognizer_SpeechRecognitionRejected;

                    try
                    {
                        _speechRecognizer.RecognizeAsyncCancel();
                        _speechRecognizer.RecognizeAsyncStop();
                    }
                    catch
                    {
                    }

                    _speechRecognizer.Dispose();
                    _speechRecognizer = null;
                }
            }
            catch
            {
            }

            _speechRecognitionReady = false;
            InitializeSpeechRecognition();
            StartBackgroundListening();
        }

        private void SpeechRecognizer_SpeechRecognized(object? sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result == null)
                return;

            string spokenText = e.Result.Text.Trim().ToLowerInvariant();
            float confidence = e.Result.Confidence;

            if (confidence < 0.55f)
            {
                return;
            }

            Dispatcher.Invoke(() =>
            {
                if (!_isAwake)
                {
                    HandleSleepingModePhrase(spokenText);
                }
                else
                {
                    HandleAwakeModePhrase(spokenText);
                }
            });
        }

        private void HandleSleepingModePhrase(string spokenText)
        {
            if (spokenText.Contains("good morning dexter"))
            {
                WakeDexter("good morning dexter");
                return;
            }

            if (spokenText.Contains("good evening dexter"))
            {
                WakeDexter("good evening dexter");
                return;
            }

            if (spokenText.Contains("hello dexter"))
            {
                WakeDexter("hello dexter");
            }
        }

        private void HandleAwakeModePhrase(string spokenText)
        {
            if (spokenText.Contains("goodnight dexter") ||
                spokenText == "that is it" ||
                spokenText == "that's it" ||
                spokenText.Contains("bye dexter"))
            {
                PutDexterToSleep();
                return;
            }

            if (spokenText.Contains("hello dexter"))
            {
                StatusTextBlock.Text = "Dexter is already awake.";
                return;
            }

            HandleCommand(spokenText);
        }

        private void WakeDexter(string wakePhrase)
        {
            _isAwake = true;

            Show();
            WindowState = WindowState.Normal;
            Activate();
            Topmost = true;
            Topmost = false;
            Focus();

            StatusTextBlock.Text = $"Dexter is awake. Heard: {wakePhrase}";
            _ = _voiceService.SpeakAsync("Good morning sir, what may I do for you?");
        }

        private void PutDexterToSleep()
        {
            _isAwake = false;
            StatusTextBlock.Text = "Dexter is waiting for 'good morning dexter', 'good evening dexter', or 'hello dexter'.";
            Hide();

            _ = _voiceService.SpeakAsync("Standing by.");
        }

        private void HandleCommand(string command)
        {
            string normalized = command.Trim().ToLowerInvariant();
            string[] supportedVerbs = { "open ", "launch ", "start ", "close " };

            string? detectedVerb = null;
            string? targetText = null;

            foreach (var verb in supportedVerbs)
            {
                if (normalized.StartsWith(verb))
                {
                    detectedVerb = verb.Trim();
                    targetText = normalized.Substring(verb.Length).Trim();
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(detectedVerb) || string.IsNullOrWhiteSpace(targetText))
            {
                StatusTextBlock.Text = $"Command not recognized yet: {command}";
                _ = _voiceService.SpeakAsync("I didn't catch that.");
                return;
            }

            List<AppConfig> matchedApps = ParseMultipleTargets(targetText);

            if (matchedApps.Count == 0)
            {
                StatusTextBlock.Text = $"I do not know how to handle '{targetText}' yet.";
                _ = _voiceService.SpeakAsync("I don't know that app yet.");
                return;
            }

            var handledNames = new List<string>();
            var failedNames = new List<string>();

            foreach (var app in matchedApps)
            {
                try
                {
                    if (detectedVerb == "close")
                    {
                        bool closed = CloseAppProcesses(app);
                        if (closed)
                            handledNames.Add(app.Name);
                        else
                            failedNames.Add(app.Name);
                    }
                    else
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = app.LaunchTarget,
                            Arguments = app.Arguments,
                            UseShellExecute = true
                        });

                        handledNames.Add(app.Name);
                    }
                }
                catch
                {
                    failedNames.Add(app.Name);
                }
            }

            if (handledNames.Count > 0 && failedNames.Count == 0)
            {
                StatusTextBlock.Text = $"{Capitalize(detectedVerb)}ed: {string.Join(", ", handledNames)}.";
                _ = _voiceService.SpeakAsync($"{Capitalize(detectedVerb)}ing {string.Join(" and ", handledNames)}.");
            }
            else if (handledNames.Count > 0 && failedNames.Count > 0)
            {
                StatusTextBlock.Text =
                    $"{Capitalize(detectedVerb)}ed: {string.Join(", ", handledNames)}. Failed: {string.Join(", ", failedNames)}.";

                _ = _voiceService.SpeakAsync(
                    $"{Capitalize(detectedVerb)}ed {string.Join(" and ", handledNames)}, but I couldn't handle {string.Join(" and ", failedNames)}."
                );
            }
            else
            {
                StatusTextBlock.Text = $"Could not {detectedVerb} requested apps.";
                _ = _voiceService.SpeakAsync($"I couldn't {detectedVerb} that.");
            }
        }

        private List<AppConfig> ParseMultipleTargets(string targetText)
        {
            var results = new List<AppConfig>();

            string normalizedTargets = targetText
                .Replace(" then ", " and ")
                .Replace(",", " and ")
                .Replace("&", " and ");

            string[] parts = normalizedTargets
                .Split(new[] { " and " }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var part in parts)
            {
                string cleaned = part.Trim().ToLowerInvariant();

                if (string.IsNullOrWhiteSpace(cleaned))
                    continue;

                if (_aliasToAppMap.TryGetValue(cleaned, out var app))
                {
                    if (seen.Add(app.Name))
                    {
                        results.Add(app);
                    }
                }
            }

            return results;
        }

        private bool CloseAppProcesses(AppConfig app)
        {
            if (app.ProcessNames == null || app.ProcessNames.Count == 0)
            {
                return false;
            }

            int closedCount = 0;

            foreach (var processName in app.ProcessNames)
            {
                if (string.IsNullOrWhiteSpace(processName))
                    continue;

                var processes = Process.GetProcessesByName(processName);

                foreach (var process in processes)
                {
                    try
                    {
                        process.CloseMainWindow();

                        if (!process.WaitForExit(2000))
                        {
                            process.Kill(true);
                        }

                        closedCount++;
                    }
                    catch
                    {
                    }
                }
            }

            return closedCount > 0;
        }

        private string Capitalize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            if (text.Length == 1)
                return text.ToUpperInvariant();

            return char.ToUpperInvariant(text[0]) + text.Substring(1);
        }

        private void OpenConfigEditorButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var editor = new ConfigEditorWindow(this)
                {
                    Owner = this
                };

                editor.ShowDialog();
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Could not open config editor: {ex.Message}";
            }
        }

        private void ReloadConfigFromMainButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadConfigFromDiskIntoState();
                RestartSpeechRecognition();
                StatusTextBlock.Text = "Config reloaded successfully.";
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Reload failed: {ex.Message}";
            }
        }

        public void ApplySavedConfig(DexterConfig config)
        {
            _config = config;
            BuildAliasMap();
            RestartSpeechRecognition();
            StatusTextBlock.Text = "Config saved successfully. Dexter commands were refreshed.";
        }

        private void SpeechRecognizer_SpeechRecognitionRejected(object? sender, SpeechRecognitionRejectedEventArgs e)
        {
            if (!_isAwake)
                return;

            if (e.Result != null && !string.IsNullOrWhiteSpace(e.Result.Text))
            {
                StatusTextBlock.Text = $"Heard something else: '{e.Result.Text}'";
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                _speechRecognizer?.RecognizeAsyncCancel();
                _speechRecognizer?.RecognizeAsyncStop();
            }
            catch
            {
            }

            _speechRecognizer?.Dispose();
            _voiceService.Dispose();

            base.OnClosed(e);
        }
    }
} 