﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace ButtplugControlLibrary
{
    public class LogList : ObservableCollection<string>
    {
    }

    [Target("ButtplugGUILogger")]
    public sealed class ButtplugGUIMessageNLogTarget : TargetWithLayoutHeaderAndFooter
    {
        private readonly LogList _logs;
        private readonly Thread _winThread;

        public ButtplugGUIMessageNLogTarget(LogList l, Thread aWinThread)
        {
            // TODO This totally needs a mutex or something
            _logs = l;
            _winThread = aWinThread;
        }

        protected override void Write(LogEventInfo aLogEvent)
        {
            Dispatcher.FromThread(_winThread).Invoke(() => _logs.Add(this.Layout.Render(aLogEvent)));
        }
    }

    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ButtplugLogControl : UserControl
    {
        private readonly LogList _logs;
        private readonly ButtplugGUIMessageNLogTarget _logTarget;
        private LoggingRule _outgoingLoggingRule;

        public ButtplugLogControl()
        {
            var c = LogManager.Configuration ?? new LoggingConfiguration();
            _logs = new LogList();
            // Null check Dispatcher, otherwise test bringup for GUI tests will fail.
            if (Dispatcher != null)
            {
                _logTarget = new ButtplugGUIMessageNLogTarget(_logs, Dispatcher.Thread);
                c.AddTarget("ButtplugGuiLogger", _logTarget);
                _outgoingLoggingRule = new LoggingRule("*", LogLevel.Debug, _logTarget);
                c.LoggingRules.Add(_outgoingLoggingRule);
                LogManager.Configuration = c;
            }
            InitializeComponent();
            LogLevelComboBox.SelectionChanged += LogLevelSelectionChangedHandler;
            LogListBox.ItemsSource = _logs;
        }

        private void SaveLogFileButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                CheckFileExists = false,
                CheckPathExists = true,
                OverwritePrompt = true
            };
            if (dialog.ShowDialog() != true)
            {
                return;
            }
            var sw = new System.IO.StreamWriter(dialog.FileName, false);
            foreach (var line in _logs.ToList())
            {
                sw.WriteLine(line);
            }
            sw.Close();
        }

        private void LogLevelSelectionChangedHandler(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {            
            var c = LogManager.Configuration;
            var level = ((ComboBoxItem)LogLevelComboBox.SelectedValue).Content.ToString();
            try
            {
                c.LoggingRules.Remove(_outgoingLoggingRule);
                _outgoingLoggingRule = new LoggingRule("*", LogLevel.FromString(level), _logTarget);
                c.LoggingRules.Add(_outgoingLoggingRule);
                LogManager.Configuration = c;                
            }
            catch (ArgumentException)
            {
                LogManager.GetCurrentClassLogger().Error($"Log Level \"{level}\" is not a valid log level!");
            }
        }
    }
}