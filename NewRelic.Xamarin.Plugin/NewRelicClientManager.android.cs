/*
 * Copyright (c) 2023-present New Relic Corporation. All rights reserved.
 * SPDX-License-Identifier: Apache-2.0 
 */

using System;
using System.Threading.Tasks;
using System.Linq;
using Android.App;
using Android.Content;
using Java.Interop;
using NRAndroidAgent = Com.Newrelic.Agent.Android.NewRelic;
using System.Collections.Generic;
using System.Net.Http;
using Android.Runtime;
using Xamarin.Forms;
using NRNetworkFailure = Com.Newrelic.Agent.Android.Util.NetworkFailure;
using NRMetricUnit = Com.Newrelic.Agent.Android.Metric.MetricUnit;

namespace NewRelic.Xamarin.Plugin
{
    /// <summary>
    /// Implementation for NewRelicClient
    /// </summary>
    public class NewRelicClientManager : Java.Lang.Object, INewRelicClientManager
    {

        private bool isStarted;
        private EventHandler<RaiseThrowableEventArgs>? _handler;

        private Dictionary<LogLevel, int> logLevelDict = new Dictionary<LogLevel, int>()
        {
            { LogLevel.ERROR, 1},
            { LogLevel.WARNING, 2 },
            { LogLevel.INFO, 3 },
            { LogLevel.VERBOSE, 4 },
            { LogLevel.AUDIT, 6 }
        };

        private Dictionary<NetworkFailure, NRNetworkFailure> networkFailureDict = new Dictionary<NetworkFailure, NRNetworkFailure>()
        {
            { NetworkFailure.Unknown, NRNetworkFailure.Unknown },
            { NetworkFailure.BadURL, NRNetworkFailure.BadURL },
            { NetworkFailure.TimedOut, NRNetworkFailure.TimedOut },
            { NetworkFailure.CannotConnectToHost, NRNetworkFailure.CannotConnectToHost },
            { NetworkFailure.DNSLookupFailed, NRNetworkFailure.DNSLookupFailed },
            { NetworkFailure.BadServerResponse, NRNetworkFailure.BadServerResponse },
            { NetworkFailure.SecureConnectionFailed, NRNetworkFailure.SecureConnectionFailed }
        };

        private Dictionary<MetricUnit, NRMetricUnit> metricUnitDict = new Dictionary<MetricUnit, NRMetricUnit>()
        {
            { MetricUnit.PERCENT, NRMetricUnit.Percent },
            { MetricUnit.BYTES, NRMetricUnit.Bytes },
            { MetricUnit.SECONDS, NRMetricUnit.Seconds },
            { MetricUnit.BYTES_PER_SECOND, NRMetricUnit.BytesPerSecond },
            { MetricUnit.OPERATIONS, NRMetricUnit.Operations }
        };

        private bool IsNumeric(Object obj)
        {
            if (obj is sbyte
                || obj is byte
                || obj is short
                || obj is ushort
                || obj is int
                || obj is uint
                || obj is long
                || obj is ulong
                || obj is float
                || obj is double
                || obj is decimal)
            {
                return true;
            }
            return false;
        }

        public NewRelicClientManager()
        {
            isStarted = false;
        }

        public void Start(string applicationToken, AgentStartConfiguration agentConfig = null)
        {

            if (agentConfig == null)
            {
                agentConfig = new AgentStartConfiguration();
            }

            if (!agentConfig.crashReportingEnabled)
            {
                NRAndroidAgent.DisableFeature(Com.Newrelic.Agent.Android.FeatureFlag.CrashReporting);
            }

            var newRelic = NRAndroidAgent.WithApplicationToken(applicationToken)
                .WithApplicationFramework(Com.Newrelic.Agent.Android.ApplicationFramework.Xamarin, "0.0.1")
                .WithLoggingEnabled(agentConfig.loggingEnabled)
                .WithLogLevel(logLevelDict[agentConfig.logLevel]);

            if (!agentConfig.collectorAddress.Equals("DEFAULT"))
            {
                newRelic.UsingCollectorAddress(agentConfig.collectorAddress);
            }

            if (!agentConfig.crashCollectorAddress.Equals("DEFAULT"))
            {
                newRelic.UsingCrashCollectorAddress(agentConfig.crashCollectorAddress);
            }

            newRelic.Start(Android.App.Application.Context);

            isStarted = true;
        }

