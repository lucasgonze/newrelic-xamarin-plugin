﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


namespace Plugin.NewRelicClient
{
	
    public interface INewRelicClientManager
    {

        void Start(string applicationToken);

        void CrashNow(string message = "");

        string CurrentSessionId();

        string StartInteraction(string interactionName);

        void EndInteraction(string interactionId);

        void NoticeHttpTransaction(string url,
            string httpMethod,
            int statusCode,
            long startTime,
            long endTime,
            long bytesSent,
            long bytesReceived,
            string responseBody);

        bool RecordBreadcrumb(string name, Dictionary<string, object> attributes);

        bool RecordCustomEvent(String eventType, String eventName, Dictionary<string, object> attributes);

        void RecordMetric(string name, string category);
        void RecordMetric(string name, string category, double value);
        //void RecordMetric(string name, string category, double value, NewRelicXamarin.MetricUnit countUnit, NewRelicXamarin.MetricUnit valueUnit);

        bool SetAttribute(string name, string value);
        bool SetAttribute(string name, double value);
        bool SetAttribute(string name, bool value);

        bool IncrementAttribute(string name, float value = 1);

        bool RemoveAttribute(string name);

        bool RemoveAllAttributes();

        void SetMaxEventBufferTime(int maxBufferTimeInSec);

        void SetMaxEventPoolSize(int maxPoolSize);

        bool SetUserId(string userId);

        void AnalyticsEventEnabled(bool enabled);

        void NetworkRequestEnabled(bool enabled);

        void NetworkErrorRequestEnabled(bool enabled);

        void HttpResponseBodyCaptureEnabled(bool enabled);

        void RecordException(System.Exception exception);

        HttpMessageHandler GetHttpMessageHandler();

        void HandleUncaughtException(bool shouldThrowFormattedException = true);



    }

}