        public void CrashNow(string message = "")
        {
            if (string.IsNullOrEmpty(message))
            {
                NRAndroidAgent.CrashNow();
            }
            else
            {
                NRAndroidAgent.CrashNow(message);
            }
        }

        public string CurrentSessionId()
        {
            return NRAndroidAgent.CurrentSessionId();
        }

        public void EndInteraction(string interactionId)
        {
            NRAndroidAgent.EndInteraction(interactionId);
            return;
        }

        public bool IncrementAttribute(string name, float value = 1)
        {
            return NRAndroidAgent.IncrementAttribute(name, value);
        }

        public void NoticeHttpTransaction(string url, string httpMethod, int statusCode, long startTime, long endTime, long bytesSent, long bytesReceived, string responseBody)
        {
            NRAndroidAgent.NoticeHttpTransaction(url, httpMethod, statusCode, startTime, endTime, bytesSent, bytesReceived, responseBody);
            return;
        }

        public void NoticeNetworkFailure(string url, string httpMethod, long startTime, long endTime, NetworkFailure failure)
        {
            NRAndroidAgent.NoticeNetworkFailure(url, httpMethod, startTime, endTime, networkFailureDict[failure]);
            return;
        }

        public bool RecordBreadcrumb(string name, Dictionary<string, object> attributes)
        {
            Dictionary<string, Java.Lang.Object> strToJavaObject = new Dictionary<string, Java.Lang.Object>();
            foreach (KeyValuePair<string, object> entry in attributes)
            {
                if (entry.Value is bool)
                {
                    strToJavaObject.Add(entry.Key, (bool)entry.Value);
                }
                else if (IsNumeric(entry.Value))
                {
                    strToJavaObject.Add(entry.Key, Convert.ToDouble(entry.Value));
                }
                else
                {
                    strToJavaObject.Add(entry.Key, entry.Value.ToString());
                }
            }
            return NRAndroidAgent.RecordBreadcrumb(name, strToJavaObject);
        }

        public bool RecordCustomEvent(string eventType, string eventName, Dictionary<string, object> attributes)
        {
            Dictionary<string, Java.Lang.Object> strToJavaObject = new Dictionary<string, Java.Lang.Object>();
            foreach (KeyValuePair<string, object> entry in attributes)
            {
                if (entry.Value is bool)
                {
                    strToJavaObject.Add(entry.Key, (bool)entry.Value);
                }
                else if (IsNumeric(entry.Value))
                {
                    strToJavaObject.Add(entry.Key, Convert.ToDouble(entry.Value));
                }
                else
                {
                    strToJavaObject.Add(entry.Key, entry.Value.ToString());
                }
            }
            return NRAndroidAgent.RecordCustomEvent(eventType, eventName, strToJavaObject);
        }

        public void RecordMetric(string name, string category)
        {
            NRAndroidAgent.RecordMetric(name, category);
            return;
        }

        public void RecordMetric(string name, string category, double value)
        {
            NRAndroidAgent.RecordMetric(name, category, value);
            return;
        }

        public void RecordMetric(string name, string category, double value, MetricUnit countUnit, MetricUnit valueUnit)
        {
            NRAndroidAgent.RecordMetric(name, category, 0, value, 0, metricUnitDict[countUnit], metricUnitDict[valueUnit]);
            return;
        }

        public bool RemoveAllAttributes()
        {
            return NRAndroidAgent.RemoveAllAttributes();
        }

        public bool RemoveAttribute(string name)
        {
            return NRAndroidAgent.RemoveAttribute(name);
        }

        public bool SetAttribute(string name, string value)
        {
            return NRAndroidAgent.SetAttribute(name, value);
        }

        public bool SetAttribute(string name, double value)
        {
            return NRAndroidAgent.SetAttribute(name, value);
        }

        public bool SetAttribute(string name, bool value)
        {
            return NRAndroidAgent.SetAttribute(name, value);
        }

        public void SetMaxEventBufferTime(int maxBufferTimeInSec)
        {
            NRAndroidAgent.SetMaxEventBufferTime(maxBufferTimeInSec);
            return;
        }

        public void SetMaxEventPoolSize(int maxPoolSize)
        {
            NRAndroidAgent.SetMaxEventPoolSize(maxPoolSize);
            return;
        }

        public bool SetUserId(string userId)
        {
            return NRAndroidAgent.SetUserId(userId);
        }

        public string StartInteraction(string interactionName)
        {
            return NRAndroidAgent.StartInteraction(interactionName);
        }

        public void AnalyticsEventEnabled(bool enabled)
        {
            if (enabled)
            {
                NRAndroidAgent.EnableFeature(Com.Newrelic.Agent.Android.FeatureFlag.AnalyticsEvents);
            }
            else
            {
                NRAndroidAgent.DisableFeature(Com.Newrelic.Agent.Android.FeatureFlag.AnalyticsEvents);
            }
            return;
        }

        public void NetworkRequestEnabled(bool enabled)
        {
            if (enabled)
            {
                NRAndroidAgent.EnableFeature(Com.Newrelic.Agent.Android.FeatureFlag.NetworkRequests);
            }
            else
            {
                NRAndroidAgent.DisableFeature(Com.Newrelic.Agent.Android.FeatureFlag.NetworkRequests);
            }
            return;
        }

        public void NetworkErrorRequestEnabled(bool enabled)
        {
            if (enabled)
            {
                NRAndroidAgent.EnableFeature(Com.Newrelic.Agent.Android.FeatureFlag.NetworkErrorRequests);
            }
            else
            {
                NRAndroidAgent.DisableFeature(Com.Newrelic.Agent.Android.FeatureFlag.NetworkErrorRequests);
            }
            return;
        }

        public void HttpResponseBodyCaptureEnabled(bool enabled)
        {
            if (enabled)
            {
                NRAndroidAgent.EnableFeature(Com.Newrelic.Agent.Android.FeatureFlag.HttpResponseBodyCapture);
            }
            else
            {
                NRAndroidAgent.DisableFeature(Com.Newrelic.Agent.Android.FeatureFlag.HttpResponseBodyCapture);
            }
            return;
        }

        public HttpMessageHandler GetHttpMessageHandler()
        {
            var handler = new NewRelicHttpClientHandler();

            // handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            // {
            //     if (cert.Issuer.Equals("CN=localhost"))
            //         return true;
            //     return errors == System.Net.Security.SslPolicyErrors.None;
            // };
            return handler;

        }

        public void RecordException(Exception exception)
        {
            NRAndroidAgent.RecordHandledException(NewRelicXamarinException.Create(exception));
        }

        public void HandleUncaughtException(bool shouldThrowFormattedException = true)
        {
            if (_handler != null)
            {
                AndroidEnvironment.UnhandledExceptionRaiser -= _handler;
            }

            _handler = (s, e) =>
            {
                if (shouldThrowFormattedException && Java.Lang.Thread.DefaultUncaughtExceptionHandler != null)
                {
                    Java.Lang.Thread.DefaultUncaughtExceptionHandler.UncaughtException(Java.Lang.Thread.CurrentThread(), NewRelicXamarinException.Create(e.Exception));
                }
                else
                {
                    RecordException(e.Exception);
                }
            };

            AndroidEnvironment.UnhandledExceptionRaiser += _handler;
        }

        public void TrackShellNavigatedEvents()
        {
            Shell.Current.Navigated += (sender, e) =>
            {
                Dictionary<string, object> attr = new Dictionary<string, object>();
                if (e.Previous != null)
                {
                    attr.Add("Previous", e.Previous.Location.ToString());
                }
                attr.Add("Current", e.Current.Location.ToString());
                attr.Add("Source", e.Source.ToString());
                this.RecordBreadcrumb("ShellNavigated", attr);
            };
        }

        public void Shutdown()
        {
            NRAndroidAgent.Shutdown();
            return;
        }

    }
}
